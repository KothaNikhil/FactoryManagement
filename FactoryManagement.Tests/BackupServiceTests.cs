using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using FactoryManagement.Models;
using FactoryManagement.Data;
using FactoryManagement.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace FactoryManagement.Tests
{
    public class BackupServiceTests : IDisposable
    {
        private readonly FactoryDbContext _context;
        private readonly BackupService _backupService;
        private readonly string _testBackupDir;

        public BackupServiceTests()
        {
            var options = new DbContextOptionsBuilder<FactoryDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            _context = new FactoryDbContext(options);
            var tempDir = Path.Combine(Path.GetTempPath(), "FactoryManagementTests", Guid.NewGuid().ToString());
            _backupService = new TestBackupService(_context, tempDir);
            _testBackupDir = _backupService.GetBackupDirectory();
        }

        private class TestBackupService : BackupService
        {
            private readonly FactoryDbContext _ctx;
            private readonly string _dir;

            public TestBackupService(FactoryDbContext ctx, string dir) : base(ctx)
            {
                _ctx = ctx;
                _dir = dir;
                if (!Directory.Exists(_dir)) Directory.CreateDirectory(_dir);
            }

            public override async Task<string> CreateBackupAsync()
            {
                var backupData = new BackupData
                {
                    Items = await _ctx.Items.AsNoTracking().ToListAsync(),
                    Parties = await _ctx.Parties.AsNoTracking().ToListAsync(),
                    Workers = await _ctx.Workers.AsNoTracking().ToListAsync(),
                    Users = await _ctx.Users.AsNoTracking().ToListAsync(),
                    LoanAccounts = await _ctx.LoanAccounts.AsNoTracking().ToListAsync(),
                    Transactions = await _ctx.Transactions.AsNoTracking().ToListAsync(),
                    FinancialTransactions = await _ctx.FinancialTransactions.AsNoTracking().ToListAsync(),
                    WageTransactions = await _ctx.WageTransactions.AsNoTracking().ToListAsync(),
                    BackupDate = DateTime.Now
                };

                var fileName = $"Backup_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                var filePath = Path.Combine(_dir, fileName);

                var options = new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNameCaseInsensitive = true,
                    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
                };

                var json = System.Text.Json.JsonSerializer.Serialize(backupData, options);
                await File.WriteAllTextAsync(filePath, json);
                return filePath;
            }

            public override List<BackupFileInfo> GetAvailableBackups()
            {
                return Directory.GetFiles(_dir, "Backup_*.json")
                    .Select(f => new BackupFileInfo
                    {
                        FileName = Path.GetFileName(f),
                        FilePath = f,
                        CreatedDate = File.GetCreationTime(f),
                        FileSize = new FileInfo(f).Length
                    })
                    .OrderByDescending(b => b.CreatedDate)
                    .ToList();
            }

            public override async Task<BackupData?> GetBackupDetailsAsync(string filePath)
            {
                var json = await File.ReadAllTextAsync(filePath);
                var options = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
                };
                return System.Text.Json.JsonSerializer.Deserialize<BackupData>(json, options);
            }

            public override void DeleteBackup(string filePath)
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }

            public override string GetBackupDirectory() => _dir;
        }

        private async Task<string> CreateBackupSafeAsync(int maxAttempts = 3)
        {
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    return await _backupService.CreateBackupAsync();
                }
                catch (IOException)
                {
                    await Task.Delay(200);
                }
            }
            throw new Exception("Failed to create backup after retries due to file lock.");
        }

        [Fact]
        public async Task CreateBackup_CreatesFileAndData()
        {
            // Arrange
            _context.Items.Add(new Item { ItemName = "TestItem", Unit = "Kg", CurrentStock = 10 });
            _context.Parties.Add(new Party { Name = "TestParty", PartyType = PartyType.Buyer });
            _context.SaveChanges();

            // Act
            var filePath = await CreateBackupSafeAsync();

            // Assert
            Assert.True(File.Exists(filePath));
            var details = await _backupService.GetBackupDetailsAsync(filePath);
            Assert.NotNull(details);
            Assert.Single(details!.Items);
            Assert.Single(details!.Parties);
        }

        [Fact]
        public async Task RestoreBackup_RestoresData()
        {
            // Arrange
            _context.Items.Add(new Item { ItemName = "ToBeDeleted", Unit = "Kg", CurrentStock = 1 });
            _context.SaveChanges();
            var filePath = await CreateBackupSafeAsync();
            _context.Items.RemoveRange(_context.Items);
            _context.SaveChanges();
            Assert.Empty(_context.Items);

            // Act
            await _backupService.RestoreBackupAsync(filePath);

            // Assert
            Assert.NotEmpty(_context.Items);
        }

        [Fact]
        public void GetAvailableBackups_ReturnsList()
        {
            // Arrange
            var files = _backupService.GetAvailableBackups();
            // Assert
            Assert.NotNull(files);
        }

        [Fact]
        public async Task DeleteBackup_RemovesFile()
        {
            // Arrange
            _context.Items.Add(new Item { ItemName = "TestItem", Unit = "Kg", CurrentStock = 10 });
            _context.SaveChanges();
            var filePath = await CreateBackupSafeAsync();
            Assert.True(File.Exists(filePath));

            // Act
            _backupService.DeleteBackup(filePath);

            // Assert
            Assert.False(File.Exists(filePath));
        }

        public void Dispose()
        {
            if (Directory.Exists(_testBackupDir))
            {
                foreach (var file in Directory.GetFiles(_testBackupDir, "Backup_*.json"))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch
                    {
                        // Ignore file lock issues during cleanup
                    }
                }
            }
            _context.Dispose();
        }
    }
}

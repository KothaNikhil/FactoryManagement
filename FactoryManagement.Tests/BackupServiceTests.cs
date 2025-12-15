using System;
using System.IO;
using System.Linq;
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
            _backupService = new BackupService(_context);
            _testBackupDir = _backupService.GetBackupDirectory();
        }

        [Fact]
        public async Task CreateBackup_CreatesFileAndData()
        {
            // Arrange
            _context.Items.Add(new Item { ItemName = "TestItem", Unit = "Kg", CurrentStock = 10 });
            _context.Parties.Add(new Party { Name = "TestParty", PartyType = PartyType.Buyer });
            _context.SaveChanges();

            // Act
            var filePath = await _backupService.CreateBackupAsync();

            // Assert
            Assert.True(File.Exists(filePath));
            var details = await _backupService.GetBackupDetailsAsync(filePath);
            Assert.Single(details.Items);
            Assert.Single(details.Parties);
        }

        [Fact]
        public async Task RestoreBackup_RestoresData()
        {
            // Arrange
            _context.Items.Add(new Item { ItemName = "ToBeDeleted", Unit = "Kg", CurrentStock = 1 });
            _context.SaveChanges();
            var filePath = await _backupService.CreateBackupAsync();
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
            var filePath = await _backupService.CreateBackupAsync();
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
                    File.Delete(file);
                }
            }
            _context.Dispose();
        }
    }
}

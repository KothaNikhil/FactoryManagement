using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using FactoryManagement.Data;
using FactoryManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace FactoryManagement.Services
{
    public class BackupData
    {
        public List<Item> Items { get; set; } = new();
        public List<Party> Parties { get; set; } = new();
        public List<Worker> Workers { get; set; } = new();
        public List<User> Users { get; set; } = new();
        public List<LoanAccount> LoanAccounts { get; set; } = new();
        public List<Transaction> Transactions { get; set; } = new();
        public List<FinancialTransaction> FinancialTransactions { get; set; } = new();
        public List<WageTransaction> WageTransactions { get; set; } = new();
        public DateTime BackupDate { get; set; }
        public string Version { get; set; } = "1.0";
    }

    public class BackupService
    {
        private readonly FactoryDbContext _context;
        private readonly string _backupDirectory;
        private const string DefaultBackupFileName = "DefaultBackup.json";

        public BackupService(FactoryDbContext context)
        {
            _context = context;
            _backupDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "FactoryManagement",
                "Backups"
            );
            
            if (!Directory.Exists(_backupDirectory))
            {
                Directory.CreateDirectory(_backupDirectory);
            }
        }

        public string GetDefaultBackupPath()
        {
            return Path.Combine(_backupDirectory, DefaultBackupFileName);
        }

        public virtual async Task<string> UpdateDefaultBackupAsync()
        {
            try
            {
                var backupData = new BackupData
                {
                    Items = await _context.Items.AsNoTracking().ToListAsync(),
                    Parties = await _context.Parties.AsNoTracking().ToListAsync(),
                    Workers = await _context.Workers.AsNoTracking().ToListAsync(),
                    Users = await _context.Users.AsNoTracking().ToListAsync(),
                    LoanAccounts = await _context.LoanAccounts.AsNoTracking().ToListAsync(),
                    Transactions = await _context.Transactions.AsNoTracking().ToListAsync(),
                    FinancialTransactions = await _context.FinancialTransactions.AsNoTracking().ToListAsync(),
                    WageTransactions = await _context.WageTransactions.AsNoTracking().ToListAsync(),
                    BackupDate = DateTime.Now
                };

                var defaultPath = GetDefaultBackupPath();

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNameCaseInsensitive = true,
                    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
                };

                var json = JsonSerializer.Serialize(backupData, options);
                Directory.CreateDirectory(_backupDirectory);

                // Ensure file is writable before overwriting
                if (File.Exists(defaultPath))
                {
                    try
                    {
                        var current = File.GetAttributes(defaultPath);
                        if ((current & FileAttributes.ReadOnly) != 0)
                        {
                            File.SetAttributes(defaultPath, current & ~FileAttributes.ReadOnly);
                        }
                    }
                    catch { /* ignore */ }
                }

                await File.WriteAllTextAsync(defaultPath, json);

                // Mark as hidden/read-only to discourage manual edits
                try
                {
                    var attrs = File.GetAttributes(defaultPath);
                    attrs |= FileAttributes.ReadOnly | FileAttributes.Hidden;
                    File.SetAttributes(defaultPath, attrs);
                }
                catch { /* best-effort; ignore attribute errors */ }

                return defaultPath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update default backup: {ex.Message}", ex);
            }
        }

        public virtual async Task<string> CreateBackupAsync()
        {
            try
            {
                var backupData = new BackupData
                {
                    Items = await _context.Items.AsNoTracking().ToListAsync(),
                    Parties = await _context.Parties.AsNoTracking().ToListAsync(),
                    Workers = await _context.Workers.AsNoTracking().ToListAsync(),
                    Users = await _context.Users.AsNoTracking().ToListAsync(),
                    LoanAccounts = await _context.LoanAccounts.AsNoTracking().ToListAsync(),
                    Transactions = await _context.Transactions.AsNoTracking().ToListAsync(),
                    FinancialTransactions = await _context.FinancialTransactions.AsNoTracking().ToListAsync(),
                    WageTransactions = await _context.WageTransactions.AsNoTracking().ToListAsync(),
                    BackupDate = DateTime.Now
                };

                var fileName = $"Backup_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                var filePath = Path.Combine(_backupDirectory, fileName);

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNameCaseInsensitive = true,
                    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
                };

                var json = JsonSerializer.Serialize(backupData, options);
                await File.WriteAllTextAsync(filePath, json);

                return filePath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create backup: {ex.Message}", ex);
            }
        }

        public virtual async Task RestoreBackupAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException("Backup file not found", filePath);
                }

                var json = await File.ReadAllTextAsync(filePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
                };

                var backupData = JsonSerializer.Deserialize<BackupData>(json, options);

                if (backupData == null)
                {
                    throw new Exception("Invalid backup file format");
                }

                // Start a transaction for atomic restore
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Clear existing data
                    _context.WageTransactions.RemoveRange(_context.WageTransactions);
                    _context.FinancialTransactions.RemoveRange(_context.FinancialTransactions);
                    _context.Transactions.RemoveRange(_context.Transactions);
                    _context.LoanAccounts.RemoveRange(_context.LoanAccounts);
                    _context.Items.RemoveRange(_context.Items);
                    _context.Workers.RemoveRange(_context.Workers);
                    _context.Parties.RemoveRange(_context.Parties);
                    _context.Users.RemoveRange(_context.Users);
                    await _context.SaveChangesAsync();

                    // Clear the change tracker to avoid tracking issues
                    _context.ChangeTracker.Clear();

                    // Restore items
                    foreach (var item in backupData.Items)
                    {
                        _context.Items.Add(item);
                    }
                    await _context.SaveChangesAsync();

                    // Clear the change tracker again
                    _context.ChangeTracker.Clear();

                    // Restore parties
                    foreach (var party in backupData.Parties)
                    {
                        _context.Parties.Add(party);
                    }
                    await _context.SaveChangesAsync();

                    // Clear the change tracker again
                    _context.ChangeTracker.Clear();

                    // Restore users
                    if (backupData.Users != null && backupData.Users.Any())
                    {
                        foreach (var user in backupData.Users)
                        {
                            _context.Users.Add(user);
                        }
                        await _context.SaveChangesAsync();
                    }

                    // Clear the change tracker again
                    _context.ChangeTracker.Clear();

                    // Restore workers
                    if (backupData.Workers != null && backupData.Workers.Any())
                    {
                        foreach (var worker in backupData.Workers)
                        {
                            _context.Workers.Add(worker);
                        }
                        await _context.SaveChangesAsync();
                    }

                    // Clear the change tracker again
                    _context.ChangeTracker.Clear();

                    // Restore loan accounts
                    if (backupData.LoanAccounts != null && backupData.LoanAccounts.Any())
                    {
                        foreach (var loanAccount in backupData.LoanAccounts)
                        {
                            _context.LoanAccounts.Add(loanAccount);
                        }
                        await _context.SaveChangesAsync();
                    }

                    // Clear the change tracker again
                    _context.ChangeTracker.Clear();

                    // Restore transactions
                    foreach (var trans in backupData.Transactions)
                    {
                        _context.Transactions.Add(trans);
                    }
                    await _context.SaveChangesAsync();

                    // Clear the change tracker again
                    _context.ChangeTracker.Clear();

                    // Restore financial transactions
                    if (backupData.FinancialTransactions != null && backupData.FinancialTransactions.Any())
                    {
                        foreach (var ft in backupData.FinancialTransactions)
                        {
                            _context.FinancialTransactions.Add(ft);
                        }
                        await _context.SaveChangesAsync();
                    }

                    // Clear the change tracker again
                    _context.ChangeTracker.Clear();

                    // Restore wage transactions
                    if (backupData.WageTransactions != null && backupData.WageTransactions.Any())
                    {
                        foreach (var wt in backupData.WageTransactions)
                        {
                            _context.WageTransactions.Add(wt);
                        }
                        await _context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? $" Inner: {ex.InnerException.Message}" : "";
                throw new Exception($"Failed to restore backup: {ex.Message}{innerMessage}", ex);
            }
        }

        public virtual List<BackupFileInfo> GetAvailableBackups()
        {
            try
            {
                var results = new List<BackupFileInfo>();

                // Include default backup if present
                var defaultPath = GetDefaultBackupPath();
                if (File.Exists(defaultPath))
                {
                    var info = new FileInfo(defaultPath);
                    results.Add(new BackupFileInfo
                    {
                        FileName = Path.GetFileName(defaultPath),
                        FilePath = defaultPath,
                        CreatedDate = info.Exists ? info.CreationTime : DateTime.MinValue,
                        FileSize = info.Exists ? info.Length : 0,
                        IsDefault = true
                    });
                }

                // Add timestamped backups
                var others = Directory.GetFiles(_backupDirectory, "Backup_*.json")
                    .Select(f => new BackupFileInfo
                    {
                        FileName = Path.GetFileName(f),
                        FilePath = f,
                        CreatedDate = File.GetCreationTime(f),
                        FileSize = new FileInfo(f).Length,
                        IsDefault = false
                    })
                    .OrderByDescending(b => b.CreatedDate)
                    .ToList();

                results.AddRange(others);

                return results;
            }
            catch
            {
                return new List<BackupFileInfo>();
            }
        }

        public virtual async Task<BackupData?> GetBackupDetailsAsync(string filePath)
        {
            try
            {
                var json = await File.ReadAllTextAsync(filePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
                };

                return JsonSerializer.Deserialize<BackupData>(json, options);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to read backup details: {ex.Message}", ex);
            }
        }

        public virtual void DeleteBackup(string filePath)
        {
            try
            {
                // Prevent deletion of the default rolling backup from the application
                var defaultPath = GetDefaultBackupPath();
                if (string.Equals(Path.GetFullPath(filePath), Path.GetFullPath(defaultPath), StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("The default backup cannot be deleted from the application.");
                }

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete backup: {ex.Message}", ex);
            }
        }

        public virtual string GetBackupDirectory()
        {
            return _backupDirectory;
        }
    }

    public class BackupFileInfo
    {
        public required string FileName { get; set; }
        public required string FilePath { get; set; }
        public DateTime CreatedDate { get; set; }
        public long FileSize { get; set; }
        public bool IsDefault { get; set; }

        public string FormattedSize => FormatFileSize(FileSize);
        public string FormattedDate => CreatedDate.ToString("yyyy-MM-dd HH:mm:ss");
        public string DisplayName => IsDefault ? $"{FileName} (Default, auto-updating)" : FileName;

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}

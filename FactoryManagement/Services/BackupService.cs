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
        public List<Transaction> Transactions { get; set; } = new();
        public DateTime BackupDate { get; set; }
        public string Version { get; set; } = "1.0";
    }

    public class BackupService
    {
        private readonly FactoryDbContext _context;
        private readonly string _backupDirectory;

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

        public virtual async Task<string> CreateBackupAsync()
        {
            try
            {
                var backupData = new BackupData
                {
                    Items = await _context.Items.ToListAsync(),
                    Parties = await _context.Parties.ToListAsync(),
                    Transactions = await _context.Transactions
                        .Include(t => t.Item)
                        .Include(t => t.Party)
                        .ToListAsync(),
                    BackupDate = DateTime.Now
                };

                var fileName = $"Backup_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                var filePath = Path.Combine(_backupDirectory, fileName);

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNameCaseInsensitive = true
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
                    PropertyNameCaseInsensitive = true
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
                    _context.Transactions.RemoveRange(_context.Transactions);
                    _context.Items.RemoveRange(_context.Items);
                    _context.Parties.RemoveRange(_context.Parties);
                    await _context.SaveChangesAsync();

                    // Clear the change tracker to avoid tracking issues
                    _context.ChangeTracker.Clear();

                    // Restore items
                    foreach (var item in backupData.Items)
                    {
                        var newItem = new Item
                        {
                            ItemId = item.ItemId,
                            ItemName = item.ItemName,
                            Unit = item.Unit,
                            CurrentStock = item.CurrentStock,
                            CreatedDate = item.CreatedDate,
                            ModifiedDate = item.ModifiedDate
                        };
                        _context.Items.Add(newItem);
                    }
                    await _context.SaveChangesAsync();

                    // Clear the change tracker again
                    _context.ChangeTracker.Clear();

                    // Restore parties
                    foreach (var party in backupData.Parties)
                    {
                        var newParty = new Party
                        {
                            PartyId = party.PartyId,
                            Name = party.Name,
                            MobileNumber = party.MobileNumber,
                            Place = party.Place,
                            PartyType = party.PartyType,
                            CreatedDate = party.CreatedDate,
                            ModifiedDate = party.ModifiedDate
                        };
                        _context.Parties.Add(newParty);
                    }
                    await _context.SaveChangesAsync();

                    // Clear the change tracker again
                    _context.ChangeTracker.Clear();

                    // Restore transactions
                    foreach (var trans in backupData.Transactions)
                    {
                        var newTransaction = new Transaction
                        {
                            TransactionId = trans.TransactionId,
                            TransactionType = trans.TransactionType,
                            ItemId = trans.ItemId,
                            PartyId = trans.PartyId,
                            Quantity = trans.Quantity,
                            PricePerUnit = trans.PricePerUnit,
                            TotalAmount = trans.TotalAmount,
                            TransactionDate = trans.TransactionDate,
                            EnteredBy = trans.EnteredBy,
                            CreatedDate = trans.CreatedDate
                        };
                        _context.Transactions.Add(newTransaction);
                    }
                    await _context.SaveChangesAsync();

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
                throw new Exception($"Failed to restore backup: {ex.Message}", ex);
            }
        }

        public virtual List<BackupFileInfo> GetAvailableBackups()
        {
            try
            {
                var backupFiles = Directory.GetFiles(_backupDirectory, "Backup_*.json")
                    .Select(f => new BackupFileInfo
                    {
                        FileName = Path.GetFileName(f),
                        FilePath = f,
                        CreatedDate = File.GetCreationTime(f),
                        FileSize = new FileInfo(f).Length
                    })
                    .OrderByDescending(b => b.CreatedDate)
                    .ToList();

                return backupFiles;
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
                    PropertyNameCaseInsensitive = true
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

        public string FormattedSize => FormatFileSize(FileSize);
        public string FormattedDate => CreatedDate.ToString("yyyy-MM-dd HH:mm:ss");

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

using Microsoft.EntityFrameworkCore;
using FactoryManagement.Models;
using System;
using System.Linq;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FactoryManagement.Services;

namespace FactoryManagement.Data
{
    public class FactoryDbContext : DbContext
    {
        public DbSet<Item> Items { get; set; }
        public DbSet<Party> Parties { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<AppSettings> AppSettings { get; set; }
        public DbSet<FinancialTransaction> FinancialTransactions { get; set; }
        public DbSet<LoanAccount> LoanAccounts { get; set; }
        public DbSet<Worker> Workers { get; set; }
        public DbSet<WageTransaction> WageTransactions { get; set; }

        public FactoryDbContext(DbContextOptions<FactoryDbContext> options) : base(options)
        {
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            var result = base.SaveChanges(acceptAllChangesOnSuccess);
            try { TryUpdateDefaultBackup(); } catch { /* swallow to not block app flow */ }
            return result;
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            try { await TryUpdateDefaultBackupAsync(); } catch { /* swallow to not block app flow */ }
            return result;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure indexes for performance
            modelBuilder.Entity<Item>()
                .HasIndex(i => i.ItemName);

            modelBuilder.Entity<Party>()
                .HasIndex(p => p.Name);

            modelBuilder.Entity<Party>()
                .HasIndex(p => p.PartyType);

            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.TransactionDate);

            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.TransactionType);

            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.ItemId);

            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.PartyId);

            // Configure decimal precision
            modelBuilder.Entity<Item>()
                .Property(i => i.CurrentStock)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Quantity)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Transaction>()
                .Property(t => t.PricePerUnit)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Transaction>()
                .Property(t => t.TotalAmount)
                .HasPrecision(18, 2);

            // Configure Financial Transaction indexes
            modelBuilder.Entity<FinancialTransaction>()
                .HasIndex(ft => ft.TransactionDate);

            modelBuilder.Entity<FinancialTransaction>()
                .HasIndex(ft => ft.TransactionType);

            modelBuilder.Entity<FinancialTransaction>()
                .HasIndex(ft => ft.PartyId);

            modelBuilder.Entity<FinancialTransaction>()
                .HasIndex(ft => ft.LinkedLoanAccountId);

            // Configure Loan Account indexes
            modelBuilder.Entity<LoanAccount>()
                .HasIndex(la => la.PartyId);

            modelBuilder.Entity<LoanAccount>()
                .HasIndex(la => la.LoanType);

            modelBuilder.Entity<LoanAccount>()
                .HasIndex(la => la.Status);

            modelBuilder.Entity<LoanAccount>()
                .HasIndex(la => la.DueDate);

            // Configure Worker indexes
            modelBuilder.Entity<Worker>()
                .HasIndex(w => w.Name);

            modelBuilder.Entity<Worker>()
                .HasIndex(w => w.Status);

            // Configure WageTransaction indexes
            modelBuilder.Entity<WageTransaction>()
                .HasIndex(wt => wt.WorkerId);

            modelBuilder.Entity<WageTransaction>()
                .HasIndex(wt => wt.TransactionDate);

            modelBuilder.Entity<WageTransaction>()
                .HasIndex(wt => wt.TransactionType);

            // Configure relationships
            modelBuilder.Entity<WageTransaction>()
                .HasOne(wt => wt.Worker)
                .WithMany(w => w.WageTransactions)
                .HasForeignKey(wt => wt.WorkerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed initial data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Users
            modelBuilder.Entity<User>().HasData(
                new User { UserId = 1, Username = "Admin", Role = "Administrator", IsActive = true },
                new User { UserId = 2, Username = "Manager", Role = "Manager", IsActive = true },
                new User { UserId = 3, Username = "Operator", Role = "Operator", IsActive = true }
            );

            // Seed Items
            modelBuilder.Entity<Item>().HasData(
                new Item { ItemId = 1, ItemName = "Rice", CurrentStock = 1000, Unit = "Kg" },
                new Item { ItemId = 2, ItemName = "Husk", CurrentStock = 500, Unit = "Kg" },
                new Item { ItemId = 3, ItemName = "Paddy", CurrentStock = 2000, Unit = "Kg" },
                new Item { ItemId = 4, ItemName = "Broken Rice", CurrentStock = 300, Unit = "Kg" },
                new Item { ItemId = 5, ItemName = "Bran", CurrentStock = 150, Unit = "Kg" }
            );

            // Seed Parties
            modelBuilder.Entity<Party>().HasData(
                new Party { PartyId = 1, Name = "ABC Traders", MobileNumber = "9876543210", Place = "Mumbai", PartyType = PartyType.Both },
                new Party { PartyId = 2, Name = "XYZ Suppliers", MobileNumber = "9876543211", Place = "Delhi", PartyType = PartyType.Seller },
                new Party { PartyId = 3, Name = "PQR Distributors", MobileNumber = "9876543212", Place = "Bangalore", PartyType = PartyType.Buyer },
                new Party { PartyId = 4, Name = "LMN Enterprises", MobileNumber = "9876543213", Place = "Chennai", PartyType = PartyType.Both }
            );

            // Seed AppSettings
            modelBuilder.Entity<AppSettings>().HasData(
                new AppSettings { SettingId = 1, CompanyName = "Factory Management System", CurrencySymbol = "₹", Address = "123 Industrial Area" }
            );
        }

        private string GetBackupsDirectory()
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "FactoryManagement", "Backups");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            return dir;
        }

        private string GetDefaultBackupPath() => Path.Combine(GetBackupsDirectory(), "DefaultBackup.json");

        private void TryUpdateDefaultBackup()
        {
            // Sync wrapper for async method
            TryUpdateDefaultBackupAsync().GetAwaiter().GetResult();
        }

        private async Task TryUpdateDefaultBackupAsync()
        {
            // Create a snapshot of all entities after successful save
            var snapshot = new BackupData
            {
                Items = await Items.AsNoTracking().ToListAsync(),
                Parties = await Parties.AsNoTracking().ToListAsync(),
                Workers = await Workers.AsNoTracking().ToListAsync(),
                Users = await Users.AsNoTracking().ToListAsync(),
                LoanAccounts = await LoanAccounts.AsNoTracking().ToListAsync(),
                Transactions = await Transactions.AsNoTracking().ToListAsync(),
                FinancialTransactions = await FinancialTransactions.AsNoTracking().ToListAsync(),
                WageTransactions = await WageTransactions.AsNoTracking().ToListAsync(),
                BackupDate = DateTime.Now
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true,
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
            };

            var json = JsonSerializer.Serialize(snapshot, options);
            var defaultPath = GetDefaultBackupPath();

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
            catch { /* ignore attribute errors */ }
        }
    }
}

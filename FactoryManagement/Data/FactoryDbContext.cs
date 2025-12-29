using Microsoft.EntityFrameworkCore;
using FactoryManagement.Models;
using System;
using System.Linq;

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

            // Configure Processing-specific fields
            modelBuilder.Entity<Transaction>()
                .Property(t => t.InputQuantity)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Transaction>()
                .Property(t => t.ConversionRate)
                .HasPrecision(5, 4);

            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.InputItemId);

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
            // Transactions: prevent cascades
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.EnteredBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Item)
                .WithMany()
                .HasForeignKey(t => t.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Party)
                .WithMany()
                .HasForeignKey(t => t.PartyId)
                .OnDelete(DeleteBehavior.SetNull);

            // Financial Transactions and Loans
            modelBuilder.Entity<FinancialTransaction>()
                .HasOne(ft => ft.User)
                .WithMany()
                .HasForeignKey(ft => ft.EnteredBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FinancialTransaction>()
                .HasOne(ft => ft.Party)
                .WithMany()
                .HasForeignKey(ft => ft.PartyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FinancialTransaction>()
                .HasOne(ft => ft.LinkedLoanAccount)
                .WithMany(l => l.Transactions)
                .HasForeignKey(ft => ft.LinkedLoanAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LoanAccount>()
                .HasOne(l => l.Party)
                .WithMany()
                .HasForeignKey(l => l.PartyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LoanAccount>()
                .HasOne(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // Party and Item created/modified by
            modelBuilder.Entity<Party>()
                .HasOne(p => p.CreatedBy)
                .WithMany()
                .HasForeignKey(p => p.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Party>()
                .HasOne(p => p.ModifiedBy)
                .WithMany()
                .HasForeignKey(p => p.ModifiedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Item>()
                .HasOne(i => i.CreatedBy)
                .WithMany()
                .HasForeignKey(i => i.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Item>()
                .HasOne(i => i.ModifiedBy)
                .WithMany()
                .HasForeignKey(i => i.ModifiedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Worker metadata
            modelBuilder.Entity<Worker>()
                .HasOne(w => w.CreatedBy)
                .WithMany()
                .HasForeignKey(w => w.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Worker>()
                .HasOne(w => w.ModifiedBy)
                .WithMany()
                .HasForeignKey(w => w.ModifiedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<WageTransaction>()
                .HasOne(wt => wt.Worker)
                .WithMany(w => w.WageTransactions)
                .HasForeignKey(wt => wt.WorkerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WageTransaction>()
                .HasOne(wt => wt.User)
                .WithMany()
                .HasForeignKey(wt => wt.EnteredBy)
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
    }
}

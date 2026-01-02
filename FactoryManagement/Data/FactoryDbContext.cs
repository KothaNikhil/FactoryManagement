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
        public DbSet<ExpenseCategory> ExpenseCategories { get; set; }
        public DbSet<OperationalExpense> OperationalExpenses { get; set; }

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

            // Configure ExpenseCategory indexes
            modelBuilder.Entity<ExpenseCategory>()
                .HasIndex(ec => ec.CategoryName)
                .IsUnique();

            modelBuilder.Entity<ExpenseCategory>()
                .HasIndex(ec => ec.IsDeleted);

            // Configure OperationalExpense indexes
            modelBuilder.Entity<OperationalExpense>()
                .HasIndex(oe => oe.ExpenseDate);

            modelBuilder.Entity<OperationalExpense>()
                .HasIndex(oe => oe.ExpenseCategoryId);

            modelBuilder.Entity<OperationalExpense>()
                .HasIndex(oe => oe.SpentBy);

            modelBuilder.Entity<OperationalExpense>()
                .HasIndex(oe => oe.IsApproved);

            modelBuilder.Entity<OperationalExpense>()
                .Property(oe => oe.Amount)
                .HasPrecision(18, 2);

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
                .OnDelete(DeleteBehavior.SetNull);

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
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<FinancialTransaction>()
                .HasOne(ft => ft.LinkedLoanAccount)
                .WithMany(l => l.Transactions)
                .HasForeignKey(ft => ft.LinkedLoanAccountId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LoanAccount>()
                .HasOne(l => l.Party)
                .WithMany()
                .HasForeignKey(l => l.PartyId)
                .OnDelete(DeleteBehavior.SetNull);

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

            // OperationalExpense relationships
            modelBuilder.Entity<OperationalExpense>()
                .HasOne(oe => oe.ExpenseCategory)
                .WithMany(ec => ec.OperationalExpenses)
                .HasForeignKey(oe => oe.ExpenseCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OperationalExpense>()
                .HasOne(oe => oe.SpentByUser)
                .WithMany()
                .HasForeignKey(oe => oe.SpentBy)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<OperationalExpense>()
                .HasOne(oe => oe.Item)
                .WithMany()
                .HasForeignKey(oe => oe.ItemId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<OperationalExpense>()
                .HasOne(oe => oe.User)
                .WithMany()
                .HasForeignKey(oe => oe.EnteredBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OperationalExpense>()
                .HasOne(oe => oe.Approver)
                .WithMany()
                .HasForeignKey(oe => oe.ApprovedBy)
                .OnDelete(DeleteBehavior.SetNull);

            // ExpenseCategory relationships
            modelBuilder.Entity<ExpenseCategory>()
                .HasOne(ec => ec.Creator)
                .WithMany()
                .HasForeignKey(ec => ec.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed initial data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed only Admin user - additional users can be created as needed
            modelBuilder.Entity<User>().HasData(
                new User { UserId = 1, Username = "Admin", Role = "Administrator", IsActive = true }
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

            // Seed ExpenseCategories
            modelBuilder.Entity<ExpenseCategory>().HasData(
                // Transportation & Logistics
                new ExpenseCategory { ExpenseCategoryId = 1, CategoryName = "Cab Charges", Description = "Transportation and cab expenses", CreatedBy = 1, CreatedDate = new DateTime(2026, 1, 1) },
                new ExpenseCategory { ExpenseCategoryId = 2, CategoryName = "Transportation Fees", Description = "General transportation costs", CreatedBy = 1, CreatedDate = new DateTime(2026, 1, 1) },
                new ExpenseCategory { ExpenseCategoryId = 3, CategoryName = "Freight Charges", Description = "Shipping and freight costs", CreatedBy = 1, CreatedDate = new DateTime(2026, 1, 1) },
                
                // Utilities
                new ExpenseCategory { ExpenseCategoryId = 4, CategoryName = "Electricity", Description = "Electricity bills and charges", CreatedBy = 1, CreatedDate = new DateTime(2026, 1, 1) },
                new ExpenseCategory { ExpenseCategoryId = 5, CategoryName = "Water", Description = "Water bills and charges", CreatedBy = 1, CreatedDate = new DateTime(2026, 1, 1) },
                new ExpenseCategory { ExpenseCategoryId = 6, CategoryName = "Internet & Phone", Description = "Communication expenses", CreatedBy = 1, CreatedDate = new DateTime(2026, 1, 1) },
                
                // Machinery & Equipment
                new ExpenseCategory { ExpenseCategoryId = 7, CategoryName = "Machinery Purchase", Description = "New machinery and equipment purchases", CreatedBy = 1, CreatedDate = new DateTime(2026, 1, 1) },
                new ExpenseCategory { ExpenseCategoryId = 8, CategoryName = "Machinery Repair", Description = "Machine repair and servicing costs", CreatedBy = 1, CreatedDate = new DateTime(2026, 1, 1) },
                new ExpenseCategory { ExpenseCategoryId = 9, CategoryName = "Machinery Maintenance", Description = "Regular maintenance costs", CreatedBy = 1, CreatedDate = new DateTime(2026, 1, 1) },
                
                // Fuel & Energy
                new ExpenseCategory { ExpenseCategoryId = 10, CategoryName = "Fuel", Description = "Fuel and petroleum expenses", CreatedBy = 1, CreatedDate = new DateTime(2026, 1, 1) },
                new ExpenseCategory { ExpenseCategoryId = 11, CategoryName = "Generator Diesel", Description = "Diesel for generators", CreatedBy = 1, CreatedDate = new DateTime(2026, 1, 1) },
                
                // Facility Costs
                new ExpenseCategory { ExpenseCategoryId = 12, CategoryName = "Rent", Description = "Facility and equipment rent", CreatedBy = 1, CreatedDate = DateTime.Now },
                new ExpenseCategory { ExpenseCategoryId = 13, CategoryName = "Insurance", Description = "Insurance premiums", CreatedBy = 1, CreatedDate = DateTime.Now },
                
                // Office & Admin
                new ExpenseCategory { ExpenseCategoryId = 14, CategoryName = "Stationery", Description = "Office supplies and stationery", CreatedBy = 1, CreatedDate = DateTime.Now },
                new ExpenseCategory { ExpenseCategoryId = 15, CategoryName = "Printing", Description = "Printing and documentation costs", CreatedBy = 1, CreatedDate = DateTime.Now },
                
                // Professional Services
                new ExpenseCategory { ExpenseCategoryId = 16, CategoryName = "Legal Fees", Description = "Legal and compliance costs", CreatedBy = 1, CreatedDate = DateTime.Now },
                new ExpenseCategory { ExpenseCategoryId = 17, CategoryName = "Accounting Fees", Description = "Accounting and auditing fees", CreatedBy = 1, CreatedDate = DateTime.Now },
                
                // Maintenance & Repairs
                new ExpenseCategory { ExpenseCategoryId = 18, CategoryName = "Building Maintenance", Description = "Building and facility maintenance", CreatedBy = 1, CreatedDate = DateTime.Now },
                new ExpenseCategory { ExpenseCategoryId = 19, CategoryName = "Repairs", Description = "General repair expenses", CreatedBy = 1, CreatedDate = DateTime.Now },
                
                // Other
                new ExpenseCategory { ExpenseCategoryId = 20, CategoryName = "Miscellaneous", Description = "Other miscellaneous expenses", CreatedBy = 1, CreatedDate = DateTime.Now }
            );
        }
    }
}

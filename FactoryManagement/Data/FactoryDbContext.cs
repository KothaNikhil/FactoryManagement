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

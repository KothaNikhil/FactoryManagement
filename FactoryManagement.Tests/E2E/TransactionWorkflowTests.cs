using FactoryManagement.Data;
using FactoryManagement.Data.Repositories;
using FactoryManagement.Models;
using FactoryManagement.Services;
using FactoryManagement.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FactoryManagement.Tests.E2E
{
    /// <summary>
    /// End-to-End tests for complete transaction workflows
    /// Tests the entire flow: ViewModel → Service → Repository → Database
    /// </summary>
    public class TransactionWorkflowTests : IDisposable
    {
        private readonly DbContextOptions<FactoryDbContext> _options;
        private readonly FactoryDbContext _context;
        private readonly ItemService _itemService;
        private readonly PartyService _partyService;
        private readonly TransactionService _transactionService;

        public TransactionWorkflowTests()
        {
            _options = new DbContextOptionsBuilder<FactoryDbContext>()
                .UseInMemoryDatabase(databaseName: $"E2ETransactionTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new FactoryDbContext(_options);
            
            var itemRepo = new Repository<Item>(_context);
            var partyRepo = new Repository<Party>(_context);
            var transactionRepo = new TransactionRepository(_context);

            _itemService = new ItemService(itemRepo);
            _partyService = new PartyService(partyRepo);
            _transactionService = new TransactionService(transactionRepo, _itemService);

            // Seed required data
            SeedTestData().Wait();
        }

        private async Task SeedTestData()
        {
            // Create a user for transactions
            await _context.Users.AddAsync(new User 
            { 
                UserId = 1, 
                Username = "admin", 
                Role = "Admin", 
                IsActive = true 
            });
            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task CompleteTransactionWorkflow_BuyFlow_ShouldIncreaseStock()
        {
            // Arrange - Simulate user creating a purchase transaction

            // Step 1: User creates a new item
            var item = new Item
            {
                ItemName = "Laptop",
                Unit = "Pcs",
                CurrentStock = 10
            };
            var createdItem = await _itemService.AddItemAsync(item);
            Assert.NotNull(createdItem);
            Assert.Equal(10, createdItem.CurrentStock);

            // Step 2: User creates a supplier party
            var supplier = new Party
            {
                Name = "Tech Supplier Ltd",
                PartyType = PartyType.Seller,
                MobileNumber = "1234567890",
                Place = "123 Tech Street"
            };
            var createdSupplier = await _partyService.AddPartyAsync(supplier);
            Assert.NotNull(createdSupplier);

            // Step 3: User creates a BUY transaction
            var buyTransaction = new Transaction
            {
                ItemId = createdItem.ItemId,
                PartyId = createdSupplier.PartyId,
                TransactionType = TransactionType.Buy,
                Quantity = 50,
                PricePerUnit = 500,
                TotalAmount = 25000,
                TransactionDate = DateTime.Now,
                EnteredBy = 1
            };

            // Act - User saves the transaction
            var savedTransaction = await _transactionService.AddTransactionAsync(buyTransaction);

            // Assert - Verify complete workflow
            Assert.NotNull(savedTransaction);
            Assert.Equal(50, savedTransaction.Quantity);
            Assert.Equal(25000, savedTransaction.TotalAmount);

            // Verify stock increased
            var updatedItem = await _itemService.GetItemByIdAsync(createdItem.ItemId);
            Assert.NotNull(updatedItem);
            Assert.Equal(60, updatedItem!.CurrentStock); // 10 + 50

            // Verify transaction appears in recent transactions
            var recentTransactions = await _transactionService.GetRecentTransactionsAsync(10);
            Assert.Contains(recentTransactions, t => t.TransactionId == savedTransaction.TransactionId);

            // Verify transaction has correct relationships
            var transactionWithDetails = await _context.Transactions
                .Include(t => t.Item)
                .Include(t => t.Party)
                .FirstOrDefaultAsync(t => t.TransactionId == savedTransaction.TransactionId);
            
            Assert.NotNull(transactionWithDetails);
            Assert.NotNull(transactionWithDetails!.Item);
            Assert.NotNull(transactionWithDetails!.Party);
            Assert.Equal("Laptop", transactionWithDetails!.Item!.ItemName);
            Assert.Equal("Tech Supplier Ltd", transactionWithDetails!.Party!.Name);
        }

        [Fact]
        public async Task CompleteTransactionWorkflow_SellFlow_ShouldDecreaseStock()
        {
            // Arrange - Setup for a sale
            var item = new Item
            {
                ItemName = "Mouse",
                Unit = "Pcs",
                CurrentStock = 100
            };
            var createdItem = await _itemService.AddItemAsync(item);

            var customer = new Party
            {
                Name = "ABC Corporation",
                PartyType = PartyType.Buyer,
                MobileNumber = "9876543210"
            };
            var createdCustomer = await _partyService.AddPartyAsync(customer);

            // Act - User creates a SELL transaction
            var sellTransaction = new Transaction
            {
                ItemId = createdItem.ItemId,
                PartyId = createdCustomer.PartyId,
                TransactionType = TransactionType.Sell,
                Quantity = 30,
                PricePerUnit = 600,
                TotalAmount = 18000,
                TransactionDate = DateTime.Now,
                EnteredBy = 1
            };

            var savedTransaction = await _transactionService.AddTransactionAsync(sellTransaction);

            // Assert - Verify stock decreased
            var updatedItem = await _itemService.GetItemByIdAsync(createdItem.ItemId);
            Assert.NotNull(updatedItem);
            Assert.Equal(70, updatedItem!.CurrentStock); // 100 - 30

            // Verify transaction type
            Assert.Equal(TransactionType.Sell, savedTransaction.TransactionType);
        }

        [Fact]
        public async Task CompleteTransactionWorkflow_WastageFlow_ShouldDecreaseStock()
        {
            // Arrange
            var item = new Item
            {
                ItemName = "Perishable Item",
                Unit = "Kg",
                CurrentStock = 50
            };
            var createdItem = await _itemService.AddItemAsync(item);

            // Act - User records wastage
            var wastageTransaction = new Transaction
            {
                ItemId = createdItem.ItemId,
                PartyId = null, // Wastage doesn't require a party
                TransactionType = TransactionType.Wastage,
                Quantity = 5,
                PricePerUnit = 0,
                TotalAmount = 0,
                TransactionDate = DateTime.Now,
                EnteredBy = 1
            };

            var savedTransaction = await _transactionService.AddTransactionAsync(wastageTransaction);

            // Assert
            var updatedItem = await _itemService.GetItemByIdAsync(createdItem.ItemId);
            Assert.NotNull(updatedItem);
            Assert.Equal(45, updatedItem!.CurrentStock); // 50 - 5
            Assert.Equal(TransactionType.Wastage, savedTransaction.TransactionType);
        }

        [Fact]
        public async Task CompleteTransactionWorkflow_MultipleTransactions_ShouldUpdateStockCorrectly()
        {
            // Arrange - Complete business scenario
            var item = new Item
            {
                ItemName = "Office Chair",
                Unit = "Pcs",
                CurrentStock = 20
            };
            var createdItem = await _itemService.AddItemAsync(item);

            var supplier = new Party { Name = "Furniture Supplier", PartyType = PartyType.Seller };
            var customer1 = new Party { Name = "Customer A", PartyType = PartyType.Buyer };
            var customer2 = new Party { Name = "Customer B", PartyType = PartyType.Buyer };

            var savedSupplier = await _partyService.AddPartyAsync(supplier);
            var savedCustomer1 = await _partyService.AddPartyAsync(customer1);
            var savedCustomer2 = await _partyService.AddPartyAsync(customer2);

            // Act - Simulate a day of transactions
            
            // Buy 100 units
            await _transactionService.AddTransactionAsync(new Transaction
            {
                ItemId = createdItem.ItemId,
                PartyId = savedSupplier.PartyId,
                TransactionType = TransactionType.Buy,
                Quantity = 100,
                PricePerUnit = 200,
                TotalAmount = 20000,
                EnteredBy = 1
            });

            // Sell 30 units to Customer A
            await _transactionService.AddTransactionAsync(new Transaction
            {
                ItemId = createdItem.ItemId,
                PartyId = savedCustomer1.PartyId,
                TransactionType = TransactionType.Sell,
                Quantity = 30,
                PricePerUnit = 250,
                TotalAmount = 7500,
                EnteredBy = 1
            });

            // Sell 25 units to Customer B
            await _transactionService.AddTransactionAsync(new Transaction
            {
                ItemId = createdItem.ItemId,
                PartyId = savedCustomer2.PartyId,
                TransactionType = TransactionType.Sell,
                Quantity = 25,
                PricePerUnit = 250,
                TotalAmount = 6250,
                EnteredBy = 1
            });

            // Record 2 units wastage
            await _transactionService.AddTransactionAsync(new Transaction
            {
                ItemId = createdItem.ItemId,
                TransactionType = TransactionType.Wastage,
                Quantity = 2,
                PricePerUnit = 0,
                TotalAmount = 0,
                EnteredBy = 1
            });

            // Assert - Verify final stock
            var finalItem = await _itemService.GetItemByIdAsync(createdItem.ItemId);
            // 20 + 100 - 30 - 25 - 2 = 63
            Assert.NotNull(finalItem);
            Assert.Equal(63, finalItem!.CurrentStock);

            // Verify transaction count
            var allTransactions = await _transactionService.GetAllTransactionsAsync();
            var itemTransactions = allTransactions.Where(t => t.ItemId == createdItem.ItemId).ToList();
            Assert.Equal(4, itemTransactions.Count);
        }

        [Fact]
        public async Task CompleteTransactionWorkflow_FilterByDateRange_ShouldReturnCorrectTransactions()
        {
            // Arrange
            var item = await _itemService.AddItemAsync(new Item { ItemName = "Test Item", Unit = "Pcs", CurrentStock = 100 });
            var party = await _partyService.AddPartyAsync(new Party { Name = "Test Party", PartyType = PartyType.Buyer });

            var today = DateTime.Now.Date;
            var yesterday = today.AddDays(-1);
            var lastWeek = today.AddDays(-7);

            // Create transactions on different dates
            await _transactionService.AddTransactionAsync(new Transaction
            {
                ItemId = item.ItemId,
                PartyId = party.PartyId,
                TransactionType = TransactionType.Sell,
                Quantity = 10,
                PricePerUnit = 100,
                TotalAmount = 1000,
                TransactionDate = lastWeek,
                EnteredBy = 1
            });

            await _transactionService.AddTransactionAsync(new Transaction
            {
                ItemId = item.ItemId,
                PartyId = party.PartyId,
                TransactionType = TransactionType.Sell,
                Quantity = 15,
                PricePerUnit = 100,
                TotalAmount = 1500,
                TransactionDate = yesterday,
                EnteredBy = 1
            });

            await _transactionService.AddTransactionAsync(new Transaction
            {
                ItemId = item.ItemId,
                PartyId = party.PartyId,
                TransactionType = TransactionType.Sell,
                Quantity = 20,
                PricePerUnit = 100,
                TotalAmount = 2000,
                TransactionDate = today,
                EnteredBy = 1
            });

            // Act - Filter by date range (yesterday to today)
            var filtered = await _transactionService.GetTransactionsByDateRangeAsync(yesterday, today);

            // Assert
            Assert.Equal(2, filtered.Count()); // Should get yesterday and today transactions
            Assert.DoesNotContain(filtered, t => t.TransactionDate.Date == lastWeek);
        }

        [Fact]
        public async Task CompleteTransactionWorkflow_FilterByType_ShouldReturnCorrectTransactions()
        {
            // Arrange
            var item = await _itemService.AddItemAsync(new Item { ItemName = "Test Item", Unit = "Pcs", CurrentStock = 100 });
            var party = await _partyService.AddPartyAsync(new Party { Name = "Test Party", PartyType = PartyType.Buyer });

            await _transactionService.AddTransactionAsync(new Transaction
            {
                ItemId = item.ItemId,
                PartyId = party.PartyId,
                TransactionType = TransactionType.Buy,
                Quantity = 10,
                PricePerUnit = 100,
                TotalAmount = 1000,
                EnteredBy = 1
            });

            await _transactionService.AddTransactionAsync(new Transaction
            {
                ItemId = item.ItemId,
                PartyId = party.PartyId,
                TransactionType = TransactionType.Sell,
                Quantity = 5,
                PricePerUnit = 120,
                TotalAmount = 600,
                EnteredBy = 1
            });

            await _transactionService.AddTransactionAsync(new Transaction
            {
                ItemId = item.ItemId,
                TransactionType = TransactionType.Wastage,
                Quantity = 2,
                PricePerUnit = 0,
                TotalAmount = 0,
                EnteredBy = 1
            });

            // Act
            var buyTransactions = await _transactionService.GetTransactionsByTypeAsync(TransactionType.Buy);
            var sellTransactions = await _transactionService.GetTransactionsByTypeAsync(TransactionType.Sell);
            var wastageTransactions = await _transactionService.GetTransactionsByTypeAsync(TransactionType.Wastage);

            // Assert
            Assert.Single(buyTransactions);
            Assert.Single(sellTransactions);
            Assert.Single(wastageTransactions);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}

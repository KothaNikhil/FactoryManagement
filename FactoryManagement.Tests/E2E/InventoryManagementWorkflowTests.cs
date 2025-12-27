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
    /// End-to-End tests for complete inventory management workflows
    /// </summary>
    public class InventoryManagementWorkflowTests : IDisposable
    {
        private readonly DbContextOptions<FactoryDbContext> _options;
        private readonly FactoryDbContext _context;
        private readonly ItemService _itemService;
        private readonly TransactionService _transactionService;
        private readonly InventoryViewModel _viewModel;

        public InventoryManagementWorkflowTests()
        {
            _options = new DbContextOptionsBuilder<FactoryDbContext>()
                .UseInMemoryDatabase(databaseName: $"E2EInventoryTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new FactoryDbContext(_options);
            
            var itemRepo = new Repository<Item>(_context);
            var transactionRepo = new TransactionRepository(_context);

            _itemService = new ItemService(itemRepo);
            _transactionService = new TransactionService(transactionRepo, _itemService);
            _viewModel = new InventoryViewModel(_itemService);

            SeedTestData().Wait();
        }

        private async Task SeedTestData()
        {
            await _context.Users.AddAsync(new User { UserId = 1, Username = "admin", Role = "Admin", IsActive = true });
            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task InventoryWorkflow_AddNewItem_ShouldBeSearchableAndEditable()
        {
            // Act - Step 1: User adds new item
            var createdItem = await _itemService.AddItemAsync(new Item
            {
                ItemName = "Dell Laptop",
                Unit = "Pcs",
                CurrentStock = 25
            });

            // Assert - Item saved successfully
            var allItems = await _itemService.GetAllItemsAsync();
            var fetchedItem = allItems.FirstOrDefault(i => i.ItemName == "Dell Laptop");
            Assert.NotNull(fetchedItem);
            Assert.Equal(25, fetchedItem.CurrentStock);

            // Act - Step 2: User searches for the item
            var searchResults = (await _itemService.GetAllItemsAsync())
                .Where(i => i.ItemName.Contains("Dell", StringComparison.OrdinalIgnoreCase));
            
            // Assert - Item appears in search
            Assert.Single(searchResults);
            Assert.Equal("Dell Laptop", searchResults.First().ItemName);

            // Act - Step 3: User edits the item
            createdItem.CurrentStock = 30;
            createdItem.Unit = "Units";
            await _itemService.UpdateItemAsync(createdItem);

            // Assert - Changes saved
            var updatedItem = await _itemService.GetItemByIdAsync(createdItem.ItemId);
            Assert.NotNull(updatedItem);
            Assert.Equal(30, updatedItem!.CurrentStock);
            Assert.Equal("Units", updatedItem!.Unit);

            // Verify in database
            var itemFromDb = await _itemService.GetItemByIdAsync(createdItem.ItemId);
            Assert.NotNull(itemFromDb);
            Assert.Equal(30, itemFromDb!.CurrentStock);
        }

        [Fact]
        public async Task InventoryWorkflow_LowStockDetection_ShouldIdentifyLowStockItems()
        {
            // Arrange - Create items with various stock levels
            await _itemService.AddItemAsync(new Item { ItemName = "High Stock Item", Unit = "Pcs", CurrentStock = 100 });
            await _itemService.AddItemAsync(new Item { ItemName = "Low Stock Item 1", Unit = "Pcs", CurrentStock = 3 });
            await _itemService.AddItemAsync(new Item { ItemName = "Low Stock Item 2", Unit = "Kg", CurrentStock = 2 });
            await _itemService.AddItemAsync(new Item { ItemName = "Zero Stock Item", Unit = "Pcs", CurrentStock = 0 });

            // Act - Get low stock items (threshold = 10)
            var lowStockItems = (await _itemService.GetAllItemsAsync()).Where(i => i.CurrentStock <= 10);

            // Assert
            Assert.Equal(3, lowStockItems.Count()); // Should find 3 low stock items
            Assert.Contains(lowStockItems, i => i.ItemName == "Low Stock Item 1");
            Assert.Contains(lowStockItems, i => i.ItemName == "Low Stock Item 2");
            Assert.Contains(lowStockItems, i => i.ItemName == "Zero Stock Item");
            Assert.DoesNotContain(lowStockItems, i => i.ItemName == "High Stock Item");
        }

        [Fact]
        public async Task InventoryWorkflow_CompleteInventoryCycle_ShouldMaintainDataIntegrity()
        {
            // Simulate complete inventory lifecycle

            // Step 1: Add new item
            var newItem = await _itemService.AddItemAsync(new Item
            {
                ItemName = "Wireless Mouse",
                Unit = "Pcs",
                CurrentStock = 50
            });

            // Step 2: Create a supplier and purchase more stock
            var supplier = new Party { Name = "Mouse Supplier", PartyType = PartyType.Seller };
            await _context.Parties.AddAsync(supplier);
            await _context.SaveChangesAsync();

            await _transactionService.AddTransactionAsync(new Transaction
            {
                ItemId = newItem.ItemId,
                PartyId = supplier.PartyId,
                TransactionType = TransactionType.Buy,
                Quantity = 100,
                PricePerUnit = 15,
                TotalAmount = 1500,
                EnteredBy = 1
            });

            // Step 3: Verify stock increased
            var afterPurchase = await _itemService.GetItemByIdAsync(newItem.ItemId);
            Assert.NotNull(afterPurchase);
            Assert.Equal(150, afterPurchase!.CurrentStock); // 50 + 100

            // Step 4: Create customer and sell items
            var customer = new Party { Name = "Customer Co", PartyType = PartyType.Buyer };
            await _context.Parties.AddAsync(customer);
            await _context.SaveChangesAsync();

            await _transactionService.AddTransactionAsync(new Transaction
            {
                ItemId = newItem.ItemId,
                PartyId = customer.PartyId,
                TransactionType = TransactionType.Sell,
                Quantity = 60,
                PricePerUnit = 25,
                TotalAmount = 1500,
                EnteredBy = 1
            });

            // Step 5: Verify stock decreased
            var afterSale = await _itemService.GetItemByIdAsync(newItem.ItemId);
            Assert.NotNull(afterSale);
            Assert.Equal(90, afterSale!.CurrentStock); // 150 - 60

            // Step 6: Record wastage
            await _transactionService.AddTransactionAsync(new Transaction
            {
                ItemId = newItem.ItemId,
                TransactionType = TransactionType.Wastage,
                Quantity = 5,
                PricePerUnit = 0,
                TotalAmount = 0,
                EnteredBy = 1
            });

            // Step 7: Verify final stock
            var finalItem = await _itemService.GetItemByIdAsync(newItem.ItemId);
            Assert.NotNull(finalItem);
            Assert.Equal(85, finalItem!.CurrentStock); // 90 - 5

            // Step 8: Search for item
            var searchResults2 = (await _itemService.GetAllItemsAsync())
                .Where(i => i.ItemName.Contains("Wireless", StringComparison.OrdinalIgnoreCase));
            Assert.Single(searchResults2);
            Assert.Equal(85, searchResults2.First().CurrentStock);

            // Step 9: Update item details
            finalItem.ItemName = "Premium Wireless Mouse";
            finalItem.Unit = "Units";
            await _itemService.UpdateItemAsync(finalItem);

            var updatedItem = await _itemService.GetItemByIdAsync(newItem.ItemId);
            Assert.NotNull(updatedItem);
            Assert.Equal("Premium Wireless Mouse", updatedItem!.ItemName);
            Assert.Equal("Units", updatedItem!.Unit);
            Assert.Equal(85, updatedItem!.CurrentStock); // Stock unchanged
        }

        [Fact]
        public async Task InventoryWorkflow_DeleteItem_ShouldRemoveFromInventory()
        {
            // Arrange
            var item = await _itemService.AddItemAsync(new Item
            {
                ItemName = "Temporary Item",
                Unit = "Pcs",
                CurrentStock = 10
            });

            var itemId = item.ItemId;

            // Verify item exists
            var exists = await _itemService.GetItemByIdAsync(itemId);
            Assert.NotNull(exists);

            // Act - Delete item
            await _itemService.DeleteItemAsync(itemId);

            // Assert - Item removed
            var allItems2 = await _itemService.GetAllItemsAsync();
            Assert.DoesNotContain(allItems2, i => i.ItemId == itemId);

            // Verify cannot find by ID
            var deletedItem = await _itemService.GetItemByIdAsync(itemId);
            Assert.Null(deletedItem);
        }

        [Fact]
        public async Task InventoryWorkflow_BulkOperations_ShouldHandleMultipleItems()
        {
            // Arrange & Act - Add multiple items
            var items = new[]
            {
                new Item { ItemName = "Item A", Unit = "Pcs", CurrentStock = 10 },
                new Item { ItemName = "Item B", Unit = "Kg", CurrentStock = 20 },
                new Item { ItemName = "Item C", Unit = "Ltr", CurrentStock = 30 },
                new Item { ItemName = "Item D", Unit = "Pcs", CurrentStock = 5 },
                new Item { ItemName = "Item E", Unit = "Kg", CurrentStock = 15 }
            };

            foreach (var item in items)
            {
                await _itemService.AddItemAsync(item);
            }

            // Assert - All items created
            var allItems3 = await _itemService.GetAllItemsAsync();
            Assert.True(allItems3.Count() >= 5);

            // Search functionality
            var searchA = (await _itemService.GetAllItemsAsync()).Where(i => i.ItemName.Contains("Item A", StringComparison.OrdinalIgnoreCase));
            Assert.Single(searchA);

            // Filter by unit
            var kgItems = (await _itemService.GetAllItemsAsync()).Where(i => i.Unit == "Kg");
            Assert.Equal(2, kgItems.Count());

            // Low stock detection
            var lowStock = (await _itemService.GetAllItemsAsync()).Where(i => i.CurrentStock <= 10);
            Assert.Contains(lowStock, i => i.ItemName == "Item D"); // Stock = 5
        }

        [Fact]
        public async Task InventoryWorkflow_ViewModel_ShouldLoadAndRefreshData()
        {
            // Arrange - Create some items
            await _itemService.AddItemAsync(new Item { ItemName = "Test Item 1", Unit = "Pcs", CurrentStock = 10 });
            await _itemService.AddItemAsync(new Item { ItemName = "Test Item 2", Unit = "Kg", CurrentStock = 20 });

            // Act - Load items in ViewModel
            await _viewModel.InitializeAsync();

            // Assert - Items loaded
            Assert.True(_viewModel.Items.Count >= 2);

            // Act - Add new item via ViewModel
            await _itemService.AddItemAsync(new Item { ItemName = "ViewModel Item", Unit = "Ltr", CurrentStock = 15 });

            // Refresh
            await _viewModel.InitializeAsync();

            // Assert - New item appears
            Assert.Contains(_viewModel.Items, i => i.ItemName == "ViewModel Item");
        }

        [Fact]
        public async Task InventoryWorkflow_StockAdjustments_ShouldUpdateCorrectly()
        {
            // Arrange - Create item with initial stock
            var item = await _itemService.AddItemAsync(new Item
            {
                ItemName = "Adjustable Item",
                Unit = "Pcs",
                CurrentStock = 100
            });

            // Create supplier and customer
            var supplier = new Party { Name = "Supplier", PartyType = PartyType.Seller };
            var customer = new Party { Name = "Customer", PartyType = PartyType.Buyer };
            await _context.Parties.AddRangeAsync(supplier, customer);
            await _context.SaveChangesAsync();

            // Act - Series of stock adjustments
            
            // Purchase 1
            await _transactionService.AddTransactionAsync(new Transaction
            {
                ItemId = item.ItemId,
                PartyId = supplier.PartyId,
                TransactionType = TransactionType.Buy,
                Quantity = 50,
                PricePerUnit = 10,
                TotalAmount = 500,
                EnteredBy = 1
            });
            var stock1 = (await _itemService.GetItemByIdAsync(item.ItemId))!.CurrentStock;
            Assert.Equal(150, stock1);

            // Sale 1
            await _transactionService.AddTransactionAsync(new Transaction
            {
                ItemId = item.ItemId,
                PartyId = customer.PartyId,
                TransactionType = TransactionType.Sell,
                Quantity = 30,
                PricePerUnit = 15,
                TotalAmount = 450,
                EnteredBy = 1
            });
            var stock2 = (await _itemService.GetItemByIdAsync(item.ItemId))!.CurrentStock;
            Assert.Equal(120, stock2);

            // Purchase 2
            await _transactionService.AddTransactionAsync(new Transaction
            {
                ItemId = item.ItemId,
                PartyId = supplier.PartyId,
                TransactionType = TransactionType.Buy,
                Quantity = 25,
                PricePerUnit = 10,
                TotalAmount = 250,
                EnteredBy = 1
            });
            var stock3 = (await _itemService.GetItemByIdAsync(item.ItemId))!.CurrentStock;
            Assert.Equal(145, stock3);

            // Wastage
            await _transactionService.AddTransactionAsync(new Transaction
            {
                ItemId = item.ItemId,
                TransactionType = TransactionType.Wastage,
                Quantity = 10,
                PricePerUnit = 0,
                TotalAmount = 0,
                EnteredBy = 1
            });
            var stock4 = (await _itemService.GetItemByIdAsync(item.ItemId))!.CurrentStock;
            Assert.Equal(135, stock4);

            // Sale 2
            await _transactionService.AddTransactionAsync(new Transaction
            {
                ItemId = item.ItemId,
                PartyId = customer.PartyId,
                TransactionType = TransactionType.Sell,
                Quantity = 40,
                PricePerUnit = 15,
                TotalAmount = 600,
                EnteredBy = 1
            });
            
            // Assert - Final stock is correct
            var finalStock = (await _itemService.GetItemByIdAsync(item.ItemId))!.CurrentStock;
            Assert.Equal(95, finalStock); // 100 + 50 - 30 + 25 - 10 - 40 = 95
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}


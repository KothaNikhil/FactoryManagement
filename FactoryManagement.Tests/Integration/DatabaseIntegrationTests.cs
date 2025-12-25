using FactoryManagement.Data;
using FactoryManagement.Data.Repositories;
using FactoryManagement.Models;
using FactoryManagement.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FactoryManagement.Tests.Integration
{
    public class DatabaseIntegrationTests : IDisposable
    {
        private readonly FactoryDbContext _context;
        private readonly IRepository<Item> _itemRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IPartyService _partyService;
        private readonly IItemService _itemService;

        public DatabaseIntegrationTests()
        {
            var options = new DbContextOptionsBuilder<FactoryDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            
            _context = new FactoryDbContext(options);
            _itemRepository = new Repository<Item>(_context);
            _transactionRepository = new TransactionRepository(_context);
            _partyService = new PartyService(new Repository<Party>(_context));
            _itemService = new ItemService(_itemRepository);
        }

        [Fact]
        public async Task ItemRepository_AddAndRetrieve_ShouldPersistData()
        {
            // Arrange
            var item = new Item
            {
                ItemName = "Test Item",
                CurrentStock = 100,
                Unit = "kg"
            };

            // Act
            await _itemRepository.AddAsync(item);
            await _context.SaveChangesAsync();
            var retrieved = await _itemRepository.GetByIdAsync(item.ItemId);

            // Assert
            Assert.NotNull(retrieved);
            Assert.Equal("Test Item", retrieved.ItemName);
            Assert.Equal(100, retrieved.CurrentStock);
            Assert.Equal("kg", retrieved.Unit);
        }

        [Fact]
        public async Task ItemRepository_Update_ShouldModifyData()
        {
            // Arrange
            var item = new Item
            {
                ItemName = "Original Name",
                CurrentStock = 50,
                Unit = "pcs"
            };
            await _itemRepository.AddAsync(item);
            await _context.SaveChangesAsync();

            // Act
            item.ItemName = "Updated Name";
            item.CurrentStock = 75;
            await _itemRepository.UpdateAsync(item);
            await _context.SaveChangesAsync();
            var updated = await _itemRepository.GetByIdAsync(item.ItemId);

            // Assert
            Assert.Equal("Updated Name", updated.ItemName);
            Assert.Equal(75, updated.CurrentStock);
        }

        [Fact]
        public async Task ItemRepository_Delete_ShouldRemoveData()
        {
            // Arrange
            var item = new Item
            {
                ItemName = "To Delete",
                CurrentStock = 10,
                Unit = "kg"
            };
            await _itemRepository.AddAsync(item);
            await _context.SaveChangesAsync();
            var itemId = item.ItemId;

            // Act
            await _itemRepository.DeleteAsync(item);
            await _context.SaveChangesAsync();
            var deleted = await _itemRepository.GetByIdAsync(itemId);

            // Assert
            Assert.Null(deleted);
        }

        [Fact]
        public async Task PartyService_AddParty_ShouldPersistToDatabase()
        {
            // Arrange
            var party = new Party
            {
                Name = "Test Party",
                MobileNumber = "1234567890",
                PartyType = PartyType.Buyer,
                Place = "Test Place"
            };

            // Act
            await _partyService.AddPartyAsync(party);
            var allParties = await _partyService.GetAllPartiesAsync();

            // Assert
            Assert.Single(allParties);
            Assert.Equal("Test Party", allParties.First().Name);
        }

        [Fact]
        public async Task MultipleItemsQuery_ShouldReturnOrderedResults()
        {
            // Arrange
            await _itemRepository.AddAsync(new Item { ItemName = "Zebra", CurrentStock = 10, Unit = "kg" });
            await _itemRepository.AddAsync(new Item { ItemName = "Apple", CurrentStock = 20, Unit = "kg" });
            await _itemRepository.AddAsync(new Item { ItemName = "Mango", CurrentStock = 15, Unit = "kg" });
            await _context.SaveChangesAsync();

            // Act
            var allItems = await _itemService.GetAllItemsAsync();
            var sortedItems = allItems.OrderBy(i => i.ItemName).ToList();

            // Assert
            Assert.Equal(3, sortedItems.Count);
            Assert.Equal("Apple", sortedItems[0].ItemName);
            Assert.Equal("Mango", sortedItems[1].ItemName);
            Assert.Equal("Zebra", sortedItems[2].ItemName);
        }

        [Fact]
        public async Task ConcurrentItemUpdates_ShouldHandleCorrectly()
        {
            // Arrange
            var item = new Item
            {
                ItemName = "Concurrent Test",
                CurrentStock = 100,
                Unit = "pcs"
            };
            await _itemRepository.AddAsync(item);
            await _context.SaveChangesAsync();

            // Act
            item.CurrentStock = 150;
            await _itemRepository.UpdateAsync(item);
            await _context.SaveChangesAsync();

            item.CurrentStock = 200;
            await _itemRepository.UpdateAsync(item);
            await _context.SaveChangesAsync();

            var final = await _itemRepository.GetByIdAsync(item.ItemId);

            // Assert
            Assert.Equal(200, final.CurrentStock);
        }

        [Fact]
        public async Task Transaction_WithPartyReference_ShouldMaintainRelationship()
        {
            // Arrange
            var party = new Party
            {
                Name = "Test Party",
                PartyType = PartyType.Buyer
            };
            await _context.Parties.AddAsync(party);
            await _context.SaveChangesAsync();

            var item = new Item
            {
                ItemName = "Test Item",
                CurrentStock = 50,
                Unit = "kg"
            };
            await _itemRepository.AddAsync(item);
            await _context.SaveChangesAsync();

            var transaction = new Transaction
            {
                ItemId = item.ItemId,
                PartyId = party.PartyId,
                TransactionType = TransactionType.Buy,
                Quantity = 10,
                PricePerUnit = 50,
                TotalAmount = 500,
                TransactionDate = DateTime.Now
            };

            // Act
            await _transactionRepository.AddAsync(transaction);
            await _context.SaveChangesAsync();

            var retrieved = await _context.Transactions
                .Include(t => t.Party)
                .FirstOrDefaultAsync(t => t.TransactionId == transaction.TransactionId);

            // Assert
            Assert.NotNull(retrieved);
            Assert.NotNull(retrieved.Party);
            Assert.Equal("Test Party", retrieved.Party.Name);
        }

        [Fact]
        public async Task BulkInsert_ShouldHandleMultipleItems()
        {
            // Arrange
            var items = Enumerable.Range(1, 50).Select(i => new Item
            {
                ItemName = $"Item{i}",
                CurrentStock = i * 10,
                Unit = "kg"
            }).ToList();

            // Act
            foreach (var item in items)
            {
                await _itemRepository.AddAsync(item);
            }
            await _context.SaveChangesAsync();

            var count = (await _itemService.GetAllItemsAsync()).Count();

            // Assert
            Assert.Equal(50, count);
        }

        [Fact]
        public async Task DatabaseContext_SaveChanges_ShouldUpdateTimestamps()
        {
            // Arrange
            var item = new Item
            {
                ItemName = "Timestamp Test",
                CurrentStock = 50,
                Unit = "kg"
            };

            // Act
            await _itemRepository.AddAsync(item);
            await _context.SaveChangesAsync();
            
            var createdDate = item.CreatedDate;
            Assert.NotEqual(default(DateTime), createdDate);

            await Task.Delay(10); // Small delay to ensure different timestamps
            
            item.CurrentStock = 100;
            await _itemRepository.UpdateAsync(item);
            await _context.SaveChangesAsync();

            // Assert
            Assert.NotEqual(default(DateTime), item.ModifiedDate);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}

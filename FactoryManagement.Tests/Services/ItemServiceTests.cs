using FactoryManagement.Data.Repositories;
using FactoryManagement.Models;
using FactoryManagement.Services;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FactoryManagement.Tests.Services
{
    public class ItemServiceTests
    {
        [Fact]
        public async Task GetAllItemsAsync_ShouldReturnAllItems()
        {
            // Arrange
            var mockRepo = new Mock<IRepository<Item>>();
            var items = new List<Item>
            {
                new Item { ItemId = 1, ItemName = "Item1" },
                new Item { ItemId = 2, ItemName = "Item2" }
            };
            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(items);
            var service = new ItemService(mockRepo.Object);

            // Act
            var result = await service.GetAllItemsAsync();

            // Assert
            Assert.Equal(2, result.Count());
            mockRepo.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateStockAsync_BuyTransaction_ShouldIncreaseStock()
        {
            // Arrange
            var mockRepo = new Mock<IRepository<Item>>();
            var item = new Item { ItemId = 1, CurrentStock = 100 };
            mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(item);
            var service = new ItemService(mockRepo.Object);

            // Act
            await service.UpdateStockAsync(1, 50, TransactionType.Buy);

            // Assert
            Assert.Equal(150, item.CurrentStock);
            mockRepo.Verify(r => r.UpdateAsync(item), Times.Once);
        }

        [Fact]
        public async Task UpdateStockAsync_SellTransaction_ShouldDecreaseStock()
        {
            // Arrange
            var mockRepo = new Mock<IRepository<Item>>();
            var item = new Item { ItemId = 1, CurrentStock = 100 };
            mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(item);
            var service = new ItemService(mockRepo.Object);

            // Act
            await service.UpdateStockAsync(1, 30, TransactionType.Sell);

            // Assert
            Assert.Equal(70, item.CurrentStock);
            mockRepo.Verify(r => r.UpdateAsync(item), Times.Once);
        }
    }
}

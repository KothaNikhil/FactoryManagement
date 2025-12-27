using FactoryManagement.Models;
using FactoryManagement.Services;
using FactoryManagement.ViewModels;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace FactoryManagement.Tests.ViewModels
{
    public class InventoryViewModelTests
    {
        [Fact]
        public async Task LoadItemsAsync_ShouldPopulateItems()
        {
            // Arrange
            var mockService = new Mock<IItemService>();
            var items = new List<Item>
            {
                new Item { ItemId = 1, ItemName = "Item1", CurrentStock = 100, Unit = "kg" },
                new Item { ItemId = 2, ItemName = "Item2", CurrentStock = 200, Unit = "pcs" }
            };
            mockService.Setup(s => s.GetAllItemsAsync()).ReturnsAsync(items);
            var viewModel = new InventoryViewModel(mockService.Object);

            // Act
            await viewModel.LoadItemsCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal(2, viewModel.Items.Count);
            Assert.Equal("Item1", viewModel.Items[0].ItemName);
        }

        [Fact]
        public void NewItem_ShouldClearForm()
        {
            // Arrange
            var mockService = new Mock<IItemService>();
            var viewModel = new InventoryViewModel(mockService.Object)
            {
                ItemName = "Test",
                CurrentStock = 100,
                Unit = "kg"
            };

            // Act
            viewModel.NewItemCommand.Execute(null);

            // Assert
            Assert.Equal(string.Empty, viewModel.ItemName);
            Assert.Equal(0, viewModel.CurrentStock);
            Assert.Equal(string.Empty, viewModel.Unit);
            Assert.False(viewModel.IsEditMode);
        }

        [Fact]
        public void EditItem_ShouldPopulateForm()
        {
            // Arrange
            var mockService = new Mock<IItemService>();
            var viewModel = new InventoryViewModel(mockService.Object);
            var item = new Item { ItemId = 1, ItemName = "Test Item", CurrentStock = 100, Unit = "kg" };

            // Act
            viewModel.EditItemCommand.Execute(item);

            // Assert
            Assert.True(viewModel.IsEditMode);
            Assert.Equal("Test Item", viewModel.ItemName);
            Assert.Equal(100, viewModel.CurrentStock);
            Assert.Equal("kg", viewModel.Unit);
        }
    }
}


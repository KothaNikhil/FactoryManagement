using FactoryManagement.Models;
using System;
using Xunit;

namespace FactoryManagement.Tests.Models
{
    public class ItemTests
    {
        [Fact]
        public void Item_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var item = new Item();

            // Assert
            Assert.Equal(0, item.ItemId);
            Assert.Equal(string.Empty, item.ItemName);
            Assert.Equal(0, item.CurrentStock);
            Assert.Equal(string.Empty, item.Unit);
            Assert.True((DateTime.Now - item.CreatedDate).TotalSeconds < 1);
            Assert.Null(item.ModifiedDate);
        }

        [Fact]
        public void Item_Properties_ShouldRetainValues()
        {
            // Arrange
            var item = new Item
            {
                ItemName = "Test Item",
                CurrentStock = 100.5m,
                Unit = "kg"
            };

            // Assert
            Assert.Equal("Test Item", item.ItemName);
            Assert.Equal(100.5m, item.CurrentStock);
            Assert.Equal("kg", item.Unit);
        }
    }
}

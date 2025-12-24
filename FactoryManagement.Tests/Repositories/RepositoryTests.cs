using FactoryManagement.Data;
using FactoryManagement.Data.Repositories;
using FactoryManagement.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace FactoryManagement.Tests.Repositories
{
    public class RepositoryTests
    {
        private FactoryDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<FactoryDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new FactoryDbContext(options);
        }

        [Fact]
        public async Task AddAsync_ShouldAddEntity()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var repository = new Repository<Item>(context);
            var item = new Item { ItemName = "Test Item", CurrentStock = 100, Unit = "kg" };

            // Act
            var result = await repository.AddAsync(item);
            await context.SaveChangesAsync();

            // Assert
            Assert.NotEqual(0, result.ItemId);
            Assert.Equal(1, await context.Items.CountAsync());
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnEntity()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var repository = new Repository<Item>(context);
            var item = new Item { ItemName = "Test Item", CurrentStock = 100, Unit = "kg" };
            await repository.AddAsync(item);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetByIdAsync(item.ItemId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Item", result.ItemName);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateEntity()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var repository = new Repository<Item>(context);
            var item = new Item { ItemName = "Test Item", CurrentStock = 100, Unit = "kg" };
            await repository.AddAsync(item);
            await context.SaveChangesAsync();

            // Act
            item.ItemName = "Updated Item";
            await repository.UpdateAsync(item);
            await context.SaveChangesAsync();

            // Assert
            var updated = await repository.GetByIdAsync(item.ItemId);
            Assert.Equal("Updated Item", updated?.ItemName);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveEntity()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var repository = new Repository<Item>(context);
            var item = new Item { ItemName = "Test Item", CurrentStock = 100, Unit = "kg" };
            await repository.AddAsync(item);
            await context.SaveChangesAsync();

            // Act
            await repository.DeleteAsync(item);
            await context.SaveChangesAsync();

            // Assert
            Assert.Equal(0, await context.Items.CountAsync());
        }
    }
}

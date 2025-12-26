using FactoryManagement.Data.Repositories;
using FactoryManagement.Models;
using FactoryManagement.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FactoryManagement.Tests.Services
{
    public class TransactionServiceTests
    {
        private readonly Mock<ITransactionRepository> _mockTransactionRepo;
        private readonly Mock<IItemService> _mockItemService;
        private readonly TransactionService _service;

        public TransactionServiceTests()
        {
            _mockTransactionRepo = new Mock<ITransactionRepository>();
            _mockItemService = new Mock<IItemService>();
            _service = new TransactionService(_mockTransactionRepo.Object, _mockItemService.Object);
        }

        [Fact]
        public async Task GetAllTransactionsAsync_ShouldReturnAllTransactions()
        {
            // Arrange
            var transactions = new List<Transaction>
            {
                new Transaction { TransactionId = 1, ItemId = 1, Quantity = 100, TransactionType = TransactionType.Buy },
                new Transaction { TransactionId = 2, ItemId = 2, Quantity = 50, TransactionType = TransactionType.Sell }
            };
            _mockTransactionRepo.Setup(r => r.GetAllWithDetailsAsync()).ReturnsAsync(transactions);

            // Act
            var result = await _service.GetAllTransactionsAsync();

            // Assert
            Assert.Equal(2, result.Count());
            _mockTransactionRepo.Verify(r => r.GetAllWithDetailsAsync(), Times.Once);
        }

        [Fact]
        public async Task GetTransactionByIdAsync_WithValidId_ShouldReturnTransaction()
        {
            // Arrange
            var transaction = new Transaction { TransactionId = 1, ItemId = 1, Quantity = 100 };
            _mockTransactionRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(transaction);

            // Act
            var result = await _service.GetTransactionByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.TransactionId);
            _mockTransactionRepo.Verify(r => r.GetByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task AddTransactionAsync_BuyTransaction_ShouldIncreaseStock()
        {
            // Arrange
            var transaction = new Transaction 
            { 
                ItemId = 1, 
                Quantity = 50, 
                TransactionType = TransactionType.Buy,
                PricePerUnit = 10
            };

            _mockItemService.Setup(r => r.UpdateStockAsync(1, 50, TransactionType.Buy)).Returns(Task.CompletedTask);
            _mockTransactionRepo.Setup(r => r.AddAsync(It.IsAny<Transaction>())).ReturnsAsync(transaction);

            // Act
            var result = await _service.AddTransactionAsync(transaction);

            // Assert
            Assert.Equal(500, result.TotalAmount); // 50 * 10
            _mockItemService.Verify(r => r.UpdateStockAsync(1, 50, TransactionType.Buy), Times.Once);
            _mockTransactionRepo.Verify(r => r.AddAsync(It.IsAny<Transaction>()), Times.Once);
        }

        [Fact]
        public async Task AddTransactionAsync_SellTransaction_ShouldDecreaseStock()
        {
            // Arrange
            var transaction = new Transaction 
            { 
                ItemId = 1, 
                Quantity = 30, 
                TransactionType = TransactionType.Sell,
                PricePerUnit = 15
            };

            _mockItemService.Setup(r => r.UpdateStockAsync(1, 30, TransactionType.Sell)).Returns(Task.CompletedTask);
            _mockTransactionRepo.Setup(r => r.AddAsync(It.IsAny<Transaction>())).ReturnsAsync(transaction);

            // Act
            var result = await _service.AddTransactionAsync(transaction);

            // Assert
            Assert.Equal(450, result.TotalAmount); // 30 * 15
            _mockItemService.Verify(r => r.UpdateStockAsync(1, 30, TransactionType.Sell), Times.Once);
        }

        [Fact]
        public async Task AddTransactionAsync_WastageTransaction_ShouldDecreaseStock()
        {
            // Arrange
            var transaction = new Transaction 
            { 
                ItemId = 1, 
                Quantity = 10, 
                TransactionType = TransactionType.Wastage,
                PricePerUnit = 0
            };

            _mockItemService.Setup(r => r.UpdateStockAsync(1, 10, TransactionType.Wastage)).Returns(Task.CompletedTask);
            _mockTransactionRepo.Setup(r => r.AddAsync(It.IsAny<Transaction>())).ReturnsAsync(transaction);

            // Act
            var result = await _service.AddTransactionAsync(transaction);

            // Assert
            Assert.Equal(0, result.TotalAmount); // 10 * 0
            _mockItemService.Verify(r => r.UpdateStockAsync(1, 10, TransactionType.Wastage), Times.Once);
        }

        [Fact]
        public async Task UpdateTransactionAsync_ShouldUpdateTransaction()
        {
            // Arrange
            var existingTransaction = new Transaction 
            { 
                TransactionId = 1, 
                ItemId = 1, 
                Quantity = 100, 
                TransactionType = TransactionType.Buy 
            };
            var item = new Item { ItemId = 1, CurrentStock = 100 };

            _mockTransactionRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingTransaction);
            _mockItemService.Setup(r => r.GetItemByIdAsync(1)).ReturnsAsync(item);
            _mockItemService.Setup(r => r.UpdateItemAsync(It.IsAny<Item>())).Returns(Task.CompletedTask);
            _mockTransactionRepo.Setup(r => r.UpdateAsync(It.IsAny<Transaction>())).Returns(Task.CompletedTask);

            // Act
            existingTransaction.Quantity = 150;
            await _service.UpdateTransactionAsync(existingTransaction);

            // Assert
            _mockTransactionRepo.Verify(r => r.UpdateAsync(It.Is<Transaction>(t => t.Quantity == 150)), Times.Once);
        }

        [Fact]
        public async Task DeleteTransactionAsync_ShouldReverseStockAndDeleteTransaction()
        {
            // Arrange
            var transaction = new Transaction 
            { 
                TransactionId = 1, 
                ItemId = 1, 
                Quantity = 50, 
                TransactionType = TransactionType.Buy 
            };

            _mockTransactionRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(transaction);
            _mockItemService.Setup(r => r.UpdateStockAsync(1, 50, TransactionType.Sell)).Returns(Task.CompletedTask);
            _mockTransactionRepo.Setup(r => r.DeleteAsync(It.IsAny<Transaction>())).Returns(Task.CompletedTask);

            // Act
            await _service.DeleteTransactionAsync(1);

            // Assert - Verify stock was reversed (Buy -> Sell to reverse)
            _mockItemService.Verify(r => r.UpdateStockAsync(1, 50, TransactionType.Sell), Times.Once);
            _mockTransactionRepo.Verify(r => r.DeleteAsync(It.IsAny<Transaction>()), Times.Once);
        }

        [Fact]
        public async Task AddTransactionAsync_Processing_ShouldNotChangeStock()
        {
            // Arrange
            var transaction = new Transaction
            {
                ItemId = 2,
                Quantity = 20,
                TransactionType = TransactionType.Processing,
                PricePerUnit = 5,
                InputItemId = 1,
                InputQuantity = 30
            };

            _mockTransactionRepo.Setup(r => r.AddAsync(It.IsAny<Transaction>())).ReturnsAsync(transaction);

            // Act
            var result = await _service.AddTransactionAsync(transaction);

            // Assert
            Assert.Equal(100, result.TotalAmount); // 20 * 5
            _mockItemService.Verify(r => r.UpdateStockAsync(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<TransactionType>()), Times.Never);
            _mockItemService.Verify(r => r.UpdateStockForProcessingAsync(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<int>(), It.IsAny<decimal>()), Times.Never);
        }

        [Fact]
        public async Task DeleteTransactionAsync_Processing_ShouldNotReverseStock()
        {
            // Arrange
            var transaction = new Transaction
            {
                TransactionId = 2,
                ItemId = 2,
                Quantity = 20,
                TransactionType = TransactionType.Processing,
                PricePerUnit = 5,
                InputItemId = 1,
                InputQuantity = 30
            };

            _mockTransactionRepo.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(transaction);
            _mockTransactionRepo.Setup(r => r.DeleteAsync(It.IsAny<Transaction>())).Returns(Task.CompletedTask);

            // Act
            await _service.DeleteTransactionAsync(2);

            // Assert
            _mockItemService.Verify(r => r.UpdateStockAsync(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<TransactionType>()), Times.Never);
            _mockItemService.Verify(r => r.UpdateStockForProcessingAsync(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<int>(), It.IsAny<decimal>()), Times.Never);
            _mockTransactionRepo.Verify(r => r.DeleteAsync(It.IsAny<Transaction>()), Times.Once);
        }

        [Fact]
        public async Task GetRecentTransactionsAsync_ShouldReturnLimitedTransactions()
        {
            // Arrange
            var transactions = new List<Transaction>
            {
                new Transaction { TransactionId = 1, TransactionDate = DateTime.Now },
                new Transaction { TransactionId = 2, TransactionDate = DateTime.Now.AddDays(-1) },
                new Transaction { TransactionId = 3, TransactionDate = DateTime.Now.AddDays(-2) },
                new Transaction { TransactionId = 4, TransactionDate = DateTime.Now.AddDays(-3) },
                new Transaction { TransactionId = 5, TransactionDate = DateTime.Now.AddDays(-4) }
            };

            _mockTransactionRepo.Setup(r => r.GetAllWithDetailsAsync()).ReturnsAsync(transactions);

            // Act
            var result = await _service.GetRecentTransactionsAsync(3);

            // Assert
            Assert.Equal(3, result.Count());
            Assert.Equal(1, result.First().TransactionId); // Most recent
            _mockTransactionRepo.Verify(r => r.GetAllWithDetailsAsync(), Times.Once);
        }
    }
}

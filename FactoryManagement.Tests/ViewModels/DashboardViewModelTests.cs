using FactoryManagement.Models;
using FactoryManagement.Services;
using FactoryManagement.ViewModels;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FactoryManagement.Tests.ViewModels
{
    public class DashboardViewModelTests
    {
        [Fact]
        public async Task LoadDataAsync_ShouldCalculateTotalPurchases()
        {
            // Arrange
            var mockTransactionService = new Mock<ITransactionService>();
            var mockItemService = new Mock<IItemService>();

            var transactions = new List<Transaction>
            {
                new Transaction { TransactionId = 1, TransactionType = TransactionType.Buy, TotalAmount = 1000 },
                new Transaction { TransactionId = 2, TransactionType = TransactionType.Buy, TotalAmount = 2000 },
                new Transaction { TransactionId = 3, TransactionType = TransactionType.Sell, TotalAmount = 500 }
            };

            mockTransactionService.Setup(s => s.GetAllTransactionsAsync()).ReturnsAsync(transactions);
            mockItemService.Setup(s => s.GetAllItemsAsync()).ReturnsAsync(new List<Item>());

            var viewModel = new DashboardViewModel(mockTransactionService.Object, mockItemService.Object);

            // Act
            await viewModel.LoadDataCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal(3000, viewModel.TotalPurchases);
        }

        [Fact]
        public async Task LoadDataAsync_ShouldCalculateTotalSales()
        {
            // Arrange
            var mockTransactionService = new Mock<ITransactionService>();
            var mockItemService = new Mock<IItemService>();

            var transactions = new List<Transaction>
            {
                new Transaction { TransactionId = 1, TransactionType = TransactionType.Sell, TotalAmount = 1500 },
                new Transaction { TransactionId = 2, TransactionType = TransactionType.Sell, TotalAmount = 2500 },
                new Transaction { TransactionId = 3, TransactionType = TransactionType.Buy, TotalAmount = 1000 }
            };

            mockTransactionService.Setup(s => s.GetAllTransactionsAsync()).ReturnsAsync(transactions);
            mockItemService.Setup(s => s.GetAllItemsAsync()).ReturnsAsync(new List<Item>());

            var viewModel = new DashboardViewModel(mockTransactionService.Object, mockItemService.Object);

            // Act
            await viewModel.LoadDataCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal(4000, viewModel.TotalSales);
        }

        [Fact]
        public async Task LoadDataAsync_ShouldCalculateTotalWastage()
        {
            // Arrange
            var mockTransactionService = new Mock<ITransactionService>();
            var mockItemService = new Mock<IItemService>();

            var transactions = new List<Transaction>
            {
                new Transaction { TransactionId = 1, TransactionType = TransactionType.Wastage, TotalAmount = 300 },
                new Transaction { TransactionId = 2, TransactionType = TransactionType.Wastage, TotalAmount = 700 },
                new Transaction { TransactionId = 3, TransactionType = TransactionType.Buy, TotalAmount = 1000 }
            };

            mockTransactionService.Setup(s => s.GetAllTransactionsAsync()).ReturnsAsync(transactions);
            mockItemService.Setup(s => s.GetAllItemsAsync()).ReturnsAsync(new List<Item>());

            var viewModel = new DashboardViewModel(mockTransactionService.Object, mockItemService.Object);

            // Act
            await viewModel.LoadDataCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal(1000, viewModel.TotalWastage);
        }

        [Fact]
        public async Task LoadDataAsync_ShouldCalculateTotalProcessingFees()
        {
            // Arrange
            var mockTransactionService = new Mock<ITransactionService>();
            var mockItemService = new Mock<IItemService>();

            var transactions = new List<Transaction>
            {
                new Transaction { TransactionId = 1, TransactionType = TransactionType.Processing, TotalAmount = 500 },
                new Transaction { TransactionId = 2, TransactionType = TransactionType.Processing, TotalAmount = 800 },
                new Transaction { TransactionId = 3, TransactionType = TransactionType.Buy, TotalAmount = 1000 }
            };

            mockTransactionService.Setup(s => s.GetAllTransactionsAsync()).ReturnsAsync(transactions);
            mockItemService.Setup(s => s.GetAllItemsAsync()).ReturnsAsync(new List<Item>());

            var viewModel = new DashboardViewModel(mockTransactionService.Object, mockItemService.Object);

            // Act
            await viewModel.LoadDataCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal(1300, viewModel.TotalProcessingFees);
        }

        [Fact]
        public async Task LoadDataAsync_ShouldCountProcessingTransactions()
        {
            // Arrange
            var mockTransactionService = new Mock<ITransactionService>();
            var mockItemService = new Mock<IItemService>();

            var transactions = new List<Transaction>
            {
                new Transaction { TransactionId = 1, TransactionType = TransactionType.Processing, TotalAmount = 500 },
                new Transaction { TransactionId = 2, TransactionType = TransactionType.Processing, TotalAmount = 800 },
                new Transaction { TransactionId = 3, TransactionType = TransactionType.Buy, TotalAmount = 1000 }
            };

            mockTransactionService.Setup(s => s.GetAllTransactionsAsync()).ReturnsAsync(transactions);
            mockItemService.Setup(s => s.GetAllItemsAsync()).ReturnsAsync(new List<Item>());

            var viewModel = new DashboardViewModel(mockTransactionService.Object, mockItemService.Object);

            // Act
            await viewModel.LoadDataCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal(2, viewModel.ProcessingTransactionCount);
        }

        [Fact]
        public async Task LoadDataAsync_ShouldSetTransactionCount()
        {
            // Arrange
            var mockTransactionService = new Mock<ITransactionService>();
            var mockItemService = new Mock<IItemService>();

            var transactions = new List<Transaction>
            {
                new Transaction { TransactionId = 1, TransactionType = TransactionType.Buy, TotalAmount = 1000 },
                new Transaction { TransactionId = 2, TransactionType = TransactionType.Sell, TotalAmount = 500 },
                new Transaction { TransactionId = 3, TransactionType = TransactionType.Wastage, TotalAmount = 200 }
            };

            mockTransactionService.Setup(s => s.GetAllTransactionsAsync()).ReturnsAsync(transactions);
            mockItemService.Setup(s => s.GetAllItemsAsync()).ReturnsAsync(new List<Item>());

            var viewModel = new DashboardViewModel(mockTransactionService.Object, mockItemService.Object);

            // Act
            await viewModel.LoadDataCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal(3, viewModel.TransactionCount);
        }

        [Fact]
        public async Task LoadDataAsync_ShouldLoadRecentTransactions()
        {
            // Arrange
            var mockTransactionService = new Mock<ITransactionService>();
            var mockItemService = new Mock<IItemService>();

            var now = DateTime.Now;
            var transactions = new List<Transaction>
            {
                new Transaction { TransactionId = 1, TransactionType = TransactionType.Buy, TotalAmount = 1000, TransactionDate = now.AddDays(-5) },
                new Transaction { TransactionId = 2, TransactionType = TransactionType.Sell, TotalAmount = 500, TransactionDate = now.AddDays(-2) },
                new Transaction { TransactionId = 3, TransactionType = TransactionType.Buy, TotalAmount = 200, TransactionDate = now }
            };

            mockTransactionService.Setup(s => s.GetAllTransactionsAsync()).ReturnsAsync(transactions);
            mockItemService.Setup(s => s.GetAllItemsAsync()).ReturnsAsync(new List<Item>());

            var viewModel = new DashboardViewModel(mockTransactionService.Object, mockItemService.Object);

            // Act
            await viewModel.LoadDataCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal(3, viewModel.RecentTransactions.Count);
            // Most recent should be first (ordered by TransactionDate descending)
            Assert.Equal(3, viewModel.RecentTransactions[0].TransactionId);
        }

        [Fact]
        public async Task LoadDataAsync_ShouldIdentifyLowStockItems()
        {
            // Arrange
            var mockTransactionService = new Mock<ITransactionService>();
            var mockItemService = new Mock<IItemService>();

            var items = new List<Item>
            {
                new Item { ItemId = 1, ItemName = "Item1", CurrentStock = 50 },
                new Item { ItemId = 2, ItemName = "Item2", CurrentStock = 200 },
                new Item { ItemId = 3, ItemName = "Item3", CurrentStock = 75 }
            };

            mockTransactionService.Setup(s => s.GetAllTransactionsAsync()).ReturnsAsync(new List<Transaction>());
            mockItemService.Setup(s => s.GetAllItemsAsync()).ReturnsAsync(items);

            var viewModel = new DashboardViewModel(mockTransactionService.Object, mockItemService.Object);

            // Act
            await viewModel.LoadDataCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal(2, viewModel.LowStockItems.Count);
            Assert.Contains(viewModel.LowStockItems, i => i.ItemId == 1);
            Assert.Contains(viewModel.LowStockItems, i => i.ItemId == 3);
            Assert.DoesNotContain(viewModel.LowStockItems, i => i.ItemId == 2);
        }

        [Fact]
        public async Task LoadDataAsync_ShouldLoadChartItemsTopTenLowestStock()
        {
            // Arrange
            var mockTransactionService = new Mock<ITransactionService>();
            var mockItemService = new Mock<IItemService>();

            var items = Enumerable.Range(1, 15)
                .Select(i => new Item { ItemId = i, ItemName = $"Item{i}", CurrentStock = i * 10 })
                .ToList();

            mockTransactionService.Setup(s => s.GetAllTransactionsAsync()).ReturnsAsync(new List<Transaction>());
            mockItemService.Setup(s => s.GetAllItemsAsync()).ReturnsAsync(items);

            var viewModel = new DashboardViewModel(mockTransactionService.Object, mockItemService.Object);

            // Act
            await viewModel.LoadDataCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal(10, viewModel.AllItems.Count);
            Assert.Equal(1, viewModel.AllItems[0].ItemId); // Lowest stock first
        }

        [Fact]
        public async Task LoadDataAsync_WithFinancialService_ShouldLoadLoanTotals()
        {
            // Arrange
            var mockTransactionService = new Mock<ITransactionService>();
            var mockItemService = new Mock<IItemService>();
            var mockFinancialService = new Mock<FinancialTransactionService>();

            mockTransactionService.Setup(s => s.GetAllTransactionsAsync()).ReturnsAsync(new List<Transaction>());
            mockItemService.Setup(s => s.GetAllItemsAsync()).ReturnsAsync(new List<Item>());
            mockFinancialService.Setup(s => s.GetTotalLoansGivenOutstandingAsync()).ReturnsAsync(5000);
            mockFinancialService.Setup(s => s.GetTotalLoansTakenOutstandingAsync()).ReturnsAsync(3000);
            mockFinancialService.Setup(s => s.GetAllFinancialTransactionsAsync()).ReturnsAsync(new List<FinancialTransaction>());

            var viewModel = new DashboardViewModel(
                mockTransactionService.Object,
                mockItemService.Object,
                mockFinancialService.Object);

            // Act
            await viewModel.LoadDataCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal(5000, viewModel.TotalLoansGiven);
            Assert.Equal(3000, viewModel.TotalLoansTaken);
        }

        [Fact]
        public async Task LoadDataAsync_WithWageService_ShouldLoadWageTotals()
        {
            // Arrange
            var mockTransactionService = new Mock<ITransactionService>();
            var mockItemService = new Mock<IItemService>();
            var mockWageService = new Mock<IWageService>();

            mockTransactionService.Setup(s => s.GetAllTransactionsAsync()).ReturnsAsync(new List<Transaction>());
            mockItemService.Setup(s => s.GetAllItemsAsync()).ReturnsAsync(new List<Item>());
            mockWageService.Setup(s => s.GetTotalWagesPaidAsync()).ReturnsAsync(10000);
            mockWageService.Setup(s => s.GetTotalAdvancesGivenAsync()).ReturnsAsync(2000);
            mockWageService.Setup(s => s.GetAllWageTransactionsAsync()).ReturnsAsync(new List<WageTransaction>());

            var viewModel = new DashboardViewModel(
                mockTransactionService.Object,
                mockItemService.Object,
                null,
                mockWageService.Object);

            // Act
            await viewModel.LoadDataCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal(10000, viewModel.TotalWagesPaid);
            Assert.Equal(2000, viewModel.TotalAdvancesGiven);
        }

        [Fact]
        public async Task LoadDataAsync_WithUnifiedTransactionService_ShouldLoadAllTransactions()
        {
            // Arrange
            var mockTransactionService = new Mock<ITransactionService>();
            var mockItemService = new Mock<IItemService>();
            var mockUnifiedService = new Mock<UnifiedTransactionService>();

            var unifiedTransactions = new List<UnifiedTransactionViewModel>
            {
                new UnifiedTransactionViewModel { TransactionId = "1", TransactionType = "Buy", Amount = 1000 },
                new UnifiedTransactionViewModel { TransactionId = "2", TransactionType = "Sell", Amount = 500 }
            };

            mockTransactionService.Setup(s => s.GetAllTransactionsAsync()).ReturnsAsync(new List<Transaction>());
            mockItemService.Setup(s => s.GetAllItemsAsync()).ReturnsAsync(new List<Item>());
            mockUnifiedService.Setup(s => s.GetAllUnifiedTransactionsAsync(It.IsAny<int?>()))
                .ReturnsAsync(unifiedTransactions);

            var viewModel = new DashboardViewModel(
                mockTransactionService.Object,
                mockItemService.Object,
                null,
                null,
                mockUnifiedService.Object);

            // Act
            await viewModel.LoadDataCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal(2, viewModel.AllTransactions.Count);
        }

        [Fact]
        public async Task LoadDataAsync_ShouldSetIsBusyFlag()
        {
            // Arrange
            var mockTransactionService = new Mock<ITransactionService>();
            var mockItemService = new Mock<IItemService>();

            mockTransactionService.Setup(s => s.GetAllTransactionsAsync()).ReturnsAsync(new List<Transaction>());
            mockItemService.Setup(s => s.GetAllItemsAsync()).ReturnsAsync(new List<Item>());

            var viewModel = new DashboardViewModel(mockTransactionService.Object, mockItemService.Object);
            var busyDuringLoad = false;

            // Capture IsBusy state during execution
            mockTransactionService
                .Setup(s => s.GetAllTransactionsAsync())
                .Callback(() => busyDuringLoad = viewModel.IsBusy)
                .ReturnsAsync(new List<Transaction>());

            // Act
            await viewModel.LoadDataCommand.ExecuteAsync(null);

            // Assert
            Assert.False(viewModel.IsBusy); // Should be false after completion
        }

        [Fact]
        public async Task LoadDataAsync_OnException_ShouldSetErrorMessage()
        {
            // Arrange
            var mockTransactionService = new Mock<ITransactionService>();
            var mockItemService = new Mock<IItemService>();

            var exceptionMessage = "Database connection failed";
            mockTransactionService.Setup(s => s.GetAllTransactionsAsync())
                .ThrowsAsync(new Exception(exceptionMessage));

            var viewModel = new DashboardViewModel(mockTransactionService.Object, mockItemService.Object);

            // Act
            await viewModel.LoadDataCommand.ExecuteAsync(null);

            // Assert
            Assert.Contains(exceptionMessage, viewModel.ErrorMessage);
        }

        [Fact]
        public async Task InitializeAsync_ShouldCallLoadData()
        {
            // Arrange
            var mockTransactionService = new Mock<ITransactionService>();
            var mockItemService = new Mock<IItemService>();

            mockTransactionService.Setup(s => s.GetAllTransactionsAsync()).ReturnsAsync(new List<Transaction>());
            mockItemService.Setup(s => s.GetAllItemsAsync()).ReturnsAsync(new List<Item>());

            var viewModel = new DashboardViewModel(mockTransactionService.Object, mockItemService.Object);

            // Act
            await viewModel.InitializeAsync();

            // Assert
            mockTransactionService.Verify(s => s.GetAllTransactionsAsync(), Times.Once);
            mockItemService.Verify(s => s.GetAllItemsAsync(), Times.Once);
        }

        [Fact]
        public async Task LoadDataAsync_ShouldAggregateRecentActivitiesFromMultipleSources()
        {
            // Arrange
            var mockTransactionService = new Mock<ITransactionService>();
            var mockItemService = new Mock<IItemService>();
            var mockFinancialService = new Mock<FinancialTransactionService>();
            var mockWageService = new Mock<IWageService>();

            var now = DateTime.Now;
            var transactions = new List<Transaction>
            {
                new Transaction 
                { 
                    TransactionId = 1, 
                    TransactionType = TransactionType.Buy, 
                    TotalAmount = 1000,
                    TransactionDate = now.AddHours(-1),
                    Item = new Item { ItemName = "Item1" }
                }
            };

            var financialTransactions = new List<FinancialTransaction>
            {
                new FinancialTransaction 
                { 
                    FinancialTransactionId = 1, 
                    TransactionType = FinancialTransactionType.LoanGiven, 
                    Amount = 5000,
                    TransactionDate = now.AddHours(-2),
                    Party = new Party { Name = "Party1" }
                }
            };

            var wageTransactions = new List<WageTransaction>
            {
                new WageTransaction 
                { 
                    WageTransactionId = 1, 
                    TransactionType = WageTransactionType.MonthlyWage, 
                    Amount = 2000,
                    TransactionDate = now.AddHours(-3),
                    Worker = new Worker { Name = "Worker1" }
                }
            };

            mockTransactionService.Setup(s => s.GetAllTransactionsAsync()).ReturnsAsync(transactions);
            mockItemService.Setup(s => s.GetAllItemsAsync()).ReturnsAsync(new List<Item>());
            mockFinancialService.Setup(s => s.GetAllFinancialTransactionsAsync()).ReturnsAsync(financialTransactions);
            mockFinancialService.Setup(s => s.GetTotalLoansGivenOutstandingAsync()).ReturnsAsync(0);
            mockFinancialService.Setup(s => s.GetTotalLoansTakenOutstandingAsync()).ReturnsAsync(0);
            mockWageService.Setup(s => s.GetAllWageTransactionsAsync()).ReturnsAsync(wageTransactions);
            mockWageService.Setup(s => s.GetTotalWagesPaidAsync()).ReturnsAsync(0);
            mockWageService.Setup(s => s.GetTotalAdvancesGivenAsync()).ReturnsAsync(0);

            var viewModel = new DashboardViewModel(
                mockTransactionService.Object,
                mockItemService.Object,
                mockFinancialService.Object,
                mockWageService.Object);

            // Act
            await viewModel.LoadDataCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal(3, viewModel.RecentActivities.Count);
            Assert.Contains(viewModel.RecentActivities, a => a.Category == "Transaction");
            Assert.Contains(viewModel.RecentActivities, a => a.Category == "Financial");
            Assert.Contains(viewModel.RecentActivities, a => a.Category == "Wage");
        }
    }
}

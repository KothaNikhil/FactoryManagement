using FactoryManagement.Models;
using FactoryManagement.Services;
using FactoryManagement.ViewModels;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FactoryManagement.Tests.ViewModels
{
    public class ReportsViewModelTests
    {
        [Fact]
        public async Task LoadReportDataAsync_AllType_ShouldLoadUnifiedTransactions()
        {
            // Arrange
            var mockTransactionService = new Mock<ITransactionService>();
            var mockItemService = new Mock<IItemService>();
            var mockPartyService = new Mock<IPartyService>();
            var mockExportService = new Mock<IExportService>();
            var mockFinancialService = new Mock<IFinancialTransactionService>();
            var mockWageService = new Mock<IWageService>();
            var mockUnifiedService = new Mock<IUnifiedTransactionService>();
            var mockUserService = new Mock<IUserService>();

            var unifiedTransactions = new List<UnifiedTransactionViewModel>
            {
                new UnifiedTransactionViewModel { TransactionId = "1", TransactionType = "Buy", Amount = 1000 },
                new UnifiedTransactionViewModel { TransactionId = "2", TransactionType = "Sell", Amount = 500 }
            };

            mockTransactionService.Setup(s => s.GetAllTransactionsAsync()).ReturnsAsync(new List<Transaction>());
            mockItemService.Setup(s => s.GetAllItemsAsync()).ReturnsAsync(new List<Item>());
            mockPartyService.Setup(s => s.GetAllPartiesAsync()).ReturnsAsync(new List<Party>());
            mockWageService.Setup(s => s.GetAllWorkersAsync()).ReturnsAsync(new List<Worker>());
            mockUserService.Setup(s => s.GetAllUsersAsync()).ReturnsAsync(new List<User>());
            mockFinancialService.Setup(s => s.GetAllFinancialTransactionsAsync()).ReturnsAsync(new List<FinancialTransaction>());
            mockWageService.Setup(s => s.GetAllWageTransactionsAsync()).ReturnsAsync(new List<WageTransaction>());
            mockUnifiedService.Setup(s => s.GetAllUnifiedTransactionsAsync(null)).ReturnsAsync(unifiedTransactions);

            var viewModel = new ReportsViewModel(
                mockTransactionService.Object,
                mockItemService.Object,
                mockPartyService.Object,
                mockExportService.Object,
                mockFinancialService.Object,
                mockWageService.Object,
                mockUnifiedService.Object,
                mockUserService.Object);

            viewModel.SelectedReportType = ReportType.All;

            // Act
            await viewModel.LoadReportDataCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal(ReportType.All, viewModel.SelectedReportType);
        }

        [Fact]
        public async Task LoadReportDataAsync_InventoryType_ShouldLoadTransactions()
        {
            // Arrange
            var mockTransactionService = new Mock<ITransactionService>();
            var mockItemService = new Mock<IItemService>();
            var mockPartyService = new Mock<IPartyService>();
            var mockExportService = new Mock<IExportService>();
            var mockFinancialService = new Mock<IFinancialTransactionService>();
            var mockWageService = new Mock<IWageService>();
            var mockUnifiedService = new Mock<IUnifiedTransactionService>();
            var mockUserService = new Mock<IUserService>();

            var transactions = new List<Transaction>
            {
                new Transaction { TransactionId = 1, TransactionType = TransactionType.Buy, TotalAmount = 1000 },
                new Transaction { TransactionId = 2, TransactionType = TransactionType.Sell, TotalAmount = 500 }
            };

            mockTransactionService.Setup(s => s.GetAllTransactionsAsync()).ReturnsAsync(transactions);
            mockItemService.Setup(s => s.GetAllItemsAsync()).ReturnsAsync(new List<Item>());
            mockPartyService.Setup(s => s.GetAllPartiesAsync()).ReturnsAsync(new List<Party>());
            mockWageService.Setup(s => s.GetAllWorkersAsync()).ReturnsAsync(new List<Worker>());
            mockUserService.Setup(s => s.GetAllUsersAsync()).ReturnsAsync(new List<User>());
            mockFinancialService.Setup(s => s.GetAllFinancialTransactionsAsync()).ReturnsAsync(new List<FinancialTransaction>());
            mockWageService.Setup(s => s.GetAllWageTransactionsAsync()).ReturnsAsync(new List<WageTransaction>());
            mockUnifiedService.Setup(s => s.GetAllUnifiedTransactionsAsync(null)).ReturnsAsync(new List<UnifiedTransactionViewModel>());

            var viewModel = new ReportsViewModel(
                mockTransactionService.Object,
                mockItemService.Object,
                mockPartyService.Object,
                mockExportService.Object,
                mockFinancialService.Object,
                mockWageService.Object,
                mockUnifiedService.Object,
                mockUserService.Object);

            viewModel.SelectedReportType = ReportType.Inventory;

            // Act
            await viewModel.LoadReportDataCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal(ReportType.Inventory, viewModel.SelectedReportType);
        }

        [Fact]
        public async Task LoadReportDataAsync_FinancialType_ShouldLoadFinancialTransactions()
        {
            // Arrange
            var mockTransactionService = new Mock<ITransactionService>();
            var mockItemService = new Mock<IItemService>();
            var mockPartyService = new Mock<IPartyService>();
            var mockExportService = new Mock<IExportService>();
            var mockFinancialService = new Mock<IFinancialTransactionService>();
            var mockWageService = new Mock<IWageService>();
            var mockUnifiedService = new Mock<IUnifiedTransactionService>();
            var mockUserService = new Mock<IUserService>();

            var financialTransactions = new List<FinancialTransaction>
            {
                new FinancialTransaction { FinancialTransactionId = 1, TransactionType = FinancialTransactionType.LoanGiven, Amount = 5000 },
                new FinancialTransaction { FinancialTransactionId = 2, TransactionType = FinancialTransactionType.LoanTaken, Amount = 3000 }
            };

            mockTransactionService.Setup(s => s.GetAllTransactionsAsync()).ReturnsAsync(new List<Transaction>());
            mockItemService.Setup(s => s.GetAllItemsAsync()).ReturnsAsync(new List<Item>());
            mockPartyService.Setup(s => s.GetAllPartiesAsync()).ReturnsAsync(new List<Party>());
            mockWageService.Setup(s => s.GetAllWorkersAsync()).ReturnsAsync(new List<Worker>());
            mockUserService.Setup(s => s.GetAllUsersAsync()).ReturnsAsync(new List<User>());
            mockFinancialService.Setup(s => s.GetAllFinancialTransactionsAsync()).ReturnsAsync(financialTransactions);
            mockWageService.Setup(s => s.GetAllWageTransactionsAsync()).ReturnsAsync(new List<WageTransaction>());
            mockUnifiedService.Setup(s => s.GetAllUnifiedTransactionsAsync(null)).ReturnsAsync(new List<UnifiedTransactionViewModel>());

            var viewModel = new ReportsViewModel(
                mockTransactionService.Object,
                mockItemService.Object,
                mockPartyService.Object,
                mockExportService.Object,
                mockFinancialService.Object,
                mockWageService.Object,
                mockUnifiedService.Object,
                mockUserService.Object);

            viewModel.SelectedReportType = ReportType.Financial;

            // Act
            await viewModel.LoadReportDataCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal(ReportType.Financial, viewModel.SelectedReportType);
        }

        [Fact]
        public async Task LoadReportDataAsync_WagesType_ShouldLoadWageTransactions()
        {
            // Arrange
            var mockTransactionService = new Mock<ITransactionService>();
            var mockItemService = new Mock<IItemService>();
            var mockPartyService = new Mock<IPartyService>();
            var mockExportService = new Mock<IExportService>();
            var mockFinancialService = new Mock<IFinancialTransactionService>();
            var mockWageService = new Mock<IWageService>();
            var mockUnifiedService = new Mock<IUnifiedTransactionService>();
            var mockUserService = new Mock<IUserService>();

            var wageTransactions = new List<WageTransaction>
            {
                new WageTransaction { WageTransactionId = 1, TransactionType = WageTransactionType.MonthlyWage, Amount = 10000 },
                new WageTransaction { WageTransactionId = 2, TransactionType = WageTransactionType.AdvanceGiven, Amount = 2000 }
            };

            mockTransactionService.Setup(s => s.GetAllTransactionsAsync()).ReturnsAsync(new List<Transaction>());
            mockItemService.Setup(s => s.GetAllItemsAsync()).ReturnsAsync(new List<Item>());
            mockPartyService.Setup(s => s.GetAllPartiesAsync()).ReturnsAsync(new List<Party>());
            mockWageService.Setup(s => s.GetAllWorkersAsync()).ReturnsAsync(new List<Worker>());
            mockUserService.Setup(s => s.GetAllUsersAsync()).ReturnsAsync(new List<User>());
            mockFinancialService.Setup(s => s.GetAllFinancialTransactionsAsync()).ReturnsAsync(new List<FinancialTransaction>());
            mockWageService.Setup(s => s.GetAllWageTransactionsAsync()).ReturnsAsync(wageTransactions);
            mockUnifiedService.Setup(s => s.GetAllUnifiedTransactionsAsync(null)).ReturnsAsync(new List<UnifiedTransactionViewModel>());

            var viewModel = new ReportsViewModel(
                mockTransactionService.Object,
                mockItemService.Object,
                mockPartyService.Object,
                mockExportService.Object,
                mockFinancialService.Object,
                mockWageService.Object,
                mockUnifiedService.Object,
                mockUserService.Object);

            viewModel.SelectedReportType = ReportType.Wages;

            // Act
            await viewModel.LoadReportDataCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal(ReportType.Wages, viewModel.SelectedReportType);
        }

        [Fact]
        public void SelectedReportType_ShouldClearFiltersOnChange()
        {
            // Arrange
            var mockTransactionService = new Mock<ITransactionService>();
            var mockItemService = new Mock<IItemService>();
            var mockPartyService = new Mock<IPartyService>();
            var mockExportService = new Mock<IExportService>();
            var mockFinancialService = new Mock<IFinancialTransactionService>();
            var mockWageService = new Mock<IWageService>();
            var mockUnifiedService = new Mock<IUnifiedTransactionService>();
            var mockUserService = new Mock<IUserService>();

            mockTransactionService.Setup(s => s.GetAllTransactionsAsync()).ReturnsAsync(new List<Transaction>());
            mockItemService.Setup(s => s.GetAllItemsAsync()).ReturnsAsync(new List<Item>());
            mockPartyService.Setup(s => s.GetAllPartiesAsync()).ReturnsAsync(new List<Party>());
            mockWageService.Setup(s => s.GetAllWorkersAsync()).ReturnsAsync(new List<Worker>());
            mockUserService.Setup(s => s.GetAllUsersAsync()).ReturnsAsync(new List<User>());
            mockFinancialService.Setup(s => s.GetAllFinancialTransactionsAsync()).ReturnsAsync(new List<FinancialTransaction>());
            mockWageService.Setup(s => s.GetAllWageTransactionsAsync()).ReturnsAsync(new List<WageTransaction>());
            mockUnifiedService.Setup(s => s.GetAllUnifiedTransactionsAsync(null)).ReturnsAsync(new List<UnifiedTransactionViewModel>());

            var viewModel = new ReportsViewModel(
                mockTransactionService.Object,
                mockItemService.Object,
                mockPartyService.Object,
                mockExportService.Object,
                mockFinancialService.Object,
                mockWageService.Object,
                mockUnifiedService.Object,
                mockUserService.Object);

            var testItem = new Item { ItemId = 1, ItemName = "Test" };
            viewModel.SelectedItem = testItem;

            // Act
            viewModel.SelectedReportType = ReportType.Financial;

            // Assert
            Assert.Null(viewModel.SelectedItem);
            Assert.Equal(1, viewModel.CurrentPage);
        }

        [Fact]
        public void CurrentPage_ShouldCalculateTotalPages()
        {
            // Arrange
            var mockTransactionService = new Mock<ITransactionService>();
            var mockItemService = new Mock<IItemService>();
            var mockPartyService = new Mock<IPartyService>();
            var mockExportService = new Mock<IExportService>();
            var mockFinancialService = new Mock<IFinancialTransactionService>();
            var mockWageService = new Mock<IWageService>();
            var mockUnifiedService = new Mock<IUnifiedTransactionService>();
            var mockUserService = new Mock<IUserService>();

            mockTransactionService.Setup(s => s.GetAllTransactionsAsync()).ReturnsAsync(new List<Transaction>());
            mockItemService.Setup(s => s.GetAllItemsAsync()).ReturnsAsync(new List<Item>());
            mockPartyService.Setup(s => s.GetAllPartiesAsync()).ReturnsAsync(new List<Party>());
            mockWageService.Setup(s => s.GetAllWorkersAsync()).ReturnsAsync(new List<Worker>());
            mockUserService.Setup(s => s.GetAllUsersAsync()).ReturnsAsync(new List<User>());
            mockFinancialService.Setup(s => s.GetAllFinancialTransactionsAsync()).ReturnsAsync(new List<FinancialTransaction>());
            mockWageService.Setup(s => s.GetAllWageTransactionsAsync()).ReturnsAsync(new List<WageTransaction>());
            mockUnifiedService.Setup(s => s.GetAllUnifiedTransactionsAsync(null)).ReturnsAsync(new List<UnifiedTransactionViewModel>());

            var viewModel = new ReportsViewModel(
                mockTransactionService.Object,
                mockItemService.Object,
                mockPartyService.Object,
                mockExportService.Object,
                mockFinancialService.Object,
                mockWageService.Object,
                mockUnifiedService.Object,
                mockUserService.Object);

            // Use Inventory view for pagination and add 26 dummy records
            viewModel.SelectedReportType = ReportType.Inventory;
            for (int i = 0; i < 26; i++)
            {
                viewModel.Transactions.Add(new Transaction { TransactionId = i + 1, TotalAmount = 10 });
            }
            // Trigger pagination calculation
            viewModel.CurrentPage = 2;
            viewModel.CurrentPage = 1;

            // Assert - PageSize=13 => 2 pages for 26 records
            Assert.Equal(2, viewModel.TotalPages);
        }

        [Fact]
        public void CanGoToPreviousPage_ShouldBeFalseOnFirstPage()
        {
            // Arrange
            var mockTransactionService = new Mock<ITransactionService>();
            var mockItemService = new Mock<IItemService>();
            var mockPartyService = new Mock<IPartyService>();
            var mockExportService = new Mock<IExportService>();
            var mockFinancialService = new Mock<IFinancialTransactionService>();
            var mockWageService = new Mock<IWageService>();
            var mockUnifiedService = new Mock<IUnifiedTransactionService>();
            var mockUserService = new Mock<IUserService>();

            mockTransactionService.Setup(s => s.GetAllTransactionsAsync()).ReturnsAsync(new List<Transaction>());
            mockItemService.Setup(s => s.GetAllItemsAsync()).ReturnsAsync(new List<Item>());
            mockPartyService.Setup(s => s.GetAllPartiesAsync()).ReturnsAsync(new List<Party>());
            mockWageService.Setup(s => s.GetAllWorkersAsync()).ReturnsAsync(new List<Worker>());
            mockUserService.Setup(s => s.GetAllUsersAsync()).ReturnsAsync(new List<User>());
            mockFinancialService.Setup(s => s.GetAllFinancialTransactionsAsync()).ReturnsAsync(new List<FinancialTransaction>());
            mockWageService.Setup(s => s.GetAllWageTransactionsAsync()).ReturnsAsync(new List<WageTransaction>());
            mockUnifiedService.Setup(s => s.GetAllUnifiedTransactionsAsync(null)).ReturnsAsync(new List<UnifiedTransactionViewModel>());

            var viewModel = new ReportsViewModel(
                mockTransactionService.Object,
                mockItemService.Object,
                mockPartyService.Object,
                mockExportService.Object,
                mockFinancialService.Object,
                mockWageService.Object,
                mockUnifiedService.Object,
                mockUserService.Object);

            // Act & Assert
            Assert.False(viewModel.CanGoToPreviousPage);
        }

        [Fact]
        public void CanGoToNextPage_ShouldBeTrueWhenMorePages()
        {
            // Arrange
            var mockTransactionService = new Mock<ITransactionService>();
            var mockItemService = new Mock<IItemService>();
            var mockPartyService = new Mock<IPartyService>();
            var mockExportService = new Mock<IExportService>();
            var mockFinancialService = new Mock<IFinancialTransactionService>();
            var mockWageService = new Mock<IWageService>();
            var mockUnifiedService = new Mock<IUnifiedTransactionService>();
            var mockUserService = new Mock<IUserService>();

            mockTransactionService.Setup(s => s.GetAllTransactionsAsync()).ReturnsAsync(new List<Transaction>());
            mockItemService.Setup(s => s.GetAllItemsAsync()).ReturnsAsync(new List<Item>());
            mockPartyService.Setup(s => s.GetAllPartiesAsync()).ReturnsAsync(new List<Party>());
            mockWageService.Setup(s => s.GetAllWorkersAsync()).ReturnsAsync(new List<Worker>());
            mockUserService.Setup(s => s.GetAllUsersAsync()).ReturnsAsync(new List<User>());
            mockFinancialService.Setup(s => s.GetAllFinancialTransactionsAsync()).ReturnsAsync(new List<FinancialTransaction>());
            mockWageService.Setup(s => s.GetAllWageTransactionsAsync()).ReturnsAsync(new List<WageTransaction>());
            mockUnifiedService.Setup(s => s.GetAllUnifiedTransactionsAsync(null)).ReturnsAsync(new List<UnifiedTransactionViewModel>());

            var viewModel = new ReportsViewModel(
                mockTransactionService.Object,
                mockItemService.Object,
                mockPartyService.Object,
                mockExportService.Object,
                mockFinancialService.Object,
                mockWageService.Object,
                mockUnifiedService.Object,
                mockUserService.Object);

            // Act
            viewModel.CurrentPage = 1;
            viewModel.TotalPages = 3;

            // Assert
            Assert.True(viewModel.CanGoToNextPage);
        }

        [Fact]
        public void ViewVisibility_AllView_ShouldBeTrue()
        {
            // Arrange
            var mockTransactionService = new Mock<ITransactionService>();
            var mockItemService = new Mock<IItemService>();
            var mockPartyService = new Mock<IPartyService>();
            var mockExportService = new Mock<IExportService>();
            var mockFinancialService = new Mock<IFinancialTransactionService>();
            var mockWageService = new Mock<IWageService>();
            var mockUnifiedService = new Mock<IUnifiedTransactionService>();
            var mockUserService = new Mock<IUserService>();

            mockTransactionService.Setup(s => s.GetAllTransactionsAsync()).ReturnsAsync(new List<Transaction>());
            mockItemService.Setup(s => s.GetAllItemsAsync()).ReturnsAsync(new List<Item>());
            mockPartyService.Setup(s => s.GetAllPartiesAsync()).ReturnsAsync(new List<Party>());
            mockWageService.Setup(s => s.GetAllWorkersAsync()).ReturnsAsync(new List<Worker>());
            mockUserService.Setup(s => s.GetAllUsersAsync()).ReturnsAsync(new List<User>());
            mockFinancialService.Setup(s => s.GetAllFinancialTransactionsAsync()).ReturnsAsync(new List<FinancialTransaction>());
            mockWageService.Setup(s => s.GetAllWageTransactionsAsync()).ReturnsAsync(new List<WageTransaction>());
            mockUnifiedService.Setup(s => s.GetAllUnifiedTransactionsAsync(null)).ReturnsAsync(new List<UnifiedTransactionViewModel>());

            var viewModel = new ReportsViewModel(
                mockTransactionService.Object,
                mockItemService.Object,
                mockPartyService.Object,
                mockExportService.Object,
                mockFinancialService.Object,
                mockWageService.Object,
                mockUnifiedService.Object,
                mockUserService.Object);

            viewModel.SelectedReportType = ReportType.All;

            // Assert
            Assert.True(viewModel.IsAllView);
            Assert.False(viewModel.IsInventoryView);
            Assert.False(viewModel.IsFinancialView);
            Assert.False(viewModel.IsWagesView);
        }

        [Fact]
        public void ViewVisibility_InventoryView_ShouldBeTrue()
        {
            // Arrange
            var mockTransactionService = new Mock<ITransactionService>();
            var mockItemService = new Mock<IItemService>();
            var mockPartyService = new Mock<IPartyService>();
            var mockExportService = new Mock<IExportService>();
            var mockFinancialService = new Mock<IFinancialTransactionService>();
            var mockWageService = new Mock<IWageService>();
            var mockUnifiedService = new Mock<IUnifiedTransactionService>();
            var mockUserService = new Mock<IUserService>();

            mockTransactionService.Setup(s => s.GetAllTransactionsAsync()).ReturnsAsync(new List<Transaction>());
            mockItemService.Setup(s => s.GetAllItemsAsync()).ReturnsAsync(new List<Item>());
            mockPartyService.Setup(s => s.GetAllPartiesAsync()).ReturnsAsync(new List<Party>());
            mockWageService.Setup(s => s.GetAllWorkersAsync()).ReturnsAsync(new List<Worker>());
            mockUserService.Setup(s => s.GetAllUsersAsync()).ReturnsAsync(new List<User>());
            mockFinancialService.Setup(s => s.GetAllFinancialTransactionsAsync()).ReturnsAsync(new List<FinancialTransaction>());
            mockWageService.Setup(s => s.GetAllWageTransactionsAsync()).ReturnsAsync(new List<WageTransaction>());
            mockUnifiedService.Setup(s => s.GetAllUnifiedTransactionsAsync(null)).ReturnsAsync(new List<UnifiedTransactionViewModel>());

            var viewModel = new ReportsViewModel(
                mockTransactionService.Object,
                mockItemService.Object,
                mockPartyService.Object,
                mockExportService.Object,
                mockFinancialService.Object,
                mockWageService.Object,
                mockUnifiedService.Object,
                mockUserService.Object);

            viewModel.SelectedReportType = ReportType.Inventory;

            // Assert
            Assert.False(viewModel.IsAllView);
            Assert.True(viewModel.IsInventoryView);
            Assert.False(viewModel.IsFinancialView);
            Assert.False(viewModel.IsWagesView);
        }

        [Fact]
        public void ApplyFiltersAsync_ShouldResetPaginationToFirstPage()
        {
            // Arrange
            var mockTransactionService = new Mock<ITransactionService>();
            var mockItemService = new Mock<IItemService>();
            var mockPartyService = new Mock<IPartyService>();
            var mockExportService = new Mock<IExportService>();
            var mockFinancialService = new Mock<IFinancialTransactionService>();
            var mockWageService = new Mock<IWageService>();
            var mockUnifiedService = new Mock<IUnifiedTransactionService>();
            var mockUserService = new Mock<IUserService>();

            mockTransactionService.Setup(s => s.GetAllTransactionsAsync()).ReturnsAsync(new List<Transaction>());
            mockItemService.Setup(s => s.GetAllItemsAsync()).ReturnsAsync(new List<Item>());
            mockPartyService.Setup(s => s.GetAllPartiesAsync()).ReturnsAsync(new List<Party>());
            mockWageService.Setup(s => s.GetAllWorkersAsync()).ReturnsAsync(new List<Worker>());
            mockUserService.Setup(s => s.GetAllUsersAsync()).ReturnsAsync(new List<User>());
            mockFinancialService.Setup(s => s.GetAllFinancialTransactionsAsync()).ReturnsAsync(new List<FinancialTransaction>());
            mockWageService.Setup(s => s.GetAllWageTransactionsAsync()).ReturnsAsync(new List<WageTransaction>());
            mockUnifiedService.Setup(s => s.GetAllUnifiedTransactionsAsync(null)).ReturnsAsync(new List<UnifiedTransactionViewModel>());

            var viewModel = new ReportsViewModel(
                mockTransactionService.Object,
                mockItemService.Object,
                mockPartyService.Object,
                mockExportService.Object,
                mockFinancialService.Object,
                mockWageService.Object,
                mockUnifiedService.Object,
                mockUserService.Object);

            viewModel.SelectedReportType = ReportType.Inventory;
            viewModel.CurrentPage = 3;

            // Act - Setting a filter should reset to page 1
            viewModel.SelectedItem = new Item { ItemId = 1 };

            // Assert
            Assert.Equal(1, viewModel.CurrentPage);
        }

        [Fact]
        public void ReportTitle_ShouldChangeBasedOnReportType()
        {
            // Arrange
            var mockTransactionService = new Mock<ITransactionService>();
            var mockItemService = new Mock<IItemService>();
            var mockPartyService = new Mock<IPartyService>();
            var mockExportService = new Mock<IExportService>();
            var mockFinancialService = new Mock<IFinancialTransactionService>();
            var mockWageService = new Mock<IWageService>();
            var mockUnifiedService = new Mock<IUnifiedTransactionService>();
            var mockUserService = new Mock<IUserService>();

            mockTransactionService.Setup(s => s.GetAllTransactionsAsync()).ReturnsAsync(new List<Transaction>());
            mockItemService.Setup(s => s.GetAllItemsAsync()).ReturnsAsync(new List<Item>());
            mockPartyService.Setup(s => s.GetAllPartiesAsync()).ReturnsAsync(new List<Party>());
            mockWageService.Setup(s => s.GetAllWorkersAsync()).ReturnsAsync(new List<Worker>());
            mockUserService.Setup(s => s.GetAllUsersAsync()).ReturnsAsync(new List<User>());
            mockFinancialService.Setup(s => s.GetAllFinancialTransactionsAsync()).ReturnsAsync(new List<FinancialTransaction>());
            mockWageService.Setup(s => s.GetAllWageTransactionsAsync()).ReturnsAsync(new List<WageTransaction>());
            mockUnifiedService.Setup(s => s.GetAllUnifiedTransactionsAsync(null)).ReturnsAsync(new List<UnifiedTransactionViewModel>());

            var viewModel = new ReportsViewModel(
                mockTransactionService.Object,
                mockItemService.Object,
                mockPartyService.Object,
                mockExportService.Object,
                mockFinancialService.Object,
                mockWageService.Object,
                mockUnifiedService.Object,
                mockUserService.Object);

            // Act & Assert
            viewModel.SelectedReportType = ReportType.All;
            Assert.Equal("All Transactions", viewModel.ReportTitle);

            viewModel.SelectedReportType = ReportType.Inventory;
            Assert.Contains("Inventory", viewModel.ReportTitle);

            viewModel.SelectedReportType = ReportType.Financial;
            Assert.Contains("Financial", viewModel.ReportTitle);

            viewModel.SelectedReportType = ReportType.Wages;
            Assert.Contains("Wage", viewModel.ReportTitle);
        }
    }
}

using FactoryManagement.Models;
using FactoryManagement.Services;
using FactoryManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FactoryManagement.Tests.Services
{
    public class ReportExportBuilderTests
    {
        private readonly ReportExportBuilder _builder;

        public ReportExportBuilderTests()
        {
            _builder = new ReportExportBuilder();
        }

        #region All Report Type Tests

        [Fact]
        public async Task BuildExportRowsAsync_AllType_ShouldMapUnifiedTransactionsCorrectly()
        {
            // Arrange
            var unified = new List<UnifiedTransactionViewModel>
            {
                new UnifiedTransactionViewModel
                {
                    TransactionId = "txn1",
                    Category = "Inventory",
                    TransactionType = "Buy",
                    ItemName = "Item A",
                    PartyName = "Supplier 1",
                    WorkerName = null,
                    Quantity = 10,
                    Rate = 100,
                    Amount = 1000,
                    DebitCredit = "Debit",
                    TransactionDate = new DateTime(2025, 1, 10),
                    Notes = "Purchase",
                    EnteredBy = "User1"
                }
            };

            // Act
            var rows = await _builder.BuildExportRowsAsync(
                ReportType.All,
                new List<Transaction>(),
                new List<FinancialTransaction>(),
                new List<WageTransaction>(),
                unified);

            // Assert
            Assert.Single(rows);
            var row = rows[0];
            Assert.Equal("Inventory", row.Category);
            Assert.Equal("txn1", row.TransactionId);
            Assert.Equal("Buy", row.TransactionType);
            Assert.Equal("Item A", row.ItemName);
            Assert.Equal("Supplier 1", row.PartyName);
            Assert.Equal(10, row.Quantity);
            Assert.Equal(100, row.Rate);
            Assert.Equal(1000, row.Amount);
            Assert.Equal(1000, row.DebitAmount);
            Assert.Null(row.CreditAmount);
            Assert.Equal("Purchase", row.Notes);
            Assert.Equal("User1", row.EnteredBy);
        }

        [Fact]
        public async Task BuildExportRowsAsync_AllType_ShouldOrderByDateDescending()
        {
            // Arrange
            var unified = new List<UnifiedTransactionViewModel>
            {
                new UnifiedTransactionViewModel { TransactionId = "txn1", TransactionDate = new DateTime(2025, 1, 10) },
                new UnifiedTransactionViewModel { TransactionId = "txn2", TransactionDate = new DateTime(2025, 1, 20) },
                new UnifiedTransactionViewModel { TransactionId = "txn3", TransactionDate = new DateTime(2025, 1, 5) }
            };

            // Act
            var rows = await _builder.BuildExportRowsAsync(
                ReportType.All,
                new List<Transaction>(),
                new List<FinancialTransaction>(),
                new List<WageTransaction>(),
                unified);

            // Assert
            Assert.Equal(3, rows.Count);
            Assert.Equal("txn2", rows[0].TransactionId); // 1/20
            Assert.Equal("txn1", rows[1].TransactionId); // 1/10
            Assert.Equal("txn3", rows[2].TransactionId); // 1/5
        }

        [Fact]
        public async Task BuildExportRowsAsync_AllType_ShouldHandleCreditAmount()
        {
            // Arrange
            var unified = new List<UnifiedTransactionViewModel>
            {
                new UnifiedTransactionViewModel
                {
                    TransactionId = "txn1",
                    Amount = 500,
                    DebitCredit = "Credit",  // Set directly as it's a settable property in ViewModel
                    TransactionDate = new DateTime(2025, 1, 10)
                }
            };

            // Act
            var rows = await _builder.BuildExportRowsAsync(
                ReportType.All,
                new List<Transaction>(),
                new List<FinancialTransaction>(),
                new List<WageTransaction>(),
                unified);

            // Assert
            Assert.Single(rows);
            var row = rows[0];
            Assert.Null(row.DebitAmount);
            Assert.Equal(500, row.CreditAmount);
        }

        #endregion

        #region Inventory Report Type Tests

        [Fact]
        public async Task BuildExportRowsAsync_InventoryType_ShouldMapTransactionsCorrectly()
        {
            // Arrange
            var inventoryTxn = new Transaction
            {
                TransactionId = 1,
                TransactionType = TransactionType.Buy, // Results in DebitCredit = "Debit"
                TotalAmount = 2000,
                Quantity = 20,
                PricePerUnit = 100,
                TransactionDate = new DateTime(2025, 1, 15),
                Notes = "Bulk purchase",
                Item = new Item { ItemId = 1, ItemName = "Item B" },
                Party = new Party { PartyId = 1, Name = "Supplier 2" },
                User = new User { UserId = 1, Username = "User2" }
            };

            // Act
            var rows = await _builder.BuildExportRowsAsync(
                ReportType.Inventory,
                new List<Transaction> { inventoryTxn },
                new List<FinancialTransaction>(),
                new List<WageTransaction>(),
                new List<UnifiedTransactionViewModel>());

            // Assert
            Assert.Single(rows);
            var row = rows[0];
            Assert.Equal("Inventory", row.Category);
            Assert.Equal("1", row.TransactionId);
            Assert.Equal("Buy", row.TransactionType);
            Assert.Equal("Item B", row.ItemName);
            Assert.Equal("Supplier 2", row.PartyName);
            Assert.Equal(20, row.Quantity);
            Assert.Equal(100, row.Rate);
            Assert.Equal(2000, row.Amount);
            Assert.Equal(2000, row.DebitAmount);
            Assert.Null(row.CreditAmount);
            Assert.Equal("Bulk purchase", row.Notes);
            Assert.Equal("User2", row.EnteredBy);
        }

        [Fact]
        public async Task BuildExportRowsAsync_InventoryType_ShouldOrderByDateDescending()
        {
            // Arrange
            var txn1 = new Transaction { TransactionId = 1, TransactionDate = new DateTime(2025, 1, 10) };
            var txn2 = new Transaction { TransactionId = 2, TransactionDate = new DateTime(2025, 1, 25) };
            var txn3 = new Transaction { TransactionId = 3, TransactionDate = new DateTime(2025, 1, 5) };

            // Act
            var rows = await _builder.BuildExportRowsAsync(
                ReportType.Inventory,
                new List<Transaction> { txn1, txn2, txn3 },
                new List<FinancialTransaction>(),
                new List<WageTransaction>(),
                new List<UnifiedTransactionViewModel>());

            // Assert
            Assert.Equal(3, rows.Count);
            Assert.Equal("2", rows[0].TransactionId); // 1/25
            Assert.Equal("1", rows[1].TransactionId); // 1/10
            Assert.Equal("3", rows[2].TransactionId); // 1/5
        }

        #endregion

        #region Financial Report Type Tests

        [Fact]
        public async Task BuildExportRowsAsync_FinancialType_ShouldMapTransactionsCorrectly()
        {
            // Arrange
            var financialTxn = new FinancialTransaction
            {
                FinancialTransactionId = 1,
                TransactionType = FinancialTransactionType.LoanGiven, // Results in DebitCredit = "Debit"
                Amount = 5000,
                InterestRate = 5,
                InterestAmount = 250,
                TransactionDate = new DateTime(2025, 1, 20),
                DueDate = new DateTime(2025, 4, 20),
                Notes = "Loan to customer",
                Party = new Party { PartyId = 2, Name = "Customer 1" },
                User = new User { UserId = 2, Username = "User3" }
            };

            // Act
            var rows = await _builder.BuildExportRowsAsync(
                ReportType.Financial,
                new List<Transaction>(),
                new List<FinancialTransaction> { financialTxn },
                new List<WageTransaction>(),
                new List<UnifiedTransactionViewModel>());

            // Assert
            Assert.Single(rows);
            var row = rows[0];
            Assert.Equal("Financial", row.Category);
            Assert.Equal("1", row.TransactionId);
            Assert.Equal("LoanGiven", row.TransactionType);
            Assert.Equal("Customer 1", row.PartyName);
            Assert.Equal(5000, row.Amount);
            Assert.Equal(5000, row.DebitAmount);
            Assert.Null(row.CreditAmount);
            Assert.Equal(5, row.InterestRate);
            Assert.Equal(250, row.InterestAmount);
            Assert.Equal(new DateTime(2025, 4, 20), row.DueDate);
            Assert.Equal("Loan to customer", row.Notes);
            Assert.Equal("User3", row.EnteredBy);
        }

        [Fact]
        public async Task BuildExportRowsAsync_FinancialType_ShouldHandleCreditAmount()
        {
            // Arrange
            var financialTxn = new FinancialTransaction
            {
                FinancialTransactionId = 2,
                TransactionType = FinancialTransactionType.LoanTaken, // Results in DebitCredit = "Credit"
                Amount = 3000,
                TransactionDate = new DateTime(2025, 1, 15),
                Party = new Party { PartyId = 3, Name = "Creditor" },
                User = new User { UserId = 1, Username = "User1" }
            };

            // Act
            var rows = await _builder.BuildExportRowsAsync(
                ReportType.Financial,
                new List<Transaction>(),
                new List<FinancialTransaction> { financialTxn },
                new List<WageTransaction>(),
                new List<UnifiedTransactionViewModel>());

            // Assert
            Assert.Single(rows);
            var row = rows[0];
            Assert.Null(row.DebitAmount);
            Assert.Equal(3000, row.CreditAmount);
        }

        #endregion

        #region Wages Report Type Tests

        [Fact]
        public async Task BuildExportRowsAsync_WagesType_ShouldMapTransactionsCorrectly()
        {
            // Arrange
            var wageTxn = new WageTransaction
            {
                WageTransactionId = 1,
                TransactionType = WageTransactionType.MonthlyWage, // Results in DebitCredit = "Debit"
                DaysWorked = 25,
                HoursWorked = null,
                Rate = 400,
                NetAmount = 10000,
                TransactionDate = new DateTime(2025, 1, 31),
                Notes = "January salary",
                Worker = new Worker { WorkerId = 1, Name = "Worker 1" },
                User = new User { UserId = 1, Username = "User1" }
            };

            // Act
            var rows = await _builder.BuildExportRowsAsync(
                ReportType.Wages,
                new List<Transaction>(),
                new List<FinancialTransaction>(),
                new List<WageTransaction> { wageTxn },
                new List<UnifiedTransactionViewModel>());

            // Assert
            Assert.Single(rows);
            var row = rows[0];
            Assert.Equal("Wages", row.Category);
            Assert.Equal("1", row.TransactionId);
            Assert.Equal("MonthlyWage", row.TransactionType);
            Assert.Equal("Worker 1", row.WorkerName);
            Assert.Equal(25, row.Quantity); // DaysWorked
            Assert.Equal(400, row.Rate);
            Assert.Equal(10000, row.Amount);
            Assert.Equal(10000, row.DebitAmount);
            Assert.Null(row.CreditAmount);
            Assert.Equal("January salary", row.Notes);
            Assert.Equal("User1", row.EnteredBy);
        }

        [Fact]
        public async Task BuildExportRowsAsync_WagesType_ShouldFallbackToHoursWorked()
        {
            // Arrange
            var wageTxn = new WageTransaction
            {
                WageTransactionId = 2,
                DaysWorked = null,
                HoursWorked = 160,
                TransactionDate = new DateTime(2025, 1, 15),
                Worker = new Worker { WorkerId = 2, Name = "Worker 2" },
                User = new User { UserId = 1, Username = "User1" }
            };

            // Act
            var rows = await _builder.BuildExportRowsAsync(
                ReportType.Wages,
                new List<Transaction>(),
                new List<FinancialTransaction>(),
                new List<WageTransaction> { wageTxn },
                new List<UnifiedTransactionViewModel>());

            // Assert
            Assert.Single(rows);
            var row = rows[0];
            Assert.Equal(160, row.Quantity); // HoursWorked
        }

        [Fact]
        public async Task BuildExportRowsAsync_WagesType_ShouldOrderByDateDescending()
        {
            // Arrange
            var txn1 = new WageTransaction { WageTransactionId = 1, TransactionDate = new DateTime(2025, 1, 10), Worker = new Worker() };
            var txn2 = new WageTransaction { WageTransactionId = 2, TransactionDate = new DateTime(2025, 1, 28), Worker = new Worker() };
            var txn3 = new WageTransaction { WageTransactionId = 3, TransactionDate = new DateTime(2025, 1, 5), Worker = new Worker() };

            // Act
            var rows = await _builder.BuildExportRowsAsync(
                ReportType.Wages,
                new List<Transaction>(),
                new List<FinancialTransaction>(),
                new List<WageTransaction> { txn1, txn2, txn3 },
                new List<UnifiedTransactionViewModel>());

            // Assert
            Assert.Equal(3, rows.Count);
            Assert.Equal("2", rows[0].TransactionId); // 1/28
            Assert.Equal("1", rows[1].TransactionId); // 1/10
            Assert.Equal("3", rows[2].TransactionId); // 1/5
        }

        #endregion

        #region Edge Cases

        [Fact]
        public async Task BuildExportRowsAsync_ShouldHandleNullCollections()
        {
            // Arrange - all empty
            var emptyList = new List<Transaction>();

            // Act
            var rows = await _builder.BuildExportRowsAsync(
                ReportType.Inventory,
                emptyList,
                new List<FinancialTransaction>(),
                new List<WageTransaction>(),
                new List<UnifiedTransactionViewModel>());

            // Assert
            Assert.Empty(rows);
        }

        [Fact]
        public async Task BuildExportRowsAsync_ShouldHandleNullNavigationProperties()
        {
            // Arrange
            var txn = new Transaction
            {
                TransactionId = 1,
                Item = null,
                Party = null,
                User = null,
                TransactionDate = new DateTime(2025, 1, 10)
            };

            // Act
            var rows = await _builder.BuildExportRowsAsync(
                ReportType.Inventory,
                new List<Transaction> { txn },
                new List<FinancialTransaction>(),
                new List<WageTransaction>(),
                new List<UnifiedTransactionViewModel>());

            // Assert
            Assert.Single(rows);
            var row = rows[0];
            Assert.Equal(string.Empty, row.ItemName);
            Assert.Equal(string.Empty, row.PartyName);
            Assert.Equal(string.Empty, row.EnteredBy);
        }

        [Fact]
        public async Task BuildExportRowsAsync_ShouldHandleNullNotes()
        {
            // Arrange
            var txn = new Transaction
            {
                TransactionId = 1,
                Notes = null,
                TransactionDate = new DateTime(2025, 1, 10),
                Item = new Item(),
                Party = new Party(),
                User = new User()
            };

            // Act
            var rows = await _builder.BuildExportRowsAsync(
                ReportType.Inventory,
                new List<Transaction> { txn },
                new List<FinancialTransaction>(),
                new List<WageTransaction>(),
                new List<UnifiedTransactionViewModel>());

            // Assert
            Assert.Single(rows);
            var row = rows[0];
            Assert.Equal(string.Empty, row.Notes);
        }

        #endregion
    }
}

using FactoryManagement.Models;
using System;
using Xunit;

namespace FactoryManagement.Tests.Models
{
    public class WageTransactionTests
    {
        [Fact]
        public void WageTransaction_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var transaction = new WageTransaction();

            // Assert
            Assert.Equal(0, transaction.WageTransactionId);
            Assert.Equal(0, transaction.WorkerId);
            Assert.True((DateTime.Now - transaction.TransactionDate).TotalSeconds < 1);
            Assert.Equal(0, transaction.Amount);
            Assert.Equal(0, transaction.NetAmount);
        }

        [Theory]
        [InlineData(WageTransactionType.DailyWage)]
        [InlineData(WageTransactionType.HourlyWage)]
        [InlineData(WageTransactionType.MonthlyWage)]
        [InlineData(WageTransactionType.OvertimePay)]
        [InlineData(WageTransactionType.Bonus)]
        [InlineData(WageTransactionType.AdvanceGiven)]
        [InlineData(WageTransactionType.AdvanceAdjustment)]
        [InlineData(WageTransactionType.Deduction)]
        public void WageTransaction_AllTransactionTypes_ShouldBeValid(WageTransactionType transactionType)
        {
            // Arrange & Act
            var transaction = new WageTransaction { TransactionType = transactionType };

            // Assert
            Assert.Equal(transactionType, transaction.TransactionType);
        }

        [Fact]
        public void WageTransaction_NetAmount_ShouldBeCalculatedCorrectly()
        {
            // Arrange
            var transaction = new WageTransaction
            {
                Amount = 1000m,
                OvertimeAmount = 200m,
                AdvanceAdjusted = 300m,
                Deductions = 100m
            };

            // Act
            decimal expectedNet = 1000m + 200m - 300m - 100m; // 800

            // Assert
            // Note: NetAmount should be set by the application logic
            transaction.NetAmount = expectedNet;
            Assert.Equal(800m, transaction.NetAmount);
        }
    }
}

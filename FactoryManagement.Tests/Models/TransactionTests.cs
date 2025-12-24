using FactoryManagement.Models;
using Xunit;

namespace FactoryManagement.Tests.Models
{
    public class TransactionTests
    {
        [Fact]
        public void Transaction_CalculatedAmount_ShouldBeCorrect()
        {
            // Arrange
            var transaction = new Transaction
            {
                Quantity = 10,
                PricePerUnit = 50.5m,
                TotalAmount = 10 * 50.5m
            };

            // Assert
            Assert.Equal(505m, transaction.TotalAmount);
        }

        [Theory]
        [InlineData(TransactionType.Buy)]
        [InlineData(TransactionType.Sell)]
        [InlineData(TransactionType.Wastage)]
        public void Transaction_AllTransactionTypes_ShouldBeValid(TransactionType transactionType)
        {
            // Arrange & Act
            var transaction = new Transaction { TransactionType = transactionType };

            // Assert
            Assert.Equal(transactionType, transaction.TransactionType);
        }
    }
}

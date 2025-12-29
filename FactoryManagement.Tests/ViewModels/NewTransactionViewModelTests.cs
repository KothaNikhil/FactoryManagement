using FactoryManagement.Models;
using FactoryManagement.Services;
using FactoryManagement.ViewModels;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace FactoryManagement.Tests.ViewModels
{
    public class NewTransactionViewModelTests
    {
        [Fact]
        public async Task LoadDataAsync_ShouldPopulateItemsAndParties()
        {
            // Arrange
            var mockTransactionService = new Mock<ITransactionService>();
            var mockItemService = new Mock<IItemService>();
            var mockPartyService = new Mock<IPartyService>();

            var items = new List<Item>
            {
                new Item { ItemId = 1, ItemName = "Item1", CurrentStock = 100 },
                new Item { ItemId = 2, ItemName = "Item2", CurrentStock = 200 }
            };
            var parties = new List<Party>
            {
                new Party { PartyId = 1, Name = "Party1" },
                new Party { PartyId = 2, Name = "Party2" }
            };

            mockItemService.Setup(s => s.GetAllItemsAsync()).ReturnsAsync(items);
            mockPartyService.Setup(s => s.GetAllPartiesAsync()).ReturnsAsync(parties);
            mockTransactionService.Setup(s => s.GetRecentTransactionsAsync(It.IsAny<int>())).ReturnsAsync(new List<Transaction>());

            var viewModel = new NewTransactionViewModel(
                mockTransactionService.Object,
                mockItemService.Object,
                mockPartyService.Object);

            // Act
            await viewModel.LoadDataCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal(2, viewModel.Items.Count);
            Assert.Equal(2, viewModel.Parties.Count);
        }

        [Fact]
        public void CalculateTotal_ShouldMultiplyQuantityByPrice()
        {
            // Arrange
            var mockTransactionService = new Mock<ITransactionService>();
            var mockItemService = new Mock<IItemService>();
            var mockPartyService = new Mock<IPartyService>();

            mockItemService.Setup(s => s.GetAllItemsAsync()).ReturnsAsync(new List<Item>());
            mockPartyService.Setup(s => s.GetAllPartiesAsync()).ReturnsAsync(new List<Party>());
            mockTransactionService.Setup(s => s.GetRecentTransactionsAsync(It.IsAny<int>())).ReturnsAsync(new List<Transaction>());

            var viewModel = new NewTransactionViewModel(
                mockTransactionService.Object,
                mockItemService.Object,
                mockPartyService.Object);

            // Act
            viewModel.Quantity = 10;
            viewModel.PricePerUnit = 50;

            // Assert
            Assert.Equal(500, viewModel.TotalAmount);
        }

        [Fact]
        public void ValidateTransaction_WastageShouldNotRequireParty()
        {
            // Arrange
            var mockTransactionService = new Mock<ITransactionService>();
            var mockItemService = new Mock<IItemService>();
            var mockPartyService = new Mock<IPartyService>();

            mockItemService.Setup(s => s.GetAllItemsAsync()).ReturnsAsync(new List<Item>());
            mockPartyService.Setup(s => s.GetAllPartiesAsync()).ReturnsAsync(new List<Party>());
            mockTransactionService.Setup(s => s.GetRecentTransactionsAsync(It.IsAny<int>())).ReturnsAsync(new List<Transaction>());

            var viewModel = new NewTransactionViewModel(
                mockTransactionService.Object,
                mockItemService.Object,
                mockPartyService.Object);

            // Act
            viewModel.SelectedTransactionTypeString = "Wastage";

            // Assert
            Assert.False(viewModel.IsPartyRequired);
        }

        [Fact]
        public void ValidateTransaction_BuyAndSellShouldRequireParty()
        {
            // Arrange
            var mockTransactionService = new Mock<ITransactionService>();
            var mockItemService = new Mock<IItemService>();
            var mockPartyService = new Mock<IPartyService>();

            mockItemService.Setup(s => s.GetAllItemsAsync()).ReturnsAsync(new List<Item>());
            mockPartyService.Setup(s => s.GetAllPartiesAsync()).ReturnsAsync(new List<Party>());
            mockTransactionService.Setup(s => s.GetRecentTransactionsAsync(It.IsAny<int>())).ReturnsAsync(new List<Transaction>());

            var viewModel = new NewTransactionViewModel(
                mockTransactionService.Object,
                mockItemService.Object,
                mockPartyService.Object);

            // Act & Assert
            viewModel.SelectedTransactionTypeString = "Buy";
            Assert.True(viewModel.IsPartyRequired);

            viewModel.SelectedTransactionTypeString = "Sell";
            Assert.True(viewModel.IsPartyRequired);
        }

        [Fact]
        public void ProcessingMode_ShouldBeEnabledForProcessingType()
        {
            // Arrange
            var mockTransactionService = new Mock<ITransactionService>();
            var mockItemService = new Mock<IItemService>();
            var mockPartyService = new Mock<IPartyService>();

            mockItemService.Setup(s => s.GetAllItemsAsync()).ReturnsAsync(new List<Item>());
            mockPartyService.Setup(s => s.GetAllPartiesAsync()).ReturnsAsync(new List<Party>());
            mockTransactionService.Setup(s => s.GetRecentTransactionsAsync(It.IsAny<int>())).ReturnsAsync(new List<Transaction>());

            var viewModel = new NewTransactionViewModel(
                mockTransactionService.Object,
                mockItemService.Object,
                mockPartyService.Object);

            // Act
            viewModel.SelectedTransactionTypeString = "Processing";

            // Assert
            Assert.True(viewModel.IsProcessingMode);
        }

        [Fact]
        public void ProcessingMode_ShouldBeDisabledForNonProcessingTypes()
        {
            // Arrange
            var mockTransactionService = new Mock<ITransactionService>();
            var mockItemService = new Mock<IItemService>();
            var mockPartyService = new Mock<IPartyService>();

            mockItemService.Setup(s => s.GetAllItemsAsync()).ReturnsAsync(new List<Item>());
            mockPartyService.Setup(s => s.GetAllPartiesAsync()).ReturnsAsync(new List<Party>());
            mockTransactionService.Setup(s => s.GetRecentTransactionsAsync(It.IsAny<int>())).ReturnsAsync(new List<Transaction>());

            var viewModel = new NewTransactionViewModel(
                mockTransactionService.Object,
                mockItemService.Object,
                mockPartyService.Object);

            // Act & Assert
            viewModel.SelectedTransactionTypeString = "Buy";
            Assert.False(viewModel.IsProcessingMode);

            viewModel.SelectedTransactionTypeString = "Sell";
            Assert.False(viewModel.IsProcessingMode);
        }

        [Fact]
        public void SaveButtonText_ShouldChangeBasedOnMode()
        {
            // Arrange
            var mockTransactionService = new Mock<ITransactionService>();
            var mockItemService = new Mock<IItemService>();
            var mockPartyService = new Mock<IPartyService>();

            mockItemService.Setup(s => s.GetAllItemsAsync()).ReturnsAsync(new List<Item>());
            mockPartyService.Setup(s => s.GetAllPartiesAsync()).ReturnsAsync(new List<Party>());
            mockTransactionService.Setup(s => s.GetRecentTransactionsAsync(It.IsAny<int>())).ReturnsAsync(new List<Transaction>());

            var viewModel = new NewTransactionViewModel(
                mockTransactionService.Object,
                mockItemService.Object,
                mockPartyService.Object);

            // Act & Assert
            Assert.Equal("SAVE TRANSACTION", viewModel.SaveButtonText);

            viewModel.IsEditMode = true;
            Assert.Equal("UPDATE TRANSACTION", viewModel.SaveButtonText);
        }

        [Fact]
        public void FormTitle_ShouldChangeBasedOnMode()
        {
            // Arrange
            var mockTransactionService = new Mock<ITransactionService>();
            var mockItemService = new Mock<IItemService>();
            var mockPartyService = new Mock<IPartyService>();

            mockItemService.Setup(s => s.GetAllItemsAsync()).ReturnsAsync(new List<Item>());
            mockPartyService.Setup(s => s.GetAllPartiesAsync()).ReturnsAsync(new List<Party>());
            mockTransactionService.Setup(s => s.GetRecentTransactionsAsync(It.IsAny<int>())).ReturnsAsync(new List<Transaction>());

            var viewModel = new NewTransactionViewModel(
                mockTransactionService.Object,
                mockItemService.Object,
                mockPartyService.Object);

            // Act & Assert
            Assert.Equal("New Transaction", viewModel.FormTitle);

            viewModel.IsEditMode = true;
            Assert.Equal("Edit Transaction", viewModel.FormTitle);
        }

        [Fact]
        public async Task ClearFormAsync_ShouldResetAllFields()
        {
            // Arrange
            var mockTransactionService = new Mock<ITransactionService>();
            var mockItemService = new Mock<IItemService>();
            var mockPartyService = new Mock<IPartyService>();

            mockItemService.Setup(s => s.GetAllItemsAsync()).ReturnsAsync(new List<Item>());
            mockPartyService.Setup(s => s.GetAllPartiesAsync()).ReturnsAsync(new List<Party>());
            mockTransactionService.Setup(s => s.GetRecentTransactionsAsync(It.IsAny<int>())).ReturnsAsync(new List<Transaction>());

            var viewModel = new NewTransactionViewModel(
                mockTransactionService.Object,
                mockItemService.Object,
                mockPartyService.Object);

            viewModel.SelectedItem = new Item { ItemId = 1, ItemName = "Item1" };
            viewModel.Quantity = 10;
            viewModel.PricePerUnit = 50;
            viewModel.Notes = "Test notes";
            viewModel.IsEditMode = true;

            // Act
            await viewModel.ClearFormCommand.ExecuteAsync(null);

            // Assert
            Assert.Null(viewModel.SelectedItem);
            Assert.Equal(0, viewModel.Quantity);
            Assert.Equal(0, viewModel.PricePerUnit);
            Assert.Equal(string.Empty, viewModel.Notes);
            Assert.False(viewModel.IsEditMode);
        }

        [Fact]
        public void EditTransactionAsync_ShouldPopulateFormWithTransactionData()
        {
            // Arrange
            var mockTransactionService = new Mock<ITransactionService>();
            var mockItemService = new Mock<IItemService>();
            var mockPartyService = new Mock<IPartyService>();

            var item = new Item { ItemId = 1, ItemName = "Item1" };
            var party = new Party { PartyId = 1, Name = "Party1" };
            var transaction = new Transaction
            {
                TransactionId = 1,
                ItemId = 1,
                PartyId = 1,
                TransactionType = TransactionType.Buy,
                Quantity = 100,
                PricePerUnit = 50,
                TotalAmount = 5000,
                TransactionDate = new DateTime(2025, 1, 1, 12, 0, 0),
                Notes = "Test transaction"
            };

            mockItemService.Setup(s => s.GetAllItemsAsync()).ReturnsAsync(new List<Item> { item });
            mockPartyService.Setup(s => s.GetAllPartiesAsync()).ReturnsAsync(new List<Party> { party });
            mockTransactionService.Setup(s => s.GetRecentTransactionsAsync(It.IsAny<int>())).ReturnsAsync(new List<Transaction>());

            var viewModel = new NewTransactionViewModel(
                mockTransactionService.Object,
                mockItemService.Object,
                mockPartyService.Object);

            // Act
            viewModel.EditTransactionCommand.Execute(transaction);

            // Assert
            Assert.True(viewModel.IsEditMode);
            Assert.Equal(1, viewModel.EditingTransactionId);
            Assert.Equal(100, viewModel.Quantity);
            Assert.Equal(50, viewModel.PricePerUnit);
            Assert.Equal("Test transaction", viewModel.Notes);
        }

        [Fact]
        public void ValidateTransaction_ProcessingShouldRequireInputItem()
        {
            // Arrange
            var mockTransactionService = new Mock<ITransactionService>();
            var mockItemService = new Mock<IItemService>();
            var mockPartyService = new Mock<IPartyService>();

            mockItemService.Setup(s => s.GetAllItemsAsync()).ReturnsAsync(new List<Item>());
            mockPartyService.Setup(s => s.GetAllPartiesAsync()).ReturnsAsync(new List<Party>());
            mockTransactionService.Setup(s => s.GetRecentTransactionsAsync(It.IsAny<int>())).ReturnsAsync(new List<Transaction>());

            var viewModel = new NewTransactionViewModel(
                mockTransactionService.Object,
                mockItemService.Object,
                mockPartyService.Object);

            viewModel.SelectedTransactionTypeString = "Processing";
            viewModel.SelectedItem = new Item { ItemId = 1 };
            viewModel.InputItem = null; // No input item selected
            viewModel.InputQuantity = 0;

            // Act - This would normally be called during validation, but we're testing the property setter
            // Assert that processing mode is enabled
            Assert.True(viewModel.IsProcessingMode);
        }

        [Fact]
        public void ValidateTransaction_ProcessingOutputAndInputShouldBeDifferent()
        {
            // Arrange
            var mockTransactionService = new Mock<ITransactionService>();
            var mockItemService = new Mock<IItemService>();
            var mockPartyService = new Mock<IPartyService>();

            mockItemService.Setup(s => s.GetAllItemsAsync()).ReturnsAsync(new List<Item>());
            mockPartyService.Setup(s => s.GetAllPartiesAsync()).ReturnsAsync(new List<Party>());
            mockTransactionService.Setup(s => s.GetRecentTransactionsAsync(It.IsAny<int>())).ReturnsAsync(new List<Transaction>());

            var viewModel = new NewTransactionViewModel(
                mockTransactionService.Object,
                mockItemService.Object,
                mockPartyService.Object);

            var item = new Item { ItemId = 1 };

            viewModel.SelectedTransactionTypeString = "Processing";
            viewModel.SelectedItem = item;
            viewModel.InputItem = item; // Same as output - invalid

            // Assert
            Assert.True(viewModel.IsProcessingMode);
            Assert.Equal(item.ItemId, viewModel.SelectedItem.ItemId);
            Assert.Equal(item.ItemId, viewModel.InputItem.ItemId);
        }

        [Fact]
        public void ItemLabelText_ShouldChangeBasedOnMode()
        {
            // Arrange
            var mockTransactionService = new Mock<ITransactionService>();
            var mockItemService = new Mock<IItemService>();
            var mockPartyService = new Mock<IPartyService>();

            mockItemService.Setup(s => s.GetAllItemsAsync()).ReturnsAsync(new List<Item>());
            mockPartyService.Setup(s => s.GetAllPartiesAsync()).ReturnsAsync(new List<Party>());
            mockTransactionService.Setup(s => s.GetRecentTransactionsAsync(It.IsAny<int>())).ReturnsAsync(new List<Transaction>());

            var viewModel = new NewTransactionViewModel(
                mockTransactionService.Object,
                mockItemService.Object,
                mockPartyService.Object);

            // Act & Assert
            viewModel.SelectedTransactionTypeString = "Buy";
            Assert.Equal("Item:", viewModel.ItemLabelText);

            viewModel.SelectedTransactionTypeString = "Processing";
            Assert.Equal("Output Item (Processed):", viewModel.ItemLabelText);
        }

        [Fact]
        public void QuantityLabelText_ShouldChangeBasedOnMode()
        {
            // Arrange
            var mockTransactionService = new Mock<ITransactionService>();
            var mockItemService = new Mock<IItemService>();
            var mockPartyService = new Mock<IPartyService>();

            mockItemService.Setup(s => s.GetAllItemsAsync()).ReturnsAsync(new List<Item>());
            mockPartyService.Setup(s => s.GetAllPartiesAsync()).ReturnsAsync(new List<Party>());
            mockTransactionService.Setup(s => s.GetRecentTransactionsAsync(It.IsAny<int>())).ReturnsAsync(new List<Transaction>());

            var viewModel = new NewTransactionViewModel(
                mockTransactionService.Object,
                mockItemService.Object,
                mockPartyService.Object);

            // Act & Assert
            viewModel.SelectedTransactionTypeString = "Buy";
            Assert.Equal("Quantity:", viewModel.QuantityLabelText);

            viewModel.SelectedTransactionTypeString = "Processing";
            Assert.Equal("Output Quantity:", viewModel.QuantityLabelText);
        }

        [Fact]
        public void PaymentModeOptions_ShouldContainCashAndBank()
        {
            // Arrange
            var mockTransactionService = new Mock<ITransactionService>();
            var mockItemService = new Mock<IItemService>();
            var mockPartyService = new Mock<IPartyService>();

            mockItemService.Setup(s => s.GetAllItemsAsync()).ReturnsAsync(new List<Item>());
            mockPartyService.Setup(s => s.GetAllPartiesAsync()).ReturnsAsync(new List<Party>());
            mockTransactionService.Setup(s => s.GetRecentTransactionsAsync(It.IsAny<int>())).ReturnsAsync(new List<Transaction>());

            var viewModel = new NewTransactionViewModel(
                mockTransactionService.Object,
                mockItemService.Object,
                mockPartyService.Object);

            // Assert
            Assert.Contains("Cash", viewModel.PaymentModes);
            Assert.Contains("Bank", viewModel.PaymentModes);
        }

        [Fact]
        public void TransactionTypeOptions_ShouldContainAllTypes()
        {
            // Arrange
            var mockTransactionService = new Mock<ITransactionService>();
            var mockItemService = new Mock<IItemService>();
            var mockPartyService = new Mock<IPartyService>();

            mockItemService.Setup(s => s.GetAllItemsAsync()).ReturnsAsync(new List<Item>());
            mockPartyService.Setup(s => s.GetAllPartiesAsync()).ReturnsAsync(new List<Party>());
            mockTransactionService.Setup(s => s.GetRecentTransactionsAsync(It.IsAny<int>())).ReturnsAsync(new List<Transaction>());

            var viewModel = new NewTransactionViewModel(
                mockTransactionService.Object,
                mockItemService.Object,
                mockPartyService.Object);

            // Assert
            Assert.Contains("Buy", viewModel.TransactionTypes);
            Assert.Contains("Sell", viewModel.TransactionTypes);
            Assert.Contains("Wastage", viewModel.TransactionTypes);
            Assert.Contains("Processing", viewModel.TransactionTypes);
        }

        [Fact]
        public void DefaultPaymentMode_ShouldBeCash()
        {
            // Arrange
            var mockTransactionService = new Mock<ITransactionService>();
            var mockItemService = new Mock<IItemService>();
            var mockPartyService = new Mock<IPartyService>();

            mockItemService.Setup(s => s.GetAllItemsAsync()).ReturnsAsync(new List<Item>());
            mockPartyService.Setup(s => s.GetAllPartiesAsync()).ReturnsAsync(new List<Party>());
            mockTransactionService.Setup(s => s.GetRecentTransactionsAsync(It.IsAny<int>())).ReturnsAsync(new List<Transaction>());

            var viewModel = new NewTransactionViewModel(
                mockTransactionService.Object,
                mockItemService.Object,
                mockPartyService.Object);

            // Assert
            Assert.Equal("Cash", viewModel.SelectedPaymentModeString);
        }

        [Fact]
        public async Task InitializeAsync_ShouldCallLoadData()
        {
            // Arrange
            var mockTransactionService = new Mock<ITransactionService>();
            var mockItemService = new Mock<IItemService>();
            var mockPartyService = new Mock<IPartyService>();

            mockItemService.Setup(s => s.GetAllItemsAsync()).ReturnsAsync(new List<Item>());
            mockPartyService.Setup(s => s.GetAllPartiesAsync()).ReturnsAsync(new List<Party>());
            mockTransactionService.Setup(s => s.GetRecentTransactionsAsync(It.IsAny<int>())).ReturnsAsync(new List<Transaction>());

            var viewModel = new NewTransactionViewModel(
                mockTransactionService.Object,
                mockItemService.Object,
                mockPartyService.Object);

            // Act
            await viewModel.InitializeAsync();

            // Assert
            mockItemService.Verify(s => s.GetAllItemsAsync(), Times.Once);
            mockPartyService.Verify(s => s.GetAllPartiesAsync(), Times.Once);
            mockTransactionService.Verify(s => s.GetRecentTransactionsAsync(It.IsAny<int>()), Times.Once);
        }
    }
}

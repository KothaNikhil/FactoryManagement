using FactoryManagement.Models;
using FactoryManagement.Services;
using FactoryManagement.ViewModels;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FactoryManagement.Tests.ViewModels
{
    public class ContactsViewModelTests
    {
        private readonly Mock<IPartyService> _mockPartyService;
        private readonly ContactsViewModel _viewModel;

        public ContactsViewModelTests()
        {
            _mockPartyService = new Mock<IPartyService>();
            _viewModel = new ContactsViewModel(_mockPartyService.Object);
        }

        [Fact]
        public async Task LoadPartiesCommand_ShouldLoadAllParties()
        {
            // Arrange
            var parties = new List<Party>
            {
                new Party { PartyId = 1, Name = "Party1", MobileNumber = "123456", PartyType = PartyType.Buyer },
                new Party { PartyId = 2, Name = "Party2", MobileNumber = "789012", PartyType = PartyType.Seller }
            };
            _mockPartyService.Setup(s => s.GetAllPartiesAsync()).ReturnsAsync(parties);

            // Act
            await _viewModel.LoadPartiesCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal(2, _viewModel.Parties.Count);
            Assert.Equal("Party1", _viewModel.Parties[0].Name);
            Assert.Equal("Party2", _viewModel.Parties[1].Name);
            _mockPartyService.Verify(s => s.GetAllPartiesAsync(), Times.Once);
        }

        [Fact]
        public async Task LoadPartiesCommand_OnError_ShouldSetErrorMessage()
        {
            // Arrange
            _mockPartyService.Setup(s => s.GetAllPartiesAsync())
                .ThrowsAsync(new System.Exception("Database error"));

            // Act
            await _viewModel.LoadPartiesCommand.ExecuteAsync(null);

            // Assert
            Assert.Contains("Error loading parties", _viewModel.ErrorMessage);
        }

        [Fact]
        public void NewPartyCommand_ShouldClearFormFields()
        {
            // Arrange - set some values first
            _viewModel.Name = "Test Party";
            _viewModel.MobileNumber = "123456";
            _viewModel.Place = "Test Place";
            _viewModel.IsEditMode = true;

            // Act
            _viewModel.NewPartyCommand.Execute(null);

            // Assert
            Assert.Equal(string.Empty, _viewModel.Name);
            Assert.Equal(string.Empty, _viewModel.MobileNumber);
            Assert.Equal(string.Empty, _viewModel.Place);
            Assert.False(_viewModel.IsEditMode);
            Assert.Equal(string.Empty, _viewModel.ErrorMessage);
        }

        [Fact]
        public async Task SavePartyCommand_WithValidData_ShouldAddNewParty()
        {
            // Arrange
            _viewModel.Name = "New Party";
            _viewModel.MobileNumber = "123456";
            _viewModel.Place = "Test Place";
            _viewModel.PartyType = PartyType.Buyer;
            _viewModel.IsEditMode = false;

            _mockPartyService.Setup(s => s.AddPartyAsync(It.IsAny<Party>(), It.IsAny<int?>()))
                .ReturnsAsync((Party p, int? _) => p);
            _mockPartyService.Setup(s => s.GetAllPartiesAsync()).ReturnsAsync(new List<Party>());

            // Act
            await _viewModel.SavePartyCommand.ExecuteAsync(null);

            // Assert
            _mockPartyService.Verify(s => s.AddPartyAsync(It.Is<Party>(p =>
                p.Name == "New Party" &&
                p.MobileNumber == "123456" &&
                p.Place == "Test Place" &&
                p.PartyType == PartyType.Buyer), It.IsAny<int?>()), Times.Once);
        }

        [Fact]
        public async Task SavePartyCommand_WithInvalidName_ShouldNotSave()
        {
            // Arrange
            _viewModel.Name = "";

            // Act
            await _viewModel.SavePartyCommand.ExecuteAsync(null);

            // Assert
            _mockPartyService.Verify(s => s.AddPartyAsync(It.IsAny<Party>(), It.IsAny<int?>()), Times.Never);
            Assert.Contains("required", _viewModel.ErrorMessage.ToLower());
        }

        [Fact]
        public async Task SavePartyCommand_InEditMode_ShouldUpdateParty()
        {
            // Arrange
            var existingParty = new Party 
            { 
                PartyId = 1, 
                Name = "Old Name", 
                MobileNumber = "111111",
                PartyType = PartyType.Seller 
            };
            _viewModel.SelectedParty = existingParty;
            _viewModel.IsEditMode = true;
            _viewModel.Name = "Updated Name";
            _viewModel.MobileNumber = "222222";
            _viewModel.PartyType = PartyType.Buyer;

            _mockPartyService.Setup(s => s.UpdatePartyAsync(It.IsAny<Party>(), It.IsAny<int?>())).Returns(Task.CompletedTask);
            _mockPartyService.Setup(s => s.GetAllPartiesAsync()).ReturnsAsync(new List<Party>());

            // Act
            await _viewModel.SavePartyCommand.ExecuteAsync(null);

            // Assert
            _mockPartyService.Verify(s => s.UpdatePartyAsync(It.Is<Party>(p =>
                p.PartyId == 1 &&
                p.Name == "Updated Name" &&
                p.MobileNumber == "222222" &&
                p.PartyType == PartyType.Buyer), It.IsAny<int?>()), Times.Once);
        }

        [Fact]
        public async Task DeletePartyCommand_ShouldDeleteParty()
        {
            // Arrange
            var partyToDelete = new Party { PartyId = 1, Name = "Test Party" };
            _mockPartyService.Setup(s => s.DeletePartyAsync(1)).Returns(Task.CompletedTask);
            _mockPartyService.Setup(s => s.GetAllPartiesAsync()).ReturnsAsync(new List<Party>());

            // Act
            await _viewModel.DeletePartyCommand.ExecuteAsync(partyToDelete);

            // Assert
            _mockPartyService.Verify(s => s.DeletePartyAsync(1), Times.Once);
        }

        [Fact]
        public void SearchText_Changed_ShouldFilterParties()
        {
            // Arrange
            var parties = new List<Party>
            {
                new Party { PartyId = 1, Name = "ABC Corp", PartyType = PartyType.Buyer },
                new Party { PartyId = 2, Name = "XYZ Ltd", PartyType = PartyType.Seller },
                new Party { PartyId = 3, Name = "ABC Trading", PartyType = PartyType.Buyer }
            };
            
            _mockPartyService.Setup(s => s.GetAllPartiesAsync()).ReturnsAsync(parties);
            _viewModel.LoadPartiesCommand.Execute(null);

            // Act
            _viewModel.SearchText = "abc";

            // Assert
            Assert.Equal(2, _viewModel.Parties.Count);
            Assert.All(_viewModel.Parties, p => Assert.Contains("ABC", p.Name));
        }

        [Fact]
        public void PropertyChanged_Name_ShouldRaiseEvent()
        {
            // Arrange
            var propertyChangedRaised = false;
            _viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(_viewModel.Name))
                    propertyChangedRaised = true;
            };

            // Act
            _viewModel.Name = "Test";

            // Assert
            Assert.True(propertyChangedRaised);
        }

        [Fact]
        public async Task InitializeAsync_ShouldLoadParties()
        {
            // Arrange
            var parties = new List<Party>
            {
                new Party { PartyId = 1, Name = "Party1" }
            };
            _mockPartyService.Setup(s => s.GetAllPartiesAsync()).ReturnsAsync(parties);

            // Act
            await _viewModel.InitializeAsync();

            // Assert
            Assert.Single(_viewModel.Parties);
            _mockPartyService.Verify(s => s.GetAllPartiesAsync(), Times.Once);
        }
    }
}


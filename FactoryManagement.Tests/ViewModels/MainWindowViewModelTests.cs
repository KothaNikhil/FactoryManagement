using FactoryManagement.ViewModels;
using Moq;
using Xunit;

namespace FactoryManagement.Tests.ViewModels
{
    public class MainWindowViewModelTests
    {
        private readonly MainWindowViewModel _viewModel;
        private readonly Mock<DashboardViewModel> _mockDashboardVM;
        private readonly Mock<NewTransactionViewModel> _mockTransactionEntryVM;
        private readonly Mock<ReportsViewModel> _mockReportsVM;
        private readonly Mock<InventoryViewModel> _mockItemsVM;
        private readonly Mock<ContactsViewModel> _mockPartiesVM;
        private readonly Mock<DataBackupViewModel> _mockBackupVM;
        private readonly Mock<FinancialRecordsViewModel> _mockFinancialVM;
        private readonly Mock<PayrollManagementViewModel> _mockWagesVM;

        public MainWindowViewModelTests()
        {
            _mockDashboardVM = new Mock<DashboardViewModel>(null!, null!, null!, null!);
            _mockTransactionEntryVM = new Mock<NewTransactionViewModel>(null!, null!, null!, null!);
            _mockReportsVM = new Mock<ReportsViewModel>(null!, null!, null!, null!, null!, null!);
            _mockItemsVM = new Mock<InventoryViewModel>(null!);
            _mockPartiesVM = new Mock<ContactsViewModel>(null!);
            _mockBackupVM = new Mock<DataBackupViewModel>(null!);
            _mockFinancialVM = new Mock<FinancialRecordsViewModel>(null!, null!);
            _mockWagesVM = new Mock<PayrollManagementViewModel>(null!);

            _viewModel = new MainWindowViewModel(
                _mockDashboardVM.Object,
                _mockTransactionEntryVM.Object,
                _mockReportsVM.Object,
                _mockItemsVM.Object,
                _mockPartiesVM.Object,
                _mockBackupVM.Object,
                _mockFinancialVM.Object,
                _mockWagesVM.Object);
        }

        [Fact]
        public void MainWindowViewModel_Initialization_ShouldSetDashboardAsCurrentView()
        {
            // Assert
            Assert.NotNull(_viewModel.CurrentView);
            Assert.Equal("Dashboard", _viewModel.CurrentViewTitle);
        }

        [Fact]
        public async System.Threading.Tasks.Task NavigateToDashboardCommand_ShouldSetDashboardAsCurrentView()
        {
            // Arrange - start with a different view
            await _viewModel.NavigateToReportsCommand.ExecuteAsync(null);
            
            // Act
            await _viewModel.NavigateToDashboardCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal("Dashboard", _viewModel.CurrentViewTitle);
        }

        [Fact]
        public async System.Threading.Tasks.Task NavigateToTransactionEntryCommand_ShouldChangeCurrentView()
        {
            // Act
            await _viewModel.NavigateToTransactionEntryCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal("New Transaction", _viewModel.CurrentViewTitle);
        }

        [Fact]
        public async System.Threading.Tasks.Task NavigateToReportsCommand_ShouldChangeCurrentView()
        {
            // Act
            await _viewModel.NavigateToReportsCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal("Reports & Analytics", _viewModel.CurrentViewTitle);
        }

        [Fact]
        public async System.Threading.Tasks.Task NavigateToItemsCommand_ShouldChangeCurrentView()
        {
            // Act
            await _viewModel.NavigateToItemsCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal("Inventory", _viewModel.CurrentViewTitle);
        }

        [Fact]
        public async System.Threading.Tasks.Task NavigateToPartiesCommand_ShouldChangeCurrentView()
        {
            // Act
            await _viewModel.NavigateToPartiesCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal("Contacts", _viewModel.CurrentViewTitle);
        }

        [Fact]
        public async System.Threading.Tasks.Task NavigateToFinancialTransactionsCommand_ShouldChangeCurrentView()
        {
            // Act
            await _viewModel.NavigateToFinancialTransactionsCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal("Financial Records", _viewModel.CurrentViewTitle);
        }

        [Fact]
        public async System.Threading.Tasks.Task NavigateToWagesCommand_ShouldChangeCurrentView()
        {
            // Act
            await _viewModel.NavigateToWagesCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal("Payroll Management", _viewModel.CurrentViewTitle);
        }

        [Fact]
        public void NavigateToBackupCommand_ShouldChangeCurrentView()
        {
            // Act
            _viewModel.NavigateToBackupCommand.Execute(null);

            // Assert
            Assert.Equal("Data Backup", _viewModel.CurrentViewTitle);
        }

        [Fact]
        public void PropertyChanged_CurrentView_ShouldRaiseEvent()
        {
            // Arrange
            var propertyChangedRaised = false;
            _viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(_viewModel.CurrentView))
                    propertyChangedRaised = true;
            };

            // Act
            _viewModel.CurrentView = _mockReportsVM.Object;

            // Assert
            Assert.True(propertyChangedRaised);
        }

        [Fact]
        public void PropertyChanged_CurrentViewTitle_ShouldRaiseEvent()
        {
            // Arrange
            var propertyChangedRaised = false;
            _viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(_viewModel.CurrentViewTitle))
                    propertyChangedRaised = true;
            };

            // Act
            _viewModel.CurrentViewTitle = "Test Title";

            // Assert
            Assert.True(propertyChangedRaised);
        }
    }
}

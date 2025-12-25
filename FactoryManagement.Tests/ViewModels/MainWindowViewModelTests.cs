using FactoryManagement.ViewModels;
using Moq;
using Xunit;

namespace FactoryManagement.Tests.ViewModels
{
    public class MainWindowViewModelTests
    {
        private readonly MainWindowViewModel _viewModel;
        private readonly Mock<DashboardViewModel> _mockDashboardVM;
        private readonly Mock<TransactionEntryViewModel> _mockTransactionEntryVM;
        private readonly Mock<ReportsViewModel> _mockReportsVM;
        private readonly Mock<ItemsManagementViewModel> _mockItemsVM;
        private readonly Mock<PartiesManagementViewModel> _mockPartiesVM;
        private readonly Mock<BackupViewModel> _mockBackupVM;
        private readonly Mock<FinancialTransactionsViewModel> _mockFinancialVM;
        private readonly Mock<WagesManagementViewModel> _mockWagesVM;

        public MainWindowViewModelTests()
        {
            _mockDashboardVM = new Mock<DashboardViewModel>(null!, null!, null!, null!);
            _mockTransactionEntryVM = new Mock<TransactionEntryViewModel>(null!, null!, null!, null!);
            _mockReportsVM = new Mock<ReportsViewModel>(null!, null!, null!, null!, null!, null!);
            _mockItemsVM = new Mock<ItemsManagementViewModel>(null!);
            _mockPartiesVM = new Mock<PartiesManagementViewModel>(null!);
            _mockBackupVM = new Mock<BackupViewModel>(null!);
            _mockFinancialVM = new Mock<FinancialTransactionsViewModel>(null!, null!);
            _mockWagesVM = new Mock<WagesManagementViewModel>(null!);

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
            Assert.Equal("Transaction Entry", _viewModel.CurrentViewTitle);
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
            Assert.Equal("Items Management", _viewModel.CurrentViewTitle);
        }

        [Fact]
        public async System.Threading.Tasks.Task NavigateToPartiesCommand_ShouldChangeCurrentView()
        {
            // Act
            await _viewModel.NavigateToPartiesCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal("Parties Management", _viewModel.CurrentViewTitle);
        }

        [Fact]
        public async System.Threading.Tasks.Task NavigateToFinancialTransactionsCommand_ShouldChangeCurrentView()
        {
            // Act
            await _viewModel.NavigateToFinancialTransactionsCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal("Financial Transactions", _viewModel.CurrentViewTitle);
        }

        [Fact]
        public async System.Threading.Tasks.Task NavigateToWagesCommand_ShouldChangeCurrentView()
        {
            // Act
            await _viewModel.NavigateToWagesCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal("Wages Management", _viewModel.CurrentViewTitle);
        }

        [Fact]
        public void NavigateToBackupCommand_ShouldChangeCurrentView()
        {
            // Act
            _viewModel.NavigateToBackupCommand.Execute(null);

            // Assert
            Assert.Equal("Backup & Restore", _viewModel.CurrentViewTitle);
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

using FactoryManagement.ViewModels;
using FactoryManagement.Data;
using Microsoft.EntityFrameworkCore;
using FactoryManagement.Services;
using Moq;
using Xunit;
using FactoryManagement.Models;
using System.Collections.Generic;

namespace FactoryManagement.Tests.ViewModels
{
    public class MainWindowViewModelTests
    {
        private readonly MainWindowViewModel _viewModel;
        private readonly DashboardViewModel _dashboardVM;
        private readonly NewTransactionViewModel _transactionEntryVM;
        private readonly ReportsViewModel _reportsVM;
        private readonly InventoryViewModel _itemsVM;
        private readonly ContactsViewModel _partiesVM;
        private readonly DataBackupViewModel _backupVM;
        private readonly FinancialRecordsViewModel _financialVM;
        private readonly PayrollManagementViewModel _wagesVM;
        private readonly WorkersManagementViewModel _workersVM;
        private readonly UsersViewModel _usersVM;
        private readonly FactoryDbContext _dbContext;
        private readonly Mock<IUserService> _mockUserService;

        public MainWindowViewModelTests()
        {
            var options = new DbContextOptionsBuilder<FactoryDbContext>()
                .UseInMemoryDatabase($"MainWindowVMTests_{System.Guid.NewGuid()}")
                .Options;
            _dbContext = new FactoryDbContext(options);

            // Create lightweight real instances with mocked dependencies so InitializeAsync works
            var txSvc = new Mock<ITransactionService>();
            txSvc.Setup(s => s.GetAllTransactionsAsync()).ReturnsAsync(System.Linq.Enumerable.Empty<Transaction>());
            var itemSvc = new Mock<IItemService>();
            itemSvc.Setup(s => s.GetAllItemsAsync()).ReturnsAsync(System.Linq.Enumerable.Empty<Item>());
            var partySvc = new Mock<IPartyService>();
            partySvc.Setup(s => s.GetAllPartiesAsync()).ReturnsAsync(System.Linq.Enumerable.Empty<Party>());
            var exportSvc = new Mock<IExportService>();
            var finSvc = new Mock<IFinancialTransactionService>();
            finSvc.Setup(s => s.GetAllFinancialTransactionsAsync()).ReturnsAsync(System.Linq.Enumerable.Empty<FinancialTransaction>());
            finSvc.Setup(s => s.GetTotalLoansGivenOutstandingAsync()).ReturnsAsync(0);
            finSvc.Setup(s => s.GetTotalLoansTakenOutstandingAsync()).ReturnsAsync(0);
            var wageSvc = new Mock<IWageService>();
            wageSvc.Setup(s => s.GetAllWorkersAsync()).ReturnsAsync(System.Linq.Enumerable.Empty<Worker>());
            wageSvc.Setup(s => s.GetAllWageTransactionsAsync()).ReturnsAsync(System.Linq.Enumerable.Empty<WageTransaction>());
            var unifiedSvc = new Mock<IUnifiedTransactionService>();
            unifiedSvc.Setup(s => s.GetAllUnifiedTransactionsAsync(null)).ReturnsAsync(new System.Collections.Generic.List<UnifiedTransactionViewModel>());
            var userSvc = new Mock<IUserService>();
            userSvc.Setup(s => s.GetActiveUsersAsync()).ReturnsAsync(System.Linq.Enumerable.Empty<User>());

            _dashboardVM = new DashboardViewModel(txSvc.Object, itemSvc.Object, finSvc.Object, wageSvc.Object, unifiedSvc.Object);
            _transactionEntryVM = new NewTransactionViewModel(txSvc.Object, itemSvc.Object, partySvc.Object);
            var reportBuilder = new Mock<IReportExportBuilder>();
            _reportsVM = new ReportsViewModel(txSvc.Object, itemSvc.Object, partySvc.Object, exportSvc.Object, finSvc.Object, wageSvc.Object, unifiedSvc.Object, userSvc.Object, reportBuilder.Object);
            _itemsVM = new InventoryViewModel(itemSvc.Object);
            _partiesVM = new ContactsViewModel(partySvc.Object);
            _backupVM = new DataBackupViewModel(new BackupService(_dbContext));
            _financialVM = new FinancialRecordsViewModel(finSvc.Object, partySvc.Object);
            _wagesVM = new PayrollManagementViewModel(wageSvc.Object);
            _workersVM = new WorkersManagementViewModel(wageSvc.Object);
            _usersVM = new UsersViewModel(userSvc.Object);

            _mockUserService = new Mock<IUserService>();

            _viewModel = new MainWindowViewModel(
                _dashboardVM,
                _transactionEntryVM,
                _reportsVM,
                _itemsVM,
                _partiesVM,
                _backupVM,
                _financialVM,
                _wagesVM,
                _workersVM,
                _usersVM,
                _dbContext,
                _mockUserService.Object);
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
            Assert.Equal("Dashboard Overview", _viewModel.CurrentViewTitle);
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
            Assert.Equal("Inventory Management", _viewModel.CurrentViewTitle);
        }

        [Fact]
        public async System.Threading.Tasks.Task NavigateToPartiesCommand_ShouldChangeCurrentView()
        {
            // Act
            await _viewModel.NavigateToPartiesCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal("Contact Management", _viewModel.CurrentViewTitle);
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
            _viewModel.CurrentView = _reportsVM;

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

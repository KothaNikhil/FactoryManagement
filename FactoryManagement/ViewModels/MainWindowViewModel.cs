using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using FactoryManagement.Data;
using FactoryManagement.Models;
using System.Linq;

namespace FactoryManagement.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private object? _currentView;

        [ObservableProperty]
        private string _currentViewTitle = "Dashboard";

        [ObservableProperty]
        private bool _isMenuPinned = true;

        [ObservableProperty]
        private bool _isMenuExpanded = true;

        private readonly DashboardViewModel _dashboardViewModel;
        private readonly NewTransactionViewModel _transactionEntryViewModel;
        private readonly ReportsViewModel _reportsViewModel;
        private readonly InventoryViewModel _itemsManagementViewModel;
        private readonly ContactsViewModel _partiesManagementViewModel;
        private readonly DataBackupViewModel _backupViewModel;
        private readonly FinancialRecordsViewModel _financialTransactionsViewModel;
        private readonly PayrollManagementViewModel _wagesManagementViewModel;

        private readonly FactoryDbContext _dbContext;

        public MainWindowViewModel(
            DashboardViewModel dashboardViewModel,
            NewTransactionViewModel transactionEntryViewModel,
            ReportsViewModel reportsViewModel,
            InventoryViewModel itemsManagementViewModel,
            ContactsViewModel partiesManagementViewModel,
            DataBackupViewModel backupViewModel,
            FinancialRecordsViewModel financialTransactionsViewModel,
            PayrollManagementViewModel wagesManagementViewModel,
            FactoryDbContext dbContext)
        {
            _dashboardViewModel = dashboardViewModel;
            _transactionEntryViewModel = transactionEntryViewModel;
            _reportsViewModel = reportsViewModel;
            _itemsManagementViewModel = itemsManagementViewModel;
            _partiesManagementViewModel = partiesManagementViewModel;
            _backupViewModel = backupViewModel;
            _financialTransactionsViewModel = financialTransactionsViewModel;
            _wagesManagementViewModel = wagesManagementViewModel;
            _dbContext = dbContext;

            // Set default view
            CurrentView = _dashboardViewModel;
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task NavigateToDashboardAsync()
        {
            CurrentView = _dashboardViewModel;
            CurrentViewTitle = "Dashboard";
            await _dashboardViewModel.InitializeAsync();
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task NavigateToTransactionEntryAsync()
        {
            CurrentView = _transactionEntryViewModel;
            CurrentViewTitle = "New Transaction";
            await _transactionEntryViewModel.InitializeAsync();
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task NavigateToReportsAsync()
        {
            CurrentView = _reportsViewModel;
            CurrentViewTitle = "Reports & Analytics";
            await _reportsViewModel.InitializeAsync();
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task NavigateToItemsAsync()
        {
            CurrentView = _itemsManagementViewModel;
            CurrentViewTitle = "Inventory";
            await _itemsManagementViewModel.InitializeAsync();
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task NavigateToPartiesAsync()
        {
            CurrentView = _partiesManagementViewModel;
            CurrentViewTitle = "Contacts";
            await _partiesManagementViewModel.InitializeAsync();
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task NavigateToFinancialTransactionsAsync()
        {
            CurrentView = _financialTransactionsViewModel;
            CurrentViewTitle = "Financial Records";
            // Load data is called in constructor, but we can optionally refresh here
            await System.Threading.Tasks.Task.CompletedTask;
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task NavigateToWagesAsync()
        {
            CurrentView = _wagesManagementViewModel;
            CurrentViewTitle = "Payroll Management";
            await _wagesManagementViewModel.InitializeAsync();
        }

        [RelayCommand]
        private void NavigateToBackup()
        {
            CurrentView = _backupViewModel;
            CurrentViewTitle = "Data Backup";
        }

        [RelayCommand]
        private void Exit()
        {
            Application.Current.Shutdown();
        }

        public async System.Threading.Tasks.Task InitializeAsync()
        {
            await NavigateToDashboardAsync();

            // Load menu pinned setting from AppSettings
            var settings = _dbContext.AppSettings.FirstOrDefault();
            if (settings != null)
            {
                IsMenuPinned = settings.IsMenuPinned;
                // Default expanded when pinned; collapsed when unpinned
                IsMenuExpanded = IsMenuPinned ? true : false;
            }
        }

        // Persist setting when toggled
        partial void OnIsMenuPinnedChanged(bool value)
        {
            var settings = _dbContext.AppSettings.FirstOrDefault();
            if (settings == null)
            {
                settings = new AppSettings { CompanyName = "Factory Management System", CurrencySymbol = "₹", Address = string.Empty };
                _dbContext.AppSettings.Add(settings);
            }
            settings.IsMenuPinned = value;
            settings.ModifiedDate = System.DateTime.UtcNow;
            _dbContext.SaveChanges();

            // Keep menu expanded when pinned; collapse when unpinned
            IsMenuExpanded = value ? true : false;
        }
    }
}

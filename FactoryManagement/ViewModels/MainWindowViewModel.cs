using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace FactoryManagement.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private object? _currentView;

        [ObservableProperty]
        private string _currentViewTitle = "Dashboard";

        private readonly DashboardViewModel _dashboardViewModel;
        private readonly NewTransactionViewModel _transactionEntryViewModel;
        private readonly ReportsViewModel _reportsViewModel;
        private readonly InventoryViewModel _itemsManagementViewModel;
        private readonly ContactsViewModel _partiesManagementViewModel;
        private readonly DataBackupViewModel _backupViewModel;
        private readonly FinancialRecordsViewModel _financialTransactionsViewModel;
        private readonly PayrollManagementViewModel _wagesManagementViewModel;

        public MainWindowViewModel(
            DashboardViewModel dashboardViewModel,
            NewTransactionViewModel transactionEntryViewModel,
            ReportsViewModel reportsViewModel,
            InventoryViewModel itemsManagementViewModel,
            ContactsViewModel partiesManagementViewModel,
            DataBackupViewModel backupViewModel,
            FinancialRecordsViewModel financialTransactionsViewModel,
            PayrollManagementViewModel wagesManagementViewModel)
        {
            _dashboardViewModel = dashboardViewModel;
            _transactionEntryViewModel = transactionEntryViewModel;
            _reportsViewModel = reportsViewModel;
            _itemsManagementViewModel = itemsManagementViewModel;
            _partiesManagementViewModel = partiesManagementViewModel;
            _backupViewModel = backupViewModel;
            _financialTransactionsViewModel = financialTransactionsViewModel;
            _wagesManagementViewModel = wagesManagementViewModel;

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
        }
    }
}

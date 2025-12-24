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
        private readonly TransactionEntryViewModel _transactionEntryViewModel;
        private readonly ReportsViewModel _reportsViewModel;
        private readonly ItemsManagementViewModel _itemsManagementViewModel;
        private readonly PartiesManagementViewModel _partiesManagementViewModel;
        private readonly BackupViewModel _backupViewModel;
        private readonly FinancialTransactionsViewModel _financialTransactionsViewModel;
        private readonly WagesManagementViewModel _wagesManagementViewModel;

        public MainWindowViewModel(
            DashboardViewModel dashboardViewModel,
            TransactionEntryViewModel transactionEntryViewModel,
            ReportsViewModel reportsViewModel,
            ItemsManagementViewModel itemsManagementViewModel,
            PartiesManagementViewModel partiesManagementViewModel,
            BackupViewModel backupViewModel,
            FinancialTransactionsViewModel financialTransactionsViewModel,
            WagesManagementViewModel wagesManagementViewModel)
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
            CurrentViewTitle = "Transaction Entry";
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
            CurrentViewTitle = "Items Management";
            await _itemsManagementViewModel.InitializeAsync();
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task NavigateToPartiesAsync()
        {
            CurrentView = _partiesManagementViewModel;
            CurrentViewTitle = "Parties Management";
            await _partiesManagementViewModel.InitializeAsync();
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task NavigateToFinancialTransactionsAsync()
        {
            CurrentView = _financialTransactionsViewModel;
            CurrentViewTitle = "Financial Transactions";
            // Load data is called in constructor, but we can optionally refresh here
            await System.Threading.Tasks.Task.CompletedTask;
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task NavigateToWagesAsync()
        {
            CurrentView = _wagesManagementViewModel;
            CurrentViewTitle = "Wages Management";
            await _wagesManagementViewModel.InitializeAsync();
        }

        [RelayCommand]
        private void NavigateToBackup()
        {
            CurrentView = _backupViewModel;
            CurrentViewTitle = "Backup & Restore";
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

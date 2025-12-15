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

        public MainWindowViewModel(
            DashboardViewModel dashboardViewModel,
            TransactionEntryViewModel transactionEntryViewModel,
            ReportsViewModel reportsViewModel,
            ItemsManagementViewModel itemsManagementViewModel,
            PartiesManagementViewModel partiesManagementViewModel,
            BackupViewModel backupViewModel)
        {
            _dashboardViewModel = dashboardViewModel;
            _transactionEntryViewModel = transactionEntryViewModel;
            _reportsViewModel = reportsViewModel;
            _itemsManagementViewModel = itemsManagementViewModel;
            _partiesManagementViewModel = partiesManagementViewModel;
            _backupViewModel = backupViewModel;

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

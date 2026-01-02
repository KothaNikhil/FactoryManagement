using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using FactoryManagement.Data;
using FactoryManagement.Models;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using FactoryManagement.Services;
using System.Threading.Tasks;

namespace FactoryManagement.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private static MainWindowViewModel? _instance;
        public static MainWindowViewModel? Instance => _instance;

        [ObservableProperty]
        private object? _currentView;

        [ObservableProperty]
        private string _currentViewTitle = "Dashboard";

        [ObservableProperty]
        private string _currentViewSubtitle = "Welcome back, here's what's happening in your factory today.";

        [ObservableProperty]
        private bool _isMenuPinned = true;

        [ObservableProperty]
        private bool _isMenuExpanded = true;

        [ObservableProperty]
        private ObservableCollection<User> _activeUsers = new();

        [ObservableProperty]
        private User? _selectedUser;

        public User? CurrentUser => SelectedUser;

        public bool IsAdminMode => SelectedUser?.Role?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true || 
                                    SelectedUser?.Role?.Equals("Administrator", StringComparison.OrdinalIgnoreCase) == true;

        private readonly DashboardViewModel _dashboardViewModel;
        private readonly NewTransactionViewModel _transactionEntryViewModel;
        private readonly ReportsViewModel _reportsViewModel;
        private readonly InventoryViewModel _itemsManagementViewModel;
        private readonly ContactsViewModel _partiesManagementViewModel;
        private readonly DataBackupViewModel _backupViewModel;
        private readonly FinancialRecordsViewModel _financialTransactionsViewModel;
        private readonly PayrollManagementViewModel _wagesManagementViewModel;
        private readonly WorkersManagementViewModel _workersManagementViewModel;
        private readonly UsersViewModel _usersViewModel;
        private readonly OperationalExpensesViewModel _operationalExpensesViewModel;
        private readonly ExpenseCategoryManagementViewModel _expenseCategoryManagementViewModel;

        private readonly FactoryDbContext _dbContext;
        private readonly IUserService _userService;

        public MainWindowViewModel(
            DashboardViewModel dashboardViewModel,
            NewTransactionViewModel transactionEntryViewModel,
            ReportsViewModel reportsViewModel,
            InventoryViewModel itemsManagementViewModel,
            ContactsViewModel partiesManagementViewModel,
            DataBackupViewModel backupViewModel,
            FinancialRecordsViewModel financialTransactionsViewModel,
            PayrollManagementViewModel wagesManagementViewModel,
            WorkersManagementViewModel workersManagementViewModel,
            UsersViewModel usersViewModel,
            OperationalExpensesViewModel operationalExpensesViewModel,
            ExpenseCategoryManagementViewModel expenseCategoryManagementViewModel,
            FactoryDbContext dbContext,
            IUserService userService)
        {
            _dashboardViewModel = dashboardViewModel;
            _transactionEntryViewModel = transactionEntryViewModel;
            _reportsViewModel = reportsViewModel;
            _itemsManagementViewModel = itemsManagementViewModel;
            _partiesManagementViewModel = partiesManagementViewModel;
            _backupViewModel = backupViewModel;
            _financialTransactionsViewModel = financialTransactionsViewModel;
            _wagesManagementViewModel = wagesManagementViewModel;
            _workersManagementViewModel = workersManagementViewModel;
            _usersViewModel = usersViewModel;
            _operationalExpensesViewModel = operationalExpensesViewModel;
            _expenseCategoryManagementViewModel = expenseCategoryManagementViewModel;
            _dbContext = dbContext;
            _userService = userService;

            // Set singleton instance
            _instance = this;

            // Set default view
            CurrentView = _dashboardViewModel;
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task NavigateToDashboardAsync()
        {
            await LoadActiveUsersAsync(); // Refresh user dropdown
            CurrentView = _dashboardViewModel;
            CurrentViewTitle = "Dashboard Overview";
            CurrentViewSubtitle = "Welcome back, here's what's happening in your factory today.";
            await _dashboardViewModel.InitializeAsync();
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task NavigateToTransactionEntryAsync()
        {
            await LoadActiveUsersAsync(); // Refresh user dropdown
            CurrentView = _transactionEntryViewModel;
            CurrentViewTitle = "New Transaction";
            CurrentViewSubtitle = "Create and record a new transaction.";
            await _transactionEntryViewModel.InitializeAsync();
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task NavigateToReportsAsync()
        {
            await LoadActiveUsersAsync(); // Refresh user dropdown
            CurrentView = _reportsViewModel;
            CurrentViewTitle = "Reports & Analytics";
            CurrentViewSubtitle = "Visualize performance and export insights.";
            await _reportsViewModel.InitializeAsync();
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task NavigateToItemsAsync()
        {
            await LoadActiveUsersAsync(); // Refresh user dropdown
            CurrentView = _itemsManagementViewModel;
            CurrentViewTitle = "Inventory Management";
            CurrentViewSubtitle = "Track stock levels and manage items.";
            await _itemsManagementViewModel.InitializeAsync();
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task NavigateToPartiesAsync()
        {
            await LoadActiveUsersAsync(); // Refresh user dropdown
            CurrentView = _partiesManagementViewModel;
            CurrentViewTitle = "Contact Management";
            CurrentViewSubtitle = "Manage suppliers, customers, and partners.";
            await _partiesManagementViewModel.InitializeAsync();
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task NavigateToFinancialTransactionsAsync()
        {
            await LoadActiveUsersAsync(); // Refresh user dropdown
            CurrentView = _financialTransactionsViewModel;
            CurrentViewTitle = "Financial Records";
            CurrentViewSubtitle = "Review and reconcile financial transactions.";
            // Load data is called in constructor, but we can optionally refresh here
            await System.Threading.Tasks.Task.CompletedTask;
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task NavigateToWagesAsync()
        {
            await LoadActiveUsersAsync(); // Refresh user dropdown
            CurrentView = _wagesManagementViewModel;
            CurrentViewTitle = "Payroll Management";
            CurrentViewSubtitle = "Record wages and manage payments.";
            await _wagesManagementViewModel.InitializeAsync();
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task NavigateToWorkersAsync()
        {
            await LoadActiveUsersAsync(); // Refresh user dropdown
            CurrentView = _workersManagementViewModel;
            CurrentViewTitle = "Workers Management";
            CurrentViewSubtitle = "Manage worker profiles and activities.";
            await _workersManagementViewModel.InitializeAsync();
        }

        [RelayCommand]
        private void NavigateToBackup()
        {
            CurrentView = _backupViewModel;
            CurrentViewTitle = "Backup & Restore";
            CurrentViewSubtitle = "Securely back up and restore your data.";
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task NavigateToUsersAsync()
        {
            CurrentView = _usersViewModel;
            CurrentViewTitle = "User Management";
            CurrentViewSubtitle = "Manage users, roles, and permissions.";
            _usersViewModel.UserListChangedCallback = LoadActiveUsersAsync;
            await _usersViewModel.InitializeAsync();
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task NavigateToOperationalExpensesAsync()
        {
            await LoadActiveUsersAsync(); // Refresh user dropdown
            CurrentView = _operationalExpensesViewModel;
            CurrentViewTitle = "Operational Expenses";
            CurrentViewSubtitle = "Track fuel, repairs, utilities, and other operational costs.";
            _operationalExpensesViewModel.CurrentUserId = SelectedUser?.UserId ?? 1;
            await _operationalExpensesViewModel.InitializeAsync();
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task NavigateToExpenseCategoriesAsync()
        {
            await LoadActiveUsersAsync(); // Refresh user dropdown
            CurrentView = _expenseCategoryManagementViewModel;
            CurrentViewTitle = "Expense Categories";
            CurrentViewSubtitle = "Manage operational expense categories.";
            _expenseCategoryManagementViewModel.CurrentUserId = SelectedUser?.UserId ?? 1;
            await _expenseCategoryManagementViewModel.InitializeAsync();
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

            // Load active users for dropdown
            await LoadActiveUsersAsync();
        }

        public async Task LoadActiveUsersAsync()
        {
            // Remember the currently selected user ID before clearing
            var currentUserId = SelectedUser?.UserId;
            
            var users = await _userService.GetActiveUsersAsync();
            ActiveUsers.Clear();
            foreach (var user in users)
            {
                ActiveUsers.Add(user);
            }

            // Try to restore the previously selected user by ID
            if (currentUserId.HasValue)
            {
                var previousUser = ActiveUsers.FirstOrDefault(u => u.UserId == currentUserId.Value);
                if (previousUser != null)
                {
                    SelectedUser = previousUser;
                    return;
                }
            }

            // Only set first user as selected if no user was previously selected
            if (SelectedUser == null && ActiveUsers.Any())
            {
                SelectedUser = ActiveUsers.First();
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

        partial void OnSelectedUserChanged(User? value)
        {
            OnPropertyChanged(nameof(IsAdminMode));
            OnPropertyChanged(nameof(CurrentUser));
        }
    }
}

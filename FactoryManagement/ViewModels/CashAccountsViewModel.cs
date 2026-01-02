using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FactoryManagement.Models;
using FactoryManagement.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FactoryManagement.ViewModels
{
    public partial class CashAccountsViewModel : PaginationViewModel
    {
        private readonly ICashAccountService _cashAccountService;
        private readonly int _currentUserId;

        [ObservableProperty]
        private ObservableCollection<CashAccount> _accounts = new();

        [ObservableProperty]
        private ObservableCollection<CashAccount> _paginatedAccounts = new();

        [ObservableProperty]
        private ObservableCollection<BalanceHistory> _balanceHistory = new();

        [ObservableProperty]
        private CashAccount? _selectedAccount;

        [ObservableProperty]
        private bool _isEditing;

        [ObservableProperty]
        private string _accountName = string.Empty;

        [ObservableProperty]
        private AccountType _accountType = AccountType.Cash;

        [ObservableProperty]
        private decimal _openingBalance = 0;

        [ObservableProperty]
        private decimal _currentBalance = 0;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private bool _isActive = true;

        [ObservableProperty]
        private decimal _totalCashBalance = 0;

        [ObservableProperty]
        private decimal _totalBankBalance = 0;

        [ObservableProperty]
        private decimal _totalCombinedBalance = 0;

        // Admin check property - observable so it updates when user changes
        [ObservableProperty]
        private bool _isAdmin;

        private int? _editingAccountId;

        public CashAccountsViewModel(ICashAccountService cashAccountService)
        {
            _cashAccountService = cashAccountService;
            // In a real app, get from logged-in user context
            _currentUserId = 1;

            // Initialize IsAdmin based on current user
            UpdateIsAdminStatus();

            // Subscribe to MainWindowViewModel user changes
            if (MainWindowViewModel.Instance != null)
            {
                MainWindowViewModel.Instance.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(MainWindowViewModel.SelectedUser))
                    {
                        UpdateIsAdminStatus();
                    }
                };
            }
        }

        private void UpdateIsAdminStatus()
        {
            IsAdmin = MainWindowViewModel.Instance?.CurrentUser?.Role == "Admin";
        }

        public async Task InitializeAsync()
        {
            await LoadAccountsAsync();
            await LoadBalanceSummaryAsync();
        }

        protected override void UpdatePaginatedData()
        {
            CalculatePagination(Accounts);
            PaginatedAccounts.Clear();
            foreach (var account in GetPagedItems(Accounts))
            {
                PaginatedAccounts.Add(account);
            }
        }

        [RelayCommand]
        private async Task LoadAccountsAsync()
        {
            try
            {
                IsBusy = true;
                var accounts = await _cashAccountService.GetAllActiveAccountsAsync();

                Accounts.Clear();
                foreach (var account in accounts.OrderBy(a => a.AccountName))
                {
                    Accounts.Add(account);
                }
                UpdatePaginatedData();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading accounts: {ex.Message}";
                MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task LoadBalanceSummaryAsync()
        {
            try
            {
                var summary = await _cashAccountService.GetAccountSummaryAsync();

                // Extract balances from summary
                TotalCashBalance = summary.ContainsKey("Cash") ? summary["Cash"] : 0;
                TotalBankBalance = summary.ContainsKey("Bank") ? summary["Bank"] : 0;
                TotalCombinedBalance = summary.ContainsKey("Total") ? summary["Total"] : 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading balance summary: {ex.Message}");
            }
        }

        [RelayCommand]
        private void NewAccount()
        {
            ClearForm();
            IsEditing = false;
            _editingAccountId = null;
        }

        [RelayCommand]
        private void EditAccount(CashAccount? account)
        {
            if (account == null) return;

            _editingAccountId = account.AccountId;
            AccountName = account.AccountName;
            AccountType = account.AccountType;
            OpeningBalance = account.OpeningBalance;
            CurrentBalance = account.CurrentBalance;
            Description = account.Description;
            IsActive = account.IsActive;
            IsEditing = true;
        }

        [RelayCommand]
        private async Task SaveAccountAsync()
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(AccountName))
                {
                    MessageBox.Show("Account name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (OpeningBalance < 0)
                {
                    MessageBox.Show("Opening balance cannot be negative.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                IsBusy = true;

                if (IsEditing && _editingAccountId.HasValue)
                {
                    // Update existing account
                    var account = await _cashAccountService.GetAccountByIdAsync(_editingAccountId.Value);
                    if (account != null)
                    {
                        var oldOpeningBalance = account.OpeningBalance;
                        
                        account.AccountName = AccountName.Trim();
                        account.AccountType = AccountType;
                        account.OpeningBalance = OpeningBalance;
                        account.Description = Description.Trim();
                        account.IsActive = IsActive;
                        
                        // Adjust current balance when opening balance changes
                        // NewCurrentBalance = OldCurrentBalance + (NewOpeningBalance - OldOpeningBalance)
                        var balanceDifference = OpeningBalance - oldOpeningBalance;
                        account.CurrentBalance += balanceDifference;

                        await _cashAccountService.UpdateAccountAsync(account, MainWindowViewModel.Instance?.CurrentUser?.Role);
                        MessageBox.Show("Account updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    // Create new account
                    var account = new CashAccount
                    {
                        AccountName = AccountName.Trim(),
                        AccountType = AccountType,
                        OpeningBalance = OpeningBalance,
                        CurrentBalance = OpeningBalance, // Initially same as opening balance
                        Description = Description.Trim(),
                        IsActive = IsActive,
                        CreatedBy = _currentUserId,
                        CreatedDate = DateTime.Now,
                        OpeningDate = DateTime.Now
                    };

                    await _cashAccountService.CreateAccountAsync(account);
                    MessageBox.Show("Account created successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                await LoadAccountsAsync();
                await LoadBalanceSummaryAsync();
                ClearForm();
                
                // Reload and select the updated account if we were editing
                if (_editingAccountId.HasValue)
                {
                    var updatedAccount = Accounts.FirstOrDefault(a => a.AccountId == _editingAccountId.Value);
                    if (updatedAccount != null)
                    {
                        SelectedAccount = updatedAccount;
                        await LoadBalanceHistoryAsync(updatedAccount);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error saving account: {ex.Message}";
                MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task LoadBalanceHistoryAsync(CashAccount? account)
        {
            if (account == null)
            {
                BalanceHistory.Clear();
                return;
            }

            try
            {
                IsBusy = true;
                var history = await _cashAccountService.GetBalanceHistoryAsync(account.AccountId);

                BalanceHistory.Clear();
                foreach (var entry in history)
                {
                    BalanceHistory.Add(entry);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading balance history: {ex.Message}";
                MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void CancelEdit()
        {
            ClearForm();
        }

        private void ClearForm()
        {
            AccountName = string.Empty;
            AccountType = AccountType.Cash;
            OpeningBalance = 0;
            CurrentBalance = 0;
            Description = string.Empty;
            IsActive = true;
            IsEditing = false;
            _editingAccountId = null;
            ErrorMessage = string.Empty;
        }

        partial void OnSelectedAccountChanged(CashAccount? value)
        {
            if (value != null)
            {
                // When account is selected, load its balance history
                LoadBalanceHistoryCommand.Execute(value);
            }
        }

        partial void OnIsEditingChanged(bool value)
        {
        }

        partial void OnIsAdminChanged(bool value)
        {
        }
    }
}

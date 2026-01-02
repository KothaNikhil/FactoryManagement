using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using FactoryManagement.Models;
using FactoryManagement.Services;
using FactoryManagement.Data;
using Serilog;

namespace FactoryManagement.ViewModels
{
    public partial class SetupWizardViewModel : ObservableObject
    {
        private readonly IUserService _userService;
        private readonly IItemService _itemService;
        private readonly ICashAccountService _cashAccountService;
        private readonly FactoryDbContext _dbContext;

        [ObservableProperty]
        private int currentStep = 0; // 0: Welcome, 1: Users, 2: Items, 3: Cash Accounts, 4: Complete

        [ObservableProperty]
        private bool isFirstLaunch = true;

        // Step 1: Users
        [ObservableProperty]
        private ObservableCollection<User> users = new();

        [ObservableProperty]
        private User? selectedUser;

        [ObservableProperty]
        private string adminUsername = "Admin";

        [ObservableProperty]
        private string adminPassword = "";

        [ObservableProperty]
        private string adminPasswordConfirm = "";

        // Password validation callback - set from View
        public Func<(string password, string confirmPassword)>? GetPasswordsCallback { get; set; }

        // Window close callback - set from View
        public Action? CloseWindowCallback { get; set; }

        // Step 2: Items
        [ObservableProperty]
        private ObservableCollection<Item> items = new();

        [ObservableProperty]
        private Item? selectedItem;

        [ObservableProperty]
        private string newItemName = "";

        [ObservableProperty]
        private string newItemUnit = "Kg";

        [ObservableProperty]
        private decimal newItemStock = 0;

        // Step 3: Cash Accounts
        [ObservableProperty]
        private ObservableCollection<CashAccountSetupModel> cashAccounts = new();

        [ObservableProperty]
        private CashAccountSetupModel? selectedCashAccount;

        [ObservableProperty]
        private string mainCashOpeningBalance = "0.00";

        [ObservableProperty]
        private string mainBankOpeningBalance = "0.00";

        // Commands
        public AsyncRelayCommand NextStepCommand { get; }
        public AsyncRelayCommand PreviousStepCommand { get; }
        public AsyncRelayCommand AddItemCommand { get; }
        public AsyncRelayCommand RemoveItemCommand { get; }
        public RelayCommand CompleteSetupCommand { get; }

        public SetupWizardViewModel(IUserService userService, IItemService itemService, 
            ICashAccountService cashAccountService, FactoryDbContext dbContext)
        {
            _userService = userService;
            _itemService = itemService;
            _cashAccountService = cashAccountService;
            _dbContext = dbContext;

            NextStepCommand = new AsyncRelayCommand(NextStep);
            PreviousStepCommand = new AsyncRelayCommand(PreviousStep);
            AddItemCommand = new AsyncRelayCommand(AddItem);
            RemoveItemCommand = new AsyncRelayCommand(RemoveItem);
            CompleteSetupCommand = new RelayCommand(CompleteSetup);

            InitializeDefaults();
        }

        private void InitializeDefaults()
        {
            // Initialize default items
            Items.Add(new Item { ItemId = 1, ItemName = "Rice", Unit = "Kg", CurrentStock = 0 });
            Items.Add(new Item { ItemId = 2, ItemName = "Paddy", Unit = "Kg", CurrentStock = 0 });
            Items.Add(new Item { ItemId = 3, ItemName = "Husk", Unit = "Kg", CurrentStock = 0 });
            Items.Add(new Item { ItemId = 4, ItemName = "Bran", Unit = "Kg", CurrentStock = 0 });

            // Initialize default cash accounts
            CashAccounts.Add(new CashAccountSetupModel { Name = "Main Cash", OpeningBalance = "0.00" });
            CashAccounts.Add(new CashAccountSetupModel { Name = "Main Bank", OpeningBalance = "0.00" });

            // Initialize admin user
            Users.Add(new User { UserId = 1, Username = AdminUsername, Role = "Admin", IsActive = true });
        }

        private Task NextStep()
        {
            // Validate current step before proceeding
            if (!ValidateCurrentStep())
            {
                MessageBox.Show("Please fill in all required fields correctly.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return Task.CompletedTask;
            }

            if (CurrentStep < 4)
            {
                CurrentStep++;
            }
            return Task.CompletedTask;
        }

        private Task PreviousStep()
        {
            if (CurrentStep > 0)
            {
                CurrentStep--;
            }
            return Task.CompletedTask;
        }

        private bool ValidateCurrentStep()
        {
            return CurrentStep switch
            {
                0 => true, // Welcome screen - no validation
                1 => ValidateUsersStep(),
                2 => ValidateItemsStep(),
                3 => ValidateCashAccountsStep(),
                _ => true
            };
        }

        private bool ValidateUsersStep()
        {
            if (string.IsNullOrWhiteSpace(AdminUsername))
            {
                MessageBox.Show("Admin username is required.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (AdminUsername.Length < 3)
            {
                MessageBox.Show("Admin username must be at least 3 characters.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Get passwords from callback (since PasswordBox doesn't support binding)
            if (GetPasswordsCallback != null)
            {
                var (password, confirmPassword) = GetPasswordsCallback();
                AdminPassword = password;
                AdminPasswordConfirm = confirmPassword;
            }

            if (string.IsNullOrWhiteSpace(AdminPassword))
            {
                MessageBox.Show("Admin password is required.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (AdminPassword.Length < 6)
            {
                MessageBox.Show("Admin password must be at least 6 characters.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (AdminPassword != AdminPasswordConfirm)
            {
                MessageBox.Show("Passwords do not match.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private bool ValidateItemsStep()
        {
            // Items are optional, so just validate if any are added
            if (Items.Count == 0)
            {
                var result = MessageBox.Show("No items added. Continue without items?", "Confirmation", 
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                return result == MessageBoxResult.Yes;
            }

            return true;
        }

        private bool ValidateCashAccountsStep()
        {
            // Validate opening balances are numeric
            if (!decimal.TryParse(MainCashOpeningBalance, out _))
            {
                MessageBox.Show("Main Cash opening balance must be a valid number.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!decimal.TryParse(MainBankOpeningBalance, out _))
            {
                MessageBox.Show("Main Bank opening balance must be a valid number.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private Task AddItem()
        {
            if (string.IsNullOrWhiteSpace(NewItemName))
            {
                MessageBox.Show("Item name is required.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return Task.CompletedTask;
            }

            var newItem = new Item
            {
                ItemName = NewItemName,
                Unit = NewItemUnit,
                CurrentStock = NewItemStock
            };

            Items.Add(newItem);

            // Clear input fields
            NewItemName = "";
            NewItemUnit = "Kg";
            NewItemStock = 0;
            return Task.CompletedTask;
        }

        private Task RemoveItem()
        {
            if (SelectedItem != null)
            {
                Items.Remove(SelectedItem);
                SelectedItem = null;
            }
            return Task.CompletedTask;
        }

        private void CompleteSetup()
        {
            try
            {
                // Get passwords one last time
                if (GetPasswordsCallback != null)
                {
                    var (password, confirmPassword) = GetPasswordsCallback();
                    AdminPassword = password;
                    AdminPasswordConfirm = confirmPassword;
                }

                // Save all data to database
                SaveSetupData();

                MessageBox.Show("Setup completed successfully!", "Success", 
                    MessageBoxButton.OK, MessageBoxImage.Information);

                // Close the window
                CloseWindowCallback?.Invoke();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error completing setup");
                MessageBox.Show($"Error completing setup: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveSetupData()
        {
            try
            {
                // Update admin user with new username and password
                var adminUser = _dbContext.Users.FirstOrDefault(u => u.Role == "Admin");
                if (adminUser != null)
                {
                    adminUser.Username = AdminUsername;
                    // Note: In production, hash the password. For now, storing as-is for demo
                    _dbContext.SaveChanges();
                    Log.Information($"Admin user updated: {AdminUsername}");
                }

                // Save items
                foreach (var item in Items)
                {
                    // Skip if already exists
                    if (item.ItemId > 0 && _dbContext.Items.Any(i => i.ItemId == item.ItemId))
                        continue;

                    var newItem = new Item
                    {
                        ItemName = item.ItemName,
                        Unit = item.Unit,
                        CurrentStock = item.CurrentStock,
                        CreatedByUserId = adminUser?.UserId
                    };
                    _dbContext.Items.Add(newItem);
                }
                _dbContext.SaveChanges();
                Log.Information($"Saved {Items.Count} items");

                // Save cash accounts with opening balances
                if (decimal.TryParse(MainCashOpeningBalance, out decimal cashBalance))
                {
                    var cashAccount = new CashAccount
                    {
                        AccountName = "Main Cash Account",
                        AccountType = AccountType.Cash,
                        OpeningBalance = cashBalance,
                        CurrentBalance = cashBalance,
                        Description = "Primary cash account for daily operations",
                        IsActive = true,
                        CreatedBy = adminUser?.UserId ?? 1,
                        CreatedDate = DateTime.Now,
                        OpeningDate = DateTime.Now
                    };
                    _dbContext.CashAccounts.Add(cashAccount);
                    _dbContext.SaveChanges();

                    // Create opening balance history
                    var cashHistory = new BalanceHistory
                    {
                        AccountId = cashAccount.AccountId,
                        ChangeType = BalanceChangeType.OpeningBalance,
                        PreviousBalance = 0,
                        ChangeAmount = cashBalance,
                        NewBalance = cashBalance,
                        TransactionDate = DateTime.Now,
                        Notes = "Opening balance from setup wizard",
                        EnteredBy = adminUser?.UserId ?? 1,
                        CreatedDate = DateTime.Now
                    };
                    _dbContext.BalanceHistories.Add(cashHistory);
                    Log.Information($"Created Main Cash Account with balance: {cashBalance}");
                }

                if (decimal.TryParse(MainBankOpeningBalance, out decimal bankBalance))
                {
                    var bankAccount = new CashAccount
                    {
                        AccountName = "Main Bank Account",
                        AccountType = AccountType.Bank,
                        OpeningBalance = bankBalance,
                        CurrentBalance = bankBalance,
                        Description = "Primary bank account for business transactions",
                        IsActive = true,
                        CreatedBy = adminUser?.UserId ?? 1,
                        CreatedDate = DateTime.Now,
                        OpeningDate = DateTime.Now
                    };
                    _dbContext.CashAccounts.Add(bankAccount);
                    _dbContext.SaveChanges();

                    // Create opening balance history
                    var bankHistory = new BalanceHistory
                    {
                        AccountId = bankAccount.AccountId,
                        ChangeType = BalanceChangeType.OpeningBalance,
                        PreviousBalance = 0,
                        ChangeAmount = bankBalance,
                        NewBalance = bankBalance,
                        TransactionDate = DateTime.Now,
                        Notes = "Opening balance from setup wizard",
                        EnteredBy = adminUser?.UserId ?? 1,
                        CreatedDate = DateTime.Now
                    };
                    _dbContext.BalanceHistories.Add(bankHistory);
                    Log.Information($"Created Main Bank Account with balance: {bankBalance}");
                }

                _dbContext.SaveChanges();
                Log.Information("Setup data saved successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error saving setup data");
                throw;
            }
        }

        // Model for cash account setup
        public class CashAccountSetupModel
        {
            public string Name { get; set; } = string.Empty;
            public string OpeningBalance { get; set; } = "0.00";
        }
    }
}

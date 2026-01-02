# Cash Balance Management & First-Time Setup Wizard - Design Document

## � Pre-Release Application

**⚠️ IMPORTANT**: This application has **NOT been published yet**. Therefore:
- ✅ **NO backward compatibility required**
- ✅ **NO migration scripts needed**
- ✅ **Database schema can be freely modified**
- ✅ **Breaking changes are allowed**
- ✅ **Data loss is acceptable** (development data only)

This simplifies implementation significantly. All features can be added directly without worrying about existing user data.

---

## �📋 Overview

This document outlines a comprehensive cash balance management system integrated with a first-time setup wizard for the Factory Management application. The design ensures maintainability, scalability, and a seamless user experience.

---

## 🎯 Objectives

1. **Opening Balance Management**: Allow users to set and track the firm's cash/bank balance
2. **Transaction-Based Updates**: Automatically adjust balances based on all financial transactions
3. **First-Time Setup Wizard**: Guide new users through initial system configuration
4. **Multi-Account Support**: Track separate cash and bank balances
5. **Historical Tracking**: Maintain audit trail of all balance changes
6. **Real-time Dashboard**: Display current balance status prominently

---

## 📊 Database Schema Design

### 1. New Model: CashAccount

```csharp
namespace FactoryManagement.Models
{
    public enum AccountType
    {
        Cash = 0,
        Bank = 1,
        Combined = 2  // Virtual account showing total
    }

    public class CashAccount
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AccountId { get; set; }

        [Required]
        [MaxLength(100)]
        public string AccountName { get; set; } = string.Empty;

        [Required]
        public AccountType AccountType { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OpeningBalance { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentBalance { get; set; }

        public DateTime OpeningDate { get; set; } = DateTime.Now;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public int CreatedBy { get; set; }

        [ForeignKey(nameof(CreatedBy))]
        public virtual User? User { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? ModifiedDate { get; set; }
    }
}
```

### 2. New Model: BalanceHistory

```csharp
namespace FactoryManagement.Models
{
    public enum BalanceChangeType
    {
        OpeningBalance = 0,
        Transaction = 1,
        ManualAdjustment = 2,
        Transfer = 3
    }

    public class BalanceHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BalanceHistoryId { get; set; }

        [Required]
        public int AccountId { get; set; }

        [ForeignKey(nameof(AccountId))]
        public virtual CashAccount? Account { get; set; }

        [Required]
        public BalanceChangeType ChangeType { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PreviousBalance { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ChangeAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal NewBalance { get; set; }

        public DateTime TransactionDate { get; set; } = DateTime.Now;

        // Link to related transaction
        public int? TransactionId { get; set; }
        public int? FinancialTransactionId { get; set; }
        public int? WageTransactionId { get; set; }
        public int? OperationalExpenseId { get; set; }

        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;

        public int EnteredBy { get; set; }

        [ForeignKey(nameof(EnteredBy))]
        public virtual User? User { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
```

### 3. Enhanced AppSettings Model

```csharp
namespace FactoryManagement.Models
{
    public class AppSettings
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SettingId { get; set; }

        [Required]
        [MaxLength(100)]
        public string CompanyName { get; set; } = "Factory Management System";

        [Required]
        [MaxLength(10)]
        public string CurrencySymbol { get; set; } = "₹";

        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;

        // NEW: First-time setup tracking
        public bool IsFirstTimeSetup { get; set; } = true;
        public DateTime? SetupCompletedDate { get; set; }
        public int? SetupCompletedBy { get; set; }

        // UI preferences
        public bool IsMenuPinned { get; set; } = true;

        public DateTime? ModifiedDate { get; set; }
    }
}
```

---

## 🏗️ Service Layer Design

### 1. ICashAccountService Interface

```csharp
namespace FactoryManagement.Services
{
    public interface ICashAccountService
    {
        // Account Management
        Task<CashAccount> CreateAccountAsync(CashAccount account);
        Task<CashAccount?> GetAccountByIdAsync(int accountId);
        Task<CashAccount?> GetAccountByTypeAsync(AccountType accountType);
        Task<List<CashAccount>> GetAllActiveAccountsAsync();
        Task<CashAccount> UpdateAccountAsync(CashAccount account);
        
        // Balance Operations
        Task<decimal> GetCurrentBalanceAsync(AccountType accountType);
        Task<decimal> GetTotalBalanceAsync(); // Cash + Bank
        Task UpdateBalanceAsync(int accountId, decimal amount, BalanceChangeType changeType, 
            string notes, int userId, int? relatedTransactionId = null);
        
        // Balance History
        Task<List<BalanceHistory>> GetBalanceHistoryAsync(int accountId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<BalanceHistory> CreateBalanceHistoryAsync(BalanceHistory history);
        
        // Reports
        Task<Dictionary<string, decimal>> GetAccountSummaryAsync();
        Task<List<BalanceHistory>> GetRecentBalanceChangesAsync(int count = 10);
    }
}
```

### 2. CashAccountService Implementation

```csharp
namespace FactoryManagement.Services
{
    public class CashAccountService : ICashAccountService
    {
        private readonly FactoryDbContext _context;

        public CashAccountService(FactoryDbContext context)
        {
            _context = context;
        }

        public async Task<CashAccount> CreateAccountAsync(CashAccount account)
        {
            account.CurrentBalance = account.OpeningBalance;
            account.CreatedDate = DateTime.Now;
            
            _context.CashAccounts.Add(account);
            await _context.SaveChangesAsync();

            // Create opening balance history entry
            var history = new BalanceHistory
            {
                AccountId = account.AccountId,
                ChangeType = BalanceChangeType.OpeningBalance,
                PreviousBalance = 0,
                ChangeAmount = account.OpeningBalance,
                NewBalance = account.OpeningBalance,
                TransactionDate = account.OpeningDate,
                Notes = $"Opening balance for {account.AccountName}",
                EnteredBy = account.CreatedBy,
                CreatedDate = DateTime.Now
            };

            _context.BalanceHistories.Add(history);
            await _context.SaveChangesAsync();

            return account;
        }

        public async Task<decimal> GetCurrentBalanceAsync(AccountType accountType)
        {
            var account = await _context.CashAccounts
                .FirstOrDefaultAsync(a => a.AccountType == accountType && a.IsActive);
            
            return account?.CurrentBalance ?? 0;
        }

        public async Task<decimal> GetTotalBalanceAsync()
        {
            var cashBalance = await GetCurrentBalanceAsync(AccountType.Cash);
            var bankBalance = await GetCurrentBalanceAsync(AccountType.Bank);
            return cashBalance + bankBalance;
        }

        public async Task UpdateBalanceAsync(int accountId, decimal amount, 
            BalanceChangeType changeType, string notes, int userId, 
            int? relatedTransactionId = null)
        {
            var account = await _context.CashAccounts.FindAsync(accountId);
            if (account == null)
                throw new InvalidOperationException($"Account {accountId} not found");

            var previousBalance = account.CurrentBalance;
            account.CurrentBalance += amount;
            account.ModifiedDate = DateTime.Now;

            var history = new BalanceHistory
            {
                AccountId = accountId,
                ChangeType = changeType,
                PreviousBalance = previousBalance,
                ChangeAmount = amount,
                NewBalance = account.CurrentBalance,
                TransactionDate = DateTime.Now,
                Notes = notes,
                EnteredBy = userId,
                CreatedDate = DateTime.Now
            };

            _context.BalanceHistories.Add(history);
            await _context.SaveChangesAsync();
        }

        public async Task<List<BalanceHistory>> GetBalanceHistoryAsync(
            int accountId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.BalanceHistories
                .Include(h => h.Account)
                .Include(h => h.User)
                .Where(h => h.AccountId == accountId);

            if (fromDate.HasValue)
                query = query.Where(h => h.TransactionDate >= fromDate.Value);
            
            if (toDate.HasValue)
                query = query.Where(h => h.TransactionDate <= toDate.Value);

            return await query.OrderByDescending(h => h.TransactionDate).ToListAsync();
        }

        public async Task<Dictionary<string, decimal>> GetAccountSummaryAsync()
        {
            var summary = new Dictionary<string, decimal>();
            
            var accounts = await _context.CashAccounts
                .Where(a => a.IsActive)
                .ToListAsync();

            foreach (var account in accounts)
            {
                summary[account.AccountName] = account.CurrentBalance;
            }

            summary["Total"] = accounts.Sum(a => a.CurrentBalance);
            
            return summary;
        }

        public async Task<List<BalanceHistory>> GetRecentBalanceChangesAsync(int count = 10)
        {
            return await _context.BalanceHistories
                .Include(h => h.Account)
                .Include(h => h.User)
                .OrderByDescending(h => h.TransactionDate)
                .Take(count)
                .ToListAsync();
        }

        // Additional methods as per interface...
    }
}
```

---

## 🔄 Transaction Integration

### Update Existing Services to Track Balance

#### 1. TransactionService Enhancement

```csharp
public async Task<Transaction> CreateTransactionAsync(Transaction transaction)
{
    using var dbTransaction = await _context.Database.BeginTransactionAsync();
    
    try
    {
        // Existing logic for creating transaction
        _context.Transactions.Add(transaction);
        await UpdateItemStockAsync(transaction);
        await _context.SaveChangesAsync();

        // NEW: Update cash/bank balance
        await UpdateBalanceForTransactionAsync(transaction);

        await dbTransaction.CommitAsync();
        return transaction;
    }
    catch
    {
        await dbTransaction.RollbackAsync();
        throw;
    }
}

private async Task UpdateBalanceForTransactionAsync(Transaction transaction)
{
    // Get appropriate account based on payment mode
    var accountType = transaction.PaymentMode == PaymentMode.Cash 
        ? AccountType.Cash 
        : AccountType.Bank;
    
    var account = await _cashAccountService.GetAccountByTypeAsync(accountType);
    if (account == null) return; // No balance tracking if account not set up

    decimal balanceChange = 0;
    string notes = "";

    switch (transaction.TransactionType)
    {
        case TransactionType.Buy:
            balanceChange = -transaction.TotalAmount; // Money out
            notes = $"Purchase from {transaction.PartyName} - {transaction.ItemName}";
            break;
        
        case TransactionType.Sell:
            balanceChange = transaction.TotalAmount; // Money in
            notes = $"Sale to {transaction.PartyName} - {transaction.ItemName}";
            break;
        
        case TransactionType.Processing:
            // Processing doesn't affect balance (internal conversion)
            return;
        
        case TransactionType.Wastage:
            // Wastage doesn't affect cash balance
            return;
    }

    await _cashAccountService.UpdateBalanceAsync(
        account.AccountId,
        balanceChange,
        BalanceChangeType.Transaction,
        notes,
        transaction.EnteredBy,
        transaction.TransactionId
    );
}
```

#### 2. FinancialTransactionService Enhancement

```csharp
private async Task UpdateBalanceForFinancialTransactionAsync(FinancialTransaction transaction)
{
    // Only update balance for Cash/Bank transactions, not Loan
    if (transaction.PaymentMode == PaymentMode.Loan) return;

    var accountType = transaction.PaymentMode == PaymentMode.Cash 
        ? AccountType.Cash 
        : AccountType.Bank;
    
    var account = await _cashAccountService.GetAccountByTypeAsync(accountType);
    if (account == null) return;

    decimal balanceChange = 0;
    string notes = "";

    switch (transaction.TransactionType)
    {
        case FinancialTransactionType.LoanGiven:
            balanceChange = -transaction.Amount; // Money out
            notes = $"Loan given to {transaction.PartyName}";
            break;
        
        case FinancialTransactionType.LoanReceived:
            balanceChange = transaction.Amount; // Money in
            notes = $"Loan received from {transaction.PartyName}";
            break;
        
        case FinancialTransactionType.LoanPayment:
            balanceChange = transaction.Amount; // Money in (repayment received)
            notes = $"Loan repayment from {transaction.PartyName}";
            break;
        
        case FinancialTransactionType.InterestPaid:
            balanceChange = -transaction.Amount; // Money out
            notes = $"Interest paid to {transaction.PartyName}";
            break;
    }

    await _cashAccountService.UpdateBalanceAsync(
        account.AccountId,
        balanceChange,
        BalanceChangeType.Transaction,
        notes,
        transaction.EnteredBy,
        transaction.FinancialTransactionId
    );
}
```

#### 3. WageService Enhancement

```csharp
private async Task UpdateBalanceForWageTransactionAsync(WageTransaction transaction)
{
    var accountType = transaction.PaymentMode == PaymentMode.Cash 
        ? AccountType.Cash 
        : AccountType.Bank;
    
    var account = await _cashAccountService.GetAccountByTypeAsync(accountType);
    if (account == null) return;

    decimal balanceChange = 0;
    string notes = "";
    var worker = await _context.Workers.FindAsync(transaction.WorkerId);
    var workerName = worker?.Name ?? "Unknown";

    switch (transaction.TransactionType)
    {
        case WageTransactionType.DailyWage:
        case WageTransactionType.HourlyWage:
        case WageTransactionType.MonthlyWage:
        case WageTransactionType.OvertimePay:
        case WageTransactionType.Bonus:
        case WageTransactionType.AdvanceGiven:
            balanceChange = -transaction.NetAmount; // Money out
            notes = $"{transaction.TransactionType} for {workerName}";
            break;
        
        case WageTransactionType.AdvanceAdjustment:
        case WageTransactionType.Deduction:
            // These reduce payment, so they increase balance
            balanceChange = transaction.Amount; // Money saved
            notes = $"{transaction.TransactionType} for {workerName}";
            break;
    }

    await _cashAccountService.UpdateBalanceAsync(
        account.AccountId,
        balanceChange,
        BalanceChangeType.Transaction,
        notes,
        transaction.EnteredBy,
        transaction.WageTransactionId
    );
}
```

#### 4. OperationalExpenseService Enhancement

```csharp
private async Task UpdateBalanceForExpenseAsync(OperationalExpense expense)
{
    var accountType = expense.PaymentMode == PaymentMode.Cash 
        ? AccountType.Cash 
        : AccountType.Bank;
    
    var account = await _cashAccountService.GetAccountByTypeAsync(accountType);
    if (account == null) return;

    var category = await _context.ExpenseCategories.FindAsync(expense.ExpenseCategoryId);
    var notes = $"{category?.CategoryName ?? "Expense"}: {expense.Description}";

    await _cashAccountService.UpdateBalanceAsync(
        account.AccountId,
        -expense.Amount, // Money out
        BalanceChangeType.Transaction,
        notes,
        expense.SpentBy,
        expense.OperationalExpenseId
    );
}
```

---

## 🚀 First-Time Setup Wizard

### Setup Wizard Flow

```
┌─────────────────────────────────────────┐
│  1. Welcome Screen                      │
│     - Company name                      │
│     - Logo/branding                     │
└────────────┬────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────┐
│  2. Cash Account Setup                  │
│     - Opening cash balance              │
│     - Opening bank balance              │
│     - Opening date                      │
└────────────┬────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────┐
│  3. Users Setup (Optional)              │
│     - Add additional users              │
│     - Default: Admin already created    │
└────────────┬────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────┐
│  4. Items Setup (Optional)              │
│     - Add initial inventory items       │
│     - Set initial stock levels          │
└────────────┬────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────┐
│  5. Parties/Contacts Setup (Optional)   │
│     - Add suppliers                     │
│     - Add customers                     │
└────────────┬────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────┐
│  6. Workers Setup (Optional)            │
│     - Add workers/employees             │
│     - Set wage rates                    │
└────────────┬────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────┐
│  7. Summary & Completion                │
│     - Review all entries                │
│     - Confirm and save                  │
└─────────────────────────────────────────┘
```

### ViewModel: SetupWizardViewModel

```csharp
namespace FactoryManagement.ViewModels
{
    public class SetupWizardViewModel : ViewModelBase
    {
        private readonly ICashAccountService _cashAccountService;
        private readonly IUserService _userService;
        private readonly IItemService _itemService;
        private readonly IPartyService _partyService;
        private readonly IWorkerService _workerService;

        private int _currentStep = 1;
        private const int TotalSteps = 7;

        // Step 1: Company Info
        public string CompanyName { get; set; } = "";
        public string CompanyAddress { get; set; } = "";

        // Step 2: Cash Account Setup (Required)
        public decimal OpeningCashBalance { get; set; }
        public decimal OpeningBankBalance { get; set; }
        public DateTime OpeningDate { get; set; } = DateTime.Now;

        // Step 3: Users
        public ObservableCollection<User> Users { get; } = new();
        
        // Step 4: Items
        public ObservableCollection<Item> Items { get; } = new();
        
        // Step 5: Parties
        public ObservableCollection<Party> Parties { get; } = new();
        
        // Step 6: Workers
        public ObservableCollection<Worker> Workers { get; } = new();

        public int CurrentStep
        {
            get => _currentStep;
            set
            {
                _currentStep = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ProgressPercentage));
                OnPropertyChanged(nameof(CanGoBack));
                OnPropertyChanged(nameof(CanGoNext));
                OnPropertyChanged(nameof(IsLastStep));
            }
        }

        public int ProgressPercentage => (CurrentStep * 100) / TotalSteps;
        public bool CanGoBack => CurrentStep > 1;
        public bool CanGoNext => CurrentStep < TotalSteps && ValidateCurrentStep();
        public bool IsLastStep => CurrentStep == TotalSteps;

        public ICommand NextCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand FinishCommand { get; }
        public ICommand SkipStepCommand { get; }

        public SetupWizardViewModel(
            ICashAccountService cashAccountService,
            IUserService userService,
            IItemService itemService,
            IPartyService partyService,
            IWorkerService workerService)
        {
            _cashAccountService = cashAccountService;
            _userService = userService;
            _itemService = itemService;
            _partyService = partyService;
            _workerService = workerService;

            NextCommand = new RelayCommand(NextStep, () => CanGoNext);
            BackCommand = new RelayCommand(PreviousStep, () => CanGoBack);
            FinishCommand = new RelayCommand(async () => await FinishSetupAsync(), () => IsLastStep);
            SkipStepCommand = new RelayCommand(SkipCurrentStep);
        }

        private bool ValidateCurrentStep()
        {
            switch (CurrentStep)
            {
                case 1: // Company info
                    return !string.IsNullOrWhiteSpace(CompanyName);
                
                case 2: // Cash account (required)
                    return OpeningCashBalance >= 0 && OpeningBankBalance >= 0;
                
                case 3:
                case 4:
                case 5:
                case 6:
                    return true; // Optional steps
                
                case 7: // Summary
                    return true;
                
                default:
                    return false;
            }
        }

        private void NextStep()
        {
            if (CurrentStep < TotalSteps)
                CurrentStep++;
        }

        private void PreviousStep()
        {
            if (CurrentStep > 1)
                CurrentStep--;
        }

        private void SkipCurrentStep()
        {
            NextStep();
        }

        private async Task FinishSetupAsync()
        {
            try
            {
                // 1. Update company settings
                var settings = await GetOrCreateSettingsAsync();
                settings.CompanyName = CompanyName;
                settings.Address = CompanyAddress;
                settings.IsFirstTimeSetup = false;
                settings.SetupCompletedDate = DateTime.Now;
                settings.ModifiedDate = DateTime.Now;
                await SaveSettingsAsync(settings);

                // 2. Create cash accounts
                var cashAccount = new CashAccount
                {
                    AccountName = "Cash Account",
                    AccountType = AccountType.Cash,
                    OpeningBalance = OpeningCashBalance,
                    OpeningDate = OpeningDate,
                    CreatedBy = 1, // Admin user
                    IsActive = true
                };
                await _cashAccountService.CreateAccountAsync(cashAccount);

                var bankAccount = new CashAccount
                {
                    AccountName = "Bank Account",
                    AccountType = AccountType.Bank,
                    OpeningBalance = OpeningBankBalance,
                    OpeningDate = OpeningDate,
                    CreatedBy = 1,
                    IsActive = true
                };
                await _cashAccountService.CreateAccountAsync(bankAccount);

                // 3. Save users
                foreach (var user in Users)
                {
                    await _userService.CreateUserAsync(user);
                }

                // 4. Save items
                foreach (var item in Items)
                {
                    await _itemService.CreateItemAsync(item);
                }

                // 5. Save parties
                foreach (var party in Parties)
                {
                    await _partyService.CreatePartyAsync(party);
                }

                // 6. Save workers
                foreach (var worker in Workers)
                {
                    await _workerService.CreateWorkerAsync(worker);
                }

                MessageBox.Show("Setup completed successfully!", "Success", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Close wizard and open main application
                OnSetupCompleted?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error completing setup: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event Action? OnSetupCompleted;
    }
}
```

---

## 🎨 UI Design

### Dashboard Balance Widget

```xaml
<!-- Add to DashboardView.xaml -->
<materialDesign:Card Grid.Row="0" Grid.Column="0" Margin="10" 
                     Background="{DynamicResource PrimaryHueDarkBrush}">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>
        
        <!-- Header -->
        <StackPanel Grid.Row="0" Margin="15,15,15,10">
            <TextBlock Text="💰 Current Balance" 
                       FontSize="16" FontWeight="SemiBold" 
                       Foreground="White"/>
        </StackPanel>
        
        <!-- Balance Display -->
        <Grid Grid.Row="1" Margin="15,0,15,15">
            <Grid.RowDefinitions>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="Auto"/>
            </Grid.RowDefinitions>
            
            <!-- Total Balance -->
            <StackPanel Grid.Row="0" Orientation="Horizontal">
                <TextBlock Text="Total:" FontSize="14" 
                           Foreground="White" Opacity="0.8" 
                           VerticalAlignment="Center"/>
                <TextBlock Text="{Binding TotalBalance, StringFormat='₹{0:N2}'}" 
                           FontSize="28" FontWeight="Bold" 
                           Foreground="White" Margin="10,0,0,0"/>
            </StackPanel>
            
            <!-- Cash Balance -->
            <StackPanel Grid.Row="1" Orientation="Horizontal" Margin="0,10,0,0">
                <materialDesign:PackIcon Kind="Cash" 
                                         Width="20" Height="20" 
                                         Foreground="White" 
                                         VerticalAlignment="Center"/>
                <TextBlock Text="Cash:" FontSize="12" 
                           Foreground="White" Opacity="0.8" 
                           Margin="5,0" VerticalAlignment="Center"/>
                <TextBlock Text="{Binding CashBalance, StringFormat='₹{0:N2}'}" 
                           FontSize="16" FontWeight="SemiBold" 
                           Foreground="White"/>
            </StackPanel>
            
            <!-- Bank Balance -->
            <StackPanel Grid.Row="2" Orientation="Horizontal" Margin="0,5,0,0">
                <materialDesign:PackIcon Kind="Bank" 
                                         Width="20" Height="20" 
                                         Foreground="White" 
                                         VerticalAlignment="Center"/>
                <TextBlock Text="Bank:" FontSize="12" 
                           Foreground="White" Opacity="0.8" 
                           Margin="5,0" VerticalAlignment="Center"/>
                <TextBlock Text="{Binding BankBalance, StringFormat='₹{0:N2}'}" 
                           FontSize="16" FontWeight="SemiBold" 
                           Foreground="White"/>
            </StackPanel>
            
            <!-- View Details Button -->
            <Button Grid.Row="3" Content="View Details" 
                    Margin="0,10,0,0" 
                    Style="{StaticResource MaterialDesignOutlineButton}"
                    Foreground="White" BorderBrush="White"
                    Command="{Binding ViewBalanceDetailsCommand}"/>
        </Grid>
    </Grid>
</materialDesign:Card>
```

### Setup Wizard Window

```xaml
<Window x:Class="FactoryManagement.Views.SetupWizardWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:materialDesign="http://materialdesigninxaml.net/winfx/xaml/themes"
        Title="Setup Wizard" Height="600" Width="900"
        WindowStartupLocation="CenterScreen"
        Background="{DynamicResource MaterialDesignPaper}">
    
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>
        
        <!-- Progress Header -->
        <Grid Grid.Row="0" Background="{DynamicResource PrimaryHueDarkBrush}">
            <Grid.RowDefinitions>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="Auto"/>
            </Grid.RowDefinitions>
            
            <TextBlock Grid.Row="0" 
                       Text="{Binding StepTitle}" 
                       FontSize="24" FontWeight="Bold" 
                       Foreground="White" 
                       Margin="30,20,30,10"/>
            
            <ProgressBar Grid.Row="1" 
                         Value="{Binding ProgressPercentage}" 
                         Minimum="0" Maximum="100" 
                         Height="6" Margin="30,0,30,20"/>
        </Grid>
        
        <!-- Step Content -->
        <ContentControl Grid.Row="1" 
                        Content="{Binding CurrentStepView}" 
                        Margin="30"/>
        
        <!-- Navigation Buttons -->
        <Grid Grid.Row="2" Margin="30,0,30,20">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="Auto"/>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="Auto"/>
                <ColumnDefinition Width="Auto"/>
            </Grid.ColumnDefinitions>
            
            <Button Grid.Column="0" Content="← Back" 
                    Command="{Binding BackCommand}" 
                    Style="{StaticResource MaterialDesignOutlineButton}"
                    Visibility="{Binding CanGoBack, Converter={StaticResource BooleanToVisibilityConverter}}"/>
            
            <Button Grid.Column="2" Content="Skip →" 
                    Command="{Binding SkipStepCommand}" 
                    Style="{StaticResource MaterialDesignOutlineButton}"
                    Margin="0,0,10,0"
                    Visibility="{Binding CanSkip, Converter={StaticResource BooleanToVisibilityConverter}}"/>
            
            <Button Grid.Column="3" 
                    Content="{Binding NextButtonText}" 
                    Command="{Binding NextCommand}" 
                    Style="{StaticResource MaterialDesignRaisedButton}"
                    Visibility="{Binding IsNotLastStep, Converter={StaticResource BooleanToVisibilityConverter}}"/>
            
            <Button Grid.Column="3" Content="Finish Setup" 
                    Command="{Binding FinishCommand}" 
                    Style="{StaticResource MaterialDesignRaisedAccentButton}"
                    Visibility="{Binding IsLastStep, Converter={StaticResource BooleanToVisibilityConverter}}"/>
        </Grid>
    </Grid>
</Window>
```

---

## 📝 Implementation Checklist

### ⚡ Pre-Release Simplification
**No backward compatibility required**
- ✅ Direct database schema additions (no migration complexity)
- ✅ Fresh installation only (no data transformation)
- ✅ Breaking changes are acceptable
- ✅ Database reset allowed anytime
- ✅ Timeline reduced by ~40%

---

### Phase 1: Database & Models (1 day)
- [ ] Create `CashAccount` model
- [ ] Create `BalanceHistory` model  
- [ ] Update `AppSettings` model with setup flags
- [ ] Create EF migration: `Add-Migration AddCashBalanceManagement`
- [ ] Update `FactoryDbContext` with new DbSets and relationships
- [ ] Test migration applies cleanly: `Update-Database`
- [ ] **No data transformation scripts needed**
- [ ] **No compatibility layer required**

### Phase 2: Service Layer (2 days)
- [ ] Create `ICashAccountService` interface
- [ ] Implement `CashAccountService` (clean implementation)
- [ ] Update `TransactionService` for balance tracking
- [ ] Update `FinancialTransactionService` for balance tracking
- [ ] Update `WageService` for balance tracking
- [ ] Update `OperationalExpenseService` for balance tracking
- [ ] Write unit tests for all services
- [ ] **No legacy code paths needed**

### Phase 3: Setup Wizard (2-3 days)
- [ ] Create `SetupWizardViewModel`
- [ ] Create `SetupWizardWindow` XAML
- [ ] Create individual step views (7 views)
- [ ] Implement navigation logic
- [ ] Implement validation
- [ ] Implement data persistence
- [ ] Add to `App.xaml.cs` startup logic
- [ ] **No support for existing data**

### Phase 4: Dashboard Integration (1 day)
- [ ] Update `DashboardViewModel` with balance properties
- [ ] Add balance widget to `DashboardView`
- [ ] Create balance details view/dialog
- [ ] Add balance change notifications
- [ ] Implement auto-refresh on transactions

### Phase 5: Reports & History (1 day)
- [ ] Create balance history view
- [ ] Add balance history report
- [ ] Create cash flow report
- [ ] Add export functionality
- [ ] Create account reconciliation view
- [ ] **No legacy report support needed**

### Phase 6: Testing & Documentation (1-2 days)
- [ ] Integration tests
- [ ] E2E tests for setup wizard
- [ ] E2E tests for balance tracking
- [ ] User documentation
- [ ] Admin guide
- [ ] **No upgrade testing needed**

### Estimated Total Timeline
- **Phase 1-6**: 8-11 days (vs 16-23 days with backward compatibility)
- **Time saved**: ~40-50% reduction
- **Key advantage**: No migration complexity, no legacy code

---

## 🔐 Security Considerations

1. **Access Control**: Only authorized users can modify opening balances
2. **Audit Trail**: All balance changes logged with user and timestamp
3. **Data Validation**: Prevent negative balances (optional setting)
4. **Backup**: Regular backups before balance adjustments
5. **Reconciliation**: Monthly balance reconciliation reports

---

## 📊 Benefits

### For Users:
✅ Clear visibility of current financial position  
✅ Automatic balance tracking reduces manual work  
✅ Historical audit trail for accountability  
✅ Easy first-time setup experience  
✅ Separate cash/bank tracking  

### For Business:
✅ Accurate financial reporting  
✅ Better cash flow management  
✅ Reduced errors from manual tracking  
✅ Complete transaction history  
✅ Scalable for future enhancements  

---

## 🚀 Future Enhancements

1. **Multiple Bank Accounts**: Support for multiple bank accounts
2. **Cash Transfer**: Transfer between cash and bank
3. **Reconciliation**: Bank statement reconciliation
4. **Forecasting**: Cash flow forecasting
5. **Alerts**: Low balance alerts
6. **Mobile App**: Mobile companion app
7. **Multi-Currency**: Support for multiple currencies
8. **Integration**: Bank API integration for auto-sync

---

## 📋 Release Checklist

### Before First Publication
- [ ] All features implemented and tested
- [ ] Documentation complete
- [ ] User training materials ready
- [ ] Database migration tested on fresh install
- [ ] No legacy code branches

### After First Publication (Future)
- [ ] Implement backward compatibility strategy
- [ ] Create migration scripts for next version
- [ ] Plan upgrade procedures
- [ ] Document API contracts
- [ ] Version management system

---

## 📚 Related Documents

- [USER_GUIDE.md](USER_GUIDE.md) - User instructions
- [FINANCIAL_RECORDS_GUIDE.md](FINANCIAL_RECORDS_GUIDE.md) - Financial features
- [QUICK_REFERENCE.md](QUICK_REFERENCE.md) - Quick reference
- [PROJECT_SUMMARY.md](PROJECT_SUMMARY.md) - Project overview

---

**Document Version**: 1.0  
**Last Updated**: January 2, 2026  
**Author**: Development Team

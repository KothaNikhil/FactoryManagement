# Setup Data Edit & Maintenance Guide

## 🔴 Pre-Release Application

**⚠️ IMPORTANT**: This application has **NOT been published yet**. Therefore:
- ✅ **NO backward compatibility required**
- ✅ **NO migration scripts needed**
- ✅ **Database schema can be freely modified**
- ✅ **Breaking changes are allowed**
- ✅ **Data loss is acceptable** (development/test data only)

All editing and maintenance features can be implemented without worrying about existing user databases.

---

## 🎯 Overview

After the initial setup wizard, users need to be able to:
- ✅ Edit opening balances
- ✅ Update company information
- ✅ Manage users, items, parties, workers
- ✅ Fix incorrect data
- ✅ Track changes via audit trail

This guide outlines the complete approach for maintaining all setup data.

---

## 📊 Access Points for Editing

### Main Entry Points (UI Navigation)

```
┌─────────────────────────────────────────────┐
│          APPLICATION MAIN MENU              │
├─────────────────────────────────────────────┤
│                                             │
│  Dashboard          [Balance Widget]        │
│  Transactions       [Buy/Sell/Processing]   │
│  Financial         [Loans/Payments]         │
│  Wages              [Worker Wages]          │
│  Expenses           [Operational Expenses]  │
│  Reports            [All Reports]           │
│  Settings           ← EDIT OPTIONS HERE     │
│  ├─ Company Info                            │
│  ├─ Cash Accounts                           │
│  ├─ Users                                   │
│  ├─ Items                                   │
│  ├─ Parties                                 │
│  ├─ Workers                                 │
│  └─ Backup & Restore                        │
│                                             │
└─────────────────────────────────────────────┘
```

---

## 🏢 1. Company Information Editor

### Access Path
`Settings → Company Info`

### Edit Dialog

```xaml
<Window x:Class="FactoryManagement.Views.CompanyInfoWindow"
        Title="Company Settings"
        Height="400" Width="600"
        Background="{DynamicResource MaterialDesignPaper}">
    
    <StackPanel Margin="30">
        
        <!-- Company Details Section -->
        <TextBlock Text="Company Information" 
                   FontSize="18" FontWeight="Bold" Margin="0,0,0,20"/>
        
        <Grid>
            <Grid.RowDefinitions>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="*"/>
                <RowDefinition Height="Auto"/>
            </Grid.RowDefinitions>
            
            <!-- Company Name -->
            <Label Grid.Row="0" Content="Company Name" FontSize="12"/>
            <TextBox Grid.Row="1" Text="{Binding CompanyName}" 
                     Margin="0,0,0,15" Height="40"
                     materialDesign:HintAssist.Hint="Enter company name"
                     Style="{StaticResource MaterialDesignOutlinedTextBox}"/>
            
            <!-- Address -->
            <Label Grid.Row="2" Content="Address" FontSize="12"/>
            <TextBox Grid.Row="3" Text="{Binding Address}" 
                     Margin="0,0,0,15" Height="60"
                     AcceptsReturn="True" TextWrapping="Wrap"
                     materialDesign:HintAssist.Hint="Enter company address"
                     Style="{StaticResource MaterialDesignOutlinedTextBox}"/>
            
            <!-- Buttons -->
            <StackPanel Grid.Row="5" Orientation="Horizontal" 
                        HorizontalAlignment="Right">
                <Button Content="Cancel" 
                        Style="{StaticResource MaterialDesignOutlineButton}"
                        Margin="0,0,10,0"
                        Command="{Binding CancelCommand}"/>
                <Button Content="Save Changes" 
                        Style="{StaticResource MaterialDesignRaisedButton}"
                        Command="{Binding SaveCommand}"/>
            </StackPanel>
        </Grid>
        
        <!-- Last Modified Info -->
        <StackPanel Margin="0,30,0,0" Opacity="0.6">
            <TextBlock FontSize="11">
                <Run Text="Last modified by:"/>
                <Run Text="{Binding LastModifiedBy}"/>
            </TextBlock>
            <TextBlock FontSize="11" Text="{Binding LastModifiedDate, StringFormat='On: {0:dd/MM/yyyy HH:mm}'}"/>
        </StackPanel>
    </StackPanel>
</Window>
```

### ViewModel

```csharp
public class CompanyInfoViewModel : ViewModelBase
{
    private readonly IAppSettingsService _settingsService;
    private readonly IUserService _userService;
    
    private string _companyName = "";
    private string _address = "";
    private string _lastModifiedBy = "";
    private DateTime? _lastModifiedDate;

    public string CompanyName
    {
        get => _companyName;
        set
        {
            _companyName = value;
            OnPropertyChanged();
        }
    }

    public string Address
    {
        get => _address;
        set
        {
            _address = value;
            OnPropertyChanged();
        }
    }

    public string LastModifiedBy
    {
        get => _lastModifiedBy;
        set
        {
            _lastModifiedBy = value;
            OnPropertyChanged();
        }
    }

    public DateTime? LastModifiedDate
    {
        get => _lastModifiedDate;
        set
        {
            _lastModifiedDate = value;
            OnPropertyChanged();
        }
    }

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public CompanyInfoViewModel(IAppSettingsService settingsService, 
                                IUserService userService)
    {
        _settingsService = settingsService;
        _userService = userService;

        SaveCommand = new RelayCommand(async () => await SaveAsync());
        CancelCommand = new RelayCommand(() => Application.Current.Windows[0].Close());

        LoadDataAsync().GetAwaiter().GetResult();
    }

    private async Task LoadDataAsync()
    {
        var settings = await _settingsService.GetAppSettingsAsync();
        if (settings != null)
        {
            CompanyName = settings.CompanyName;
            Address = settings.Address;
            LastModifiedDate = settings.ModifiedDate;
            
            if (settings.ModifiedDate.HasValue)
            {
                LastModifiedBy = await _settingsService.GetModifiedByUserNameAsync();
            }
        }
    }

    private async Task SaveAsync()
    {
        try
        {
            var settings = await _settingsService.GetAppSettingsAsync();
            if (settings != null)
            {
                settings.CompanyName = CompanyName;
                settings.Address = Address;
                settings.ModifiedDate = DateTime.Now;

                await _settingsService.UpdateAppSettingsAsync(settings);
                
                MessageBox.Show("Company information updated successfully!", 
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                
                Application.Current.MainWindow.Close();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving changes: {ex.Message}", 
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
```

---

## 💰 2. Cash Account Editor

### Access Path
`Settings → Cash Accounts`

### Edit View (List + Edit Dialog)

```xaml
<Window x:Class="FactoryManagement.Views.CashAccountsWindow"
        Title="Manage Cash Accounts"
        Height="600" Width="900"
        Background="{DynamicResource MaterialDesignPaper}">
    
    <Grid Margin="20">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Header -->
        <StackPanel Grid.Row="0" Margin="0,0,0,20">
            <TextBlock Text="Cash Accounts Management" 
                       FontSize="18" FontWeight="Bold"/>
            <TextBlock Text="Edit opening balances and account details" 
                       Foreground="{DynamicResource MaterialDesignBodyLight}" 
                       Margin="0,5,0,0"/>
        </StackPanel>

        <!-- Accounts List -->
        <DataGrid Grid.Row="1" ItemsSource="{Binding Accounts}"
                  SelectedItem="{Binding SelectedAccount}"
                  AutoGenerateColumns="False"
                  CanUserAddRows="False"
                  Style="{StaticResource MaterialDesignDataGrid}">
            
            <DataGrid.Columns>
                <DataGridTextColumn Header="Account Name" 
                                    Binding="{Binding AccountName}" Width="200"/>
                <DataGridTextColumn Header="Type" 
                                    Binding="{Binding AccountTypeName}" Width="120"/>
                <DataGridTextColumn Header="Opening Balance" 
                                    Binding="{Binding OpeningBalance, StringFormat='₹{0:N2}'}" 
                                    Width="150"/>
                <DataGridTextColumn Header="Current Balance" 
                                    Binding="{Binding CurrentBalance, StringFormat='₹{0:N2}'}" 
                                    Width="150"/>
                <DataGridTextColumn Header="Status" 
                                    Binding="{Binding Status}" Width="100"/>
                
                <DataGridTemplateColumn Header="Actions" Width="100">
                    <DataGridTemplateColumn.CellTemplate>
                        <DataTemplate>
                            <StackPanel Orientation="Horizontal" Spacing="5">
                                <Button Content="Edit" 
                                        Command="{Binding DataContext.EditCommand, 
                                                RelativeSource={RelativeSource AncestorType=Window}}"
                                        CommandParameter="{Binding}"
                                        Style="{StaticResource MaterialDesignIconButton}"/>
                                <Button Content="View History" 
                                        Command="{Binding DataContext.ViewHistoryCommand, 
                                                RelativeSource={RelativeSource AncestorType=Window}}"
                                        CommandParameter="{Binding}"
                                        Style="{StaticResource MaterialDesignIconButton}"/>
                            </StackPanel>
                        </DataTemplate>
                    </DataGridTemplateColumn.CellTemplate>
                </DataGridTemplateColumn>
            </DataGrid.Columns>
        </DataGrid>

        <!-- Edit Panel -->
        <Grid Grid.Row="2" Margin="0,20,0,0">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="Auto"/>
                <ColumnDefinition Width="Auto"/>
            </Grid.ColumnDefinitions>

            <StackPanel Grid.Column="0" 
                       Visibility="{Binding IsEditingAccount, Converter={StaticResource BooleanToVisibilityConverter}}">
                
                <TextBlock Text="Edit Account" FontSize="14" FontWeight="SemiBold" Margin="0,0,0,10"/>
                
                <Grid>
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="200"/>
                        <ColumnDefinition Width="200"/>
                        <ColumnDefinition Width="200"/>
                    </Grid.ColumnDefinitions>

                    <!-- Opening Balance -->
                    <StackPanel Grid.Column="0" Margin="0,0,20,0">
                        <TextBlock Text="Opening Balance" FontSize="12"/>
                        <TextBox Text="{Binding EditingOpeningBalance, UpdateSourceTrigger=PropertyChanged}"
                                 Height="40" Margin="0,5,0,0"
                                 Style="{StaticResource MaterialDesignOutlinedTextBox}"/>
                    </StackPanel>

                    <!-- Current Balance (Read-only) -->
                    <StackPanel Grid.Column="1" Margin="0,0,20,0">
                        <TextBlock Text="Current Balance (Read-only)" FontSize="12"/>
                        <TextBox Text="{Binding SelectedAccount.CurrentBalance, StringFormat='₹{0:N2}', Mode=OneWay}"
                                 Height="40" Margin="0,5,0,0"
                                 IsReadOnly="True"
                                 Style="{StaticResource MaterialDesignOutlinedTextBox}"/>
                    </StackPanel>

                    <!-- Active Status -->
                    <StackPanel Grid.Column="2">
                        <TextBlock Text="Status" FontSize="12"/>
                        <CheckBox Content="Active" Margin="0,5,0,0"
                                  IsChecked="{Binding EditingIsActive}"/>
                    </StackPanel>
                </Grid>
            </StackPanel>

            <!-- Action Buttons -->
            <StackPanel Grid.Column="1" Orientation="Horizontal" VerticalAlignment="Bottom" Spacing="10">
                <Button Content="Reset" 
                        Style="{StaticResource MaterialDesignOutlineButton}"
                        Command="{Binding ResetCommand}"
                        Visibility="{Binding IsEditingAccount, Converter={StaticResource BooleanToVisibilityConverter}}"/>
                <Button Content="Save" 
                        Style="{StaticResource MaterialDesignRaisedButton}"
                        Command="{Binding SaveAccountChangesCommand}"
                        Visibility="{Binding IsEditingAccount, Converter={StaticResource BooleanToVisibilityConverter}}"/>
            </StackPanel>

            <Button Grid.Column="2" Content="Close" 
                    Style="{StaticResource MaterialDesignOutlineButton}"
                    VerticalAlignment="Bottom"/>
        </Grid>
    </Grid>
</Window>
```

### ViewModel

```csharp
public class CashAccountsViewModel : ViewModelBase
{
    private readonly ICashAccountService _cashAccountService;
    private readonly IUserService _userService;
    
    private ObservableCollection<CashAccountViewModel> _accounts = new();
    private CashAccountViewModel? _selectedAccount;
    private bool _isEditingAccount;
    private decimal _editingOpeningBalance;
    private bool _editingIsActive;
    private decimal _editingOriginalBalance;

    public ObservableCollection<CashAccountViewModel> Accounts
    {
        get => _accounts;
        set
        {
            _accounts = value;
            OnPropertyChanged();
        }
    }

    public CashAccountViewModel? SelectedAccount
    {
        get => _selectedAccount;
        set
        {
            _selectedAccount = value;
            OnPropertyChanged();
            if (value != null)
                EditingOpeningBalance = value.OpeningBalance;
        }
    }

    public bool IsEditingAccount
    {
        get => _isEditingAccount;
        set
        {
            _isEditingAccount = value;
            OnPropertyChanged();
        }
    }

    public decimal EditingOpeningBalance
    {
        get => _editingOpeningBalance;
        set
        {
            _editingOpeningBalance = value;
            OnPropertyChanged();
        }
    }

    public bool EditingIsActive
    {
        get => _editingIsActive;
        set
        {
            _editingIsActive = value;
            OnPropertyChanged();
        }
    }

    public ICommand EditCommand { get; }
    public ICommand SaveAccountChangesCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand ViewHistoryCommand { get; }

    public CashAccountsViewModel(ICashAccountService cashAccountService,
                                 IUserService userService)
    {
        _cashAccountService = cashAccountService;
        _userService = userService;

        EditCommand = new RelayCommand<CashAccountViewModel>(account =>
        {
            if (account != null)
            {
                SelectedAccount = account;
                IsEditingAccount = true;
                _editingOriginalBalance = account.OpeningBalance;
            }
        });

        SaveAccountChangesCommand = new RelayCommand(async () => 
            await SaveAccountChangesAsync(), 
            () => SelectedAccount != null && EditingOpeningBalance >= 0);

        ResetCommand = new RelayCommand(() =>
        {
            if (SelectedAccount != null)
                EditingOpeningBalance = _editingOriginalBalance;
        });

        ViewHistoryCommand = new RelayCommand<CashAccountViewModel>(async account =>
        {
            if (account != null)
                await ShowBalanceHistoryAsync(account.AccountId);
        });

        LoadDataAsync().GetAwaiter().GetResult();
    }

    private async Task LoadDataAsync()
    {
        var accounts = await _cashAccountService.GetAllActiveAccountsAsync();
        Accounts.Clear();
        foreach (var account in accounts)
        {
            Accounts.Add(new CashAccountViewModel(account));
        }
    }

    private async Task SaveAccountChangesAsync()
    {
        if (SelectedAccount == null) return;

        try
        {
            var account = await _cashAccountService.GetAccountByIdAsync(SelectedAccount.AccountId);
            if (account != null)
            {
                // Track if opening balance changed
                var balanceChange = EditingOpeningBalance - SelectedAccount.OpeningBalance;
                
                account.OpeningBalance = EditingOpeningBalance;
                account.IsActive = EditingIsActive;
                account.ModifiedDate = DateTime.Now;

                await _cashAccountService.UpdateAccountAsync(account);

                // If opening balance changed, log it
                if (balanceChange != 0)
                {
                    var currentUser = await _userService.GetCurrentUserAsync();
                    await _cashAccountService.CreateBalanceHistoryAsync(new BalanceHistory
                    {
                        AccountId = account.AccountId,
                        ChangeType = BalanceChangeType.ManualAdjustment,
                        PreviousBalance = SelectedAccount.CurrentBalance - balanceChange,
                        ChangeAmount = balanceChange,
                        NewBalance = SelectedAccount.CurrentBalance,
                        TransactionDate = DateTime.Now,
                        Notes = $"Opening balance adjusted from ₹{SelectedAccount.OpeningBalance - balanceChange:N2} to ₹{EditingOpeningBalance:N2}",
                        EnteredBy = currentUser?.UserId ?? 1,
                        CreatedDate = DateTime.Now
                    });
                }

                await LoadDataAsync();
                IsEditingAccount = false;
                
                MessageBox.Show("Account updated successfully!", "Success", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving account: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task ShowBalanceHistoryAsync(int accountId)
    {
        // Show a dialog with balance history for this account
        var history = await _cashAccountService.GetBalanceHistoryAsync(accountId);
        
        var historyWindow = new BalanceHistoryWindow
        {
            DataContext = new BalanceHistoryViewModel(history)
        };
        
        historyWindow.ShowDialog();
    }
}
```

---

## 👥 3. Users Manager

### Access Path
`Settings → Users`

### Features & Restrictions

#### Protected Users
- **Admin User**: ❌ **CANNOT BE DELETED** - The system requires at least one Admin user to function. Consider deactivating instead.
- **Guest User**: ❌ **CANNOT BE DELETED** - System default user.

#### Available Actions

```xaml
<Grid>
    <!-- Toolbar -->
    <StackPanel Orientation="Horizontal" Margin="0,0,0,15">
        <Button Content="➕ Add User" 
                Command="{Binding AddUserCommand}"
                Style="{StaticResource MaterialDesignRaisedButton}"/>
        <Button Content="✏️ Edit Selected" 
                Command="{Binding EditUserCommand}"
                CommandParameter="{Binding SelectedUser}"
                Style="{StaticResource MaterialDesignOutlineButton}"
                Margin="10,0,0,0"/>
        <Button Content="🔄 Change Password" 
                Command="{Binding ChangePasswordCommand}"
                CommandParameter="{Binding SelectedUser}"
                Style="{StaticResource MaterialDesignOutlineButton}"
                Margin="10,0,0,0"/>
        <Button Content="❌ Deactivate" 
                Command="{Binding DeactivateUserCommand}"
                CommandParameter="{Binding SelectedUser}"
                Style="{StaticResource MaterialDesignOutlineButton}"
                Margin="10,0,0,0"
                Visibility="{Binding CanDeactivateSelected, Converter={StaticResource BooleanToVisibilityConverter}}"/>
    </StackPanel>

    <!-- Users DataGrid -->
    <DataGrid ItemsSource="{Binding Users}"
              SelectedItem="{Binding SelectedUser}">
        <!-- Columns: Username, Role, Status, Created By, Created Date, Last Login -->
    </DataGrid>
</Grid>
```

#### User Management Rules

```
Add User:
  ✅ Allowed for all users

Edit User:
  ✅ Can change username, role, status
  ❌ Cannot delete Admin user
  ❌ Cannot delete Guest user
  ⚠️ Deleting user only deactivates (soft delete)
  
Deactivate User:
  ✅ Deactivates the user account
  ✅ User data preserved for history
  ✅ Can reactivate anytime
  ❌ Cannot deactivate last active user
  ❌ Cannot deactivate Admin user (delete button disabled)
```

---

## 📦 4. Items Manager

### Access Path
`Settings → Items` OR `Inventory → Manage Items`

### Edit Dialog

```xaml
<Dialog Title="Edit Item">
    <StackPanel Margin="20">
        
        <TextBlock Text="Item Name" FontSize="12" FontWeight="SemiBold"/>
        <TextBox Text="{Binding ItemName}" Height="40" Margin="0,5,0,15"/>
        
        <TextBlock Text="Current Stock" FontSize="12" FontWeight="SemiBold"/>
        <TextBox Text="{Binding CurrentStock}" Height="40" Margin="0,5,0,15" IsReadOnly="True"/>
        <TextBlock Text="ℹ️ To adjust stock, use inventory transactions" FontSize="10" Foreground="Gray"/>
        
        <TextBlock Text="Unit (kg, bags, etc.)" FontSize="12" FontWeight="SemiBold"/>
        <TextBox Text="{Binding Unit}" Height="40" Margin="0,5,0,15"/>
        
        <TextBlock Text="Description (Optional)" FontSize="12" FontWeight="SemiBold"/>
        <TextBox Text="{Binding Description}" Height="60" Margin="0,5,0,15" 
                 AcceptsReturn="True" TextWrapping="Wrap"/>
        
        <!-- Action Buttons -->
        <StackPanel Orientation="Horizontal" HorizontalAlignment="Right" Spacing="10">
            <Button Content="Cancel" Style="{StaticResource MaterialDesignOutlineButton}"/>
            <Button Content="Save" Style="{StaticResource MaterialDesignRaisedButton}"/>
        </StackPanel>
    </StackPanel>
</Dialog>
```

---

## 🏢 5. Parties/Contacts Manager

### Access Path
`Settings → Parties` OR `Management → Contacts`

### Edit Dialog

```xaml
<Dialog Title="Edit Party/Contact">
    <StackPanel Margin="20">
        
        <TextBlock Text="Name" FontSize="12" FontWeight="SemiBold"/>
        <TextBox Text="{Binding Name}" Height="40" Margin="0,5,0,15"/>
        
        <TextBlock Text="Type" FontSize="12" FontWeight="SemiBold"/>
        <ComboBox ItemsSource="{Binding PartyTypes}" 
                  SelectedItem="{Binding PartyType}" 
                  Height="40" Margin="0,5,0,15"/>
        
        <TextBlock Text="Mobile Number" FontSize="12" FontWeight="SemiBold"/>
        <TextBox Text="{Binding MobileNumber}" Height="40" Margin="0,5,0,15"/>
        
        <TextBlock Text="Place/City" FontSize="12" FontWeight="SemiBold"/>
        <TextBox Text="{Binding Place}" Height="40" Margin="0,5,0,15"/>
        
        <TextBlock Text="Email (Optional)" FontSize="12" FontWeight="SemiBold"/>
        <TextBox Text="{Binding Email}" Height="40" Margin="0,5,0,15"/>
        
        <TextBlock Text="Address (Optional)" FontSize="12" FontWeight="SemiBold"/>
        <TextBox Text="{Binding Address}" Height="60" Margin="0,5,0,15" 
                 AcceptsReturn="True" TextWrapping="Wrap"/>
        
        <!-- Action Buttons -->
        <StackPanel Orientation="Horizontal" HorizontalAlignment="Right" Spacing="10">
            <Button Content="Cancel" Style="{StaticResource MaterialDesignOutlineButton}"/>
            <Button Content="Save" Style="{StaticResource MaterialDesignRaisedButton}"/>
        </StackPanel>
    </StackPanel>
</Dialog>
```

---

## 👷 6. Workers Manager

### Access Path
`Settings → Workers` OR `Payroll → Manage Workers`

### Edit Dialog

```xaml
<Dialog Title="Edit Worker">
    <StackPanel Margin="20">
        
        <TextBlock Text="Worker Name" FontSize="12" FontWeight="SemiBold"/>
        <TextBox Text="{Binding Name}" Height="40" Margin="0,5,0,15"/>
        
        <TextBlock Text="Mobile Number" FontSize="12" FontWeight="SemiBold"/>
        <TextBox Text="{Binding MobileNumber}" Height="40" Margin="0,5,0,15"/>
        
        <TextBlock Text="Role/Position" FontSize="12" FontWeight="SemiBold"/>
        <TextBox Text="{Binding Role}" Height="40" Margin="0,5,0,15"/>
        
        <TextBlock Text="Wage Type" FontSize="12" FontWeight="SemiBold"/>
        <ComboBox ItemsSource="{Binding WageTypes}" 
                  SelectedItem="{Binding WageType}" 
                  Height="40" Margin="0,5,0,15"/>
        
        <TextBlock Text="Daily/Hourly Rate" FontSize="12" FontWeight="SemiBold"/>
        <TextBox Text="{Binding Rate}" Height="40" Margin="0,5,0,15"/>
        
        <TextBlock Text="Status" FontSize="12" FontWeight="SemiBold"/>
        <ComboBox ItemsSource="{Binding Statuses}" 
                  SelectedItem="{Binding Status}" 
                  Height="40" Margin="0,5,0,15"/>
        
        <!-- Action Buttons -->
        <StackPanel Orientation="Horizontal" HorizontalAlignment="Right" Spacing="10">
            <Button Content="Cancel" Style="{StaticResource MaterialDesignOutlineButton}"/>
            <Button Content="Save" Style="{StaticResource MaterialDesignRaisedButton}"/>
        </StackPanel>
    </StackPanel>
</Dialog>
```

---

## 🔐 Permissions & Controls

### Who Can Edit What?

| Feature | Admin | Manager | Operator |
|---------|-------|---------|----------|
| Company Info | ✅ | ✅ | ❌ |
| Opening Balance | ✅ | ⚠️* | ❌ |
| Users Management | ✅ | ❌ | ❌ |
| Delete Admin User | ❌ **BLOCKED** | ❌ | ❌ |
| Items | ✅ | ✅ | ✅ |
| Parties | ✅ | ✅ | ✅ |
| Workers | ✅ | ✅ | ✅ |
| View Balance History | ✅ | ✅ | ⚠️** |

**⚠️* Manager can edit opening balance with approval/logging**  
**⚠️** Operator can view only their own transactions

---

## 🛡️ Critical User Protection

### Admin User - System Critical

The **Admin user cannot be deleted** for security and operational reasons:

```
❌ CANNOT BE DELETED: Admin role is protected
❌ CANNOT DEACTIVATE: Last safeguard for system access
✅ CAN BE DEACTIVATED: Only if other Admin users exist
✅ CAN CHANGE PASSWORD: For security
✅ CAN CHANGE USERNAME: If needed

WHY?
- Admin is the only role that can manage users
- Admin controls system settings and backups
- System cannot function without at least one Admin
- Accidental deletion would lock out entire system
```

### Error Message When Attempting to Delete Admin

```
╔════════════════════════════════════════════╗
║  Cannot Delete Admin User                  ║
╠════════════════════════════════════════════╣
║  The Admin user cannot be deleted.          ║
║                                            ║
║  The system must have at least one Admin   ║
║  user to function properly.                ║
║                                            ║
║  Consider deactivating instead if the      ║
║  user is no longer needed.                 ║
║                                            ║
║              [  OK  ]                      ║
╚════════════════════════════════════════════╝
```

---

### Guest User - System Default

The **Guest user cannot be deleted**:

```
❌ CANNOT BE DELETED: System default user
✅ CAN BE DEACTIVATED: If needed
✅ CAN CHANGE PASSWORD: For security

WHY?
- Guest is a fallback/demo account
- Provides basic system access
- Useful for training/testing
```

---

## 📋 Audit Trail Features

### Every Edit is Tracked

```csharp
public class EditAuditLog
{
    public int AuditId { get; set; }
    
    public string EntityType { get; set; } // "CashAccount", "Item", "Party", etc.
    public int EntityId { get; set; }
    
    public string FieldName { get; set; } // "OpeningBalance", "CompanyName"
    public string OldValue { get; set; }
    public string NewValue { get; set; }
    
    public int EditedBy { get; set; } // User ID
    public DateTime EditDate { get; set; }
    
    public string Reason { get; set; } = ""; // Optional: why was it changed?
}
```

### View Audit Trail Dialog

```xaml
<Window Title="Edit History">
    <DataGrid ItemsSource="{Binding AuditLogs}">
        <DataGrid.Columns>
            <DataGridTextColumn Header="Date" Binding="{Binding EditDate, StringFormat='dd/MM/yyyy HH:mm'}"/>
            <DataGridTextColumn Header="Field" Binding="{Binding FieldName}"/>
            <DataGridTextColumn Header="Old Value" Binding="{Binding OldValue}"/>
            <DataGridTextColumn Header="New Value" Binding="{Binding NewValue}"/>
            <DataGridTextColumn Header="Edited By" Binding="{Binding UserName}"/>
            <DataGridTextColumn Header="Reason" Binding="{Binding Reason}"/>
        </DataGrid.Columns>
    </DataGrid>
</Window>
```

---

## 🚨 Safety Features

### 1. Confirmation Dialogs

```csharp
if (MessageBox.Show(
    "Are you sure you want to change the opening balance?\n\n" +
    "Old: ₹" + oldBalance + "\n" +
    "New: ₹" + newBalance + "\n\n" +
    "This will create an audit log entry.",
    "Confirm Change",
    MessageBoxButton.YesNo,
    MessageBoxImage.Warning) == MessageBoxResult.Yes)
{
    // Save changes
}
```

### 2. Backup Before Critical Changes

```csharp
private async Task SaveWithBackupAsync()
{
    // Create backup
    var backup = await _backupService.CreateQuickBackupAsync("Before opening balance edit");
    
    // Make changes
    await _cashAccountService.UpdateAccountAsync(account);
    
    // Log the backup reference
    await LogChangeAsync($"Backup ID: {backup.BackupId}");
}
```

### 3. Change Reason Logging

```xaml
<Dialog Title="Change Opening Balance">
    <!-- Balance fields -->
    
    <TextBlock Text="Reason for Change (Optional)" FontSize="12" FontWeight="SemiBold" Margin="0,20,0,0"/>
    <TextBox Text="{Binding ChangeReason}" Height="60" 
             AcceptsReturn="True" TextWrapping="Wrap"
             materialDesign:HintAssist.Hint="e.g., 'Correction of initial entry', 'Year-end reconciliation'"/>
</Dialog>
```

---

## 📈 Recovery Options

### If You Made a Mistake

#### Option 1: Undo (Last 24 Hours)
```csharp
public async Task UndoLastChangeAsync(string entityType, int entityId)
{
    var lastChange = await _auditService.GetLastChangeAsync(entityType, entityId);
    if (lastChange != null && DateTime.Now.Subtract(lastChange.EditDate).TotalHours <= 24)
    {
        // Revert to old value
    }
}
```

#### Option 2: Restore from Backup
```
Settings → Backup & Restore
├─ View Backups
├─ Create Backup Now
├─ Restore from Backup
└─ Auto-Restore Points
```

#### Option 3: Manual Adjustment
```xaml
<Dialog Title="Manual Adjustment">
    <StackPanel Margin="20">
        <TextBlock Text="Make a correction to any field"/>
        
        <TextBlock Text="Current Value" FontSize="12"/>
        <TextBox Text="{Binding CurrentValue, Mode=OneWay}" 
                 IsReadOnly="True" Height="40"/>
        
        <TextBlock Text="New Value" FontSize="12" Margin="0,15,0,0"/>
        <TextBox Text="{Binding NewValue}" Height="40"/>
        
        <TextBlock Text="Reason" FontSize="12" Margin="0,15,0,0"/>
        <TextBox Text="{Binding Reason}" Height="60" 
                 AcceptsReturn="True" TextWrapping="Wrap"/>
    </StackPanel>
</Dialog>
```

---

## 🔄 Bulk Edit Features

### Edit Multiple Items

```xaml
<Window Title="Bulk Edit Items">
    <Grid>
        <!-- Select multiple items in list -->
        <DataGrid SelectionMode="Extended"/>
        
        <!-- Common fields to edit -->
        <StackPanel Margin="0,20,0,0">
            <TextBlock Text="Update Selected Items" FontSize="14" FontWeight="Bold"/>
            
            <CheckBox Content="Update Unit?" Margin="0,10,0,0"/>
            <TextBox Text="{Binding NewUnit}" Margin="20,5,0,0" 
                     IsEnabled="{Binding IsUpdateUnitChecked}"/>
            
            <CheckBox Content="Update Description?" Margin="0,10,0,0"/>
            <TextBox Text="{Binding NewDescription}" Margin="20,5,0,0" 
                     IsEnabled="{Binding IsUpdateDescriptionChecked}"
                     AcceptsReturn="True" TextWrapping="Wrap"/>
        </StackPanel>
    </Grid>
</Window>
```

---

## 📊 Summary of Edit Options

### Quick Reference Table

| What | Where | How | Restrictions |
|------|-------|-----|--------------|
| **Company Name** | Settings → Company Info | Type new name | Admin/Manager only |
| **Address** | Settings → Company Info | Type new address | Admin/Manager only |
| **Opening Balance** | Settings → Cash Accounts | Type new amount | Admin with audit trail |
| **Current Balance** | Settings → Cash Accounts | View only | Read-only (auto-updated) |
| **Items** | Settings → Items | Edit name, unit, description | Anyone, audit logged |
| **Parties** | Settings → Parties | Edit all fields | Anyone, audit logged |
| **Workers** | Settings → Workers | Edit all fields except status | Anyone, audit logged |
| **Users** | Settings → Users | Edit role, status | Admin only |
| **Delete Admin User** | Settings → Users | ❌ **NOT ALLOWED** | **BLOCKED** - System protection |
| **Delete Guest User** | Settings → Users | ❌ **NOT ALLOWED** | **BLOCKED** - System default |
| **Passwords** | Settings → Users | Change password | User can change own |

---

## ⚠️ What Happens If You Try to Delete Admin?

### Scenario: User Attempts to Delete Admin Account

**Step 1:** User selects Admin user from list

**Step 2:** User clicks "Delete" button

**Step 3:** System shows warning dialog:

```
┌─────────────────────────────────────────┐
│  🛑 Cannot Delete Admin User            │
├─────────────────────────────────────────┤
│                                         │
│  The Admin user cannot be deleted.      │
│                                         │
│  The system must have at least one      │
│  Admin user to function properly.       │
│                                         │
│  Consider deactivating instead if       │
│  the user is no longer needed.          │
│                                         │
│            [ OK ]                       │
│                                         │
└─────────────────────────────────────────┘
```

**Step 4:** Dialog closes, no deletion occurs

**Step 5:** User can instead:
- ✅ Change Admin's password
- ✅ Change Admin's username
- ✅ Deactivate Admin (only if other Admin users exist)

---

## 🔄 Deactivating vs. Deleting

### Deactivate (Recommended)
```
What: Marks user as Inactive
Result: User removed from login dropdown
Data: All history preserved
Reversible: Yes - can reactivate anytime
Best for: Users no longer needed but have transaction history

When available: All users EXCEPT if they're the last active user
```

### Delete (Soft Delete)
```
What: Actually deactivates (not hard delete)
Result: User marked as Inactive, row preserved
Data: All transactions remain linked
Reversible: Yes - can reactivate anytime
Best for: Removing unnecessary accounts

When allowed:
  ✅ Regular users
  ✅ Guest user (only deactivated)
  ❌ Admin user (NEVER)
  ❌ Last active user (system protection)
```

---

## 🎓 Best Practices for Admin Management

### Creating Multiple Admin Users
To ensure system continuity, create at least 2-3 Admin users:

```
1. Primary Admin: Main administrator
2. Backup Admin: For emergencies/absences
3. (Optional) 3rd Admin: Extra redundancy
```

### Retiring an Admin User
If an Admin user is retiring:

```
✅ DO:
  - Create a new Admin user first
  - Deactivate the retiring Admin
  - Keep all their transaction history
  - Document the transition

❌ DON'T:
  - Try to delete the Admin user (system won't allow it)
  - Leave zero Admin users active
  - Delete without creating a replacement
```

### Emergency Access
If locked out:

```
System Protection: At least 1 Admin always exists
Fallback: Another Admin can restore access
Last Resort: Database recovery/backup restore
```

---

## 🛠️ Technical Implementation

### Service Method for Edits

```csharp
public async Task<T> UpdateAsync<T>(T entity, string changeReason = "") 
    where T : class, ITrackable
{
    using var transaction = await _context.Database.BeginTransactionAsync();
    
    try
    {
        // Get old values
        var original = await _context.Entry(entity).GetDatabaseValuesAsync();
        
        // Update entity
        entity.ModifiedDate = DateTime.Now;
        _context.Update(entity);
        
        // Log changes
        var changes = await _context.Entry(entity).GetDatabaseValuesAsync();
        await LogAuditTrailAsync(entity, original, changes, changeReason);
        
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        
        return entity;
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}
```

---

## ✅ User Workflow Example

### Scenario: Correct Wrong Opening Balance

**Step 1:** User realizes opening cash balance was entered as ₹50,000 but should be ₹60,000

**Step 2:** Go to `Settings → Cash Accounts`

**Step 3:** Click on "Cash Account" → Click "Edit"

**Step 4:** Dialog appears showing:
- Current Value: ₹50,000
- Edit field with new value: ₹60,000
- Reason: "Initial entry error - actual cash count"

**Step 5:** Click "Save"

**Step 6:** System logs:
- Old value: ₹50,000
- New value: ₹60,000
- Changed by: Admin (Jan 2, 2026 10:30 AM)
- Reason: "Initial entry error - actual cash count"

**Step 7:** Balance history is updated
- Previous balance: ₹50,000
- Adjustment: +₹10,000
- New balance: ₹60,000

**Step 8:** Dashboard reflects new balance immediately

---

## 📚 Complete Menu Structure

```
SETTINGS
│
├─ Company Information
│  └─ Edit: Name, Address
│     Show: Last modified by/date
│
├─ Cash Accounts
│  ├─ List all accounts with current balance
│  ├─ Edit: Opening balance (with audit log)
│  └─ View: Complete balance history
│
├─ User Management
│  ├─ Add user
│  ├─ Edit user (role, status)
│  ├─ Change password
│  └─ View: Login history
│
├─ Inventory
│  ├─ Items
│  │  ├─ Add item
│  │  ├─ Edit: Name, Unit, Description
│  │  └─ View: Stock history
│  │
│  ├─ Parties/Contacts
│  │  ├─ Add party
│  │  ├─ Edit: All details
│  │  └─ View: Transaction history
│  │
│  └─ Workers
│     ├─ Add worker
│     ├─ Edit: Name, Role, Rate, Status
│     └─ View: Wage history
│
├─ Audit & History
│  ├─ View all edits (who, what, when)
│  ├─ Balance history
│  └─ User activity log
│
└─ Backup & Restore
   ├─ Create backup
   ├─ View backups
   ├─ Restore from backup
   └─ Auto-restore points
```

---

## 🎓 Training Checklist for Users

- [ ] How to access Settings
- [ ] How to edit company information
- [ ] How to view/edit opening balance
- [ ] How to add/edit items
- [ ] How to add/edit parties
- [ ] How to add/edit workers
- [ ] How to understand audit trails
- [ ] How to restore from backup
- [ ] Who can edit what (permissions)
- [ ] Why changes are logged

---

**Document Version**: 1.0  
**Last Updated**: January 2, 2026  
**For**: Factory Management System Users

# Operational Expenses Module - Design Document

## Executive Summary

This document outlines the design for adding **Operational/Miscellaneous Expense tracking** to the Factory Management System. This module will handle various factory operational costs like cab charges, fuel, machine repairs, machinery purchases, utilities, rent, and other factory-specific expenses.

---

## 1. Current Architecture Analysis

### Existing Transaction Systems

Your application currently has **three separate transaction systems**, each with its own domain, purpose, and workflow:

| System | Model | Purpose | Key Characteristics |
|--------|-------|---------|---------------------|
| **Inventory** | `Transaction` | Track item buy/sell/wastage/processing | Item-centric, quantity-based, affects stock |
| **Financial** | `FinancialTransaction` | Loan management, interest tracking | Party-centric, loan-focused, tracks receivables/payables |
| **Payroll** | `WageTransaction` | Worker wages, advances, deductions | Worker-centric, time-based, payroll-specific |

### Architecture Pattern

✅ **Current Pattern**: **Separate Domain Models** (Clean Separation)
- Each transaction type has its own:
  - Domain Model (`Transaction`, `FinancialTransaction`, `WageTransaction`)
  - Repository (`TransactionRepository`, `FinancialTransactionRepository`, `WageTransactionRepository`)
  - Service (`TransactionService`, `FinancialTransactionService`, `WageService`)
  - Dedicated UI views
- Unified view via `UnifiedTransactionService` for cross-system reporting

### Dashboard & Reports Integration

**Dashboard:**
- Shows summary cards for each transaction system
- Recent activities from all systems
- Dedicated metrics per domain

**Reports:**
- `ReportType` enum: `All`, `Inventory`, `Financial`, `Wages`
- Separate collections per transaction type
- Unified export capability via `ReportExportBuilder`

---

## 2. Recommended Design: Operational Expenses Module

### Design Philosophy

Following your existing **clean separation pattern**, we'll create a **fourth transaction domain** specifically for operational expenses.

### Why NOT extend existing systems?

❌ **Don't use `Transaction`** - It's item-centric (quantity, stock impact)  
❌ **Don't use `FinancialTransaction`** - It's loan-centric (interest, repayments)  
❌ **Don't use `WageTransaction`** - It's worker/payroll-specific  

✅ **Create `OperationalExpense`** - Purpose-built for factory operational costs

---

## 3. Data Model Design

### 3.1 Expense Category Model

```csharp
namespace FactoryManagement.Models
{
    public enum ExpenseCategory
    {
        // Transportation & Logistics
        CabCharges,
        TransportationFees,
        FreightCharges,
        
        // Utilities
        Electricity,
        Water,
        Gas,
        Internet,
        Phone,
        
        // Machinery & Equipment
        MachineryPurchase,
        MachineryRepair,
        MachineryMaintenance,
        EquipmentRental,
        
        // Fuel & Energy
        Fuel,
        DieselGenerator,
        
        // Facility Costs
        Rent,
        PropertyTax,
        Insurance,
        
        // Office & Admin
        Stationery,
        PrintingCharges,
        Postage,
        
        // Professional Services
        LegalFees,
        AccountingFees,
        ConsultingFees,
        
        // Maintenance & Repairs
        BuildingMaintenance,
        PlumbingRepairs,
        ElectricalRepairs,
        
        // Other
        Miscellaneous,
        Custom
    }
}
```

### 3.2 Operational Expense Model

```csharp
namespace FactoryManagement.Models
{
    public class OperationalExpense
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OperationalExpenseId { get; set; }

        [Required]
        public ExpenseCategory Category { get; set; }

        // For custom categories
        [MaxLength(100)]
        public string? CustomCategoryName { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime ExpenseDate { get; set; } = DateTime.Now;

        // Optional: Link to Party (vendor/service provider)
        public int? PartyId { get; set; }

        [ForeignKey(nameof(PartyId))]
        public virtual Party? Party { get; set; }

        [MaxLength(200)]
        public string? VendorName { get; set; }

        // Payment details
        [Required]
        public PaymentMode PaymentMode { get; set; } = PaymentMode.Cash;

        // Optional: Link to Item (for machinery purchase)
        public int? ItemId { get; set; }

        [ForeignKey(nameof(ItemId))]
        public virtual Item? Item { get; set; }

        // Invoice/Receipt tracking
        [MaxLength(100)]
        public string? InvoiceNumber { get; set; }

        public DateTime? InvoiceDate { get; set; }

        // Approval workflow (future enhancement)
        public bool IsApproved { get; set; } = true;
        
        public int? ApprovedBy { get; set; }

        [ForeignKey(nameof(ApprovedBy))]
        public virtual User? Approver { get; set; }

        // Tax handling
        [Column(TypeName = "decimal(5,2)")]
        public decimal? TaxRate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TaxAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TotalAmountWithTax { get; set; }

        // Audit trail
        [Required]
        public int EnteredBy { get; set; }

        [ForeignKey(nameof(EnteredBy))]
        public virtual User? User { get; set; }

        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? ModifiedDate { get; set; }

        // Attachments (future enhancement)
        [MaxLength(500)]
        public string? AttachmentPath { get; set; }

        // Computed property for display
        [NotMapped]
        public string CategoryDisplay => Category == ExpenseCategory.Custom 
            ? CustomCategoryName ?? "Custom" 
            : Category.ToString();

        [NotMapped]
        public string DebitCredit => "Debit"; // All expenses are debits
    }
}
```

---

## 4. Database Schema

### 4.1 DbContext Changes

```csharp
public class FactoryDbContext : DbContext
{
    public DbSet<OperationalExpense> OperationalExpenses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ... existing configurations ...

        // OperationalExpense configuration
        modelBuilder.Entity<OperationalExpense>()
            .HasIndex(oe => oe.ExpenseDate);

        modelBuilder.Entity<OperationalExpense>()
            .HasIndex(oe => oe.Category);

        modelBuilder.Entity<OperationalExpense>()
            .HasIndex(oe => oe.PartyId);

        modelBuilder.Entity<OperationalExpense>()
            .Property(oe => oe.Amount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<OperationalExpense>()
            .Property(oe => oe.TaxAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<OperationalExpense>()
            .Property(oe => oe.TotalAmountWithTax)
            .HasPrecision(18, 2);
    }
}
```

### 4.2 Migration

```powershell
# Create migration
Add-Migration AddOperationalExpenses

# Update database
Update-Database
```

---

## 5. Repository Layer

### 5.1 Interface

```csharp
namespace FactoryManagement.Data.Repositories
{
    public interface IOperationalExpenseRepository : IRepository<OperationalExpense>
    {
        Task<IEnumerable<OperationalExpense>> GetByCategoryAsync(ExpenseCategory category);
        Task<IEnumerable<OperationalExpense>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<OperationalExpense>> GetByPartyIdAsync(int partyId);
        Task<decimal> GetTotalAmountByCategoryAsync(ExpenseCategory category);
        Task<decimal> GetTotalAmountByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<Dictionary<ExpenseCategory, decimal>> GetExpensesByCategoryAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<OperationalExpense>> GetUnapprovedExpensesAsync();
    }
}
```

### 5.2 Implementation

```csharp
namespace FactoryManagement.Data.Repositories
{
    public class OperationalExpenseRepository : Repository<OperationalExpense>, IOperationalExpenseRepository
    {
        public OperationalExpenseRepository(FactoryDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<OperationalExpense>> GetAllAsync()
        {
            return await _context.OperationalExpenses
                .Include(oe => oe.Party)
                .Include(oe => oe.User)
                .Include(oe => oe.Approver)
                .Include(oe => oe.Item)
                .OrderByDescending(oe => oe.ExpenseDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<OperationalExpense>> GetByCategoryAsync(ExpenseCategory category)
        {
            return await _context.OperationalExpenses
                .Include(oe => oe.Party)
                .Include(oe => oe.User)
                .Where(oe => oe.Category == category)
                .OrderByDescending(oe => oe.ExpenseDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<OperationalExpense>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.OperationalExpenses
                .Include(oe => oe.Party)
                .Include(oe => oe.User)
                .Where(oe => oe.ExpenseDate >= startDate && oe.ExpenseDate <= endDate)
                .OrderByDescending(oe => oe.ExpenseDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<OperationalExpense>> GetByPartyIdAsync(int partyId)
        {
            return await _context.OperationalExpenses
                .Include(oe => oe.Party)
                .Include(oe => oe.User)
                .Where(oe => oe.PartyId == partyId)
                .OrderByDescending(oe => oe.ExpenseDate)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalAmountByCategoryAsync(ExpenseCategory category)
        {
            return await _context.OperationalExpenses
                .Where(oe => oe.Category == category)
                .SumAsync(oe => oe.Amount);
        }

        public async Task<decimal> GetTotalAmountByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.OperationalExpenses
                .Where(oe => oe.ExpenseDate >= startDate && oe.ExpenseDate <= endDate)
                .SumAsync(oe => oe.Amount);
        }

        public async Task<Dictionary<ExpenseCategory, decimal>> GetExpensesByCategoryAsync(
            DateTime? startDate = null, 
            DateTime? endDate = null)
        {
            var query = _context.OperationalExpenses.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(oe => oe.ExpenseDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(oe => oe.ExpenseDate <= endDate.Value);

            return await query
                .GroupBy(oe => oe.Category)
                .Select(g => new { Category = g.Key, Total = g.Sum(oe => oe.Amount) })
                .ToDictionaryAsync(x => x.Category, x => x.Total);
        }

        public async Task<IEnumerable<OperationalExpense>> GetUnapprovedExpensesAsync()
        {
            return await _context.OperationalExpenses
                .Include(oe => oe.Party)
                .Include(oe => oe.User)
                .Where(oe => !oe.IsApproved)
                .OrderByDescending(oe => oe.ExpenseDate)
                .ToListAsync();
        }
    }
}
```

---

## 6. Service Layer

### 6.1 Interface

```csharp
namespace FactoryManagement.Services
{
    public interface IOperationalExpenseService
    {
        // CRUD Operations
        Task<OperationalExpense> CreateExpenseAsync(OperationalExpense expense);
        Task<OperationalExpense?> GetExpenseByIdAsync(int id);
        Task<IEnumerable<OperationalExpense>> GetAllExpensesAsync();
        Task<OperationalExpense> UpdateExpenseAsync(OperationalExpense expense);
        Task<bool> DeleteExpenseAsync(int id);

        // Query Operations
        Task<IEnumerable<OperationalExpense>> GetExpensesByCategoryAsync(ExpenseCategory category);
        Task<IEnumerable<OperationalExpense>> GetExpensesByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<OperationalExpense>> GetExpensesByPartyAsync(int partyId);

        // Aggregation Operations
        Task<decimal> GetTotalExpensesAsync();
        Task<decimal> GetTotalExpensesByCategoryAsync(ExpenseCategory category);
        Task<decimal> GetTotalExpensesByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<Dictionary<ExpenseCategory, decimal>> GetExpenseBreakdownAsync(DateTime? startDate = null, DateTime? endDate = null);

        // Dashboard Metrics
        Task<decimal> GetMonthlyExpensesAsync(int year, int month);
        Task<decimal> GetYearlyExpensesAsync(int year);
        Task<Dictionary<ExpenseCategory, decimal>> GetTopExpenseCategoriesAsync(int topCount = 5);

        // Approval Workflow (future enhancement)
        Task<bool> ApproveExpenseAsync(int expenseId, int approvedBy);
        Task<IEnumerable<OperationalExpense>> GetUnapprovedExpensesAsync();
    }
}
```

### 6.2 Implementation

```csharp
namespace FactoryManagement.Services
{
    public class OperationalExpenseService : IOperationalExpenseService
    {
        private readonly IOperationalExpenseRepository _repository;

        public OperationalExpenseService(IOperationalExpenseRepository repository)
        {
            _repository = repository;
        }

        public async Task<OperationalExpense> CreateExpenseAsync(OperationalExpense expense)
        {
            // Business logic validation
            if (expense.Amount <= 0)
                throw new ArgumentException("Expense amount must be greater than zero.");

            if (expense.Category == ExpenseCategory.Custom && string.IsNullOrWhiteSpace(expense.CustomCategoryName))
                throw new ArgumentException("Custom category name is required for custom expenses.");

            // Calculate tax if applicable
            if (expense.TaxRate.HasValue && expense.TaxRate.Value > 0)
            {
                expense.TaxAmount = expense.Amount * (expense.TaxRate.Value / 100);
                expense.TotalAmountWithTax = expense.Amount + expense.TaxAmount;
            }
            else
            {
                expense.TotalAmountWithTax = expense.Amount;
            }

            expense.CreatedDate = DateTime.Now;
            return await _repository.AddAsync(expense);
        }

        public async Task<OperationalExpense?> GetExpenseByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<OperationalExpense>> GetAllExpensesAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<OperationalExpense> UpdateExpenseAsync(OperationalExpense expense)
        {
            // Recalculate tax if changed
            if (expense.TaxRate.HasValue && expense.TaxRate.Value > 0)
            {
                expense.TaxAmount = expense.Amount * (expense.TaxRate.Value / 100);
                expense.TotalAmountWithTax = expense.Amount + expense.TaxAmount;
            }
            else
            {
                expense.TaxAmount = null;
                expense.TotalAmountWithTax = expense.Amount;
            }

            expense.ModifiedDate = DateTime.Now;
            return await _repository.UpdateAsync(expense);
        }

        public async Task<bool> DeleteExpenseAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }

        public async Task<IEnumerable<OperationalExpense>> GetExpensesByCategoryAsync(ExpenseCategory category)
        {
            return await _repository.GetByCategoryAsync(category);
        }

        public async Task<IEnumerable<OperationalExpense>> GetExpensesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _repository.GetByDateRangeAsync(startDate, endDate);
        }

        public async Task<IEnumerable<OperationalExpense>> GetExpensesByPartyAsync(int partyId)
        {
            return await _repository.GetByPartyIdAsync(partyId);
        }

        public async Task<decimal> GetTotalExpensesAsync()
        {
            var allExpenses = await _repository.GetAllAsync();
            return allExpenses.Sum(e => e.Amount);
        }

        public async Task<decimal> GetTotalExpensesByCategoryAsync(ExpenseCategory category)
        {
            return await _repository.GetTotalAmountByCategoryAsync(category);
        }

        public async Task<decimal> GetTotalExpensesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _repository.GetTotalAmountByDateRangeAsync(startDate, endDate);
        }

        public async Task<Dictionary<ExpenseCategory, decimal>> GetExpenseBreakdownAsync(
            DateTime? startDate = null, 
            DateTime? endDate = null)
        {
            return await _repository.GetExpensesByCategoryAsync(startDate, endDate);
        }

        public async Task<decimal> GetMonthlyExpensesAsync(int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            return await GetTotalExpensesByDateRangeAsync(startDate, endDate);
        }

        public async Task<decimal> GetYearlyExpensesAsync(int year)
        {
            var startDate = new DateTime(year, 1, 1);
            var endDate = new DateTime(year, 12, 31);
            return await GetTotalExpensesByDateRangeAsync(startDate, endDate);
        }

        public async Task<Dictionary<ExpenseCategory, decimal>> GetTopExpenseCategoriesAsync(int topCount = 5)
        {
            var breakdown = await GetExpenseBreakdownAsync();
            return breakdown
                .OrderByDescending(kvp => kvp.Value)
                .Take(topCount)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public async Task<bool> ApproveExpenseAsync(int expenseId, int approvedBy)
        {
            var expense = await _repository.GetByIdAsync(expenseId);
            if (expense == null) return false;

            expense.IsApproved = true;
            expense.ApprovedBy = approvedBy;
            expense.ModifiedDate = DateTime.Now;

            await _repository.UpdateAsync(expense);
            return true;
        }

        public async Task<IEnumerable<OperationalExpense>> GetUnapprovedExpensesAsync()
        {
            return await _repository.GetUnapprovedExpensesAsync();
        }
    }
}
```

---

## 7. Dashboard Integration

### 7.1 DashboardViewModel Changes

```csharp
public partial class DashboardViewModel : PaginationViewModel
{
    // Add new properties
    [ObservableProperty]
    private decimal _totalOperationalExpenses;

    [ObservableProperty]
    private decimal _monthlyOperationalExpenses;

    private readonly IOperationalExpenseService? _operationalExpenseService;

    public DashboardViewModel(
        ITransactionService transactionService, 
        IItemService itemService,
        IFinancialTransactionService? financialTransactionService = null,
        IWageService? wageService = null,
        IUnifiedTransactionService? unifiedTransactionService = null,
        IOperationalExpenseService? operationalExpenseService = null)  // Add parameter
    {
        _transactionService = transactionService;
        _itemService = itemService;
        _financialTransactionService = financialTransactionService;
        _wageService = wageService;
        _unifiedTransactionService = unifiedTransactionService;
        _operationalExpenseService = operationalExpenseService;  // Assign
    }

    public override async Task InitializeAsync()
    {
        await LoadDataCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            // Parallel loading for performance
            var loadTasks = new List<Task>
            {
                LoadInventoryMetricsAsync(),
                LoadItemsDataAsync(),
                LoadFinancialMetricsAsync(),
                LoadWageMetricsAsync(),
                LoadOperationalExpenseMetricsAsync(),  // NEW
                LoadUnifiedTransactionsAsync()
            };

            await Task.WhenAll(loadTasks);

            // After all data loaded, update recent activities
            UpdateRecentActivities();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading dashboard: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadOperationalExpenseMetricsAsync()
    {
        if (_operationalExpenseService == null) return;

        try
        {
            // Total expenses
            TotalOperationalExpenses = await _operationalExpenseService.GetTotalExpensesAsync();

            // Current month expenses
            var now = DateTime.Now;
            MonthlyOperationalExpenses = await _operationalExpenseService
                .GetMonthlyExpensesAsync(now.Year, now.Month);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading operational expenses: {ex.Message}");
        }
    }
}
```

### 7.2 DashboardView XAML Changes

Add new summary card to dashboard:

```xml
<!-- Operational Expenses Summary Card -->
<Controls:SummaryCard 
    Title="Operational Expenses" 
    Value="{Binding TotalOperationalExpenses, StringFormat='{}{0:N2}'}" 
    Subtitle="Total operational costs"
    Icon="CurrencyRupee"
    Color="{DynamicResource SurfaceColor2}"/>

<!-- Monthly Operational Expenses Card -->
<Controls:SummaryCard 
    Title="This Month Expenses" 
    Value="{Binding MonthlyOperationalExpenses, StringFormat='{}{0:N2}'}" 
    Subtitle="Current month operational costs"
    Icon="CalendarMonth"
    Color="{DynamicResource SurfaceColor3}"/>
```

---

## 8. Reports Integration

### 8.1 Update ReportType Enum

```csharp
public enum ReportType
{
    All,
    Inventory,
    Financial,
    Wages,
    OperationalExpenses  // NEW
}
```

### 8.2 ReportsViewModel Changes

```csharp
public partial class ReportsViewModel : ViewModelBase
{
    private readonly IOperationalExpenseService _operationalExpenseService;

    // Add collection
    private ObservableCollection<OperationalExpense> _allOperationalExpenses = new();

    [ObservableProperty]
    private ObservableCollection<OperationalExpense> _paginatedOperationalExpenses = new();

    public ObservableCollection<OperationalExpense> OperationalExpenses => _allOperationalExpenses;

    // Constructor injection
    public ReportsViewModel(
        ITransactionService transactionService,
        IItemService itemService,
        IPartyService partyService,
        IExportService exportService,
        IFinancialTransactionService financialService,
        IWageService wageService,
        IUnifiedTransactionService unifiedTransactionService,
        IReportExportBuilder reportExportBuilder,
        IUserService userService,
        IOperationalExpenseService operationalExpenseService)  // NEW
    {
        // ... existing initialization ...
        _operationalExpenseService = operationalExpenseService;
    }

    // Update CurrentTransactions property
    public System.Collections.IEnumerable CurrentTransactions
    {
        get
        {
            return SelectedReportType switch
            {
                ReportType.All => PaginatedAllTransactions,
                ReportType.Inventory => PaginatedTransactions,
                ReportType.Financial => PaginatedFinancialTransactions,
                ReportType.Wages => PaginatedWageTransactions,
                ReportType.OperationalExpenses => PaginatedOperationalExpenses,  // NEW
                _ => PaginatedTransactions
            };
        }
    }

    // Load operational expenses
    private async Task LoadOperationalExpensesAsync()
    {
        try
        {
            var expenses = await _operationalExpenseService.GetAllExpensesAsync();
            _allOperationalExpenses = new ObservableCollection<OperationalExpense>(expenses);
            
            // Apply filters if needed
            ApplyOperationalExpenseFilters();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading operational expenses: {ex.Message}");
        }
    }

    private void ApplyOperationalExpenseFilters()
    {
        var filtered = _allOperationalExpenses.AsEnumerable();

        // Date filter
        if (StartDate.HasValue)
            filtered = filtered.Where(e => e.ExpenseDate >= StartDate.Value);

        if (EndDate.HasValue)
            filtered = filtered.Where(e => e.ExpenseDate <= EndDate.Value);

        // Category filter
        if (SelectedExpenseCategory.HasValue)
            filtered = filtered.Where(e => e.Category == SelectedExpenseCategory.Value);

        // Update paginated collection
        UpdatePaginatedOperationalExpenses(filtered.ToList());
    }
}
```

---

## 9. UI Implementation

### 9.1 New View: OperationalExpensesView.xaml

Create a dedicated management page similar to Financial Records view with:
- **Add Expense Form**
  - Category dropdown (with custom option)
  - Amount input
  - Date picker
  - Vendor/Party selector (optional)
  - Payment mode selector
  - Tax rate input (optional)
  - Invoice number
  - Notes
  - Submit button

- **Expenses List**
  - DataGrid with columns:
    - Date
    - Category
    - Vendor
    - Amount
    - Payment Mode
    - Invoice #
    - Entered By
    - Actions (Edit/Delete)
  - Filters:
    - Date range
    - Category
    - Payment mode
  - Pagination

- **Summary Cards**
  - Total Expenses
  - Monthly Expenses
  - Expenses by Category (Top 5)
  - Pending Approvals (future)

### 9.2 Menu Integration

Add to MainWindow navigation:

```xml
<Button 
    Style="{StaticResource MenuItemButton}"
    Command="{Binding NavigateToOperationalExpensesCommand}">
    <StackPanel Orientation="Horizontal">
        <TextBlock Text="&#xE8A1;" FontFamily="Segoe MDL2 Assets"/>
        <TextBlock Text="Operational Expenses" Margin="10,0,0,0"/>
    </StackPanel>
</Button>
```

---

## 10. UnifiedTransactionService Integration

Update `UnifiedTransactionService` to include operational expenses:

```csharp
public class UnifiedTransactionViewModel
{
    public TransactionSource Source { get; set; }  // Add OperationalExpense to enum
    // ... existing properties ...
}

public enum TransactionSource
{
    Inventory,
    Financial,
    Wages,
    OperationalExpense  // NEW
}

public async Task<IEnumerable<UnifiedTransactionViewModel>> GetAllTransactionsAsync()
{
    var tasks = new[]
    {
        MapInventoryTransactions(),
        MapFinancialTransactions(),
        MapWageTransactions(),
        MapOperationalExpenses()  // NEW
    };

    var results = await Task.WhenAll(tasks);
    return results.SelectMany(x => x).OrderByDescending(x => x.Date);
}

private async Task<IEnumerable<UnifiedTransactionViewModel>> MapOperationalExpenses()
{
    if (_operationalExpenseService == null) return Enumerable.Empty<UnifiedTransactionViewModel>();

    var expenses = await _operationalExpenseService.GetAllExpensesAsync();
    return expenses.Select(e => new UnifiedTransactionViewModel
    {
        Id = e.OperationalExpenseId,
        Source = TransactionSource.OperationalExpense,
        Date = e.ExpenseDate,
        Type = e.CategoryDisplay,
        Description = $"{e.CategoryDisplay} - {e.VendorName ?? "N/A"}",
        Amount = e.Amount,
        DebitCredit = "Debit",
        PaymentMode = e.PaymentMode.ToString(),
        EnteredBy = e.User?.Username ?? "Unknown",
        Notes = e.Notes
    });
}
```

---

## 11. Dependency Injection Setup

Update `App.xaml.cs`:

```csharp
// Register Repository
services.AddScoped<IOperationalExpenseRepository, OperationalExpenseRepository>();

// Register Service
services.AddScoped<IOperationalExpenseService, OperationalExpenseService>();

// Register ViewModel
services.AddTransient<OperationalExpensesViewModel>();

// Update existing ViewModels to include new service
// DashboardViewModel, ReportsViewModel, UnifiedTransactionService
```

---

## 12. Testing Strategy

### 12.1 Unit Tests

Create `OperationalExpenseServiceTests.cs`:
- ✅ `CreateExpenseAsync_ShouldCalculateTaxCorrectly()`
- ✅ `CreateExpenseAsync_ShouldValidateAmount()`
- ✅ `CreateExpenseAsync_ShouldRequireCustomCategoryName()`
- ✅ `GetExpensesByCategoryAsync_ShouldFilterCorrectly()`
- ✅ `GetMonthlyExpensesAsync_ShouldCalculateCorrectly()`
- ✅ `GetTopExpenseCategoriesAsync_ShouldReturnTopN()`

### 12.2 Integration Tests

Create `OperationalExpenseWorkflowTests.cs`:
- ✅ End-to-end expense creation and retrieval
- ✅ Dashboard integration tests
- ✅ Reports integration tests

### 12.3 UI Tests

- ✅ Form validation
- ✅ Category selection
- ✅ Tax calculation
- ✅ Filter functionality

---

## 13. Migration & Backward Compatibility

### Phase 1: Data Model (Week 1)
1. Create model classes
2. Add DbContext configuration
3. Create migration
4. Update database

### Phase 2: Backend (Week 2)
1. Repository implementation
2. Service implementation
3. Unit tests
4. Integration tests

### Phase 3: Dashboard & Reports (Week 3)
1. Update DashboardViewModel
2. Update ReportsViewModel
3. Update UnifiedTransactionService
4. Update UI

### Phase 4: Dedicated Management UI (Week 4)
1. Create OperationalExpensesView
2. Create OperationalExpensesViewModel
3. Add navigation
4. UI tests

---

## 14. Benefits of This Design

✅ **Consistency**: Follows existing architectural pattern  
✅ **Separation of Concerns**: Clear domain boundaries  
✅ **Maintainability**: Easy to extend categories  
✅ **Testability**: Well-isolated components  
✅ **Flexibility**: Custom categories support  
✅ **Scalability**: Can add approval workflows, budgets, etc.  
✅ **Reporting**: Integrates seamlessly with existing reports  
✅ **Performance**: Indexed queries, parallel loading  

---

## 15. Future Enhancements

### Phase 2 Features
- 📊 **Budget Management**: Set category-wise budgets, alerts
- 📈 **Expense Analytics**: Trend analysis, forecasting
- 📎 **Attachment Support**: Store receipts/invoices
- ✅ **Approval Workflow**: Multi-level approval for high-value expenses
- 🔔 **Notifications**: Budget exceeded alerts
- 📊 **Charts**: Expense breakdown pie charts, trend lines
- 🏷️ **Tags**: Custom tags for better categorization
- 🔁 **Recurring Expenses**: Auto-generate monthly rent, utilities

---

## 16. Database Indexes for Performance

```sql
CREATE INDEX IX_OperationalExpenses_ExpenseDate ON OperationalExpenses(ExpenseDate);
CREATE INDEX IX_OperationalExpenses_Category ON OperationalExpenses(Category);
CREATE INDEX IX_OperationalExpenses_PartyId ON OperationalExpenses(PartyId);
CREATE INDEX IX_OperationalExpenses_IsApproved ON OperationalExpenses(IsApproved);
```

---

## 17. Summary & Next Steps

### This Design Provides:
1. ✅ **Clean separation** - New domain model, not polluting existing systems
2. ✅ **Easy maintenance** - Follows established patterns
3. ✅ **Dashboard integration** - New summary cards
4. ✅ **Reports integration** - New report type
5. ✅ **Unified view** - Integrated with UnifiedTransactionService
6. ✅ **Extensibility** - Custom categories, approval workflows ready

### Implementation Order:
1. **Models & Database** → Repository → Service
2. **Dashboard Integration** → Summary cards, metrics
3. **Reports Integration** → New report type, filters
4. **Dedicated UI** → Full management page
5. **Testing** → Comprehensive test coverage

### Estimated Timeline:
- **Models & Backend**: 1 week
- **Dashboard & Reports**: 1 week
- **UI Implementation**: 1-2 weeks
- **Testing & Documentation**: 1 week

**Total: 4-5 weeks** for complete implementation

---

## 18. Questions for Consideration

Before implementation, consider:

1. **Categories**: Do you need more specific categories for your factory?
2. **Approval**: Do expenses need approval workflows?
3. **Budgets**: Do you want budget tracking per category?
4. **Recurring**: Do you have recurring monthly expenses to automate?
5. **Tax**: Do you need GST/tax compliance features?
6. **Attachments**: Should receipts/invoices be stored?

---

**Ready to implement?** Let me know if you'd like me to start with any specific phase!

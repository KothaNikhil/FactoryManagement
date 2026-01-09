# FactoryManagement.Core - Usage Examples

## ?? Library Overview

The `FactoryManagement.Core` library provides programmatic access to all Factory Management business logic without requiring the WPF UI.

---

## ?? Quick Start

### Basic Usage

```csharp
using FactoryManagement.Core;
using FactoryManagement.Core.Models;

// Create client (uses default database location)
using var client = new FactoryManagementClient();

// Get all active users
var users = await client.Users.GetActiveUsersAsync();
foreach (var user in users)
{
    Console.WriteLine($"User: {user.Username} ({user.Role})");
}

// Get all items
var items = await client.Items.GetAllItemsAsync();
foreach (var item in items)
{
    Console.WriteLine($"Item: {item.ItemName} - Stock: {item.CurrentStock} {item.Unit}");
}
```

---

## ?? Advanced Usage

### Custom Database Location

```csharp
// Use a specific database file
using var client = new FactoryManagementClient("C:\\MyData\\production.db");

// Use a test database
using var testClient = new FactoryManagementClient("C:\\Tests\\test.db");
```

### Custom DI Configuration

```csharp
using var client = new FactoryManagementClient(services =>
{
    // Add custom logging
    services.AddLogging(builder =>
    {
        builder.AddConsole();
        builder.AddDebug();
    });
    
    // Add custom services
    services.AddSingleton<IMyCustomService, MyCustomService>();
});
```

---

## ?? Complete Examples

### Example 1: User Management

```csharp
using FactoryManagement.Core;
using FactoryManagement.Core.Models;

using var client = new FactoryManagementClient();

// Create a new user
var newUser = new User
{
    Username = "john_doe",
    Role = "Operator",
    IsActive = true
};
await client.Users.CreateUserAsync(newUser);

// Get user by username
var user = await client.Users.GetUserByUsernameAsync("john_doe");

// Update user
user.Role = "Manager";
await client.Users.UpdateUserAsync(user);

// Get all active users
var activeUsers = await client.Users.GetActiveUsersAsync();
```

### Example 2: Party (Customer/Supplier) Management

```csharp
// Create a new supplier
var supplier = new Party
{
    Name = "ABC Suppliers Ltd",
    MobileNumber = "9876543210",
    Place = "Mumbai",
    PartyType = PartyType.Seller,
    IsActive = true
};
await client.Parties.AddPartyAsync(supplier, userId: 1);

// Get all parties
var allParties = await client.Parties.GetAllPartiesAsync();

// Get parties by type
var sellers = await client.Parties.GetPartiesByTypeAsync(PartyType.Seller);
var buyers = await client.Parties.GetPartiesByTypeAsync(PartyType.Buyer);
```

### Example 3: Item/Inventory Management

```csharp
// Add a new item
var newItem = new Item
{
    ItemName = "Premium Rice",
    CurrentStock = 1000,
    Unit = "Kg"
};
await client.Items.AddItemAsync(newItem, userId: 1);

// Update stock (for buy/sell/wastage)
await client.Items.UpdateStockAsync(
    itemId: newItem.ItemId,
    quantityChange: 500,
    transactionType: TransactionType.Buy
);

// Get item by ID
var item = await client.Items.GetItemByIdAsync(newItem.ItemId);
Console.WriteLine($"Current Stock: {item.CurrentStock} {item.Unit}");
```

### Example 4: Transaction Management

```csharp
// Record a purchase transaction
var purchase = new Transaction
{
    ItemId = 1,
    ItemName = "Rice",
    PartyId = 1,
    PartyName = "ABC Suppliers",
    TransactionType = TransactionType.Buy,
    Quantity = 500,
    PricePerUnit = 45.50m,
    TotalAmount = 22750m,
    PaymentMode = PaymentMode.Cash,
    TransactionDate = DateTime.Now,
    EnteredBy = 1,
    Notes = "Monthly stock purchase"
};
await client.Transactions.AddTransactionAsync(purchase);

// Record a sales transaction
var sale = new Transaction
{
    ItemId = 1,
    ItemName = "Rice",
    PartyId = 2,
    PartyName = "XYZ Traders",
    TransactionType = TransactionType.Sell,
    Quantity = 200,
    PricePerUnit = 50m,
    TotalAmount = 10000m,
    PaymentMode = PaymentMode.Bank,
    TransactionDate = DateTime.Now,
    EnteredBy = 1,
    Notes = "Weekly sale"
};
await client.Transactions.AddTransactionAsync(sale);

// Get recent transactions
var recentTrans = await client.Transactions.GetRecentTransactionsAsync(50);

// Get transactions for a specific date range
var startDate = DateTime.Now.AddDays(-30);
var endDate = DateTime.Now;
var monthlyTrans = await client.Transactions.GetTransactionsByDateRangeAsync(startDate, endDate);
```

### Example 5: Financial Transactions & Loans

```csharp
// Create a loan given to a party
var loan = new LoanAccount
{
    PartyId = 3,
    LoanType = LoanType.Given,
    OriginalAmount = 100000m,
    InterestRate = 12m, // 12% per annum
    StartDate = DateTime.Now,
    DueDate = DateTime.Now.AddMonths(6),
    CreatedBy = 1,
    Notes = "Business expansion loan"
};
await client.FinancialTransactions.CreateLoanAsync(loan, PaymentMode.Cash);

// Record a payment against the loan
await client.FinancialTransactions.RecordPaymentAsync(
    loanAccountId: loan.LoanAccountId,
    paymentAmount: 10000m,
    paymentMode: PaymentMode.Cash,
    userId: 1,
    notes: "Monthly installment"
);

// Update interest
await client.FinancialTransactions.UpdateLoanInterestAsync(loan.LoanAccountId);

// Get all loans
var allLoans = await client.FinancialTransactions.GetAllLoansAsync();

// Get overdue loans
var overdueLoans = await client.FinancialTransactions.GetOverdueLoansAsync();

// Get financial summary
var summary = await client.FinancialTransactions.GetFinancialSummaryAsync();
Console.WriteLine($"Total Loans Given: {summary["TotalLoansGiven"]}");
Console.WriteLine($"Total Loans Taken: {summary["TotalLoansTaken"]}");
```

### Example 6: Worker Wage Management

```csharp
// Add a new worker
var worker = new Worker
{
    Name = "Ramesh Kumar",
    MobileNumber = "9876543210",
    Address = "Delhi",
    Status = WorkerStatus.Active,
    Rate = 500m, // Daily rate
    JoiningDate = DateTime.Now
};
await client.Wages.AddWorkerAsync(worker, userId: 1);

// Record daily wage payment
var wageTransaction = new WageTransaction
{
    WorkerId = worker.WorkerId,
    TransactionType = WageTransactionType.DailyWage,
    TransactionDate = DateTime.Now,
    DaysWorked = 1,
    Rate = 500m,
    Amount = 500m,
    NetAmount = 500m,
    PaymentMode = PaymentMode.Cash,
    EnteredBy = 1
};
await client.Wages.RecordWageAsync(wageTransaction);

// Record advance payment
var advance = new WageTransaction
{
    WorkerId = worker.WorkerId,
    TransactionType = WageTransactionType.AdvanceGiven,
    TransactionDate = DateTime.Now,
    Amount = 2000m,
    NetAmount = 2000m,
    PaymentMode = PaymentMode.Cash,
    EnteredBy = 1,
    Notes = "Festival advance"
};
await client.Wages.RecordWageAsync(advance);

// Get all workers
var workers = await client.Wages.GetAllWorkersAsync();

// Get transactions for a worker
var workerTransactions = await client.Wages.GetTransactionsByWorkerAsync(worker.WorkerId);
```

### Example 7: Operational Expenses

```csharp
// Create expense category (one-time setup)
var category = new ExpenseCategory
{
    CategoryName = "Fuel",
    Description = "Vehicle fuel expenses",
    CreatedBy = 1
};
await client.ExpenseCategories.CreateCategoryAsync(category);

// Record an operational expense
var expense = new OperationalExpense
{
    ExpenseCategoryId = category.ExpenseCategoryId,
    Amount = 5000m,
    ExpenseDate = DateTime.Now,
    SpentBy = 2,
    PaymentMode = PaymentMode.Cash,
    EnteredBy = 1,
    Notes = "Truck diesel for delivery"
};
await client.OperationalExpenses.CreateExpenseAsync(expense);

// Get expenses by date range
var expenses = await client.OperationalExpenses.GetExpensesByDateRangeAsync(
    startDate: DateTime.Now.AddMonths(-1),
    endDate: DateTime.Now
);

// Get total expenses
var totalExpenses = await client.OperationalExpenses.GetTotalExpensesAsync();
Console.WriteLine($"Total Expenses: {totalExpenses:C}");

// Get monthly expenses
var monthlyExpenses = await client.OperationalExpenses.GetMonthlyExpensesAsync(
    year: 2026,
    month: 1
);
```

### Example 8: Cash Book Management

```csharp
// Initialize cash book with opening balance
await client.CashBook.InitializeAsync(
    openingBalance: 50000m,
    userId: 1,
    notes: "Initial cash on hand"
);

// Get today's cash flow
var today = DateTime.Today;
var cashFlow = await client.CashBook.CalculateCashFlowAsync(today);
Console.WriteLine($"Opening: {cashFlow.OpeningBalance:C}");
Console.WriteLine($"Cash In: {cashFlow.TotalCashIn:C}");
Console.WriteLine($"Cash Out: {cashFlow.TotalCashOut:C}");
Console.WriteLine($"Expected Closing: {cashFlow.ExpectedClosingBalance:C}");

// Reconcile cash for the day
await client.CashBook.ReconcileCashAsync(
    date: today,
    actualCashCounted: 48500m,
    userId: 1,
    notes: "End of day reconciliation",
    discrepancyReason: "Small shortage"
);

// Get unreconciled days
var unreconciledDays = await client.CashBook.GetUnreconciledDaysAsync();
Console.WriteLine($"Unreconciled days: {unreconciledDays.Count()}");

// Get current cash in hand
var currentCash = await client.CashBook.GetCurrentCashInHandAsync();
Console.WriteLine($"Current Cash: {currentCash:C}");
```

### Example 9: Unified Transaction View

```csharp
// Get all transactions (inventory + financial + wages) for a date range
var allTransactions = await client.UnifiedTransactions.GetUnifiedTransactionsAsync(
    startDate: DateTime.Now.AddMonths(-1),
    endDate: DateTime.Now
);

foreach (var trans in allTransactions)
{
    Console.WriteLine($"{trans.Date:yyyy-MM-dd} | {trans.Type} | {trans.Description} | {trans.Amount:C}");
}

// Filter by payment mode
var cashTransactions = allTransactions.Where(t => t.PaymentMode == "Cash");

// Get summary
var totalDebit = allTransactions.Where(t => t.DebitCredit == "Debit").Sum(t => t.Amount);
var totalCredit = allTransactions.Where(t => t.DebitCredit == "Credit").Sum(t => t.Amount);
Console.WriteLine($"Total Debit: {totalDebit:C}");
Console.WriteLine($"Total Credit: {totalCredit:C}");
Console.WriteLine($"Net: {(totalCredit - totalDebit):C}");
```

---

## ?? Usage in ASP.NET Core Web API

### Program.cs (Minimal API - .NET 8)

```csharp
using FactoryManagement.Core;
using FactoryManagement.Core.Models;

var builder = WebApplication.CreateBuilder(args);

// Add Factory Management Core services
builder.Services.AddFactoryManagementCore("Data Source=factory.db");

// Add controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### Example Controller

```csharp
using Microsoft.AspNetCore.Mvc;
using FactoryManagement.Core.Services;
using FactoryManagement.Core.Models;

namespace FactoryManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetAll()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetById(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
            return NotFound();
        return Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<User>> Create(User user)
    {
        var created = await _userService.CreateUserAsync(user);
        return CreatedAtAction(nameof(GetById), new { id = created.UserId }, created);
    }
}
```

---

## ?? Best Practices

### 1. Always Use `using` for Client

```csharp
// Good - disposes properly
using var client = new FactoryManagementClient();
// ... use client

// Bad - may leak resources
var client = new FactoryManagementClient();
// ... use client
// forgot to dispose!
```

### 2. Handle Exceptions

```csharp
try
{
    using var client = new FactoryManagementClient();
    var users = await client.Users.GetAllUsersAsync();
}
catch (InvalidOperationException ex)
{
    // Business logic error
    Console.WriteLine($"Business error: {ex.Message}");
}
catch (Exception ex)
{
    // Unexpected error
    Console.WriteLine($"Unexpected error: {ex.Message}");
}
```

### 3. Use Dependency Injection in ASP.NET Core

```csharp
// Instead of creating FactoryManagementClient in controllers,
// inject individual services:

public class ProductsController : ControllerBase
{
    private readonly IItemService _itemService;
    private readonly ITransactionService _transactionService;

    public ProductsController(
        IItemService itemService,
        ITransactionService transactionService)
    {
        _itemService = itemService;
        _transactionService = transactionService;
    }
}
```

---

## ?? Thread Safety

The `FactoryManagementClient` is **NOT thread-safe**. Each thread should create its own instance:

```csharp
// Good - one instance per thread
await Parallel.ForEachAsync(items, async (item, ct) =>
{
    using var client = new FactoryManagementClient();
    await client.Items.UpdateItemAsync(item);
});

// Bad - sharing client across threads
using var client = new FactoryManagementClient(); // DON'T DO THIS
await Parallel.ForEachAsync(items, async (item, ct) =>
{
    await client.Items.UpdateItemAsync(item); // NOT THREAD-SAFE
});
```

---

## ?? Performance Tips

### 1. Reuse DbContext for Batch Operations

```csharp
using var client = new FactoryManagementClient();
var dbContext = client.GetDbContext();

// Multiple operations in one transaction
using var transaction = await dbContext.Database.BeginTransactionAsync();
try
{
    await client.Items.AddItemAsync(item1);
    await client.Items.AddItemAsync(item2);
    await client.Items.AddItemAsync(item3);
    
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

### 2. Use Async/Await Properly

```csharp
// Good - proper async
var users = await client.Users.GetAllUsersAsync();

// Bad - blocking async
var users = client.Users.GetAllUsersAsync().Result; // DON'T DO THIS
```

---

## ?? Next Steps

- See `README-IMPLEMENTATION.md` for WPF integration
- Check `VERIFICATION_CHECKLIST.md` for testing guidelines
- Refer to XML documentation in code for detailed API docs

---

## ?? Support

For issues or questions about the Core library:
1. Check the implementation progress in `IMPLEMENTATION_PROGRESS.md`
2. Review this usage guide
3. Examine the XML documentation in the code

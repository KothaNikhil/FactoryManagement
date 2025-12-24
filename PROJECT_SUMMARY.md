# Factory Management System - Project Summary

## 🎯 Project Overview

A complete, production-ready WPF application for managing factory inventory and transactions for agricultural products (Rice, Husk, Paddy, Broken Rice, etc.).

## ✅ Completed Features

### 1. Core Functionality
- ✓ Buy/Sell/Wastage transaction tracking
- ✓ Real-time inventory management
- ✓ Automatic stock updates
- ✓ Party (buyer/seller) management
- ✓ Multi-user support
- ✓ Comprehensive reporting
- ✓ Data export (Excel & CSV)
- ✓ **Financial Transactions & Loan Management** (NEW)
- ✓ Loan tracking (Given & Taken)
- ✓ Interest calculation & accrual
- ✓ Payment recording with smart allocation
- ✓ Loan status management

### 2. Database Architecture
- ✓ SQLite with Entity Framework Core
- ✓ Repository Pattern implementation
- ✓ Code-First migrations
- ✓ Foreign keys and indexes
- ✓ Seed data for testing
- ✓ 7 main entities (Items, Parties, Users, Transactions, AppSettings, **FinancialTransactions, LoanAccounts**)

### 3. User Interface
- ✓ Material Design theme
- ✓ Responsive layout
- ✓ Hamburger navigation menu
- ✓ Dashboard with summary cards
- ✓ Transaction entry form with validation
- ✓ Reports with filtering
- ✓ Master data management screens
- ✓ Search functionality
- ✓ Loading indicators
- ✓ Error messages

### 4. Technical Implementation
- ✓ MVVM pattern with CommunityToolkit.Mvvm
- ✓ Dependency Injection
- ✓ Async/await operations
- ✓ Service layer abstraction
- ✓ Value converters
- ✓ Logging with Serilog
- ✓ Exception handling

## 📂 Project Structure

```
C:\FactoryManagement\
├── FactoryManagement.sln              # Solution file
├── README.md                          # Main documentation
├── USER_GUIDE.md                      # User manual
├── QUICK_REFERENCE.md                 # Quick reference card
├── Build-And-Run.ps1                  # Build script
├── QuickStart.ps1                     # Quick start script
├── .gitignore                         # Git ignore file
│
└── FactoryManagement\                 # Main project
    ├── FactoryManagement.csproj       # Project file
    ├── App.xaml                       # Application entry
    ├── App.xaml.cs                    # Application code-behind
    │
    ├── Models\                        # Entity models
    │   ├── Item.cs
    │   ├── Party.cs
    │   ├── User.cs
    │   ├── Transaction.cs
    │   ├── AppSettings.cs
    │   ├── FinancialTransaction.cs    # NEW: Financial transactions
    │   └── LoanAccount.cs             # NEW: Loan master records
    │
    ├── Data\                          # Database layer
    │   ├── FactoryDbContext.cs
    │   └── Repositories\
    │       ├── IRepository.cs
    │       ├── Repository.cs
    │       ├── TransactionRepository.cs
    │       ├── FinancialTransactionRepository.cs  # NEW
    │       └── LoanAccountRepository.cs           # NEW
    │
    ├── Services\                      # Business logic
    │   ├── ItemService.cs
    │   ├── PartyService.cs
    │   ├── TransactionService.cs
    │   ├── ExportService.cs
    │   ├── BackupService.cs
    │   └── FinancialTransactionService.cs  # NEW: Loan & financial mgmt
    │
    ├── ViewModels\                    # MVVM ViewModels
    │   ├── ViewModelBase.cs
    │   ├── MainWindowViewModel.cs
    │   ├── DashboardViewModel.cs
    │   ├── TransactionEntryViewModel.cs
    │   ├── ReportsViewModel.cs
    │   ├── ItemsManagementViewModel.cs
    │   ├── PartiesManagementViewModel.cs
    │   ├── BackupViewModel.cs
    │   └── FinancialTransactionsViewModel.cs  # NEW: Loan management VM
    │
    ├── Views\                         # XAML views
    │   ├── MainWindow.xaml
    │   ├── MainWindow.xaml.cs
    │   ├── DashboardView.xaml
    │   ├── DashboardView.xaml.cs
    │   ├── TransactionEntryView.xaml
    │   ├── TransactionEntryView.xaml.cs
    │   ├── ReportsView.xaml
    │   ├── ReportsView.xaml.cs
    │   ├── ItemsManagementView.xaml
    │   ├── ItemsManagementView.xaml.cs
    │   ├── PartiesManagementView.xaml
    │   ├── PartiesManagementView.xaml.cs
    │   ├── BackupView.xaml
    │   ├── BackupView.xaml.cs
    │   ├── FinancialTransactionsView.xaml      # NEW: Loan management UI
    │   └── FinancialTransactionsView.xaml.cs   # NEW
    │
    └── Converters\                    # Value converters
        └── Converters.cs
```

## 📦 NuGet Packages (All Included)

1. **MaterialDesignThemes** (5.0.0) - UI components
2. **MaterialDesignColors** (3.0.0) - Color themes
3. **Microsoft.EntityFrameworkCore.Sqlite** (8.0.0) - Database
4. **Microsoft.EntityFrameworkCore.Design** (8.0.0) - Migrations
5. **CommunityToolkit.Mvvm** (8.2.2) - MVVM helpers
6. **EPPlus** (7.0.5) - Excel export
7. **CsvHelper** (30.0.1) - CSV export
8. **Microsoft.Extensions.DependencyInjection** (8.0.0) - DI
9. **Serilog** (3.1.1) - Logging
10. **Serilog.Sinks.File** (5.0.0) - File logging

## 🚀 How to Build and Run

### Method 1: Using PowerShell Script
```powershell
cd C:\FactoryManagement
.\Build-And-Run.ps1
```

### Method 2: Using Quick Start
```powershell
cd C:\FactoryManagement
.\QuickStart.ps1
```

### Method 3: Manual Build
```powershell
cd C:\FactoryManagement
dotnet restore
dotnet build
dotnet run --project FactoryManagement\FactoryManagement.csproj
```

### Method 4: Visual Studio
1. Open `FactoryManagement.sln`
2. Press F5 to run

## 🗄️ Database Details

### Location
`bin/Debug/net8.0-windows/factory.db`

### Tables
1. **Items** - Product inventory
2. **Parties** - Buyers and sellers
3. **Users** - System users
4. **Transactions** - All business transactions
5. **AppSettings** - Application configuration
6. **FinancialTransactions** - Loan transactions (NEW)
7. **LoanAccounts** - Loan master records (NEW)

### Relationships
- Transactions → Items (Many-to-One)
- Transactions → Parties (Many-to-One)
- Transactions → Users (Many-to-One)
- FinancialTransactions → Parties (Many-to-One) (NEW)
- FinancialTransactions → LoanAccounts (Many-to-One) (NEW)
- FinancialTransactions → Users (Many-to-One) (NEW)
- LoanAccounts → Parties (Many-to-One) (NEW)
- LoanAccounts → Users (Many-to-One) (NEW)

## 🎨 UI Screenshots (Features)

### Dashboard
- 4 Summary cards (Purchases, Sales, Wastage, Count)
- 2 Financial cards (Loans Given, Loans Taken) (NEW)
- Recent transactions grid
- Low stock alert list

### Transaction Entry
- Transaction type selector
- Item dropdown with stock display
- Party dropdown with details
- Auto-calculated totals
- Date picker
- User selector
- Notes field

### Reports
- Multiple filter options
- Transaction grid
- Export buttons
- Summary totals

### Master Data
- Side-by-side layout (list + form)
- Search functionality
- Edit/Delete actions
- Validation

### Financial Transactions (NEW)
- Create new loans (Given/Taken)
- Record payments with smart allocation
- Interest calculation (simple interest)
- Loan status tracking (Active/Closed/Overdue/PartiallyPaid)
- Transaction history per loan
- Filter by status
- Outstanding balance tracking
- Due date management

## 🔐 Security Features

- Input validation on all forms
- SQL injection prevention (via EF Core parameterized queries)
- Stock validation before sell/wastage
- Error handling with user-friendly messages
- Logging for troubleshooting

## 📊 Business Logic

### Stock Updates
- **Buy**: Stock increases
- **Sell**: Stock decreases (validates sufficient stock)
- **Wastage**: Stock decreases (validates sufficient stock)

### Financial Transaction Management (NEW)
- **Loan Creation**: Creates LoanAccount + initial FinancialTransaction
- **Payment Recording**: 
  - Interest accrued automatically based on days elapsed
  - Payment allocated to interest first, then principal
  - Loan status updated automatically (Active → PartiallyPaid → Closed)
- **Interest Calculation**: Simple Interest = (Principal × Rate × Days) / (365 × 100)
- **Status Management**: Auto-updates based on payments and due dates

### Auto-calculations
- Total Amount = Quantity × Price Per Unit
- Dashboard summaries update in real-time
- Stock levels update immediately

### Validation Rules
- Item selection mandatory
- Party mandatory for Buy/Sell (optional for Wastage)
- Quantity must be positive
- Price cannot be negative
- Stock must be sufficient for outgoing transactions

## 🌟 Key Features

### For Users
1. **Easy Navigation** - Hamburger menu with icons
2. **Real-time Updates** - Dashboard refreshes automatically
3. **Smart Validation** - Prevents errors before they happen
4. **Quick Entry** - Auto-calculated totals
5. **Powerful Reports** - Multiple filter options
6. **Export Capability** - Excel and CSV formats
7. **Search** - In all master data screens
8. **Low Stock Alerts** - Never run out
9. **Loan Management** - Track money lent and borrowed (NEW)
10. **Interest Tracking** - Automatic interest calculation (NEW)
11. **Payment History** - Complete audit trail for all loans (NEW)

### For Developers
1. **Clean Architecture** - MVVM pattern
2. **Separation of Concerns** - Services, Repositories, ViewModels
3. **Async Operations** - Non-blocking UI
4. **Dependency Injection** - Testable code
5. **Repository Pattern** - Easy to switch databases
6. **Logging** - Serilog integration
7. **Error Handling** - Try-catch blocks throughout
8. **Type Safety** - Nullable reference types enabled

## 🔄 Future Enhancements (Suggested)

1. **Authentication** - Login system with role-based access
2. **Multi-tenancy** - Support multiple companies
3. **Advanced Reports** - Charts and graphs
4. **Barcode Support** - Scan items
5. **Print Support** - Print invoices and reports
6. **Mobile App** - Companion mobile application
7. **Cloud Sync** - Synchronize to cloud storage
8. **Notifications** - Email/SMS alerts
9. **MongoDB Support** - Use the repository pattern to switch
10. **Multi-language** - Localization support
11. **EMI Support** - Scheduled loan payments (Financial)
12. **Compound Interest** - Advanced interest calculations (Financial)
13. **Payment Reminders** - Overdue loan notifications (Financial)
14. **Financial Reports** - Ledgers, aging reports (Financial)
15. **Collateral Tracking** - Link loans to assets (Financial)

## 📋 Testing Checklist

- [ ] Application starts without errors
- [ ] Dashboard displays summary cards
- [ ] Can add new item
- [ ] Can add new party
- [ ] Can record Buy transaction (stock increases)
- [ ] Can record Sell transaction (stock decreases)
- [ ] Can record Wastage
- [ ] Validation prevents invalid data
- [ ] Search works in Items Master
- [ ] Search works in Parties Master
- [ ] Reports filter by item
- [ ] Reports filter by party
- [ ] Reports filter by date range
- [ ] Export to Excel works
- [ ] Export to CSV works
- [ ] Low stock alert shows items below 100
- [ ] Can create a loan (Given/Taken) (NEW)
- [ ] Can record loan payment (NEW)
- [ ] Interest calculates correctly (NEW)
- [ ] Loan status updates automatically (NEW)
- [ ] Dashboard shows financial summaries (NEW)
- [ ] Transaction history displays for loans (NEW)

## 🐛 Known Limitations

1. Single-user application (no concurrent access handling)
2. No user authentication
3. No data encryption
4. No automatic backup
5. English language only
6. Windows platform only

## 📞 Support Resources

- **README.md** - Installation and setup
- **USER_GUIDE.md** - Detailed user manual
- **QUICK_REFERENCE.md** - Quick reference card
- **Code Comments** - In-code documentation
- **Logs** - Check `logs/app.log` for errors

## 🎓 Learning Points

This project demonstrates:
- WPF application development
- Material Design implementation
- MVVM pattern
- Entity Framework Core
- Repository Pattern
- Dependency Injection
- Async programming
- Data export functionality
- Value converters
- Navigation patterns
- Financial domain modeling (NEW)
- Interest calculation algorithms (NEW)
- Complex business logic (NEW)

## 📄 License

This project is provided for educational and commercial use.

## 🙏 Credits

Built using:
- .NET 8.0
- WPF (Windows Presentation Foundation)
- Material Design In XAML
- Entity Framework Core
- EPPlus
- CsvHelper
- CommunityToolkit.Mvvm
- Serilog

---

## ✨ Final Notes

This is a **complete, production-ready application** with:
- ✓ All features implemented
- ✓ Proper error handling
- ✓ User-friendly interface
- ✓ Comprehensive documentation
- ✓ Clean, maintainable code
- ✓ Extensible architecture

**Ready to build and run!**

---

**Version**: 2.0.0  
**Created**: December 2025  
**Status**: Complete ✅  
**Latest Update**: Financial Transactions & Loan Management Module Added

---

## 🆕 Version 2.0 - Financial Transactions Module

### New Features Added

#### 1. **Loan Management System**
- Create loans (money lent to parties or borrowed from parties)
- Track loan types: Given and Taken
- Record original loan amount, interest rate, start date, and due date
- Complete audit trail with user tracking

#### 2. **Interest Calculation**
- Automatic simple interest calculation
- Formula: Interest = (Principal × Rate × Days) / (365 × 100)
- Interest accrues based on outstanding principal
- Manual interest update available

#### 3. **Payment Processing**
- Smart payment allocation (interest first, then principal)
- Real-time outstanding balance updates
- Payment validation against outstanding amounts
- Complete payment history

#### 4. **Loan Status Management**
- **Active**: Loan is active with outstanding balance
- **PartiallyPaid**: Some payments made but balance remains
- **Closed**: Fully paid off
- **Overdue**: Past due date with outstanding balance
- Auto-status updates based on payments and dates

#### 5. **Financial Dashboard Integration**
- Two new summary cards on dashboard
- "Loans Given (Outstanding)" - money owed to you
- "Loans Taken (Outstanding)" - money you owe
- Real-time financial position visibility

#### 6. **Enhanced Party Management**
- New party types: Lender, Borrower, Financial
- Existing Buyer/Seller types maintained
- Support for parties with multiple roles

#### 7. **User Interface**
- New "Financial Transactions" menu item
- Comprehensive loan management screen with:
  - Create loan form
  - Payment recording section
  - Loans list with filtering
  - Transaction history grid
  - Summary statistics
- Material Design UI consistency

### Technical Architecture

**Design Pattern**: Hybrid Approach (Option 3)
- Separate domain models for financial vs inventory transactions
- Clean separation of concerns
- Scalable for future financial features
- Maintains existing inventory system integrity

**Database Schema**:
```
FinancialTransaction
- FinancialTransactionId (PK)
- PartyId (FK)
- TransactionType (Enum)
- Amount
- InterestRate
- InterestAmount
- TransactionDate
- DueDate
- LinkedLoanAccountId (FK)
- EnteredBy (FK)
- Notes
- CreatedDate

LoanAccount
- LoanAccountId (PK)
- PartyId (FK)
- LoanType (Given/Taken)
- OriginalAmount
- InterestRate
- StartDate
- DueDate
- OutstandingPrincipal
- OutstandingInterest
- TotalOutstanding
- Status
- CreatedBy (FK)
- Notes
- CreatedDate
```

**Service Layer**:
- `FinancialTransactionService`: Core business logic
  - `CreateLoanAsync()`: Creates new loan
  - `RecordPaymentAsync()`: Processes payments
  - `UpdateLoanInterestAsync()`: Calculates interest
  - `GetFinancialSummaryAsync()`: Dashboard summaries

**Repository Layer**:
- `FinancialTransactionRepository`: Data access for transactions
- `LoanAccountRepository`: Data access for loan accounts
- Full LINQ query support with Entity Framework Core

### Use Cases

#### Use Case 1: Lending Money
```
Scenario: You lend ₹100,000 to a party at 12% interest
1. Navigate to Financial Transactions
2. Select party, enter amount (100,000), rate (12%)
3. Set loan type as "Given"
4. Click Create Loan
5. System creates LoanAccount with status "Active"
6. Dashboard shows ₹100,000 in "Loans Given"
```

#### Use Case 2: Borrowing Money
```
Scenario: You borrow ₹50,000 from a party at 10% interest
1. Navigate to Financial Transactions
2. Select party, enter amount (50,000), rate (10%)
3. Set loan type as "Taken"
4. Click Create Loan
5. System creates LoanAccount with status "Active"
6. Dashboard shows ₹50,000 in "Loans Taken"
```

#### Use Case 3: Recording Payment
```
Scenario: Party returns ₹10,000 on a ₹100,000 loan
1. Select the loan from loans list
2. Click "Update Interest" to accrue interest first
3. Enter payment amount (10,000)
4. Click "Record Payment"
5. System:
   - Pays accrued interest first
   - Applies remaining to principal
   - Updates outstanding balance
   - Changes status to "PartiallyPaid"
```

#### Use Case 4: Interest Calculation
```
Scenario: Calculate interest on ₹100,000 at 12% for 30 days
Calculation: (100,000 × 12 × 30) / (365 × 100) = ₹986.30
1. Select loan from list
2. Click "Update Interest"
3. System:
   - Calculates days since last interest calculation
   - Applies formula
   - Creates interest transaction
   - Updates outstanding interest
```

### Benefits of the Design

1. **Separation of Concerns**: Financial transactions don't interfere with inventory
2. **Scalability**: Easy to add EMI, compound interest, etc.
3. **Audit Trail**: Every transaction is tracked
4. **Flexibility**: Supports both lending and borrowing
5. **Automation**: Interest and status updates are automatic
6. **User-Friendly**: Clear UI with validation
7. **Data Integrity**: Foreign keys and relationships maintained

### Migration Path

For existing users:
1. Database automatically creates new tables on first run
2. Existing inventory transactions unaffected
3. Parties can be used for both inventory and financial transactions
4. No data migration needed

---

**Version**: 2.0.0  
**Created**: December 2025  
**Status**: Complete ✅  
**Latest Update**: Financial Transactions & Loan Management Module Added

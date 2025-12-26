## Backup Tests & OneDrive Note

When running backup-related tests, file sync tools like OneDrive can momentarily lock new backup files under Documents, causing intermittent IO exceptions. The test suite uses a temp-directory backed `TestBackupService` to avoid this, but if you run backups manually:

- Preferred: use a non-synced local folder for backups during testing.
- If using Documents/OneDrive, add a short delay/retry after creation before accessing or deleting.
- See FactoryManagement.Tests/BackupServiceTests.cs for the test-specific implementation that writes backups to `%TEMP%/FactoryManagementTests/<GUID>`.

This keeps tests reliable and prevents flakiness due to external file locks.

## Default Auto Backup

The application maintains a rolling default backup file named `DefaultBackup.json` in Documents\FactoryManagement\Backups. It is automatically updated after each successful data save and on startup. This default backup is write-protected and cannot be deleted from within the application UI. Use timestamped backups (`Backup_yyyyMMdd_HHmmss.json`) for manual archive/restore operations.
# Factory Management System

A comprehensive WPF application for managing factory inventory and transactions for agricultural products.

## Features

### Core Functionality
- **Transaction Management**: Track Buy, Sell, and Wastage transactions
- **Inventory Tracking**: Real-time stock updates for items like Rice, Husk, Paddy, Broken Rice, etc.
- **Party Management**: Manage buyers, sellers, and suppliers
- **Reports & Analytics**: Comprehensive reporting with filtering by item, party, and date range
- **Data Export**: Export reports to Excel (.xlsx) and CSV formats

### Technical Features
- **Modern UI**: Material Design-inspired interface using MaterialDesignThemes
- **MVVM Pattern**: Clean architecture with separation of concerns
- **Repository Pattern**: Abstraction layer for future database migration
- **Entity Framework Core**: SQLite database with Code-First approach
- **Async/Await**: Non-blocking database operations
- **Dependency Injection**: Service-based architecture

## Prerequisites

- .NET 8.0 SDK or later
- Visual Studio 2022 or VS Code
- Windows 10/11

## Installation & Setup

### 1. Restore NuGet Packages

```powershell
cd C:\FactoryManagement
dotnet restore
```

### 2. Create Initial Database Migration

```powershell
cd C:\FactoryManagement\FactoryManagement
dotnet ef migrations add InitialCreate
```

### 3. Build the Application

```powershell
cd C:\FactoryManagement
dotnet build
```

### 4. Run the Application

```powershell
cd C:\FactoryManagement
dotnet run --project FactoryManagement\FactoryManagement.csproj
```

## Database Structure

### Tables

1. **Items**: Product inventory
   - ItemId, ItemName, CurrentStock, Unit

2. **Parties**: Buyers and sellers
   - PartyId, Name, MobileNumber, Place, PartyType

3. **Users**: System users
   - UserId, Username, Role

4. **Transactions**: All business transactions
   - TransactionId, ItemId, PartyId, TransactionType, Quantity, PricePerUnit, TotalAmount, TransactionDate, EnteredBy, Notes

5. **AppSettings**: Application configuration
   - CompanyName, CurrencySymbol, Address

## Usage Guide

### Dashboard
- View summary cards for total purchases, sales, and wastage
- Monitor recent transactions
- Check low stock alerts

### Transaction Entry
1. Select transaction type (Buy/Sell/Wastage)
2. Choose item from dropdown (shows current stock)
3. Select party (optional for wastage)
4. Enter quantity and price per unit
5. Total amount is auto-calculated
6. Select date and entering user
7. Add optional notes
8. Click "SAVE TRANSACTION"

### Reports & Analytics
- **Filter by Item**: View all transactions for a specific item
- **Filter by Party**: View all transactions for a specific buyer/seller
- **Date Range**: Filter transactions between two dates
- **Export**: Download filtered data as Excel or CSV

### Master Data Management

#### Items Management
- Add, edit, and delete inventory items
- Search functionality
- Track current stock levels

#### Parties Management
- Manage buyers, sellers, and suppliers
- Store contact information and location
- Categorize as Buyer, Seller, or Both

## Project Structure

```
FactoryManagement/
├── Models/                  # Entity models
│   ├── Item.cs
│   ├── Party.cs
│   ├── User.cs
│   ├── Transaction.cs
│   └── AppSettings.cs
├── Data/                    # Database context and repositories
│   ├── FactoryDbContext.cs
│   └── Repositories/
├── Services/                # Business logic layer
│   ├── ItemService.cs
│   ├── PartyService.cs
│   ├── TransactionService.cs
│   └── ExportService.cs
├── ViewModels/              # MVVM ViewModels
│   ├── MainWindowViewModel.cs
│   ├── DashboardViewModel.cs
│   ├── TransactionEntryViewModel.cs
│   ├── ReportsViewModel.cs
│   ├── ItemsManagementViewModel.cs
│   └── PartiesManagementViewModel.cs
├── Views/                   # XAML views
│   ├── MainWindow.xaml
│   ├── DashboardView.xaml
│   ├── TransactionEntryView.xaml
│   ├── ReportsView.xaml
│   ├── ItemsManagementView.xaml
│   └── PartiesManagementView.xaml
└── Converters/              # Value converters for XAML binding
```

## NuGet Packages Used

- **MaterialDesignThemes** (5.0.0): Material Design UI components
- **Microsoft.EntityFrameworkCore.Sqlite** (8.0.0): Database ORM
- **CommunityToolkit.Mvvm** (8.2.2): MVVM helpers
- **EPPlus** (7.0.5): Excel export functionality
- **CsvHelper** (30.0.1): CSV export functionality
- **Serilog** (3.1.1): Logging framework

## Default Seed Data

### Users
- Admin (Administrator)
- Manager (Manager)
- Operator (Operator)

### Items
- Rice (1000 Kg)
- Husk (500 Kg)
- Paddy (2000 Kg)
- Broken Rice (300 Kg)
- Bran (150 Kg)

### Parties
- ABC Traders (Mumbai)
- XYZ Suppliers (Delhi)
- PQR Distributors (Bangalore)
- LMN Enterprises (Chennai)

## Troubleshooting

### Database Issues
If the database doesn't initialize:
```powershell
# Delete the database file
Remove-Item C:\FactoryManagement\FactoryManagement\bin\Debug\net8.0-windows\factory.db

# Run the application again - it will recreate the database
dotnet run --project FactoryManagement\FactoryManagement.csproj
```

### Build Errors
Ensure all NuGet packages are restored:
```powershell
dotnet restore
dotnet clean
dotnet build
```

## Future Enhancements

1. **Database Migration**: Easy migration to MongoDB/NoSQL
2. **Multi-user Support**: User authentication and authorization
3. **Advanced Reports**: Charts and graphs for data visualization
4. **Backup/Restore**: Automated database backup functionality
5. **Print Support**: Print invoices and reports
6. **Barcode Integration**: Barcode scanning for items
7. **Multi-currency Support**: Handle multiple currencies
8. **Cloud Sync**: Synchronize data to cloud storage

## License

This project is for educational and commercial use.

## Support

For issues or questions, please check the code comments or create an issue in the repository.

---

**Version**: 1.0.0  
**Last Updated**: December 2025  
**Platform**: .NET 8.0 / WPF / Windows

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using FactoryManagement.Data;
using FactoryManagement.Data.Repositories;
using FactoryManagement.Models;
using FactoryManagement.Services;
using FactoryManagement.ViewModels;
using FactoryManagement.Views;
using Serilog;

namespace FactoryManagement
{
    public partial class App : Application
    {
        private ServiceProvider? _serviceProvider;

        public App()
        {
            // Disable animations system-wide for maximum performance
            Timeline.DesiredFrameRateProperty.OverrideMetadata(
                typeof(Timeline),
                new FrameworkPropertyMetadata { DefaultValue = 30 });
            
            // Use hardware rendering
            RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.Default;
            
            ConfigureLogging();
        }

        private void ConfigureLogging()
        {
            var appDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Factory Management");
            var logsDir = Path.Combine(appDataDir, "logs");
            Directory.CreateDirectory(logsDir);
            var logPath = Path.Combine(logsDir, "app.log");
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            // Check if database exists and has data before initializing
            bool isFirstLaunch = false;
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<FactoryDbContext>();
                try
                {
                    // Check if database is empty (no users at all)
                    isFirstLaunch = !context.Users.Any();
                    Log.Information($"Database check - IsFirstLaunch: {isFirstLaunch}, UserCount: {context.Users.Count()}");
                }
                catch
                {
                    // Database doesn't exist yet
                    isFirstLaunch = true;
                }
            }

            // Initialize database (creates tables if needed)
            InitializeDatabase();

            // Show setup wizard on first launch
            if (isFirstLaunch)
            {
                Log.Information("Showing setup wizard for first launch");
                var setupViewModel = _serviceProvider.GetRequiredService<SetupWizardViewModel>();
                var setupWizard = new SetupWizard(setupViewModel);
                var wizardResult = setupWizard.ShowDialog();
                Log.Information($"Setup wizard completed with result: {wizardResult}");
            }

            // Show login window
            var loginWindow = _serviceProvider.GetRequiredService<LoginWindow>();
            var userService = _serviceProvider.GetRequiredService<IUserService>();
            var loginViewModel = new LoginViewModel(userService, loginWindow);
            loginWindow.DataContext = loginViewModel;
            
            var result = loginWindow.ShowDialog();
            
            if (result == true && loginViewModel.LoggedInUser != null)
            {
                // User logged in successfully, show main window
                var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                var mainViewModel = _serviceProvider.GetRequiredService<MainWindowViewModel>();
                mainWindow.DataContext = mainViewModel;
                
                // Store the logged in user ID to set after initialization
                var loggedInUserId = loginViewModel.LoggedInUser.UserId;
                
                // Set as main application window and show
                MainWindow = mainWindow;
                mainWindow.Show();
                
                // Initialize the dashboard and load data
                await mainViewModel.InitializeAsync();
                
                // Set the logged in user as the selected user (from the loaded ActiveUsers collection)
                mainViewModel.SelectedUser = mainViewModel.ActiveUsers.FirstOrDefault(u => u.UserId == loggedInUserId);
            }
            else
            {
                // User cancelled login, exit application
                Shutdown();
            }
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Database: use per-user writable location under LocalAppData
            var appDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Factory Management");
            Directory.CreateDirectory(appDataDir);
            var dbPath = Path.Combine(appDataDir, "factory.db");
            services.AddDbContext<FactoryDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            // Repositories
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<IFinancialTransactionRepository, FinancialTransactionRepository>();
            services.AddScoped<ILoanAccountRepository, LoanAccountRepository>();
            services.AddScoped<IWorkerRepository, WorkerRepository>();
            services.AddScoped<IWageTransactionRepository, WageTransactionRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IExpenseCategoryRepository, ExpenseCategoryRepository>();
            services.AddScoped<IOperationalExpenseRepository, OperationalExpenseRepository>();
            services.AddScoped<ICashAccountRepository, CashAccountRepository>();
            services.AddScoped<IBalanceHistoryRepository, BalanceHistoryRepository>();

            // Services
            services.AddScoped<IItemService, ItemService>();
            services.AddScoped<IPartyService, PartyService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IExportService, ExportService>();
            services.AddScoped<BackupService>();
            services.AddScoped<IFinancialTransactionService, FinancialTransactionService>();
            services.AddScoped<IWageService, WageService>();
            services.AddScoped<IUnifiedTransactionService, UnifiedTransactionService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IReportExportBuilder, ReportExportBuilder>();
            services.AddScoped<IExpenseCategoryService, ExpenseCategoryService>();
            services.AddScoped<IOperationalExpenseService, OperationalExpenseService>();
            services.AddScoped<ICashAccountService, CashAccountService>();

            // ViewModels
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<NewTransactionViewModel>();
            services.AddTransient<ReportsViewModel>();
            services.AddTransient<InventoryViewModel>();
            services.AddTransient<ContactsViewModel>();
            services.AddTransient<DataBackupViewModel>();
            services.AddTransient<FinancialRecordsViewModel>();
            services.AddTransient<PayrollManagementViewModel>();
            services.AddTransient<WorkersManagementViewModel>();
            services.AddTransient<UsersViewModel>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<OperationalExpensesViewModel>();
            services.AddTransient<ExpenseCategoryManagementViewModel>();
            services.AddTransient<CashAccountsViewModel>();
            services.AddTransient<SetupWizardViewModel>();

            // Views
            services.AddTransient<MainWindow>();
            services.AddTransient<LoginWindow>();
            services.AddTransient<SetupWizard>();
        }

        private void InitializeDatabase()
        {
            using var scope = _serviceProvider!.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<FactoryDbContext>();
            
            try
            {
                // Use EnsureCreated instead of Migrate for simple database creation
                context.Database.EnsureCreated();
                // Apply lightweight schema upgrades for newly added columns
                ApplySchemaUpgrades(context);
                Log.Information("Database initialized successfully");
                
                // Seed initial data if needed
                SeedData(context);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error initializing database");
                MessageBox.Show($"Error initializing database: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Minimal, forward-only schema upgrade helpers for SQLite when model evolves
        private void ApplySchemaUpgrades(FactoryDbContext context)
        {
            try
            {
                // Add missing columns on Transactions table if they don't exist yet
                // SQLite doesn't support IF NOT EXISTS for columns, so try/catch duplicate errors
                TryAddColumn(context, "Transactions", "InputItemId", "INTEGER NULL");
                TryAddColumn(context, "Transactions", "InputQuantity", "NUMERIC NULL");
                TryAddColumn(context, "Transactions", "ConversionRate", "REAL NULL");
                // Payment mode columns
                TryAddColumn(context, "Transactions", "PaymentMode", "INTEGER NOT NULL DEFAULT 0");
                TryAddColumn(context, "FinancialTransactions", "PaymentMode", "INTEGER NOT NULL DEFAULT 0");
                TryAddColumn(context, "WageTransactions", "PaymentMode", "INTEGER NOT NULL DEFAULT 0");
                
                // Add PartyName columns to preserve party names after deletion
                TryAddColumn(context, "Transactions", "PartyName", "TEXT DEFAULT ''");
                TryAddColumn(context, "FinancialTransactions", "PartyName", "TEXT DEFAULT ''");
                TryAddColumn(context, "LoanAccounts", "PartyName", "TEXT DEFAULT ''");
                TryAddColumn(context, "FinancialTransactions", "ModifiedDate", "DATETIME");
                
                // Add ItemName column to preserve item names after deletion
                TryAddColumn(context, "Transactions", "ItemName", "TEXT DEFAULT ''");
                
                // Update NULL values to empty string and populate from related tables
                try
                {
#pragma warning disable EF1002
                    // Update NULL values to empty string
                    context.Database.ExecuteSqlRaw("UPDATE Transactions SET PartyName = '' WHERE PartyName IS NULL");
                    context.Database.ExecuteSqlRaw("UPDATE FinancialTransactions SET PartyName = '' WHERE PartyName IS NULL");
                    context.Database.ExecuteSqlRaw("UPDATE LoanAccounts SET PartyName = '' WHERE PartyName IS NULL");
                    
                    // Populate PartyName from Party table for existing records where PartyName is empty
                    context.Database.ExecuteSqlRaw(@"
                        UPDATE Transactions 
                        SET PartyName = (SELECT Name FROM Parties WHERE Parties.PartyId = Transactions.PartyId) 
                        WHERE PartyId IS NOT NULL AND (PartyName IS NULL OR PartyName = '')");
                    
                    context.Database.ExecuteSqlRaw(@"
                        UPDATE FinancialTransactions 
                        SET PartyName = (SELECT Name FROM Parties WHERE Parties.PartyId = FinancialTransactions.PartyId) 
                        WHERE PartyId IS NOT NULL AND (PartyName IS NULL OR PartyName = '')");
                    
                    context.Database.ExecuteSqlRaw(@"
                        UPDATE LoanAccounts 
                        SET PartyName = (SELECT Name FROM Parties WHERE Parties.PartyId = LoanAccounts.PartyId) 
                        WHERE PartyId IS NOT NULL AND (PartyName IS NULL OR PartyName = '')");
                    
                    // Populate ItemName from Items table for existing records where ItemName is empty
                    context.Database.ExecuteSqlRaw(@"
                        UPDATE Transactions 
                        SET ItemName = (SELECT ItemName FROM Items WHERE Items.ItemId = Transactions.ItemId) 
                        WHERE ItemId IS NOT NULL AND (ItemName IS NULL OR ItemName = '')");
#pragma warning restore EF1002
                    Log.Information("Updated PartyName and ItemName values from related tables");
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, "PartyName update (may already be handled)");
                }

                // Add missing columns on AppSettings for UI preferences
                TryAddColumn(context, "AppSettings", "IsMenuPinned", "INTEGER NOT NULL DEFAULT 1");

                // Add user tracking columns
                TryAddColumn(context, "Items", "CreatedByUserId", "INTEGER NULL");
                TryAddColumn(context, "Items", "ModifiedByUserId", "INTEGER NULL");
                TryAddColumn(context, "Parties", "CreatedByUserId", "INTEGER NULL");
                TryAddColumn(context, "Parties", "ModifiedByUserId", "INTEGER NULL");
                TryAddColumn(context, "Workers", "CreatedByUserId", "INTEGER NULL");
                TryAddColumn(context, "Workers", "ModifiedByUserId", "INTEGER NULL");
                TryAddColumn(context, "WageTransactions", "EnteredBy", "INTEGER NOT NULL DEFAULT 1");

                // Add Operational Expenses tables if they don't exist
                CreateOperationalExpenseTablesIfNeeded(context);

                // Add Cash Balance Management tables if they don't exist
                CreateCashBalanceTablesIfNeeded(context);

                // Add IsDeleted column to existing ExpenseCategories table
                TryAddColumn(context, "ExpenseCategories", "IsDeleted", "INTEGER NOT NULL DEFAULT 0");

                // Note: ExpenseCategories and OperationalExpenses tables will be created
                // automatically by EnsureCreated() if they don't exist, with seed data
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Schema upgrade attempt encountered an issue");
            }
        }

        private void CreateOperationalExpenseTablesIfNeeded(FactoryDbContext context)
        {
            try
            {
                // Check if ExpenseCategories table exists
                var tableExists = context.Database.ExecuteSqlRaw(@"
                    SELECT name FROM sqlite_master WHERE type='table' AND name='ExpenseCategories'
                ") >= 0;

                // If tables don't exist, create them
                if (!tableExists)
                {
                    Log.Information("Creating Operational Expense tables...");

                    // Create ExpenseCategories table
                    context.Database.ExecuteSqlRaw(@"
                        CREATE TABLE IF NOT EXISTS ExpenseCategories (
                            ExpenseCategoryId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                            CategoryName TEXT NOT NULL,
                            Description TEXT NULL,
                            IsDeleted INTEGER NOT NULL DEFAULT 0,
                            CreatedBy INTEGER NOT NULL,
                            CreatedDate DATETIME NOT NULL,
                            ModifiedDate DATETIME NULL,
                            CONSTRAINT FK_ExpenseCategories_Users_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users (UserId) ON DELETE RESTRICT
                        )
                    ");

                    context.Database.ExecuteSqlRaw(@"
                        CREATE UNIQUE INDEX IX_ExpenseCategories_CategoryName ON ExpenseCategories (CategoryName)
                    ");

                    // Create OperationalExpenses table
                    context.Database.ExecuteSqlRaw(@"
                        CREATE TABLE IF NOT EXISTS OperationalExpenses (
                            OperationalExpenseId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                            ExpenseCategoryId INTEGER NOT NULL,
                            Amount NUMERIC NOT NULL,
                            ExpenseDate DATETIME NOT NULL,
                            VendorName TEXT NULL,
                            SpentBy INTEGER NULL,
                            ItemId INTEGER NULL,
                            PaymentMode INTEGER NOT NULL DEFAULT 0,
                            InvoiceNumber TEXT NULL,
                            InvoiceDate DATETIME NULL,
                            IsApproved INTEGER NOT NULL DEFAULT 1,
                            ApprovedBy INTEGER NULL,
                            EnteredBy INTEGER NOT NULL,
                            Notes TEXT NOT NULL DEFAULT '',
                            CreatedDate DATETIME NOT NULL,
                            ModifiedDate DATETIME NULL,
                            AttachmentPath TEXT NULL,
                            CONSTRAINT FK_OperationalExpenses_ExpenseCategories_ExpenseCategoryId FOREIGN KEY (ExpenseCategoryId) REFERENCES ExpenseCategories (ExpenseCategoryId) ON DELETE RESTRICT,
                            CONSTRAINT FK_OperationalExpenses_Users_SpentBy FOREIGN KEY (SpentBy) REFERENCES Users (UserId) ON DELETE SET NULL,
                            CONSTRAINT FK_OperationalExpenses_Items_ItemId FOREIGN KEY (ItemId) REFERENCES Items (ItemId) ON DELETE SET NULL,
                            CONSTRAINT FK_OperationalExpenses_Users_ApprovedBy FOREIGN KEY (ApprovedBy) REFERENCES Users (UserId) ON DELETE SET NULL
                            CONSTRAINT FK_OperationalExpenses_Users_ApprovedBy FOREIGN KEY (ApprovedBy) REFERENCES Users (UserId) ON DELETE RESTRICT
                        )
                    ");

                    context.Database.ExecuteSqlRaw(@"
                        CREATE INDEX IX_OperationalExpenses_ExpenseDate ON OperationalExpenses (ExpenseDate)
                    ");

                    context.Database.ExecuteSqlRaw(@"
                        CREATE INDEX IX_OperationalExpenses_ExpenseCategoryId ON OperationalExpenses (ExpenseCategoryId)
                    ");

                    context.Database.ExecuteSqlRaw(@"
                        CREATE INDEX IX_OperationalExpenses_SpentBy ON OperationalExpenses (SpentBy)
                    ");

                    context.Database.ExecuteSqlRaw(@"
                        CREATE INDEX IX_OperationalExpenses_IsApproved ON OperationalExpenses (IsApproved)
                    ");

                    Log.Information("Operational Expense tables created successfully");
                }
                else
                {
                    Log.Debug("Operational Expense tables already exist");
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Error creating Operational Expense tables (they may already exist)");
            }
        }

        private void CreateCashBalanceTablesIfNeeded(FactoryDbContext context)
        {
            try
            {
                // Check if CashAccounts table exists
                var cashAccountsExists = false;
                try
                {
                    context.Database.ExecuteSqlRaw("SELECT 1 FROM CashAccounts LIMIT 1");
                    cashAccountsExists = true;
                }
                catch
                {
                    cashAccountsExists = false;
                }

                // If tables don't exist, create them
                if (!cashAccountsExists)
                {
                    Log.Information("Creating Cash Balance Management tables...");

                    // Create CashAccounts table
                    context.Database.ExecuteSqlRaw(@"
                        CREATE TABLE IF NOT EXISTS CashAccounts (
                            AccountId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                            AccountName TEXT NOT NULL,
                            AccountType INTEGER NOT NULL,
                            OpeningBalance NUMERIC NOT NULL,
                            CurrentBalance NUMERIC NOT NULL,
                            OpeningDate DATETIME NOT NULL,
                            Description TEXT NOT NULL DEFAULT '',
                            IsActive INTEGER NOT NULL DEFAULT 1,
                            CreatedBy INTEGER NOT NULL,
                            CreatedDate DATETIME NOT NULL,
                            ModifiedDate DATETIME NULL,
                            CONSTRAINT FK_CashAccounts_Users_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users (UserId) ON DELETE RESTRICT
                        )
                    ");

                    context.Database.ExecuteSqlRaw(@"
                        CREATE INDEX IX_CashAccounts_AccountType ON CashAccounts (AccountType)
                    ");

                    context.Database.ExecuteSqlRaw(@"
                        CREATE INDEX IX_CashAccounts_IsActive ON CashAccounts (IsActive)
                    ");

                    // Create BalanceHistories table
                    context.Database.ExecuteSqlRaw(@"
                        CREATE TABLE IF NOT EXISTS BalanceHistories (
                            BalanceHistoryId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                            AccountId INTEGER NOT NULL,
                            ChangeType INTEGER NOT NULL,
                            PreviousBalance NUMERIC NOT NULL,
                            ChangeAmount NUMERIC NOT NULL,
                            NewBalance NUMERIC NOT NULL,
                            TransactionDate DATETIME NOT NULL,
                            Notes TEXT NOT NULL DEFAULT '',
                            EnteredBy INTEGER NOT NULL,
                            CreatedDate DATETIME NOT NULL,
                            TransactionId INTEGER NULL,
                            FinancialTransactionId INTEGER NULL,
                            WageTransactionId INTEGER NULL,
                            OperationalExpenseId INTEGER NULL,
                            CONSTRAINT FK_BalanceHistories_CashAccounts_AccountId FOREIGN KEY (AccountId) REFERENCES CashAccounts (AccountId) ON DELETE CASCADE,
                            CONSTRAINT FK_BalanceHistories_Users_EnteredBy FOREIGN KEY (EnteredBy) REFERENCES Users (UserId) ON DELETE RESTRICT,
                            CONSTRAINT FK_BalanceHistories_Transactions_TransactionId FOREIGN KEY (TransactionId) REFERENCES Transactions (TransactionId) ON DELETE SET NULL,
                            CONSTRAINT FK_BalanceHistories_FinancialTransactions_FinancialTransactionId FOREIGN KEY (FinancialTransactionId) REFERENCES FinancialTransactions (FinancialTransactionId) ON DELETE SET NULL,
                            CONSTRAINT FK_BalanceHistories_WageTransactions_WageTransactionId FOREIGN KEY (WageTransactionId) REFERENCES WageTransactions (WageTransactionId) ON DELETE SET NULL,
                            CONSTRAINT FK_BalanceHistories_OperationalExpenses_OperationalExpenseId FOREIGN KEY (OperationalExpenseId) REFERENCES OperationalExpenses (OperationalExpenseId) ON DELETE SET NULL
                        )
                    ");

                    context.Database.ExecuteSqlRaw(@"
                        CREATE INDEX IX_BalanceHistories_AccountId ON BalanceHistories (AccountId)
                    ");

                    context.Database.ExecuteSqlRaw(@"
                        CREATE INDEX IX_BalanceHistories_TransactionDate ON BalanceHistories (TransactionDate)
                    ");

                    Log.Information("Cash Balance Management tables created successfully");

                    // Seed default cash accounts
                    SeedDefaultCashAccounts(context);
                }
                else
                {
                    Log.Debug("Cash Balance Management tables already exist");
                    
                    // Check if default accounts exist, if not seed them
                    var hasAccounts = false;
                    try
                    {
                        hasAccounts = context.CashAccounts.Any();
                    }
                    catch { }
                    
                    if (!hasAccounts)
                    {
                        SeedDefaultCashAccounts(context);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Error creating Cash Balance Management tables (they may already exist)");
            }
        }

        private void SeedDefaultCashAccounts(FactoryDbContext context)
        {
            try
            {
                // Get admin user ID (should be 2, guest is 1)
                var adminUser = context.Users.FirstOrDefault(u => u.Role == "Admin");
                if (adminUser == null)
                {
                    Log.Warning("Admin user not found, skipping cash account seeding");
                    return;
                }

                Log.Information("Seeding default cash accounts...");

                // Insert Main Cash Account
                context.Database.ExecuteSqlRaw(@"
                    INSERT INTO CashAccounts (AccountName, AccountType, OpeningBalance, CurrentBalance, OpeningDate, Description, IsActive, CreatedBy, CreatedDate)
                    VALUES ('Main Cash Account', 0, 0, 0, datetime('now'), 'Default cash account for daily operations', 1, {0}, datetime('now'))
                ", adminUser.UserId);

                var cashAccountId = context.Database.ExecuteSqlRaw("SELECT last_insert_rowid()");

                // Insert Main Bank Account
                context.Database.ExecuteSqlRaw(@"
                    INSERT INTO CashAccounts (AccountName, AccountType, OpeningBalance, CurrentBalance, OpeningDate, Description, IsActive, CreatedBy, CreatedDate)
                    VALUES ('Main Bank Account', 1, 0, 0, datetime('now'), 'Default bank account for business transactions', 1, {0}, datetime('now'))
                ", adminUser.UserId);

                var bankAccountId = context.Database.ExecuteSqlRaw("SELECT last_insert_rowid()");

                // Create opening balance history entries
                var cashAccount = context.CashAccounts.FirstOrDefault(a => a.AccountName == "Main Cash Account");
                var bankAccount = context.CashAccounts.FirstOrDefault(a => a.AccountName == "Main Bank Account");

                if (cashAccount != null)
                {
                    context.Database.ExecuteSqlRaw(@"
                        INSERT INTO BalanceHistories (AccountId, ChangeType, PreviousBalance, ChangeAmount, NewBalance, TransactionDate, Notes, EnteredBy, CreatedDate)
                        VALUES ({0}, 0, 0, 0, 0, datetime('now'), 'Opening balance', {1}, datetime('now'))
                    ", cashAccount.AccountId, adminUser.UserId);
                }

                if (bankAccount != null)
                {
                    context.Database.ExecuteSqlRaw(@"
                        INSERT INTO BalanceHistories (AccountId, ChangeType, PreviousBalance, ChangeAmount, NewBalance, TransactionDate, Notes, EnteredBy, CreatedDate)
                        VALUES ({0}, 0, 0, 0, 0, datetime('now'), 'Opening balance', {1}, datetime('now'))
                    ", bankAccount.AccountId, adminUser.UserId);
                }

                Log.Information("Default cash accounts seeded successfully");
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Error seeding default cash accounts (they may already exist)");
            }
        }

        private void TryAddColumn(FactoryDbContext context, string table, string column, string sqliteType)
        {
            try
            {
#pragma warning disable EF1002
                context.Database.ExecuteSqlRaw($"ALTER TABLE {table} ADD COLUMN {column} {sqliteType}");
#pragma warning restore EF1002
                Log.Information("Added missing column {Column} to {Table}", column, table);
            }
            catch (Microsoft.Data.Sqlite.SqliteException sex)
            {
                // 1: SQL error; 19: constraint; 2610 duplicate column is reported as error with specific message
                if (sex.Message.Contains("duplicate column name", StringComparison.OrdinalIgnoreCase))
                {
                    // Column already exists; safe to ignore
                    Log.Debug("Column {Column} already exists on {Table}", column, table);
                }
                else
                {
                    throw;
                }
            }
        }
        
        private void SeedData(FactoryDbContext context)
        {
            try
            {
                // Only seed Admin user on first launch - everything else comes from wizard
                if (!context.Users.Any())
                {
                    var adminUser = new User
                    {
                        Username = "Admin",
                        Role = "Admin",
                        IsActive = true
                    };
                    context.Users.Add(adminUser);
                    context.SaveChanges();
                    Log.Information("Admin user created successfully");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error seeding data");
                MessageBox.Show($"Error seeding initial data: {ex.Message}\n\nPlease check the log file for details.", 
                    "Seeding Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.CloseAndFlush();
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }
}

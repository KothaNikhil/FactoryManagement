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
using FactoryManagement.Helpers;
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

            // Initialize database
            InitializeDatabase();

            // Show login window first
            var loginWindow = _serviceProvider.GetRequiredService<LoginWindow>();
            var userService = _serviceProvider.GetRequiredService<IUserService>();
            var loginViewModel = new LoginViewModel(userService, loginWindow);
            loginWindow.DataContext = loginViewModel;
            
            var result = loginWindow.ShowDialog();
            
            if (result == true && loginViewModel.LoggedInUser != null)
            {
                FileLogger.Log($"[App.OnStartup] Login successful - User: {loginViewModel.LoggedInUser.Username} (Role: {loginViewModel.LoggedInUser.Role})");
                // User logged in successfully, show main window
                var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                var mainViewModel = _serviceProvider.GetRequiredService<MainWindowViewModel>();
                mainWindow.DataContext = mainViewModel;
                
                // Store the logged in user ID
                var loggedInUserId = loginViewModel.LoggedInUser.UserId;
                
                // Mark the logged in user as authenticated BEFORE initialization (if admin)
                if (PasswordHelper.IsAdminRole(loginViewModel.LoggedInUser.Role))
                {
                    FileLogger.Log($"[App.OnStartup] Admin user, setting authenticated user");
                    mainViewModel.SetAuthenticatedUser(loggedInUserId);
                }
                
                // Set the selected user first (before loading users) so LoadActiveUsersAsync can preserve it
                FileLogger.Log($"[App.OnStartup] Setting SelectedUser to: {loginViewModel.LoggedInUser.Username}");
                mainViewModel.SelectedUser = loginViewModel.LoggedInUser;
                
                // Load active users and restore the selected user
                FileLogger.Log($"[App.OnStartup] Calling LoadActiveUsersAsync");
                await mainViewModel.LoadActiveUsersAsync();
                
                // Set as main application window and show
                MainWindow = mainWindow;
                mainWindow.Show();
                
                // Initialize the dashboard and load data (this won't change SelectedUser now)
                await mainViewModel.InitializeAsync();
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

            // Views
            services.AddTransient<MainWindow>();
            services.AddTransient<LoginWindow>();
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

        protected override void OnExit(ExitEventArgs e)
        {
            Log.CloseAndFlush();
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }
}

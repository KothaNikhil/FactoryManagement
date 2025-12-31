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
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Schema upgrade attempt encountered an issue");
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
            // Check if data already exists
            if (context.Items.Any() || context.Parties.Any() || context.Users.Any())
                return;
                
            try
            {
                // Seed Users - Guest user is required and cannot be deleted
                var guestUser = new User
                {
                    Username = "Guest",
                    Role = "Guest",
                    IsActive = true
                };
                var adminUser = new User
                {
                    Username = "Admin",
                    Role = "Administrator",
                    IsActive = true
                };
                context.Users.AddRange(guestUser, adminUser);
                context.SaveChanges();
                
                // Seed Items
                var items = new[]
                {
                    new Item { ItemName = "Rice", Unit = "Kg", CurrentStock = 0 },
                    new Item { ItemName = "Husk", Unit = "Kg", CurrentStock = 0 },
                    new Item { ItemName = "Paddy", Unit = "Kg", CurrentStock = 0 },
                    new Item { ItemName = "Broken Rice", Unit = "Kg", CurrentStock = 0 }
                };
                context.Items.AddRange(items);
                
                // Seed Parties
                var parties = new[]
                {
                    new Party { Name = "Sample Supplier", MobileNumber = "9876543210", Place = "City A", PartyType = PartyType.Seller },
                    new Party { Name = "Sample Buyer", MobileNumber = "9876543211", Place = "City B", PartyType = PartyType.Buyer }
                };
                context.Parties.AddRange(parties);
                
                context.SaveChanges();
                Log.Information("Seed data created successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error seeding data");
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

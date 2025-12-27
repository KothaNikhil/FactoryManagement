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

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            // Initialize database
            InitializeDatabase();

            // Show main window
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
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
            services.AddScoped<FinancialTransactionService>();
            services.AddScoped<IWageService, WageService>();
            services.AddScoped<UnifiedTransactionService>();
            services.AddScoped<IUserService, UserService>();

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
            services.AddTransient<UsersViewModel>();

            // Views
            services.AddTransient<MainWindow>();
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
                // Seed Users
                var defaultUser = new User
                {
                    Username = "Admin",
                    Role = "Administrator"
                };
                context.Users.Add(defaultUser);
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

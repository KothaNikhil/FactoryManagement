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

            // Services
            services.AddScoped<IItemService, ItemService>();
            services.AddScoped<IPartyService, PartyService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IExportService, ExportService>();
            services.AddScoped<BackupService>();
            services.AddScoped<FinancialTransactionService>();
            services.AddScoped<IWageService, WageService>();

            // ViewModels
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<TransactionEntryViewModel>();
            services.AddTransient<ReportsViewModel>();
            services.AddTransient<ItemsManagementViewModel>();
            services.AddTransient<PartiesManagementViewModel>();
            services.AddTransient<BackupViewModel>();
            services.AddTransient<FinancialTransactionsViewModel>();
            services.AddTransient<WagesManagementViewModel>();

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

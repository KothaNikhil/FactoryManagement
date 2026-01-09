using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using FactoryManagement.Core.Data;
using FactoryManagement.Core.Data.Repositories;
using FactoryManagement.Core.Services;
using System;
using System.IO;

namespace FactoryManagement.Core
{
    /// <summary>
    /// Main entry point for programmatic access to Factory Management system.
    /// Provides a clean, unified API for all business logic operations.
    /// </summary>
    public class FactoryManagementClient : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        private bool _disposed = false;

        #region Service Properties

        /// <summary>
        /// Access to user management operations
        /// </summary>
        public IUserService Users { get; }

        /// <summary>
        /// Access to party (customer/supplier) management operations
        /// </summary>
        public IPartyService Parties { get; }

        /// <summary>
        /// Access to item/inventory management operations
        /// </summary>
        public IItemService Items { get; }

        /// <summary>
        /// Access to transaction (buy/sell/processing) operations
        /// </summary>
        public ITransactionService Transactions { get; }

        /// <summary>
        /// Access to financial transaction and loan management operations
        /// </summary>
        public IFinancialTransactionService FinancialTransactions { get; }

        /// <summary>
        /// Access to worker wage and payroll operations
        /// </summary>
        public IWageService Wages { get; }

        /// <summary>
        /// Access to operational expense management
        /// </summary>
        public IOperationalExpenseService OperationalExpenses { get; }

        /// <summary>
        /// Access to cash book and daily cash tracking
        /// </summary>
        public ICashBookService CashBook { get; }

        /// <summary>
        /// Access to expense category management
        /// </summary>
        public IExpenseCategoryService ExpenseCategories { get; }

        /// <summary>
        /// Access to unified transaction view (all transaction types)
        /// </summary>
        public IUnifiedTransactionService UnifiedTransactions { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance with default database location
        /// Database will be created at: %LocalAppData%\Factory Management\factory.db
        /// </summary>
        public FactoryManagementClient() : this((string?)null)
        {
        }

        /// <summary>
        /// Creates a new instance with custom database path
        /// </summary>
        /// <param name="databasePath">Full path to SQLite database file. If null, uses default location.</param>
        public FactoryManagementClient(string? databasePath)
        {
            var services = new ServiceCollection();
            ConfigureServices(services, databasePath);
            _serviceProvider = services.BuildServiceProvider();

            // Initialize service properties
            Users = _serviceProvider.GetRequiredService<IUserService>();
            Parties = _serviceProvider.GetRequiredService<IPartyService>();
            Items = _serviceProvider.GetRequiredService<IItemService>();
            Transactions = _serviceProvider.GetRequiredService<ITransactionService>();
            FinancialTransactions = _serviceProvider.GetRequiredService<IFinancialTransactionService>();
            Wages = _serviceProvider.GetRequiredService<IWageService>();
            OperationalExpenses = _serviceProvider.GetRequiredService<IOperationalExpenseService>();
            CashBook = _serviceProvider.GetRequiredService<ICashBookService>();
            ExpenseCategories = _serviceProvider.GetRequiredService<IExpenseCategoryService>();
            UnifiedTransactions = _serviceProvider.GetRequiredService<IUnifiedTransactionService>();

            // Initialize database
            InitializeDatabase();
        }

        /// <summary>
        /// Creates a new instance with custom service configuration
        /// For advanced scenarios requiring custom DI setup
        /// </summary>
        /// <param name="configureServices">Action to configure additional services</param>
        public FactoryManagementClient(Action<IServiceCollection> configureServices)
        {
            var services = new ServiceCollection();
            ConfigureServices(services, null);
            configureServices?.Invoke(services);
            _serviceProvider = services.BuildServiceProvider();

            // Initialize service properties
            Users = _serviceProvider.GetRequiredService<IUserService>();
            Parties = _serviceProvider.GetRequiredService<IPartyService>();
            Items = _serviceProvider.GetRequiredService<IItemService>();
            Transactions = _serviceProvider.GetRequiredService<ITransactionService>();
            FinancialTransactions = _serviceProvider.GetRequiredService<IFinancialTransactionService>();
            Wages = _serviceProvider.GetRequiredService<IWageService>();
            OperationalExpenses = _serviceProvider.GetRequiredService<IOperationalExpenseService>();
            CashBook = _serviceProvider.GetRequiredService<ICashBookService>();
            ExpenseCategories = _serviceProvider.GetRequiredService<IExpenseCategoryService>();
            UnifiedTransactions = _serviceProvider.GetRequiredService<IUnifiedTransactionService>();

            // Initialize database
            InitializeDatabase();
        }

        #endregion

        #region Private Configuration Methods

        private void ConfigureServices(IServiceCollection services, string? databasePath)
        {
            // Determine database path
            var dbPath = databasePath ?? GetDefaultDatabasePath();
            EnsureDirectoryExists(dbPath);

            // Register DbContext
            services.AddDbContext<FactoryDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            // Register Repositories
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<IFinancialTransactionRepository, FinancialTransactionRepository>();
            services.AddScoped<ILoanAccountRepository, LoanAccountRepository>();
            services.AddScoped<IWorkerRepository, WorkerRepository>();
            services.AddScoped<IWageTransactionRepository, WageTransactionRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IExpenseCategoryRepository, ExpenseCategoryRepository>();
            services.AddScoped<IOperationalExpenseRepository, OperationalExpenseRepository>();
            services.AddScoped<ICashBalanceRepository, CashBalanceRepository>();

            // Register Services
            services.AddScoped<IItemService, ItemService>();
            services.AddScoped<IPartyService, PartyService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IFinancialTransactionService, FinancialTransactionService>();
            services.AddScoped<IWageService, WageService>();
            services.AddScoped<IUnifiedTransactionService, UnifiedTransactionService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IExpenseCategoryService, ExpenseCategoryService>();
            services.AddScoped<IOperationalExpenseService, OperationalExpenseService>();
            services.AddScoped<ICashBookService, CashBookService>();
        }

        private string GetDefaultDatabasePath()
        {
            var appDataDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Factory Management");
            return Path.Combine(appDataDir, "factory.db");
        }

        private void EnsureDirectoryExists(string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        #endregion

        #region Database Management

        /// <summary>
        /// Initializes the database, creating it if it doesn't exist.
        /// Runs migrations and seeds initial data.
        /// </summary>
        public void InitializeDatabase()
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<FactoryDbContext>();
            dbContext.Database.EnsureCreated();
        }

        /// <summary>
        /// Applies any pending database migrations.
        /// Use this when upgrading the application with schema changes.
        /// </summary>
        public void MigrateDatabase()
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<FactoryDbContext>();
            dbContext.Database.Migrate();
        }

        /// <summary>
        /// Gets the database context for advanced operations.
        /// Use with caution - prefer using the service properties instead.
        /// </summary>
        /// <returns>The FactoryDbContext instance</returns>
        public FactoryDbContext GetDbContext()
        {
            return _serviceProvider.GetRequiredService<FactoryDbContext>();
        }

        #endregion

        #region IDisposable Implementation

        /// <summary>
        /// Disposes the service provider and releases all resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _serviceProvider?.Dispose();
                }
                _disposed = true;
            }
        }

        #endregion
    }

    #region Extension Methods for IServiceCollection

    /// <summary>
    /// Extension methods for easy DI registration in ASP.NET Core or other applications
    /// </summary>
    public static class FactoryManagementServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Factory Management Core services to the service collection.
        /// Use this in ASP.NET Core Startup.cs or Program.cs
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="connectionString">SQLite connection string</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddFactoryManagementCore(
            this IServiceCollection services,
            string connectionString)
        {
            // Register DbContext
            services.AddDbContext<FactoryDbContext>(options =>
                options.UseSqlite(connectionString));

            // Register Repositories
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<IFinancialTransactionRepository, FinancialTransactionRepository>();
            services.AddScoped<ILoanAccountRepository, LoanAccountRepository>();
            services.AddScoped<IWorkerRepository, WorkerRepository>();
            services.AddScoped<IWageTransactionRepository, WageTransactionRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IExpenseCategoryRepository, ExpenseCategoryRepository>();
            services.AddScoped<IOperationalExpenseRepository, OperationalExpenseRepository>();
            services.AddScoped<ICashBalanceRepository, CashBalanceRepository>();

            // Register Services
            services.AddScoped<IItemService, ItemService>();
            services.AddScoped<IPartyService, PartyService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IFinancialTransactionService, FinancialTransactionService>();
            services.AddScoped<IWageService, WageService>();
            services.AddScoped<IUnifiedTransactionService, UnifiedTransactionService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IExpenseCategoryService, ExpenseCategoryService>();
            services.AddScoped<IOperationalExpenseService, OperationalExpenseService>();
            services.AddScoped<ICashBookService, CashBookService>();

            return services;
        }
    }

    #endregion
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FactoryManagement.Models;
using FactoryManagement.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace FactoryManagement.ViewModels
{
    public partial class DashboardViewModel : PaginationViewModel
    {
        private readonly ITransactionService _transactionService;
        private readonly IItemService _itemService;
        private readonly IFinancialTransactionService? _financialTransactionService;
        private readonly IWageService? _wageService;
        private readonly IUnifiedTransactionService? _unifiedTransactionService;
        private readonly IOperationalExpenseService? _operationalExpenseService;

        private const int LowStockThreshold = 100;
        private const int StockChartTopCount = 10;
        private const int RecentActivitiesPerSource = 20;
        private const int RecentActivitiesDisplayCount = 10;

        [ObservableProperty]
        private decimal _totalPurchases;

        [ObservableProperty]
        private decimal _totalSales;

        [ObservableProperty]
        private decimal _totalWastage;

        [ObservableProperty]
        private decimal _totalProcessingFees;

        [ObservableProperty]
        private int _processingTransactionCount;

        [ObservableProperty]
        private int _transactionCount;

        [ObservableProperty]
        private decimal _totalLoansGiven;

        [ObservableProperty]
        private decimal _totalLoansTaken;

        [ObservableProperty]
        private decimal _totalWagesPaid;

        [ObservableProperty]
        private decimal _totalAdvancesGiven;

        [ObservableProperty]
        private decimal _totalOperationalExpenses;

        [ObservableProperty]
        private decimal _monthlyOperationalExpenses;

        [ObservableProperty]
        private ObservableCollection<Transaction> _recentTransactions = new();

        [ObservableProperty]
        private ObservableCollection<Services.UnifiedTransactionViewModel> _allTransactions = new();

        [ObservableProperty]
        private ObservableCollection<Services.UnifiedTransactionViewModel> _paginatedAllTransactions = new();

        [ObservableProperty]
        private ObservableCollection<RecentActivity> _recentActivities = new();

        [ObservableProperty]
        private ObservableCollection<RecentActivity> _paginatedRecentActivities = new();

        [ObservableProperty]
        private ObservableCollection<Item> _lowStockItems = new();

        [ObservableProperty]
        private ObservableCollection<Item> _allItems = new();

        public DashboardViewModel(
            ITransactionService transactionService, 
            IItemService itemService,
            IFinancialTransactionService? financialTransactionService = null,
            IWageService? wageService = null,
            IUnifiedTransactionService? unifiedTransactionService = null,
            IOperationalExpenseService? operationalExpenseService = null)
        {
            _transactionService = transactionService;
            _itemService = itemService;
            _financialTransactionService = financialTransactionService;
            _wageService = wageService;
            _unifiedTransactionService = unifiedTransactionService;
            _operationalExpenseService = operationalExpenseService;
        }

        protected override void UpdatePaginatedData()
        {
            CalculatePagination(AllTransactions, DefaultPageSize);
            PaginatedAllTransactions.Clear();
            foreach (var transaction in GetPagedItems(AllTransactions, DefaultPageSize))
            {
                PaginatedAllTransactions.Add(transaction);
            }
        }

        [RelayCommand]
        private async Task LoadDataAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;
                
                // Start fetching all possible data concurrently
                var transactionsTask = _transactionService.GetAllTransactionsAsync();
                var itemsTask = _itemService.GetAllItemsAsync();

                Task<System.Collections.Generic.IEnumerable<FinancialTransaction>>? financialAllTask = null;
                Task<decimal>? loansGivenTask = null;
                Task<decimal>? loansTakenTask = null;
                if (_financialTransactionService != null)
                {
                    financialAllTask = _financialTransactionService.GetAllFinancialTransactionsAsync();
                    loansGivenTask = _financialTransactionService.GetTotalLoansGivenOutstandingAsync();
                    loansTakenTask = _financialTransactionService.GetTotalLoansTakenOutstandingAsync();
                }

                Task<System.Collections.Generic.IEnumerable<WageTransaction>>? wageAllTask = null;
                Task<decimal>? wagesPaidTask = null;
                Task<decimal>? advancesGivenTask = null;
                if (_wageService != null)
                {
                    wageAllTask = _wageService.GetAllWageTransactionsAsync();
                    wagesPaidTask = _wageService.GetTotalWagesPaidAsync();
                    advancesGivenTask = _wageService.GetTotalAdvancesGivenAsync();
                }

                Task<System.Collections.Generic.List<Services.UnifiedTransactionViewModel>>? unifiedTask = null;
                if (_unifiedTransactionService != null)
                {
                    unifiedTask = _unifiedTransactionService.GetAllUnifiedTransactionsAsync(limit: 20);
                }

                Task<decimal>? opExpensesTotalTask = null;
                Task<decimal>? opExpensesMonthlyTask = null;
                if (_operationalExpenseService != null)
                {
                    opExpensesTotalTask = _operationalExpenseService.GetTotalExpensesAsync();
                    var now = DateTime.Now;
                    opExpensesMonthlyTask = _operationalExpenseService.GetMonthlyExpensesAsync(now.Year, now.Month);
                }

                // Await the minimum required first
                await Task.WhenAll(transactionsTask, itemsTask);

                cancellationToken.ThrowIfCancellationRequested();

                var transactions = (await transactionsTask).ToList();

                TotalPurchases = transactions
                    .Where(t => t.TransactionType == TransactionType.Buy)
                    .Sum(t => t.TotalAmount);

                TotalSales = transactions
                    .Where(t => t.TransactionType == TransactionType.Sell)
                    .Sum(t => t.TotalAmount);

                TotalWastage = transactions
                    .Where(t => t.TransactionType == TransactionType.Wastage)
                    .Sum(t => t.TotalAmount);

                TotalProcessingFees = transactions
                    .Where(t => t.TransactionType == TransactionType.Processing)
                    .Sum(t => t.TotalAmount);

                ProcessingTransactionCount = transactions
                    .Count(t => t.TransactionType == TransactionType.Processing);

                TransactionCount = transactions.Count;

                var recentList = transactions
                    .OrderByDescending(t => t.TransactionDate)
                    .Take(10)
                    .ToList();
                SetCollection(RecentTransactions, recentList);

                // Build unified recent activities list
                var activities = new System.Collections.Generic.List<RecentActivity>();

                // Add transaction entries
                foreach (var t in transactions.OrderByDescending(x => x.TransactionDate).Take(RecentActivitiesPerSource))
                {
                    activities.Add(new RecentActivity
                    {
                        Date = t.TransactionDate,
                        Category = "Transaction",
                        Type = t.TransactionType.ToString(),
                        Description = t.ItemName ?? "Unknown Item",
                        Party = t.PartyName,
                        Amount = t.TotalAmount
                    });
                }

                // Add financial transactions if available
                if (_financialTransactionService != null)
                {
                    var financialTrans = financialAllTask != null ? await financialAllTask : new System.Collections.Generic.List<FinancialTransaction>();
                    foreach (var ft in financialTrans.OrderByDescending(x => x.TransactionDate).Take(RecentActivitiesPerSource))
                    {
                        activities.Add(new RecentActivity
                        {
                            Date = ft.TransactionDate,
                            Category = "Financial",
                            Type = ft.TransactionType.ToString(),
                            Description = ft.TransactionType.ToString(),
                            Party = ft.Party?.Name,
                            Amount = ft.Amount
                        });
                    }
                }

                // Add wage transactions if available
                if (_wageService != null)
                {
                    var wageTrans = wageAllTask != null ? await wageAllTask : new System.Collections.Generic.List<WageTransaction>();
                    foreach (var wt in wageTrans.OrderByDescending(x => x.TransactionDate).Take(RecentActivitiesPerSource))
                    {
                        activities.Add(new RecentActivity
                        {
                            Date = wt.TransactionDate,
                            Category = "Wage",
                            Type = wt.TransactionType.ToString(),
                            Description = wt.TransactionType.ToString(),
                            Party = wt.Worker?.Name,
                            Amount = wt.Amount
                        });
                    }
                }

                // Add operational expenses if available
                if (_operationalExpenseService != null)
                {
                    var opExpenses = await _operationalExpenseService.GetAllExpensesAsync();
                    foreach (var oe in opExpenses.OrderByDescending(x => x.ExpenseDate).Take(RecentActivitiesPerSource))
                    {
                        activities.Add(new RecentActivity
                        {
                            Date = oe.ExpenseDate,
                            Category = "Expense",
                            Type = oe.CategoryDisplay,
                            Description = oe.CategoryDisplay,
                            Party = oe.VendorDisplay,
                            Amount = oe.Amount
                        });
                    }
                }

                // Sort all activities by date and take top 15
                var recentActivities = activities
                    .OrderByDescending(a => a.Date)
                    .Take(RecentActivitiesDisplayCount)
                    .ToList();
                SetCollection(RecentActivities, recentActivities);
                UpdatePaginatedData();

                var allItems = await itemsTask;
                var lowStockList = allItems
                    .Where(i => i.CurrentStock < LowStockThreshold)
                    .OrderBy(i => i.CurrentStock)
                    .ToList();
                SetCollection(LowStockItems, lowStockList);

                // Load all items for stock chart (top 10 by lowest stock)
                var chartItems = allItems
                    .OrderBy(i => i.CurrentStock)
                    .Take(StockChartTopCount)
                    .ToList();
                SetCollection(AllItems, chartItems);

                // Load financial data if service is available
                if (_financialTransactionService != null)
                {
                    TotalLoansGiven = loansGivenTask != null ? await loansGivenTask : 0m;
                    TotalLoansTaken = loansTakenTask != null ? await loansTakenTask : 0m;
                }

                // Load operational expense data if service is available
                if (_operationalExpenseService != null)
                {
                    TotalOperationalExpenses = opExpensesTotalTask != null ? await opExpensesTotalTask : 0m;
                    MonthlyOperationalExpenses = opExpensesMonthlyTask != null ? await opExpensesMonthlyTask : 0m;
                }

                // Load wage data if service is available
                if (_wageService != null)
                {
                    TotalWagesPaid = wagesPaidTask != null ? await wagesPaidTask : 0m;
                    TotalAdvancesGiven = advancesGivenTask != null ? await advancesGivenTask : 0m;
                }

                // Load recent 15 unified transactions if service is available
                if (_unifiedTransactionService != null)
                {
                    var recentUnified = unifiedTask != null ? await unifiedTask : new System.Collections.Generic.List<Services.UnifiedTransactionViewModel>();
                    SetCollection(AllTransactions, recentUnified);
                    UpdatePaginatedData();
                }
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

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            await LoadDataAsync(cancellationToken);
        }

        private static void SetCollection<T>(ObservableCollection<T> target, System.Collections.Generic.IEnumerable<T> items)
        {
            target.Clear();
            foreach (var item in items)
            {
                target.Add(item);
            }
        }
    }
}

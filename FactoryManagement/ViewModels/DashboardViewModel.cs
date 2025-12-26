using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FactoryManagement.Models;
using FactoryManagement.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace FactoryManagement.ViewModels
{
    public partial class DashboardViewModel : ViewModelBase
    {
        private readonly ITransactionService _transactionService;
        private readonly IItemService _itemService;
        private readonly FinancialTransactionService? _financialTransactionService;
        private readonly IWageService? _wageService;

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
        private ObservableCollection<Transaction> _recentTransactions = new();

        [ObservableProperty]
        private ObservableCollection<RecentActivity> _recentActivities = new();

        [ObservableProperty]
        private ObservableCollection<Item> _lowStockItems = new();

        [ObservableProperty]
        private ObservableCollection<Item> _allItems = new();

        public DashboardViewModel(
            ITransactionService transactionService, 
            IItemService itemService,
            FinancialTransactionService? financialTransactionService = null,
            IWageService? wageService = null)
        {
            _transactionService = transactionService;
            _itemService = itemService;
            _financialTransactionService = financialTransactionService;
            _wageService = wageService;
        }

        [RelayCommand]
        private async Task LoadDataAsync()
        {
            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                var allTransactions = await _transactionService.GetAllTransactionsAsync();
                var transactions = allTransactions.ToList();

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

                var recentList = transactions.OrderByDescending(t => t.TransactionDate).Take(10).ToList();
                RecentTransactions.Clear();
                foreach (var item in recentList)
                    RecentTransactions.Add(item);

                // Build unified recent activities list
                var activities = new System.Collections.Generic.List<RecentActivity>();

                // Add transaction entries
                foreach (var t in transactions.OrderByDescending(x => x.TransactionDate).Take(20))
                {
                    activities.Add(new RecentActivity
                    {
                        Date = t.TransactionDate,
                        Category = "Transaction",
                        Type = t.TransactionType.ToString(),
                        Description = t.Item?.ItemName ?? "Unknown Item",
                        Party = t.Party?.Name,
                        Amount = t.TotalAmount
                    });
                }

                // Add financial transactions if available
                if (_financialTransactionService != null)
                {
                    var financialTrans = await _financialTransactionService.GetAllFinancialTransactionsAsync();
                    foreach (var ft in financialTrans.OrderByDescending(x => x.TransactionDate).Take(20))
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
                    var wageTrans = await _wageService.GetAllWageTransactionsAsync();
                    foreach (var wt in wageTrans.OrderByDescending(x => x.TransactionDate).Take(20))
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

                // Sort all activities by date and take top 15
                RecentActivities.Clear();
                foreach (var activity in activities.OrderByDescending(a => a.Date).Take(15))
                    RecentActivities.Add(activity);

                var allItems = await _itemService.GetAllItemsAsync();
                var lowStockList = allItems.Where(i => i.CurrentStock < 100).OrderBy(i => i.CurrentStock).ToList();
                LowStockItems.Clear();
                foreach (var item in lowStockList)
                    LowStockItems.Add(item);

                // Load all items for stock chart (top 10 by lowest stock)
                var chartItems = allItems.OrderBy(i => i.CurrentStock).Take(10).ToList();
                AllItems.Clear();
                foreach (var item in chartItems)
                    AllItems.Add(item);

                // Load financial data if service is available
                if (_financialTransactionService != null)
                {
                    TotalLoansGiven = await _financialTransactionService.GetTotalLoansGivenOutstandingAsync();
                    TotalLoansTaken = await _financialTransactionService.GetTotalLoansTakenOutstandingAsync();
                }

                // Load wage data if service is available
                if (_wageService != null)
                {
                    TotalWagesPaid = await _wageService.GetTotalWagesPaidAsync();
                    TotalAdvancesGiven = await _wageService.GetTotalAdvancesGivenAsync();
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

        public async Task InitializeAsync()
        {
            await LoadDataAsync();
        }
    }
}

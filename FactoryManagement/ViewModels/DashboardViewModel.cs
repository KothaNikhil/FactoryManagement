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

        [ObservableProperty]
        private decimal _totalPurchases;

        [ObservableProperty]
        private decimal _totalSales;

        [ObservableProperty]
        private decimal _totalWastage;

        [ObservableProperty]
        private int _transactionCount;

        [ObservableProperty]
        private decimal _totalLoansGiven;

        [ObservableProperty]
        private decimal _totalLoansTaken;

        [ObservableProperty]
        private ObservableCollection<Transaction> _recentTransactions = new();

        [ObservableProperty]
        private ObservableCollection<Item> _lowStockItems = new();

        [ObservableProperty]
        private ObservableCollection<Item> _allItems = new();

        public DashboardViewModel(
            ITransactionService transactionService, 
            IItemService itemService,
            FinancialTransactionService? financialTransactionService = null)
        {
            _transactionService = transactionService;
            _itemService = itemService;
            _financialTransactionService = financialTransactionService;
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

                TransactionCount = transactions.Count;

                var recentList = transactions.OrderByDescending(t => t.TransactionDate).Take(10).ToList();
                RecentTransactions.Clear();
                foreach (var item in recentList)
                    RecentTransactions.Add(item);

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

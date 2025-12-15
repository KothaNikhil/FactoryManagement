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

        [ObservableProperty]
        private decimal _totalPurchases;

        [ObservableProperty]
        private decimal _totalSales;

        [ObservableProperty]
        private decimal _totalWastage;

        [ObservableProperty]
        private int _transactionCount;

        [ObservableProperty]
        private ObservableCollection<Transaction> _recentTransactions = new();

        [ObservableProperty]
        private ObservableCollection<Item> _lowStockItems = new();

        public DashboardViewModel(ITransactionService transactionService, IItemService itemService)
        {
            _transactionService = transactionService;
            _itemService = itemService;
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

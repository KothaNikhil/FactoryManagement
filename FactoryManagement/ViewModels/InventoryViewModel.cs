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
    public partial class InventoryViewModel : PaginationViewModel
    {
        private readonly IItemService _itemService;
        private readonly ITransactionService _transactionService;

        [ObservableProperty]
        private ObservableCollection<Item> _items = new();

        [ObservableProperty]
        private ObservableCollection<Item> _paginatedItems = new();

        [ObservableProperty]
        private Item? _selectedItem;

        [ObservableProperty]
        private ObservableCollection<Transaction> _itemTransactions = new();

        [ObservableProperty]
        private ObservableCollection<Transaction> _paginatedItemTransactions = new();

        private const int TransactionPageSize = 13;

        [ObservableProperty]
        private int _transactionCurrentPage = 1;

        [ObservableProperty]
        private int _transactionTotalPages = 1;

        [ObservableProperty]
        private int _transactionTotalRecords = 0;

        // Date navigation for item transactions
        [ObservableProperty]
        private DateTime _currentItemTransactionDate = DateTime.Today;

        public bool CanGoToTransactionPreviousPage => TransactionCurrentPage > 1;
        public bool CanGoToTransactionNextPage => TransactionCurrentPage < TransactionTotalPages;

        public bool CanGoToNextItemTransactionDay => CurrentItemTransactionDate < DateTime.Today;
        public bool CanGoToPreviousItemTransactionDay => true;

        public int TotalItems => Items.Count;
        
        public decimal TotalStockValue => Items.Sum(i => i.CurrentStock);
        
        public int LowStockCount => Items.Count(i => i.CurrentStock < 10);
        
        public int CategoryCount => Items.Select(i => i.Unit).Distinct().Count();

        [ObservableProperty]
        private string _itemName = string.Empty;

        [ObservableProperty]
        private decimal _currentStock;

        [ObservableProperty]
        private string _unit = string.Empty;

        [ObservableProperty]
        private bool _isEditMode;

        [ObservableProperty]
        private string _searchText = string.Empty;

        private ObservableCollection<Item> _allItems = new();

        public InventoryViewModel(IItemService itemService, ITransactionService transactionService)
        {
            _itemService = itemService;
            _transactionService = transactionService;
        }

        partial void OnSearchTextChanged(string value)
        {
            FilterItems();
            UpdatePaginatedData();
        }

        partial void OnSelectedItemChanged(Item? value)
        {
            if (value != null)
            {
                _ = LoadItemTransactionsAsync();
            }
            else
            {
                ItemTransactions.Clear();
                PaginatedItemTransactions.Clear();
            }
        }

        partial void OnTransactionCurrentPageChanged(int value)
        {
            UpdateTransactionPaginatedData();
            OnPropertyChanged(nameof(CanGoToTransactionPreviousPage));
            OnPropertyChanged(nameof(CanGoToTransactionNextPage));
            GoToTransactionPreviousPageCommand.NotifyCanExecuteChanged();
            GoToTransactionNextPageCommand.NotifyCanExecuteChanged();
        }

        protected override void UpdatePaginatedData()
        {
            CalculatePagination(Items);
            PaginatedItems.Clear();
            foreach (var item in GetPagedItems(Items))
            {
                PaginatedItems.Add(item);
            }
        }

        private void UpdateTransactionPaginatedData()
        {
            // Filter transactions by current date
            var transactionsForDate = ItemTransactions
                .Where(t => t.TransactionDate.Date == CurrentItemTransactionDate.Date)
                .OrderByDescending(t => t.TransactionDate)
                .ToList();

            PaginatedItemTransactions.Clear();
            foreach (var transaction in transactionsForDate)
            {
                PaginatedItemTransactions.Add(transaction);
            }

            TransactionTotalRecords = transactionsForDate.Count;
        }

        partial void OnCurrentItemTransactionDateChanged(DateTime value)
        {
            UpdateTransactionPaginatedData();
            OnPropertyChanged(nameof(CanGoToNextItemTransactionDay));
            OnPropertyChanged(nameof(CanGoToPreviousItemTransactionDay));
            GoToNextItemTransactionDayCommand.NotifyCanExecuteChanged();
            GoToPreviousItemTransactionDayCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand(CanExecute = nameof(CanGoToPreviousItemTransactionDay))]
        private void GoToPreviousItemTransactionDay()
        {
            CurrentItemTransactionDate = CurrentItemTransactionDate.AddDays(-1);
        }

        [RelayCommand(CanExecute = nameof(CanGoToNextItemTransactionDay))]
        private void GoToNextItemTransactionDay()
        {
            CurrentItemTransactionDate = CurrentItemTransactionDate.AddDays(1);
        }

        [RelayCommand]
        private void GoToTodayItemTransaction()
        {
            CurrentItemTransactionDate = DateTime.Today;
        }

        [RelayCommand]
        private async Task LoadItemsAsync()
        {
            try
            {
                IsBusy = true;
                var items = await _itemService.GetAllItemsAsync();
                _allItems.Clear();
                Items.Clear();
                foreach (var item in items)
                {
                    _allItems.Add(item);
                    Items.Add(item);
                }
                UpdateSummaryProperties();
                UpdatePaginatedData();
                
                // Select first item by default
                if (Items.Count > 0)
                {
                    SelectedItem = Items[0];
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading items: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadItemTransactionsAsync()
        {
            try
            {
                if (SelectedItem == null || SelectedItem.ItemId <= 0) return;

                IsBusy = true;
                var transactions = await _transactionService.GetTransactionsByItemAsync(SelectedItem.ItemId);
                
                ItemTransactions.Clear();
                foreach (var transaction in transactions.OrderByDescending(t => t.TransactionDate))
                {
                    ItemTransactions.Add(transaction);
                }

                TransactionCurrentPage = 1;
                UpdateTransactionPaginatedData();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading item transactions: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void NewItem()
        {
            IsEditMode = false;
            SelectedItem = null;
            ItemName = string.Empty;
            CurrentStock = 0;
            Unit = string.Empty;
            ErrorMessage = string.Empty;
        }

        [RelayCommand]
        private void EditItem(Item? item)
        {
            if (item == null) return;

            if (!MainWindowViewModel.Instance?.IsAdminMode ?? false)
            {
                ErrorMessage = "Only administrators can edit items.";
                return;
            }

            IsEditMode = true;
            SelectedItem = item;
            ItemName = item.ItemName;
            CurrentStock = item.CurrentStock;
            Unit = item.Unit;
            ErrorMessage = string.Empty;
        }

        [RelayCommand]
        private async Task SaveItemAsync()
        {
            try
            {
                if (!MainWindowViewModel.Instance?.IsAdminMode ?? false)
                {
                    ErrorMessage = "Only administrators can add or edit items.";
                    return;
                }

                if (!ValidateItem())
                    return;

                IsBusy = true;

                if (IsEditMode && SelectedItem != null)
                {
                    SelectedItem.ItemName = ItemName;
                    SelectedItem.CurrentStock = CurrentStock;
                    SelectedItem.Unit = Unit;
                    await _itemService.UpdateItemAsync(SelectedItem, MainWindowViewModel.Instance?.CurrentUser?.UserId);
                    ErrorMessage = "Item updated successfully!";
                }
                else
                {
                    var newItem = new Item
                    {
                        ItemName = ItemName,
                        CurrentStock = CurrentStock,
                        Unit = Unit
                    };
                    await _itemService.AddItemAsync(newItem, MainWindowViewModel.Instance?.CurrentUser?.UserId);
                    ErrorMessage = "Item added successfully!";
                }

                await LoadItemsAsync();
                NewItem();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error saving item: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task DeleteItemAsync(Item? item)
        {
            if (item == null) return;

            if (!MainWindowViewModel.Instance?.IsAdminMode ?? false)
            {
                ErrorMessage = "Only administrators can delete items.";
                return;
            }

            try
            {
                IsBusy = true;
                await _itemService.DeleteItemAsync(item.ItemId);
                await LoadItemsAsync();
                ErrorMessage = "Item deleted successfully!";
                NewItem();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error deleting item: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool ValidateItem()
        {
            if (string.IsNullOrWhiteSpace(ItemName))
            {
                ErrorMessage = "Item name is required";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Unit))
            {
                ErrorMessage = "Unit is required";
                return false;
            }

            if (CurrentStock < 0)
            {
                ErrorMessage = "Stock cannot be negative";
                return false;
            }

            return true;
        }

        private void FilterItems()
        {
            Items.Clear();
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                foreach (var item in _allItems)
                    Items.Add(item);
            }
            else
            {
                var filtered = _allItems.Where(i =>
                    i.ItemName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    i.Unit.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                );
                foreach (var item in filtered)
                    Items.Add(item);
            }
            UpdateSummaryProperties();
        }

        private void UpdateSummaryProperties()
        {
            OnPropertyChanged(nameof(TotalItems));
            OnPropertyChanged(nameof(TotalStockValue));
            OnPropertyChanged(nameof(LowStockCount));
            OnPropertyChanged(nameof(CategoryCount));
        }

        [RelayCommand]
        private void GoToTransactionFirstPage()
        {
            TransactionCurrentPage = 1;
        }

        [RelayCommand]
        private void GoToTransactionPreviousPage()
        {
            if (CanGoToTransactionPreviousPage)
                TransactionCurrentPage--;
        }

        [RelayCommand]
        private void GoToTransactionNextPage()
        {
            if (CanGoToTransactionNextPage)
                TransactionCurrentPage++;
        }

        [RelayCommand]
        private void GoToTransactionLastPage()
        {
            TransactionCurrentPage = TransactionTotalPages;
        }

        public async Task InitializeAsync()
        {
            await LoadItemsAsync();
        }
    }
}


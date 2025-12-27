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
    public partial class InventoryViewModel : ViewModelBase
    {
        private readonly IItemService _itemService;

        [ObservableProperty]
        private ObservableCollection<Item> _items = new();

        [ObservableProperty]
        private Item? _selectedItem;

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

        public InventoryViewModel(IItemService itemService)
        {
            _itemService = itemService;
        }

        partial void OnSearchTextChanged(string value)
        {
            FilterItems();
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
                if (!ValidateItem())
                    return;

                IsBusy = true;

                if (IsEditMode && SelectedItem != null)
                {
                    SelectedItem.ItemName = ItemName;
                    SelectedItem.CurrentStock = CurrentStock;
                    SelectedItem.Unit = Unit;
                    await _itemService.UpdateItemAsync(SelectedItem);
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
                    await _itemService.AddItemAsync(newItem);
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

        public async Task InitializeAsync()
        {
            await LoadItemsAsync();
        }
    }
}


using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FactoryManagement.Models;
using FactoryManagement.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace FactoryManagement.ViewModels
{
    public partial class InventoryViewModel : PaginationViewModel
    {
        private readonly IItemService _itemService;
        private readonly IStockPackageService _packageService;

        [ObservableProperty]
        private ObservableCollection<Item> _items = new();

        [ObservableProperty]
        private ObservableCollection<Item> _paginatedItems = new();

        [ObservableProperty]
        private Item? _selectedItem;

        // Stock entry mode selection
        [ObservableProperty]
        private bool _isLooseStock = true;

        [ObservableProperty]
        private bool _isPackagedStock = false;

        // Package entry collection
        [ObservableProperty]
        private ObservableCollection<StockPackageEntry> _packageEntries = new();

        // Calculated total stock
        [ObservableProperty]
        private decimal _calculatedTotalStock;

        public int TotalItems => Items.Count;
        
        public decimal TotalStockValue => Items.Sum(i => i.CurrentStock);
        
        public int LowStockCount => Items.Count(i => i.CurrentStock < 10);
        
        public int CategoryCount => Items.Select(i => i.Unit).Distinct().Count();

        public int PackagedItemsCount => Items.Count(i => i.IsPackaged);
        
        public int LooseItemsCount => Items.Count(i => !i.IsPackaged);

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

        public InventoryViewModel(IItemService itemService, IStockPackageService packageService)
        {
            _itemService = itemService;
            _packageService = packageService;
        }

        partial void OnSearchTextChanged(string value)
        {
            FilterItems();
            UpdatePaginatedData();
        }

        // Handle loose stock mode toggle
        partial void OnIsLooseStockChanged(bool value)
        {
            if (value)
            {
                IsPackagedStock = false;
                PackageEntries.Clear();
                CalculatedTotalStock = CurrentStock;
            }
        }

        // Handle packaged stock mode toggle
        partial void OnIsPackagedStockChanged(bool value)
        {
            if (value)
            {
                IsLooseStock = false;
                if (IsEditMode && SelectedItem?.IsPackaged == true)
                {
                    LoadPackagesForEditAsync().ConfigureAwait(false);
                }
            }
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
        private void AddPackageEntry()
        {
            var entry = new StockPackageEntry
            {
                PackageSize = 25,
                PackageCount = 1
            };
            
            // Subscribe to property changes for this entry
            entry.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(StockPackageEntry.PackageSize) || 
                    e.PropertyName == nameof(StockPackageEntry.PackageCount))
                {
                    RecalculateStock();
                }
            };
            
            PackageEntries.Add(entry);
            RecalculateStock();
        }

        [RelayCommand]
        private void RemovePackageEntry(StockPackageEntry entry)
        {
            PackageEntries.Remove(entry);
            RecalculateStock();
        }

        private void RecalculateStock()
        {
            CalculatedTotalStock = PackageEntries.Sum(p => p.TotalQuantity);
            if (IsPackagedStock)
            {
                CurrentStock = CalculatedTotalStock;
            }
        }

        private async Task LoadPackagesForEditAsync()
        {
            if (SelectedItem == null) return;

            try
            {
                var packages = await _packageService.GetPackagesByItemIdAsync(SelectedItem.ItemId);
                PackageEntries.Clear();
                foreach (var package in packages)
                {
                    var entry = new StockPackageEntry
                    {
                        StockPackageId = package.StockPackageId,
                        PackageSize = package.PackageSize,
                        PackageCount = package.PackageCount,
                        Location = package.Location
                    };
                    
                    // Subscribe to property changes for this entry
                    entry.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == nameof(StockPackageEntry.PackageSize) || 
                            e.PropertyName == nameof(StockPackageEntry.PackageCount))
                        {
                            RecalculateStock();
                        }
                    };
                    
                    PackageEntries.Add(entry);
                }
                RecalculateStock();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading packages: {ex.Message}";
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
            IsLooseStock = true;
            IsPackagedStock = false;
            PackageEntries.Clear();
            CalculatedTotalStock = 0;
        }

        [RelayCommand]
        private async Task EditItemAsync(Item? item)
        {
            if (item == null) return;

            try
            {
                IsEditMode = true;
                SelectedItem = item;
                ItemName = item.ItemName;
                CurrentStock = item.CurrentStock;
                Unit = item.Unit;
                ErrorMessage = string.Empty;

                // Check if item has packages
                var packages = await _packageService.GetPackagesByItemIdAsync(item.ItemId);
                if (packages.Any())
                {
                    IsPackagedStock = true;
                    IsLooseStock = false;
                    await LoadPackagesForEditAsync();
                }
                else
                {
                    IsLooseStock = true;
                    IsPackagedStock = false;
                    PackageEntries.Clear();
                    CalculatedTotalStock = CurrentStock;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error editing item: {ex.Message}";
            }
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
                    SelectedItem.Unit = Unit;

                    if (IsPackagedStock)
                    {
                        // Delete all existing packages
                        await _packageService.DeleteAllPackagesForItemAsync(SelectedItem.ItemId);

                        // Add new packages
                        foreach (var entry in PackageEntries)
                        {
                            await _packageService.AddPackageAsync(new StockPackage
                            {
                                ItemId = SelectedItem.ItemId,
                                PackageSize = entry.PackageSize,
                                PackageCount = entry.PackageCount,
                                Location = entry.Location
                            }, MainWindowViewModel.Instance?.CurrentUser?.UserId);
                        }

                        // Stock is auto-synced by service
                        SelectedItem.CurrentStock = CalculatedTotalStock;
                    }
                    else
                    {
                        // Loose stock - delete all packages and set stock directly
                        await _packageService.DeleteAllPackagesForItemAsync(SelectedItem.ItemId);
                        SelectedItem.CurrentStock = CurrentStock;
                    }

                    await _itemService.UpdateItemAsync(SelectedItem, MainWindowViewModel.Instance?.CurrentUser?.UserId);
                    ErrorMessage = "Item updated successfully!";
                }
                else
                {
                    // Add new item
                    var newItem = new Item
                    {
                        ItemName = ItemName,
                        CurrentStock = IsLooseStock ? CurrentStock : CalculatedTotalStock,
                        Unit = Unit
                    };
                    var createdItem = await _itemService.AddItemAsync(newItem, MainWindowViewModel.Instance?.CurrentUser?.UserId);

                    if (IsPackagedStock)
                    {
                        foreach (var entry in PackageEntries)
                        {
                            await _packageService.AddPackageAsync(new StockPackage
                            {
                                ItemId = createdItem.ItemId,
                                PackageSize = entry.PackageSize,
                                PackageCount = entry.PackageCount,
                                Location = entry.Location
                            }, MainWindowViewModel.Instance?.CurrentUser?.UserId);
                        }
                    }

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

            if (IsPackagedStock && !PackageEntries.Any())
            {
                ErrorMessage = "At least one package entry is required";
                return false;
            }

            if (IsPackagedStock && PackageEntries.Any(p => p.PackageSize <= 0 || p.PackageCount <= 0))
            {
                ErrorMessage = "All packages must have size > 0 and count > 0";
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
            OnPropertyChanged(nameof(PackagedItemsCount));
            OnPropertyChanged(nameof(LooseItemsCount));
        }

        public async Task InitializeAsync()
        {
            await LoadItemsAsync();
        }
    }
}

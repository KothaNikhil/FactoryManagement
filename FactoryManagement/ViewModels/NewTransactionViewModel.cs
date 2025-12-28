using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FactoryManagement.Models;
using FactoryManagement.Services;
using FactoryManagement.Data.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MaterialDesignThemes.Wpf;

namespace FactoryManagement.ViewModels
{
    public partial class NewTransactionViewModel : ViewModelBase
    {
        private readonly ITransactionService _transactionService;
        private readonly IItemService _itemService;
        private readonly IPartyService _partyService;
        private Transaction? _lastDeletedTransaction;

        public ISnackbarMessageQueue SnackbarMessageQueue { get; } = new SnackbarMessageQueue(TimeSpan.FromSeconds(4));

        [ObservableProperty]
        private ObservableCollection<Item> _items = new();

        [ObservableProperty]
        private ObservableCollection<Party> _parties = new();

        [ObservableProperty]
        private ObservableCollection<Transaction> _recentTransactions = new();

        [ObservableProperty]
        private Item? _selectedItem;

        [ObservableProperty]
        private Party? _selectedParty;

        [ObservableProperty]
        private string _selectedTransactionTypeString = "Buy";

        public TransactionType SelectedTransactionType
        {
            get => Enum.Parse<TransactionType>(SelectedTransactionTypeString);
            set => SelectedTransactionTypeString = value.ToString();
        }

        [ObservableProperty]
        private decimal _quantity;

        [ObservableProperty]
        private decimal _pricePerUnit;

        [ObservableProperty]
        private decimal _totalAmount;

        [ObservableProperty]
        private DateTime _transactionDate = DateTime.Now;

        [ObservableProperty]
        private DateTime? _transactionTime = DateTime.Now;

        [ObservableProperty]
        private string _notes = string.Empty;

        [ObservableProperty]
        private bool _isPartyRequired = true;

        [ObservableProperty]
        private bool _isEditMode = false;

        [ObservableProperty]
        private int _editingTransactionId = 0;

        [ObservableProperty]
        private decimal _originalQuantity = 0;

        [ObservableProperty]
        private TransactionType _originalTransactionType;

        // Processing-specific properties
        [ObservableProperty]
        private Item? _inputItem;

        [ObservableProperty]
        private decimal _inputQuantity;

        [ObservableProperty]
        private decimal _conversionRate;

        [ObservableProperty]
        private bool _isProcessingMode;

        public ObservableCollection<string> TransactionTypes { get; } = new()
        {
            "Buy", "Sell", "Wastage", "Processing"
        };

        public string SaveButtonText => IsEditMode ? "UPDATE TRANSACTION" : "SAVE TRANSACTION";
        public string FormTitle => IsEditMode ? "Edit Transaction" : "New Transaction";

        public NewTransactionViewModel(
            ITransactionService transactionService,
            IItemService itemService,
            IPartyService partyService)
        {
            _transactionService = transactionService;
            _itemService = itemService;
            _partyService = partyService;
        }

        partial void OnQuantityChanged(decimal value)
        {
            CalculateTotal();
            CalculateConversionRate();
        }

        partial void OnPricePerUnitChanged(decimal value)
        {
            CalculateTotal();
        }

        partial void OnInputQuantityChanged(decimal value)
        {
            CalculateConversionRate();
        }

        partial void OnSelectedTransactionTypeStringChanged(string value)
        {
            IsPartyRequired = SelectedTransactionType != TransactionType.Wastage;
            IsProcessingMode = SelectedTransactionType == TransactionType.Processing;
            OnPropertyChanged(nameof(SelectedTransactionType));
            OnPropertyChanged(nameof(ItemLabelText));
            OnPropertyChanged(nameof(QuantityLabelText));
        }

        public string ItemLabelText => IsProcessingMode ? "Output Item (Processed):" : "Item:";
        public string QuantityLabelText => IsProcessingMode ? "Output Quantity:" : "Quantity:";

        private void CalculateConversionRate()
        {
            if (IsProcessingMode && InputQuantity > 0 && Quantity > 0)
            {
                ConversionRate = (Quantity / InputQuantity) * 100;
            }
        }

        private void CalculateTotal()
        {
            TotalAmount = Quantity * PricePerUnit;
        }

        [RelayCommand]
        private async Task LoadDataAsync()
        {
            try
            {
                IsBusy = true;
                
                var items = await _itemService.GetAllItemsAsync();
                Items.Clear();
                foreach (var item in items)
                    Items.Add(item);

                var parties = await _partyService.GetAllPartiesAsync();
                Parties.Clear();
                foreach (var party in parties)
                    Parties.Add(party);

                // Load recent transactions (last 100 - grid will scroll)
                var recentTrans = await _transactionService.GetRecentTransactionsAsync(100);
                RecentTransactions.Clear();
                foreach (var trans in recentTrans)
                    RecentTransactions.Add(trans);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading data: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task SaveTransactionAsync()
        {
            try
            {
                if (!ValidateTransaction())
                    return;

                IsBusy = true;
                ErrorMessage = string.Empty;

                if (IsEditMode)
                {
                    // Update existing transaction
                    var transaction = await _transactionService.GetTransactionByIdAsync(EditingTransactionId);
                    if (transaction == null)
                    {
                        ErrorMessage = "Transaction not found";
                        return;
                    }

                    // Store original values for stock reversal
                    var oldQuantity = transaction.Quantity;
                    var oldType = transaction.TransactionType;
                    var oldItemId = transaction.ItemId;

                    // Update transaction properties
                    transaction.ItemId = SelectedItem!.ItemId;
                    transaction.PartyId = SelectedParty?.PartyId;
                    transaction.TransactionType = SelectedTransactionType;
                    transaction.Quantity = Quantity;
                    transaction.PricePerUnit = PricePerUnit;
                    transaction.TotalAmount = TotalAmount;
                    transaction.TransactionDate = CombineDateAndTime(TransactionDate, TransactionTime);
                    transaction.EnteredBy = MainWindowViewModel.Instance?.CurrentUser?.UserId ?? 1;
                    transaction.Notes = Notes;

                    // Reverse old stock impact with correct semantics per type
                    switch (oldType)
                    {
                        case TransactionType.Buy:
                            await _itemService.UpdateStockAsync(oldItemId, oldQuantity, TransactionType.Sell);
                            break;
                        case TransactionType.Sell:
                            await _itemService.UpdateStockAsync(oldItemId, oldQuantity, TransactionType.Buy);
                            break;
                        case TransactionType.Wastage:
                            // Wastage reduced stock originally; reversal increases
                            await _itemService.UpdateStockAsync(oldItemId, oldQuantity, TransactionType.Buy);
                            break;
                        case TransactionType.Processing:
                            // Processing never altered stock; nothing to reverse
                            break;
                    }

                    // Apply new stock impact
                    if (transaction.TransactionType != TransactionType.Processing)
                    {
                        await _itemService.UpdateStockAsync(transaction.ItemId, transaction.Quantity, transaction.TransactionType);
                    }

                    await _transactionService.UpdateTransactionAsync(transaction);
                    ErrorMessage = "✓ Transaction updated successfully!";
                }
                else
                {
                    // Add new transaction
                    var transaction = new Transaction
                    {
                        ItemId = SelectedItem!.ItemId,
                        PartyId = SelectedParty?.PartyId,
                        TransactionType = SelectedTransactionType,
                        Quantity = Quantity,
                        PricePerUnit = PricePerUnit,
                        TotalAmount = TotalAmount,
                        TransactionDate = CombineDateAndTime(TransactionDate, TransactionTime),
                        EnteredBy = MainWindowViewModel.Instance?.CurrentUser?.UserId ?? 1,
                        Notes = Notes
                    };

                    // Add processing-specific data
                    if (SelectedTransactionType == TransactionType.Processing)
                    {
                        transaction.InputItemId = InputItem?.ItemId;
                        transaction.InputQuantity = InputQuantity;
                        transaction.ConversionRate = ConversionRate / 100; // Store as decimal (0.70 instead of 70%)
                    }

                    await _transactionService.AddTransactionAsync(transaction);
                    ErrorMessage = "✓ Transaction saved successfully!";
                }
                
                // Clear form after a short delay
                await Task.Delay(1000);
                await ClearFormAsync();
                
                // Reload items to refresh stock and recent transactions
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? $" Inner: {ex.InnerException.Message}" : "";
                ErrorMessage = $"Error saving transaction: {ex.Message}{innerMessage}";
                System.Diagnostics.Debug.WriteLine($"Transaction save error: {ex}");
                Serilog.Log.Error(ex, "Error saving transaction");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task ClearFormAsync()
        {
            SelectedItem = null;
            SelectedParty = null;
            Quantity = 0;
            PricePerUnit = 0;
            TotalAmount = 0;
            TransactionDate = DateTime.Now;
            TransactionTime = DateTime.Now;
            Notes = string.Empty;
            ErrorMessage = string.Empty;
            IsEditMode = false;
            EditingTransactionId = 0;
            OriginalQuantity = 0;
            
            // Clear processing fields
            InputItem = null;
            InputQuantity = 0;
            ConversionRate = 0;
            
            OnPropertyChanged(nameof(SaveButtonText));
            OnPropertyChanged(nameof(FormTitle));
            
            await Task.CompletedTask;
        }

        [RelayCommand]
        private async Task EditTransactionAsync(Transaction transaction)
        {
            try
            {
                IsEditMode = true;
                EditingTransactionId = transaction.TransactionId;
                OriginalQuantity = transaction.Quantity;
                OriginalTransactionType = transaction.TransactionType;

                // Populate form with transaction data
                SelectedTransactionTypeString = transaction.TransactionType.ToString();
                SelectedItem = Items.FirstOrDefault(i => i.ItemId == transaction.ItemId);
                SelectedParty = Parties.FirstOrDefault(p => p.PartyId == transaction.PartyId);
                Quantity = transaction.Quantity;
                PricePerUnit = transaction.PricePerUnit;
                TransactionDate = transaction.TransactionDate.Date;
                TransactionTime = transaction.TransactionDate;
                Notes = transaction.Notes ?? string.Empty;

                // Load processing-specific data
                if (transaction.TransactionType == TransactionType.Processing)
                {
                    InputItem = transaction.InputItemId.HasValue 
                        ? Items.FirstOrDefault(i => i.ItemId == transaction.InputItemId.Value) 
                        : null;
                    InputQuantity = transaction.InputQuantity ?? 0;
                    ConversionRate = (transaction.ConversionRate ?? 0) * 100; // Display as percentage
                }

                OnPropertyChanged(nameof(SaveButtonText));
                OnPropertyChanged(nameof(FormTitle));
                ErrorMessage = string.Empty;
                
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading transaction: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task DeleteTransactionAsync(Transaction transaction)
        {
            try
            {
                // Confirm delete
                var result = System.Windows.MessageBox.Show(
                    $"Delete this transaction dated {transaction.TransactionDate:dd-MMM-yyyy}?",
                    "Confirm Delete",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Warning);
                if (result != System.Windows.MessageBoxResult.Yes)
                    return;

                IsBusy = true;
                ErrorMessage = string.Empty;

                // Store for potential undo
                _lastDeletedTransaction = transaction;

                await _transactionService.DeleteTransactionAsync(transaction.TransactionId);

                // Refresh recent transactions and stocks
                await LoadDataAsync();

                SnackbarMessageQueue.Enqueue(
                    "Transaction deleted",
                    "UNDO",
                    () => Application.Current.Dispatcher.Invoke(async () => await UndoDeleteTransactionAsync()));
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error deleting transaction: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task UndoDeleteTransactionAsync()
        {
            if (_lastDeletedTransaction == null) return;
            try
            {
                IsBusy = true;
                var restore = new Transaction
                {
                    ItemId = _lastDeletedTransaction.ItemId,
                    PartyId = _lastDeletedTransaction.PartyId,
                    TransactionType = _lastDeletedTransaction.TransactionType,
                    Quantity = _lastDeletedTransaction.Quantity,
                    PricePerUnit = _lastDeletedTransaction.PricePerUnit,
                    TotalAmount = _lastDeletedTransaction.TotalAmount,
                    TransactionDate = _lastDeletedTransaction.TransactionDate,
                    EnteredBy = _lastDeletedTransaction.EnteredBy,
                    Notes = _lastDeletedTransaction.Notes,
                    InputItemId = _lastDeletedTransaction.InputItemId,
                    InputQuantity = _lastDeletedTransaction.InputQuantity,
                    ConversionRate = _lastDeletedTransaction.ConversionRate
                };
                await _transactionService.AddTransactionAsync(restore);
                _lastDeletedTransaction = null;
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error undoing delete: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool ValidateTransaction()
        {
            // Processing-specific validation
            if (SelectedTransactionType == TransactionType.Processing)
            {
                if (InputItem == null)
                {
                    ErrorMessage = "Please select input material";
                    return false;
                }

                if (InputQuantity <= 0)
                {
                    ErrorMessage = "Input quantity must be greater than zero";
                    return false;
                }

                if (SelectedItem != null && InputItem != null && InputItem.ItemId == SelectedItem.ItemId)
                {
                    ErrorMessage = "Input and output items must be different";
                    return false;
                }
            }

            if (SelectedItem == null)
            {
                ErrorMessage = "Please select an item";
                return false;
            }

            if (IsPartyRequired && SelectedParty == null)
            {
                ErrorMessage = "Please select a party";
                return false;
            }

            if (MainWindowViewModel.Instance?.CurrentUser == null)
            {
                ErrorMessage = "Please select a user from the header";
                return false;
            }

            if (Quantity <= 0)
            {
                ErrorMessage = "Quantity must be greater than 0";
                return false;
            }

            if (PricePerUnit < 0)
            {
                ErrorMessage = "Price per unit cannot be negative";
                return false;
            }

            // Check stock for sell/wastage (not for processing output)
            if (SelectedTransactionType == TransactionType.Sell || SelectedTransactionType == TransactionType.Wastage)
            {
                decimal requiredStock;
                if (IsEditMode)
                {
                    // For edit mode, calculate the additional stock needed
                    // If decreasing quantity, no stock check needed
                    // If increasing quantity, check if we have enough additional stock
                    decimal quantityDifference = Quantity - OriginalQuantity;
                    requiredStock = quantityDifference;
                }
                else
                {
                    // For new transactions, check the full quantity
                    requiredStock = Quantity;
                }

                if (requiredStock > 0 && SelectedItem.CurrentStock < requiredStock)
                {
                    ErrorMessage = "Insufficient stock available";
                    return false;
                }
            }

            return true;
        }

        private DateTime CombineDateAndTime(DateTime date, DateTime? time)
        {
            if (time.HasValue && time.Value != DateTime.MinValue)
            {
                try
                {
                    return new DateTime(date.Year, date.Month, date.Day, 
                                      time.Value.Hour, time.Value.Minute, time.Value.Second);
                }
                catch
                {
                    // If time combination fails, return date with current time
                    return new DateTime(date.Year, date.Month, date.Day,
                                      DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                }
            }
            // If no time specified, use current time
            return new DateTime(date.Year, date.Month, date.Day,
                              DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
        }

        [RelayCommand]
        private async Task QuickAddPartyAsync()
        {
            try
            {
                // This will be called from the view to open the dialog
                // The actual dialog handling is in the view code-behind
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
            }
        }

        public async Task InitializeAsync()
        {
            await LoadDataAsync();
        }
    }
}

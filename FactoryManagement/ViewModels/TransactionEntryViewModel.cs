using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FactoryManagement.Models;
using FactoryManagement.Services;
using FactoryManagement.Data.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace FactoryManagement.ViewModels
{
    public partial class TransactionEntryViewModel : ViewModelBase
    {
        private readonly ITransactionService _transactionService;
        private readonly IItemService _itemService;
        private readonly IPartyService _partyService;
        private readonly IRepository<User> _userRepository;

        [ObservableProperty]
        private ObservableCollection<Item> _items = new();

        [ObservableProperty]
        private ObservableCollection<Party> _parties = new();

        [ObservableProperty]
        private ObservableCollection<User> _users = new();

        [ObservableProperty]
        private ObservableCollection<Transaction> _recentTransactions = new();

        [ObservableProperty]
        private Item? _selectedItem;

        [ObservableProperty]
        private Party? _selectedParty;

        [ObservableProperty]
        private User? _selectedUser;

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

        public ObservableCollection<string> TransactionTypes { get; } = new()
        {
            "Buy", "Sell", "Wastage"
        };

        public string SaveButtonText => IsEditMode ? "UPDATE TRANSACTION" : "SAVE TRANSACTION";
        public string FormTitle => IsEditMode ? "Edit Transaction" : "New Transaction";

        public TransactionEntryViewModel(
            ITransactionService transactionService,
            IItemService itemService,
            IPartyService partyService,
            IRepository<User> userRepository)
        {
            _transactionService = transactionService;
            _itemService = itemService;
            _partyService = partyService;
            _userRepository = userRepository;
        }

        partial void OnQuantityChanged(decimal value)
        {
            CalculateTotal();
        }

        partial void OnPricePerUnitChanged(decimal value)
        {
            CalculateTotal();
        }

        partial void OnSelectedTransactionTypeStringChanged(string value)
        {
            IsPartyRequired = SelectedTransactionType != TransactionType.Wastage;
            OnPropertyChanged(nameof(SelectedTransactionType));
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

                var users = await _userRepository.GetAllAsync();
                Users.Clear();
                foreach (var user in users.Where(u => u.IsActive))
                    Users.Add(user);
                
                if (Users.Any())
                    SelectedUser = Users.First();

                // Load recent transactions (last 5)
                var recentTrans = await _transactionService.GetRecentTransactionsAsync(5);
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
                    transaction.TransactionDate = TransactionDate;
                    transaction.EnteredBy = SelectedUser!.UserId;
                    transaction.Notes = Notes;

                    // Reverse old stock impact
                    var reverseOldType = oldType == TransactionType.Buy ? TransactionType.Sell : TransactionType.Buy;
                    await _itemService.UpdateStockAsync(oldItemId, oldQuantity, reverseOldType);

                    // Apply new stock impact
                    await _itemService.UpdateStockAsync(transaction.ItemId, transaction.Quantity, transaction.TransactionType);

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
                        TransactionDate = TransactionDate,
                        EnteredBy = SelectedUser!.UserId,
                        Notes = Notes
                    };

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
                ErrorMessage = $"Error saving transaction: {ex.Message}";
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
            Notes = string.Empty;
            ErrorMessage = string.Empty;
            IsEditMode = false;
            EditingTransactionId = 0;
            OriginalQuantity = 0;
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
                TransactionDate = transaction.TransactionDate;
                SelectedUser = Users.FirstOrDefault(u => u.UserId == transaction.EnteredBy);
                Notes = transaction.Notes ?? string.Empty;

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

        private bool ValidateTransaction()
        {
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

            if (SelectedUser == null)
            {
                ErrorMessage = "Please select a user";
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

            // Check stock for sell/wastage
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

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FactoryManagement.Models;
using FactoryManagement.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MaterialDesignThemes.Wpf;

namespace FactoryManagement.ViewModels
{
    public partial class PayrollManagementViewModel : PaginationViewModel
    {
        private readonly IWageService _wageService;

        [ObservableProperty]
        private ObservableCollection<Worker> _workers = new();

        [ObservableProperty]
        private ObservableCollection<WageTransaction> _transactions = new();

        [ObservableProperty]
        private ObservableCollection<WageTransaction> _paginatedTransactions = new();

        [ObservableProperty]
        private ObservableCollection<WageTransaction> _allTransactions = new();

        [ObservableProperty]
        private Worker? _selectedWorker;

        [ObservableProperty]
        private WageTransaction? _selectedTransaction;

        // Simplified Worker Form Properties (no WorkerType)
        [ObservableProperty]
        private string _workerName = string.Empty;

        [ObservableProperty]
        private string _mobileNumber = string.Empty;

        [ObservableProperty]
        private string _address = string.Empty;

        [ObservableProperty]
        private WorkerStatus _workerStatus = WorkerStatus.Active;

        [ObservableProperty]
        private decimal _workerRate;

        [ObservableProperty]
        private string _workerNotes = string.Empty;

        [ObservableProperty]
        private bool _isEditMode;

        [ObservableProperty]
        private int _editingWageTransactionId;

        // Wage Payment Form Properties
        [ObservableProperty]
        private object _transactionType = "DailyWage";

        [ObservableProperty]
        private decimal _daysWorked;

        [ObservableProperty]
        private decimal _hoursWorked;

        [ObservableProperty]
        private decimal _rate;

        [ObservableProperty]
        private decimal _amount;

        [ObservableProperty]
        private decimal _overtimeHours;

        [ObservableProperty]

        private decimal _overtimeAmount;

        [ObservableProperty]
        private decimal _advanceAdjustment;

        [ObservableProperty]
        private decimal _deductions;

        [ObservableProperty]
        private decimal _netAmount;

        [ObservableProperty]
        private string _notes = string.Empty;

        // Payment mode selection
        public ObservableCollection<string> PaymentModes { get; } = new() { "Cash", "Bank", "Loan" };

        [ObservableProperty]
        private string _selectedPaymentModeString = "Cash";

        public PaymentMode SelectedPaymentMode => Enum.Parse<PaymentMode>(SelectedPaymentModeString);

        // Summary Properties
        [ObservableProperty]
        private decimal _totalWagesPaid;

        [ObservableProperty]
        private decimal _totalAdvancesGiven;

        [ObservableProperty]
        private decimal _selectedWorkerTotalWages;

        [ObservableProperty]
        private decimal _selectedWorkerOutstandingAdvance;

        [ObservableProperty]
        private bool _workerInfoVisible;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        private WageTransaction? _lastDeletedWageTransaction;
        private ISnackbarMessageQueue? _snackbarMessageQueue;
        public ISnackbarMessageQueue? SnackbarMessageQueue => _snackbarMessageQueue ??= CreateSnackbarIfUiThread();

        private ObservableCollection<Worker> _allWorkers = new();

        public ObservableCollection<string> WorkerStatuses { get; } = new() { "Active", "Inactive", "OnLeave", "Terminated" };
        public ObservableCollection<string> TransactionTypes { get; } = new() 
        { 
            "Wage Payment", "Advance Given", "Advance Returned"
        };

        public PayrollManagementViewModel(IWageService wageService)
        {
            _wageService = wageService;
        }

        protected override void UpdatePaginatedData()
        {
            CalculatePagination(AllTransactions);
            PaginatedTransactions.Clear();
            foreach (var transaction in GetPagedItems(AllTransactions))
            {
                PaginatedTransactions.Add(transaction);
            }
        }

        private static ISnackbarMessageQueue? CreateSnackbarIfUiThread()
        {
            try
            {
                if (Application.Current?.Dispatcher?.CheckAccess() == true)
                {
                    return new SnackbarMessageQueue(TimeSpan.FromSeconds(4));
                }
            }
            catch { }
            return null;
        }

        partial void OnSelectedWorkerChanged(Worker? value)
        {
            WorkerInfoVisible = value != null;
            if (value != null)
            {
                _ = LoadWorkerDetailsAsync(value.WorkerId);
            }
        }

        [RelayCommand]
        private async Task LoadWorkersAsync()
        {
            try
            {
                IsBusy = true;
                var workers = await _wageService.GetAllWorkersAsync();
                _allWorkers = new ObservableCollection<Worker>(workers.OrderBy(w => w.Name));
                Workers = new ObservableCollection<Worker>(_allWorkers);
                await LoadSummaryAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading workers: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task SaveWagePaymentAsync()
        {
            try
            {
                ErrorMessage = string.Empty;

                if (SelectedWorker == null)
                {
                    ErrorMessage = "Please select a worker first!";
                    return;
                }

                if (Amount <= 0)
                {
                    ErrorMessage = "Amount must be greater than zero!";
                    return;
                }

                IsBusy = true;

                // Simplified transaction type mapping
                var transType = TransactionType.ToString() switch
                {
                    "Advance Given" => WageTransactionType.AdvanceGiven,
                    "Advance Returned" => WageTransactionType.AdvanceAdjustment,
                    _ => WageTransactionType.DailyWage
                };

                var transaction = new WageTransaction
                {
                    WageTransactionId = EditingWageTransactionId,
                    WorkerId = SelectedWorker.WorkerId,
                    TransactionType = transType,
                    TransactionDate = DateTime.Now,
                    Amount = Amount,
                    NetAmount = Amount,
                    PaymentMode = SelectedPaymentMode,
                    EnteredBy = MainWindowViewModel.Instance?.CurrentUser?.UserId ?? 1,
                    Notes = Notes
                };

                if (IsEditMode && EditingWageTransactionId > 0)
                {
                    await _wageService.UpdateWagePaymentAsync(transaction, MainWindowViewModel.Instance?.CurrentUser?.UserId);
                    MessageBox.Show("Payment updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    await _wageService.RecordWagePaymentAsync(transaction);
                    MessageBox.Show("Payment recorded successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                await LoadWorkersAsync();
                if (SelectedWorker != null)
                {
                    await LoadWorkerDetailsAsync(SelectedWorker.WorkerId);
                }
                await LoadSummaryAsync();
                
                // Clear form
                Amount = 0;
                Notes = string.Empty;
                SelectedPaymentModeString = "Cash";
                ErrorMessage = string.Empty;
                IsEditMode = false;
                EditingWageTransactionId = 0;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
                MessageBox.Show($"Error recording payment: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void EditWageTransaction(WageTransaction? transaction)
        {
            if (transaction == null) return;

            // Populate form fields from selected transaction
            SelectedWorker = Workers.FirstOrDefault(w => w.WorkerId == transaction.WorkerId) ?? transaction.Worker;

            // Map transaction type to UI string
            TransactionType = transaction.TransactionType switch
            {
                WageTransactionType.AdvanceGiven => "Advance Given",
                WageTransactionType.AdvanceAdjustment => "Advance Returned",
                _ => "Wage Payment"
            };

            SelectedPaymentModeString = transaction.PaymentMode.ToString();
            Amount = transaction.Amount;
            Notes = transaction.Notes ?? string.Empty;

            EditingWageTransactionId = transaction.WageTransactionId;
            IsEditMode = true;
            SnackbarMessageQueue?.Enqueue("Editing transaction loaded");
        }

        [RelayCommand]
        private void CancelEdit()
        {
            IsEditMode = false;
            EditingWageTransactionId = 0;
            Amount = 0;
            Notes = string.Empty;
            SelectedPaymentModeString = "Cash";
        }

        [RelayCommand]
        private void ClearPaymentForm()
        {
            // Do not change selected worker or edit mode; just clear fields
            Amount = 0;
            Notes = string.Empty;
            SelectedPaymentModeString = "Cash";
            TransactionType = "Wage Payment";
            ErrorMessage = string.Empty;
        }

        [RelayCommand]
        private async Task QuickAddWorkerAsync()
        {
            try
            {
                var dialog = new Views.QuickAddWorkerDialog();
                if (dialog.ShowDialog() == true && dialog.NewWorker != null)
                {
                    IsBusy = true;
                    await _wageService.AddWorkerAsync(dialog.NewWorker);
                    await LoadWorkersAsync();
                    SelectedWorker = Workers.FirstOrDefault(w => w.Name == dialog.NewWorker.Name);
                    SnackbarMessageQueue?.Enqueue("Worker added successfully!");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error adding worker: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task DeleteWageTransactionAsync(WageTransaction? transaction)
        {
            if (transaction == null) return;

            if (!MainWindowViewModel.Instance?.IsAdminMode ?? false)
            {
                ErrorMessage = "Only administrators can delete wage transactions.";
                MessageBox.Show(
                    "Only administrators can delete wage transactions.",
                    "Access Denied",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsBusy = true;
                var confirm = MessageBox.Show($"Delete {transaction.TransactionType} on {transaction.TransactionDate:dd-MMM-yyyy}?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (confirm != MessageBoxResult.Yes) { IsBusy = false; return; }
                _lastDeletedWageTransaction = transaction;
                await _wageService.DeleteWageTransactionAsync(transaction.WageTransactionId);
                if (SelectedWorker != null)
                {
                    await LoadWorkerDetailsAsync(SelectedWorker.WorkerId);
                }
                await LoadSummaryAsync();
                await LoadWorkersAsync();

                SnackbarMessageQueue?.Enqueue(
                    "Transaction deleted",
                    "UNDO",
                    () => Application.Current.Dispatcher.Invoke(async () => await UndoDeleteWageTransactionAsync()));
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
        private async Task UndoDeleteWageTransactionAsync()
        {
            if (_lastDeletedWageTransaction == null) return;
            try
            {
                IsBusy = true;
                await _wageService.RestoreWageTransactionAsync(_lastDeletedWageTransaction);
                _lastDeletedWageTransaction = null;
                if (SelectedWorker != null)
                {
                    await LoadWorkerDetailsAsync(SelectedWorker.WorkerId);
                }
                await LoadSummaryAsync();
                await LoadWorkersAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error undoing transaction delete: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadWorkerDetailsAsync(int workerId)
        {
            try
            {
                SelectedWorkerTotalWages = await _wageService.GetWorkerTotalWagesAsync(workerId);
                SelectedWorkerOutstandingAdvance = await _wageService.GetWorkerOutstandingAdvanceAsync(workerId);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading worker details: {ex.Message}";
            }
        }

        private async Task LoadSummaryAsync()
        {
            try
            {
                TotalWagesPaid = await _wageService.GetTotalWagesPaidAsync();
                TotalAdvancesGiven = await _wageService.GetTotalAdvancesGivenAsync();

                // Load all recent transactions
                var recentTransactions = await _wageService.GetTransactionsByDateRangeAsync(
                    DateTime.Now.AddDays(-30), DateTime.Now);
                AllTransactions = new ObservableCollection<WageTransaction>(
                    recentTransactions.OrderByDescending(t => t.TransactionDate));
                UpdatePaginatedData();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading summary: {ex.Message}";
            }
        }

        public async Task InitializeAsync()
        {
            await LoadWorkersAsync();
        }
    }
}


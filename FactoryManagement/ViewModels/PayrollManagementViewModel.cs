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
    public partial class PayrollManagementViewModel : ViewModelBase
    {
        private readonly IWageService _wageService;

        [ObservableProperty]
        private ObservableCollection<Worker> _workers = new();

        [ObservableProperty]
        private ObservableCollection<WageTransaction> _transactions = new();

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
        private decimal _overtimeRate;

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
        private string _searchText = string.Empty;

        [ObservableProperty]
        private bool _workerInfoVisible;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        private Worker? _lastDeletedWorker;
        private WageTransaction? _lastDeletedWageTransaction;
        public ISnackbarMessageQueue SnackbarMessageQueue { get; } = new SnackbarMessageQueue(TimeSpan.FromSeconds(4));

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

        partial void OnSearchTextChanged(string value)
        {
            FilterWorkers();
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
        private async Task NewWorkerAsync()
        {
            try
            {
                var dialog = new Views.QuickAddWorkerDialog();
                if (dialog.ShowDialog() == true && dialog.NewWorker != null)
                {
                    IsBusy = true;
                    await _wageService.AddWorkerAsync(dialog.NewWorker);
                    await LoadWorkersAsync();
                    MessageBox.Show("Worker added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding worker: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task EditWorkerAsync(Worker? worker)
        {
            if (worker == null) return;

            try
            {
                var dialog = new Views.QuickAddWorkerDialog(worker);
                if (dialog.ShowDialog() == true && dialog.NewWorker != null)
                {
                    IsBusy = true;
                    dialog.NewWorker.WorkerId = worker.WorkerId;
                    await _wageService.UpdateWorkerAsync(dialog.NewWorker);
                    await LoadWorkersAsync();
                    MessageBox.Show("Worker updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating worker: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    WorkerId = SelectedWorker.WorkerId,
                    TransactionType = transType,
                    TransactionDate = DateTime.Now,
                    Amount = Amount,
                    NetAmount = Amount,
                    EnteredBy = MainWindowViewModel.Instance?.CurrentUser?.UserId ?? 1,
                    Notes = Notes
                };

                await _wageService.RecordWagePaymentAsync(transaction);
                MessageBox.Show("Payment recorded successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                await LoadWorkersAsync();
                if (SelectedWorker != null)
                {
                    await LoadWorkerDetailsAsync(SelectedWorker.WorkerId);
                }
                
                // Clear form
                Amount = 0;
                Notes = string.Empty;
                ErrorMessage = string.Empty;
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
                    MessageBox.Show("Worker added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error adding worker: {ex.Message}";
                MessageBox.Show($"Error adding worker: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task DeleteWorkerAsync(Worker? worker)
        {
            if (worker == null) return;
            try
            {
                IsBusy = true;
                var confirm = MessageBox.Show($"Delete worker {worker.Name}?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (confirm != MessageBoxResult.Yes) { IsBusy = false; return; }
                _lastDeletedWorker = worker;
                await _wageService.DeleteWorkerAsync(worker.WorkerId);
                await LoadWorkersAsync();

                SnackbarMessageQueue.Enqueue(
                    "Worker deleted",
                    "UNDO",
                    () => Application.Current.Dispatcher.Invoke(async () => await UndoDeleteWorkerAsync()));
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error deleting worker: {ex.Message}";
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
                await LoadWorkersAsync();

                SnackbarMessageQueue.Enqueue(
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
        private async Task UndoDeleteWorkerAsync()
        {
            if (_lastDeletedWorker == null) return;
            try
            {
                IsBusy = true;
                // Restore the worker record
                var worker = new Worker
                {
                    Name = _lastDeletedWorker.Name,
                    MobileNumber = _lastDeletedWorker.MobileNumber,
                    Address = _lastDeletedWorker.Address,
                    Status = _lastDeletedWorker.Status,
                    Rate = _lastDeletedWorker.Rate,
                    DailyRate = _lastDeletedWorker.DailyRate,
                    HourlyRate = _lastDeletedWorker.HourlyRate,
                    MonthlyRate = _lastDeletedWorker.MonthlyRate,
                    TotalAdvance = _lastDeletedWorker.TotalAdvance,
                    TotalWagesPaid = _lastDeletedWorker.TotalWagesPaid,
                    JoiningDate = _lastDeletedWorker.JoiningDate,
                    LeavingDate = _lastDeletedWorker.LeavingDate,
                    Notes = _lastDeletedWorker.Notes
                };
                await _wageService.AddWorkerAsync(worker);
                _lastDeletedWorker = null;
                await LoadWorkersAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error undoing worker delete: {ex.Message}";
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
                var transactions = await _wageService.GetWorkerTransactionsAsync(workerId);
                Transactions = new ObservableCollection<WageTransaction>(transactions);

                SelectedWorkerTotalWages = await _wageService.GetWorkerTotalWagesAsync(workerId);
                SelectedWorkerOutstandingAdvance = await _wageService.GetWorkerOutstandingAdvanceAsync(workerId);

                // Use the new simplified Rate field
                if (SelectedWorker != null)
                {
                    // Normalize legacy DailyRate into Rate if Rate is not set or zero
                    if (SelectedWorker.Rate <= 0 && SelectedWorker.DailyRate > 0)
                    {
                        SelectedWorker.Rate = SelectedWorker.DailyRate;
                    }

                    // Always use Rate as the authoritative field
                    Rate = SelectedWorker.Rate;
                }
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
                Transactions = new ObservableCollection<WageTransaction>(
                    recentTransactions.OrderByDescending(t => t.TransactionDate));
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading summary: {ex.Message}";
            }
        }

        private void FilterWorkers()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                Workers = new ObservableCollection<Worker>(_allWorkers);
                return;
            }

            var filtered = _allWorkers.Where(w =>
                w.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                w.MobileNumber.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                w.Address.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

            Workers = new ObservableCollection<Worker>(filtered);
        }

        public async Task InitializeAsync()
        {
            await LoadWorkersAsync();
        }
    }
}


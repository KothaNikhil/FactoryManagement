using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FactoryManagement.Models;
using FactoryManagement.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MaterialDesignThemes.Wpf;

namespace FactoryManagement.ViewModels
{
    public partial class WorkersManagementViewModel : ViewModelBase
    {
        private readonly IWageService _wageService;

        [ObservableProperty]
        private ObservableCollection<Worker> _workers = new();

        [ObservableProperty]
        private Worker? _selectedWorker;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private string _workerName = string.Empty;

        [ObservableProperty]
        private string _mobileNumber = string.Empty;

        [ObservableProperty]
        private string _address = string.Empty;

        [ObservableProperty]
        private string _workerStatusString = "Active";

        [ObservableProperty]
        private string _workerNotes = string.Empty;

        [ObservableProperty]
        private bool _isEditMode;

        [ObservableProperty]
        private int _editingWorkerId;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        private Worker? _lastDeletedWorker;
        public ISnackbarMessageQueue SnackbarMessageQueue { get; } = new SnackbarMessageQueue(TimeSpan.FromSeconds(4));

        public ObservableCollection<string> WorkerStatuses { get; } = new() { "Active", "Inactive", "OnLeave", "Terminated" };

        private ObservableCollection<Worker> _allWorkers = new();

        public WorkersManagementViewModel(IWageService wageService)
        {
            _wageService = wageService;
        }

        partial void OnSearchTextChanged(string value)
        {
            FilterWorkers();
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
        private async Task SaveWorkerAsync()
        {
            try
            {
                ErrorMessage = string.Empty;

                if (string.IsNullOrWhiteSpace(WorkerName))
                {
                    ErrorMessage = "Worker name is required!";
                    return;
                }

                IsBusy = true;

                var status = Enum.Parse<WorkerStatus>(WorkerStatusString);
                
                var worker = new Worker
                {
                    WorkerId = IsEditMode ? EditingWorkerId : 0,
                    Name = WorkerName.Trim(),
                    MobileNumber = MobileNumber?.Trim() ?? string.Empty,
                    Address = Address?.Trim() ?? string.Empty,
                    Status = status,
                    Notes = WorkerNotes?.Trim() ?? string.Empty,
                    JoiningDate = IsEditMode ? DateTime.Now : DateTime.Now
                };

                if (IsEditMode)
                {
                    await _wageService.UpdateWorkerAsync(worker);
                    SnackbarMessageQueue.Enqueue("Worker updated successfully!");
                }
                else
                {
                    await _wageService.AddWorkerAsync(worker);
                    SnackbarMessageQueue.Enqueue("Worker added successfully!");
                }

                await LoadWorkersAsync();
                
                // Clear form
                CancelEditCommand.Execute(null);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void CancelEdit()
        {
            WorkerName = string.Empty;
            MobileNumber = string.Empty;
            Address = string.Empty;
            WorkerStatusString = "Active";
            WorkerNotes = string.Empty;
            ErrorMessage = string.Empty;
            IsEditMode = false;
            EditingWorkerId = 0;
        }

        [RelayCommand]
        private void EditWorkerAsync(Worker? worker)
        {
            if (worker == null) return;

            try
            {
                IsEditMode = true;
                EditingWorkerId = worker.WorkerId;
                WorkerName = worker.Name;
                MobileNumber = worker.MobileNumber ?? string.Empty;
                Address = worker.Address ?? string.Empty;
                WorkerStatusString = worker.Status.ToString();
                WorkerNotes = worker.Notes ?? string.Empty;
                ErrorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading worker: {ex.Message}";
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

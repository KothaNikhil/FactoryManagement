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
    /// <summary>
    /// ViewModel for Cash Book management - tracking daily cash balances and reconciliation
    /// </summary>
    public partial class CashBookViewModel : PaginationViewModel
    {
        private readonly ICashBookService _cashBookService;
        private readonly IUserService _userService;

        // Collections
        [ObservableProperty]
        private ObservableCollection<CashBalance> _allCashRecords = new();

        [ObservableProperty]
        private ObservableCollection<CashBalance> _paginatedCashRecords = new();

        // Selected record for details/edit
        [ObservableProperty]
        private CashBalance? _selectedRecord;

        // Summary metrics
        [ObservableProperty]
        private decimal _currentCashInHand;

        [ObservableProperty]
        private decimal _todayExpectedClosing;

        [ObservableProperty]
        private decimal _unreconciledDays;

        [ObservableProperty]
        private decimal _totalDiscrepancies;

        // Reconciliation form
        [ObservableProperty]
        private DateTime _reconciliationDate = DateTime.Today;

        [ObservableProperty]
        private decimal _actualCashCounted;

        [ObservableProperty]
        private string _discrepancyReason = string.Empty;

        [ObservableProperty]
        private string _reconciliationNotes = string.Empty;

        // Opening balance form (first time setup)
        [ObservableProperty]
        private DateTime _openingBalanceDate = DateTime.Today;

        [ObservableProperty]
        private decimal _openingBalanceAmount;

        [ObservableProperty]
        private string _openingBalanceNotes = "Initial opening balance";

        // Filters
        [ObservableProperty]
        private DateTime _filterStartDate = DateTime.Today.AddMonths(-1);

        [ObservableProperty]
        private DateTime _filterEndDate = DateTime.Today;

        [ObservableProperty]
        private bool _showOnlyUnreconciled;

        [ObservableProperty]
        private bool _showOnlyWithDiscrepancies;

        // Flags
        [ObservableProperty]
        private bool _isInitialized;

        [ObservableProperty]
        private bool _showOpeningBalanceForm;

        [ObservableProperty]
        private bool _showReconciliationForm;

        // Current user
        public int CurrentUserId { get; set; } = 1;

        // Success message for user feedback
        [ObservableProperty]
        private string _successMessage = string.Empty;

        // Cash flow details for selected date
        [ObservableProperty]
        private decimal _selectedDateOpeningBalance;

        [ObservableProperty]
        private decimal _selectedDateCashIn;

        [ObservableProperty]
        private decimal _selectedDateCashOut;

        [ObservableProperty]
        private decimal _selectedDateExpectedClosing;

        public CashBookViewModel(
            ICashBookService cashBookService,
            IUserService userService)
        {
            _cashBookService = cashBookService ?? throw new ArgumentNullException(nameof(cashBookService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        public async Task InitializeAsync()
        {
            await LoadDataAsync();
        }

        [RelayCommand]
        private async Task LoadDataAsync()
        {
            if (IsBusy) return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                // Check if cash book is initialized (only if not already set)
                if (!IsInitialized)
                {
                    IsInitialized = await _cashBookService.IsInitializedAsync();
                }

                if (!IsInitialized)
                {
                    ShowOpeningBalanceForm = true;
                    return;
                }

                // Ensure opening balance form is hidden
                ShowOpeningBalanceForm = false;

                // Load data in parallel
                var recordsTask = LoadCashRecordsAsync();
                var metricsTask = LoadSummaryMetricsAsync();

                await Task.WhenAll(recordsTask, metricsTask);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading cash book data: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadCashRecordsAsync()
        {
            var records = await _cashBookService.GetByDateRangeAsync(FilterStartDate, FilterEndDate);
            var filtered = records.AsEnumerable();

            if (ShowOnlyUnreconciled)
                filtered = filtered.Where(r => !r.IsReconciled);

            if (ShowOnlyWithDiscrepancies)
                filtered = filtered.Where(r => r.IsReconciled && r.Discrepancy != 0);

            AllCashRecords.Clear();
            foreach (var record in filtered.OrderByDescending(r => r.Date))
                AllCashRecords.Add(record);

            CurrentPage = 1;
            UpdatePaginatedData();
        }

        private async Task LoadSummaryMetricsAsync()
        {
            try
            {
                CurrentCashInHand = await _cashBookService.GetCurrentCashInHandAsync();
                TodayExpectedClosing = await _cashBookService.GetExpectedClosingBalanceAsync(DateTime.Today);

                var unreconciled = await _cashBookService.GetUnreconciledDaysAsync();
                UnreconciledDays = unreconciled.Count();

                TotalDiscrepancies = await _cashBookService.GetTotalDiscrepancyAsync(
                    DateTime.Today.AddMonths(-1), DateTime.Today);
            }
            catch (Exception)
            {
                // If there's an error loading metrics (e.g., no records yet), set to defaults
                CurrentCashInHand = 0;
                TodayExpectedClosing = 0;
                UnreconciledDays = 0;
                TotalDiscrepancies = 0;
            }
        }

        /// <summary>
        /// Force refresh all data and UI elements
        /// </summary>
        private async Task RefreshAllDataAsync()
        {
            // Clear collections first to force UI update
            AllCashRecords.Clear();
            PaginatedCashRecords.Clear();
            
            // Load fresh data
            await LoadCashRecordsAsync();
            await LoadSummaryMetricsAsync();
            
            // Force property notifications
            OnPropertyChanged(nameof(AllCashRecords));
            OnPropertyChanged(nameof(PaginatedCashRecords));
            OnPropertyChanged(nameof(CurrentCashInHand));
            OnPropertyChanged(nameof(TodayExpectedClosing));
            OnPropertyChanged(nameof(UnreconciledDays));
            OnPropertyChanged(nameof(TotalDiscrepancies));
        }

        protected override void UpdatePaginatedData()
        {
            var startIndex = (CurrentPage - 1) * DefaultPageSize;
            var pageData = AllCashRecords
                .Skip(startIndex)
                .Take(DefaultPageSize)
                .ToList();

            PaginatedCashRecords.Clear();
            foreach (var record in pageData)
                PaginatedCashRecords.Add(record);

            TotalPages = (int)Math.Ceiling((double)AllCashRecords.Count / DefaultPageSize);
            OnPropertyChanged(nameof(CanGoToPreviousPage));
            OnPropertyChanged(nameof(CanGoToNextPage));
        }

        /// <summary>
        /// Set opening balance for first time setup
        /// </summary>
        [RelayCommand]
        private async Task SetOpeningBalanceAsync()
        {
            if (IsBusy) return;
            if (OpeningBalanceAmount < 0)
            {
                ErrorMessage = "Opening balance cannot be negative.";
                return;
            }

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                await _cashBookService.SetOpeningBalanceAsync(
                    OpeningBalanceDate,
                    OpeningBalanceAmount,
                    CurrentUserId,
                    OpeningBalanceNotes);

                // Update state and allow UI to process changes
                IsInitialized = true;
                ShowOpeningBalanceForm = false;
                
                // Small delay to allow UI to update visibility
                await Task.Delay(50);
                
                // Force refresh of all data
                await RefreshAllDataAsync();
                
                SuccessMessage = "✓ Opening balance set successfully";

                // Reset form
                OpeningBalanceAmount = 0;
                OpeningBalanceNotes = "Initial opening balance";
                OpeningBalanceDate = DateTime.Today;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error setting opening balance: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Reconcile cash for a specific date
        /// </summary>
        [RelayCommand]
        private async Task ReconcileCashAsync()
        {
            if (IsBusy) return;
            if (ActualCashCounted < 0)
            {
                ErrorMessage = "Actual cash counted cannot be negative.";
                return;
            }

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                var reconciledRecord = await _cashBookService.ReconcileCashAsync(
                    ReconciliationDate,
                    ActualCashCounted,
                    CurrentUserId,
                    string.IsNullOrEmpty(DiscrepancyReason) ? null : DiscrepancyReason,
                    string.IsNullOrEmpty(ReconciliationNotes) ? null : ReconciliationNotes);

                if (reconciledRecord.Discrepancy == 0)
                {
                    SuccessMessage = "✓ Cash reconciled successfully - Balanced!";
                }
                else if (reconciledRecord.Discrepancy > 0)
                {
                    SuccessMessage = $"✓ Cash reconciled - Surplus of ₹{reconciledRecord.Discrepancy:N2}";
                }
                else
                {
                    SuccessMessage = $"⚠ Cash reconciled - Shortage of ₹{Math.Abs(reconciledRecord.Discrepancy ?? 0):N2}";
                }

                ShowReconciliationForm = false;

                // Reload and refresh all data
                await RefreshAllDataAsync();

                // Reset form
                ResetReconciliationForm();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error reconciling cash: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Show reconciliation form for a specific date
        /// </summary>
        [RelayCommand]
        private async Task ShowReconciliationFormForDate(DateTime? date)
        {
            ReconciliationDate = date ?? DateTime.Today;
            
            // Get expected closing for this date
            var flow = await _cashBookService.GetCashFlowForDateAsync(ReconciliationDate);
            TodayExpectedClosing = flow.ExpectedClosingBalance;
            
            // Pre-fill with expected amount
            ActualCashCounted = flow.ExpectedClosingBalance;
            
            ShowReconciliationForm = true;
        }

        /// <summary>
        /// View cash flow details for selected record
        /// </summary>
        [RelayCommand]
        private async Task ViewCashFlowDetails(CashBalance? record)
        {
            if (record == null) return;

            SelectedRecord = record;
            
            var flow = await _cashBookService.GetCashFlowForDateAsync(record.Date);
            SelectedDateOpeningBalance = flow.OpeningBalance;
            SelectedDateCashIn = flow.TotalCashIn;
            SelectedDateCashOut = flow.TotalCashOut;
            SelectedDateExpectedClosing = flow.ExpectedClosingBalance;
        }

        /// <summary>
        /// Refresh today's cash flow calculation
        /// </summary>
        [RelayCommand]
        private async Task RefreshTodayAsync()
        {
            if (IsBusy) return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                await _cashBookService.CreateOrUpdateDailyCashRecordAsync(DateTime.Today);
                SuccessMessage = "✓ Today's cash flow refreshed";
                await RefreshAllDataAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error refreshing: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Apply filters and reload
        /// </summary>
        [RelayCommand]
        private async Task ApplyFiltersAsync()
        {
            await LoadCashRecordsAsync();
        }

        /// <summary>
        /// Clear all filters
        /// </summary>
        [RelayCommand]
        private async Task ClearFiltersAsync()
        {
            FilterStartDate = DateTime.Today.AddMonths(-1);
            FilterEndDate = DateTime.Today;
            ShowOnlyUnreconciled = false;
            ShowOnlyWithDiscrepancies = false;
            await LoadCashRecordsAsync();
        }

        /// <summary>
        /// Delete a cash balance record (admin function)
        /// </summary>
        [RelayCommand]
        private async Task DeleteRecordAsync(int? recordId)
        {
            if (!recordId.HasValue || IsBusy) return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                var success = await _cashBookService.DeleteAsync(recordId.Value);
                if (success)
                {
                    SuccessMessage = "✓ Record deleted successfully";
                    await LoadDataAsync();
                }
                else
                {
                    ErrorMessage = "Record not found";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error deleting record: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ResetReconciliationForm()
        {
            ReconciliationDate = DateTime.Today;
            ActualCashCounted = 0;
            DiscrepancyReason = string.Empty;
            ReconciliationNotes = string.Empty;
        }

        // Cancel actions
        [RelayCommand]
        private void CancelOpeningBalance()
        {
            ShowOpeningBalanceForm = false;
        }

        [RelayCommand]
        private void CancelReconciliation()
        {
            ShowReconciliationForm = false;
            ResetReconciliationForm();
        }
    }
}

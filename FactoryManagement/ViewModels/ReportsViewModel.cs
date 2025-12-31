using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FactoryManagement.Models;
using FactoryManagement.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace FactoryManagement.ViewModels
{
    public enum ReportType
    {
        All,
        Inventory,
        Financial,
        Wages
    }

    public partial class ReportsViewModel : ViewModelBase
    {
            public System.Collections.IEnumerable CurrentTransactions
            {
                get
                {
                    return SelectedReportType switch
                    {
                        ReportType.All => PaginatedAllTransactions,
                        ReportType.Inventory => PaginatedTransactions,
                        ReportType.Financial => PaginatedFinancialTransactions,
                        ReportType.Wages => PaginatedWageTransactions,
                        _ => PaginatedTransactions
                    };
                }
            }
        private readonly ITransactionService _transactionService;
        private readonly IItemService _itemService;
        private readonly IPartyService _partyService;
        private readonly IExportService _exportService;
        private readonly IFinancialTransactionService _financialService;
        private readonly IWageService _wageService;
        private readonly IUnifiedTransactionService _unifiedTransactionService;
        private readonly IReportExportBuilder? _reportExportBuilder;
        private readonly IUserService _userService;

        // Full collections (all data)
        private ObservableCollection<Transaction> _allInventoryTransactions = new();
        private ObservableCollection<FinancialTransaction> _allFinancialTransactions = new();
        private ObservableCollection<WageTransaction> _allWageTransactions = new();
        private ObservableCollection<Services.UnifiedTransactionViewModel> _allUnifiedTransactions = new();

        // Paginated collections (displayed data)
        [ObservableProperty]
        private ObservableCollection<Transaction> _paginatedTransactions = new();

        [ObservableProperty]
        private ObservableCollection<FinancialTransaction> _paginatedFinancialTransactions = new();

        [ObservableProperty]
        private ObservableCollection<WageTransaction> _paginatedWageTransactions = new();

        [ObservableProperty]
        private ObservableCollection<Services.UnifiedTransactionViewModel> _paginatedAllTransactions = new();

        // Legacy properties for backward compatibility with exports
        public ObservableCollection<Transaction> Transactions => _allInventoryTransactions;
        public ObservableCollection<FinancialTransaction> FinancialTransactions => _allFinancialTransactions;
        public ObservableCollection<WageTransaction> WageTransactions => _allWageTransactions;
        public ObservableCollection<Services.UnifiedTransactionViewModel> AllTransactions => _allUnifiedTransactions;

        // Pagination properties
        private const int PageSize = 13;

        [ObservableProperty]
        private int _currentPage = 1;

        [ObservableProperty]
        private int _totalPages = 1;

        [ObservableProperty]
        private int _totalRecords = 0;

        public bool CanGoToPreviousPage => CurrentPage > 1;
        public bool CanGoToNextPage => CurrentPage < TotalPages;

        [ObservableProperty]
        private ObservableCollection<Item> _items = new();

        [ObservableProperty]
        private ObservableCollection<Party> _parties = new();

        [ObservableProperty]
        private ObservableCollection<Worker> _workers = new();

        [ObservableProperty]
        private ObservableCollection<User> _users = new();

        // Unified names collection for searchable filter (contains all parties and workers)
        [ObservableProperty]
        private ObservableCollection<string> _allNames = new();

        [ObservableProperty]
        private Item? _selectedItem;

        [ObservableProperty]
        private Party? _selectedParty;

        [ObservableProperty]
        private Worker? _selectedWorker;

        [ObservableProperty]
        private string? _selectedName;

        [ObservableProperty]
        private User? _selectedUser;

        [ObservableProperty]
        private DateTime _startDate = DateTime.Now.AddMonths(-1);

        [ObservableProperty]
        private DateTime _endDate = DateTime.Now;

        [ObservableProperty]
        private ReportType _selectedReportType = ReportType.All;

        // Column visibility helpers
        public bool IsAllView => SelectedReportType == ReportType.All;
        public bool IsInventoryView => SelectedReportType == ReportType.Inventory;
        public bool IsFinancialView => SelectedReportType == ReportType.Financial;
        public bool IsWagesView => SelectedReportType == ReportType.Wages;

        [ObservableProperty]
        private decimal _totalAmount;

        [ObservableProperty]
        private decimal _totalDebit;

        [ObservableProperty]
        private decimal _totalCredit;

        [ObservableProperty]
        private decimal _totalDifference;

        [ObservableProperty]
        private decimal _totalInventoryAmount;

        [ObservableProperty]
        private decimal _totalFinancialAmount;

        [ObservableProperty]
        private decimal _totalWagesAmount;

        [ObservableProperty]
        private int _transactionCount;

        [ObservableProperty]
        private string _reportTitle = "All Transactions";

        public ObservableCollection<ReportType> ReportTypes { get; } = new()
        {
            ReportType.All,
            ReportType.Inventory,
            ReportType.Financial,
            ReportType.Wages
        };

        public ReportsViewModel(
            ITransactionService transactionService,
            IItemService itemService,
            IPartyService partyService,
            IExportService exportService,
            IFinancialTransactionService financialService,
            IWageService wageService,
            IUnifiedTransactionService unifiedTransactionService,
            IUserService userService,
            IReportExportBuilder? reportExportBuilder = null)
        {
            _transactionService = transactionService;
            _itemService = itemService;
            _partyService = partyService;
            _exportService = exportService;
            _financialService = financialService;
            _wageService = wageService;
            _unifiedTransactionService = unifiedTransactionService;
            _userService = userService;
            _reportExportBuilder = reportExportBuilder;
        }

        // Back-compat ctor for tests and callers not yet updated
        public ReportsViewModel(
            ITransactionService transactionService,
            IItemService itemService,
            IPartyService partyService,
            IExportService exportService,
            IFinancialTransactionService financialService,
            IWageService wageService,
            IUnifiedTransactionService unifiedTransactionService,
            IUserService userService)
            : this(
                transactionService,
                itemService,
                partyService,
                exportService,
                financialService,
                wageService,
                unifiedTransactionService,
                userService,
                null)
        {
        }

        partial void OnSelectedReportTypeChanged(ReportType value)
        {
            // Clear filters when changing report type
            SelectedItem = null;
            SelectedParty = null;
            SelectedWorker = null;
            CurrentPage = 1;
            
            _ = LoadReportDataAsync();
            OnPropertyChanged(nameof(CurrentTransactions));
            OnPropertyChanged(nameof(IsAllView));
            OnPropertyChanged(nameof(IsInventoryView));
            OnPropertyChanged(nameof(IsFinancialView));
            OnPropertyChanged(nameof(IsWagesView));
        }

        partial void OnCurrentPageChanged(int value)
        {
            UpdatePaginatedData();
            OnPropertyChanged(nameof(CanGoToPreviousPage));
            OnPropertyChanged(nameof(CanGoToNextPage));
            GoToPreviousPageCommand.NotifyCanExecuteChanged();
            GoToNextPageCommand.NotifyCanExecuteChanged();
        }

        partial void OnSelectedItemChanged(Item? value)
        {
            if (SelectedReportType == ReportType.Inventory)
            {
                _ = ApplyFiltersAsync();
            }
        }

        partial void OnSelectedPartyChanged(Party? value)
        {
            if (SelectedReportType == ReportType.Inventory || SelectedReportType == ReportType.Financial)
            {
                _ = ApplyFiltersAsync();
            }
        }

        partial void OnSelectedWorkerChanged(Worker? value)
        {
            if (SelectedReportType == ReportType.Wages)
            {
                _ = ApplyFiltersAsync();
            }
        }

        partial void OnSelectedUserChanged(User? value)
        {
            _ = ApplyFiltersAsync();
        }

        partial void OnSelectedNameChanged(string? value)
        {
            _ = ApplyFiltersAsync();
        }

        partial void OnStartDateChanged(DateTime value)
        {
            _ = ApplyFiltersAsync();
        }

        partial void OnEndDateChanged(DateTime value)
        {
            _ = ApplyFiltersAsync();
        }

        [RelayCommand]
        private async Task LoadDataAsync()
        {
            try
            {
                IsBusy = true;

                // Load all transactions to get distinct values (including deleted items/parties)
                var allTransactions = await _transactionService.GetAllTransactionsAsync();
                var allFinancial = await _financialService.GetAllFinancialTransactionsAsync();
                var allWages = await _wageService.GetTransactionsByDateRangeAsync(DateTime.MinValue, DateTime.MaxValue);

                // Load active items and parties from master tables
                var activeItems = await _itemService.GetAllItemsAsync();
                var activeParties = await _partyService.GetAllPartiesAsync();

                // Populate Items dropdown: active items + historical deleted items from transactions
                Items.Clear();
                Items.Add(new Item { ItemId = 0, ItemName = "All Items" });
                
                // Combine active item names with historical item names from transactions
                var activeItemNames = activeItems.Select(i => i.ItemName);
                var historicalItemNames = allTransactions
                    .Where(t => !string.IsNullOrEmpty(t.ItemName))
                    .Select(t => t.ItemName);
                var allItemNames = activeItemNames.Concat(historicalItemNames)
                    .Distinct()
                    .OrderBy(name => name);
                
                foreach (var itemName in allItemNames)
                    Items.Add(new Item { ItemId = 0, ItemName = itemName });

                // Populate Parties dropdown: active parties + historical deleted parties from transactions
                Parties.Clear();
                Parties.Add(new Party { PartyId = 0, Name = "All Parties" });
                
                // Combine active party names with historical party names from transactions and financial records
                var activePartyNames = activeParties.Select(p => p.Name);
                var historicalPartyNames = allTransactions
                    .Where(t => !string.IsNullOrEmpty(t.PartyName))
                    .Select(t => t.PartyName)
                    .Concat(allFinancial.Where(f => !string.IsNullOrEmpty(f.PartyName)).Select(f => f.PartyName));
                var allPartyNames = activePartyNames.Concat(historicalPartyNames)
                    .Distinct()
                    .OrderBy(name => name);
                
                foreach (var partyName in allPartyNames)
                    Parties.Add(new Party { PartyId = 0, Name = partyName });

                // Populate Workers dropdown from Workers table (keeping original behavior)
                var workers = await _wageService.GetAllWorkersAsync();
                Workers.Clear();
                Workers.Add(new Worker { WorkerId = 0, Name = "All Workers" });
                foreach (var worker in workers)
                    Workers.Add(worker);

                var users = await _userService.GetAllUsersAsync();
                Users.Clear();
                // Add "All Users" option
                Users.Add(new User { UserId = 0, Username = "All Users" });
                foreach (var user in users)
                    Users.Add(user);

                // Populate unified names collection from all party and worker names
                AllNames.Clear();
                AllNames.Add("All Names"); // Default option
                foreach (var name in allPartyNames)
                    AllNames.Add(name);
                foreach (var worker in workers)
                    if (!string.IsNullOrEmpty(worker.Name))
                        AllNames.Add(worker.Name);

                // Set default selections
                SelectedItem = Items.Count > 0 ? Items[0] : null;
                SelectedParty = Parties.Count > 0 ? Parties[0] : null;
                SelectedWorker = Workers.Count > 0 ? Workers[0] : null;
                SelectedUser = Users.Count > 0 ? Users[0] : null;
                SelectedName = AllNames.Count > 0 ? AllNames[0] : null;

                await LoadReportDataAsync();
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
        private async Task LoadReportDataAsync()
        {
            switch (SelectedReportType)
            {
                case ReportType.All:
                    await LoadAllCombinedTransactionsAsync();
                    break;
                case ReportType.Inventory:
                    await LoadAllTransactionsAsync();
                    break;
                case ReportType.Financial:
                    await LoadAllFinancialTransactionsAsync();
                    break;
                case ReportType.Wages:
                    await LoadAllWageTransactionsAsync();
                    break;
            }
        }

        [RelayCommand]
        private async Task LoadAllCombinedTransactionsAsync()
        {
            try
            {
                IsBusy = true;
                
                // Use the shared service to get all unified transactions
                var unifiedTransactions = await _unifiedTransactionService.GetAllUnifiedTransactionsAsync();
                
                _allUnifiedTransactions.Clear();
                foreach (var t in unifiedTransactions)
                    _allUnifiedTransactions.Add(t);
                
                CurrentPage = 1;
                UpdatePaginatedData();
                CalculateTotals();
                ReportTitle = "All Transactions (Combined)";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading combined transactions: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ApplyFiltersAsync()
        {
            try
            {
                IsBusy = true;

                switch (SelectedReportType)
                {
                    case ReportType.All:
                        await ApplyAllTransactionsFiltersAsync();
                        break;
                    case ReportType.Inventory:
                        await ApplyInventoryFiltersAsync();
                        break;
                    case ReportType.Financial:
                        await ApplyFinancialFiltersAsync();
                        break;
                    case ReportType.Wages:
                        await ApplyWageFiltersAsync();
                        break;
                }

                CalculateTotals();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error applying filters: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ApplyAllTransactionsFiltersAsync()
        {
            IEnumerable<UnifiedTransactionViewModel> transactions = await _unifiedTransactionService.GetAllUnifiedTransactionsAsync();

            // Apply date filter
            transactions = transactions.Where(t => t.TransactionDate >= StartDate && t.TransactionDate <= EndDate.AddDays(1).AddSeconds(-1));

            // Apply user filter (skip if "All Users" is selected)
            if (SelectedUser != null && SelectedUser.UserId != 0)
            {
                transactions = transactions.Where(t => t.EnteredBy == SelectedUser.Username);
            }

            // Apply name filter (skip if "All Names" is selected)
            if (!string.IsNullOrEmpty(SelectedName) && SelectedName != "All Names")
            {
                transactions = transactions.Where(t => t.Name == SelectedName);
            }

            _allUnifiedTransactions.Clear();
            foreach (var t in transactions)
                _allUnifiedTransactions.Add(t);

            CurrentPage = 1;
            UpdatePaginatedData();
            UpdateReportTitle();
        }

        private async Task ApplyInventoryFiltersAsync()
        {
            IEnumerable<Transaction> transactions = await _transactionService.GetAllTransactionsAsync();

            // Apply item filter (skip if "All Items" is selected)
            if (SelectedItem != null && SelectedItem.ItemName != "All Items")
            {
                transactions = transactions.Where(t => t.ItemName == SelectedItem.ItemName);
            }

            // Apply party filter (skip if "All Parties" is selected)
            if (SelectedParty != null && SelectedParty.Name != "All Parties")
            {
                transactions = transactions.Where(t => t.PartyName == SelectedParty.Name);
            }

            // Apply user filter (skip if "All Users" is selected)
            if (SelectedUser != null && SelectedUser.UserId != 0)
            {
                transactions = transactions.Where(t => t.EnteredBy == SelectedUser.UserId);
            }

            // Apply date range filter
            transactions = transactions.Where(t => t.TransactionDate >= StartDate && t.TransactionDate <= EndDate);

            _allInventoryTransactions.Clear();
            foreach (var t in transactions)
                _allInventoryTransactions.Add(t);

            CurrentPage = 1;
            UpdatePaginatedData();
            UpdateReportTitle();
        }

        private async Task ApplyFinancialFiltersAsync()
        {
            IEnumerable<FinancialTransaction> transactions = await _financialService.GetAllFinancialTransactionsAsync();

            // Apply party filter (skip if "All Parties" is selected)
            if (SelectedParty != null && SelectedParty.Name != "All Parties")
            {
                transactions = transactions.Where(t => t.PartyName == SelectedParty.Name);
            }

            // Apply user filter (skip if "All Users" is selected)
            if (SelectedUser != null && SelectedUser.UserId != 0)
            {
                transactions = transactions.Where(t => t.EnteredBy == SelectedUser.UserId);
            }

            // Apply date range filter
            transactions = transactions.Where(t => t.TransactionDate >= StartDate && t.TransactionDate <= EndDate);

            _allFinancialTransactions.Clear();
            foreach (var t in transactions)
                _allFinancialTransactions.Add(t);

            CurrentPage = 1;
            UpdatePaginatedData();
            UpdateReportTitle();
        }

        private async Task ApplyWageFiltersAsync()
        {
            var transactions = await _wageService.GetTransactionsByDateRangeAsync(StartDate, EndDate);

            // Apply worker filter (skip if "All Workers" is selected)
            if (SelectedWorker != null && SelectedWorker.WorkerId != 0)
            {
                transactions = transactions.Where(t => t.WorkerId == SelectedWorker.WorkerId);
            }

            // Apply user filter (skip if "All Users" is selected)
            if (SelectedUser != null && SelectedUser.UserId != 0)
            {
                transactions = transactions.Where(t => t.EnteredBy == SelectedUser.UserId);
            }

            _allWageTransactions.Clear();
            foreach (var t in transactions)
                _allWageTransactions.Add(t);

            CurrentPage = 1;
            UpdatePaginatedData();
            UpdateReportTitle();
        }

        private void UpdateReportTitle()
        {
            var filters = new List<string>();

            if (SelectedItem != null && SelectedItem.ItemId != 0)
                filters.Add($"Item: {SelectedItem.ItemName}");
            if (SelectedParty != null && SelectedParty.PartyId != 0)
                filters.Add($"Party: {SelectedParty.Name}");
            if (SelectedWorker != null && SelectedWorker.WorkerId != 0)
                filters.Add($"Worker: {SelectedWorker.Name}");
            if (StartDate != DateTime.MinValue || EndDate != DateTime.MaxValue)
                filters.Add($"{StartDate:dd/MM/yyyy} to {EndDate:dd/MM/yyyy}");

            ReportTitle = filters.Count > 0 
                ? $"{SelectedReportType} Transactions - {string.Join(", ", filters)}"
                : $"All {SelectedReportType} Transactions";
        }

        [RelayCommand]
        private async Task ClearFiltersAsync()
        {
            SelectedItem = Items.Count > 0 ? Items[0] : null;
            SelectedParty = Parties.Count > 0 ? Parties[0] : null;
            SelectedWorker = Workers.Count > 0 ? Workers[0] : null;
            SelectedName = AllNames.Count > 0 ? AllNames[0] : null;
            SelectedUser = Users.Count > 0 ? Users[0] : null;
            StartDate = DateTime.Now.AddMonths(-1);
            EndDate = DateTime.Now;
            
            // Reload data for current category
            await LoadReportDataAsync();
        }

        [RelayCommand]
        private async Task LoadAllTransactionsAsync()
        {
            try
            {
                IsBusy = true;
                var transactions = await _transactionService.GetAllTransactionsAsync();
                var transList = transactions.ToList();
                _allInventoryTransactions.Clear();
                foreach (var t in transList)
                    _allInventoryTransactions.Add(t);
                CurrentPage = 1;
                UpdatePaginatedData();
                CalculateTotals();
                ReportTitle = "All Inventory Transactions";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading transactions: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task LoadAllFinancialTransactionsAsync()
        {
            try
            {
                IsBusy = true;
                var transactions = await _financialService.GetAllFinancialTransactionsAsync();
                var transList = transactions.ToList();
                _allFinancialTransactions.Clear();
                foreach (var t in transList)
                    _allFinancialTransactions.Add(t);
                CurrentPage = 1;
                UpdatePaginatedData();
                CalculateTotals();
                ReportTitle = "All Financial Transactions";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading financial transactions: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task LoadAllWageTransactionsAsync()
        {
            try
            {
                IsBusy = true;
                var startDate = DateTime.MinValue;
                var endDate = DateTime.MaxValue;
                var transactions = await _wageService.GetTransactionsByDateRangeAsync(startDate, endDate);
                var transList = transactions.ToList();
                _allWageTransactions.Clear();
                foreach (var t in transList)
                    _allWageTransactions.Add(t);
                CurrentPage = 1;
                UpdatePaginatedData();
                CalculateTotals();
                ReportTitle = "All Wage Transactions";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading wage transactions: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task FilterByItemAsync()
        {
            if (SelectedItem == null) return;

            try
            {
                IsBusy = true;
                var transactions = await _transactionService.GetTransactionsByItemAsync(SelectedItem.ItemId);
                var transList = transactions.ToList();
                _allInventoryTransactions.Clear();
                foreach (var t in transList)
                    _allInventoryTransactions.Add(t);
                CurrentPage = 1;
                UpdatePaginatedData();
                CalculateTotals();
                ReportTitle = $"Transactions for {SelectedItem.ItemName}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error filtering by item: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task FilterByPartyAsync()
        {
            if (SelectedParty == null) return;

            try
            {
                IsBusy = true;
                var transactions = await _transactionService.GetTransactionsByPartyAsync(SelectedParty.PartyId);
                var transList = transactions.ToList();
                _allInventoryTransactions.Clear();
                foreach (var t in transList)
                    _allInventoryTransactions.Add(t);
                CurrentPage = 1;
                UpdatePaginatedData();
                CalculateTotals();
                ReportTitle = $"Transactions for {SelectedParty.Name}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error filtering by party: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task FilterByDateRangeAsync()
        {
            try
            {
                IsBusy = true;
                var transactions = await _transactionService.GetTransactionsByDateRangeAsync(StartDate, EndDate);
                var transList = transactions.ToList();
                _allInventoryTransactions.Clear();
                foreach (var t in transList)
                    _allInventoryTransactions.Add(t);
                CurrentPage = 1;
                UpdatePaginatedData();
                CalculateTotals();
                ReportTitle = $"Transactions from {StartDate:dd/MM/yyyy} to {EndDate:dd/MM/yyyy}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error filtering by date range: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task ExportToExcelAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[ReportsViewModel] Starting Excel export for {SelectedReportType}");
                
                if (_reportExportBuilder == null)
                {
                    ErrorMessage = "Report export service is not available.";
                    return;
                }

                var dialog = new SaveFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx",
                    DefaultExt = "xlsx",
                    FileName = $"{SelectedReportType}Transactions_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };

                    if (dialog.ShowDialog() == true)
                {
                    IsBusy = true;
                    var rows = await _reportExportBuilder.BuildExportRowsAsync(
                        SelectedReportType,
                        _allInventoryTransactions,
                        _allFinancialTransactions,
                        _allWageTransactions,
                        _allUnifiedTransactions);
                    System.Diagnostics.Debug.WriteLine($"[ReportsViewModel] Calling ExportService with {rows.Count} rows");
                    await _exportService.ExportToExcelAsync(rows, dialog.FileName, $"{SelectedReportType}Transactions");
                    ErrorMessage = "Export to Excel completed successfully!";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ReportsViewModel] EXCEPTION in ExportToExcelAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[ReportsViewModel] Stack trace: {ex.StackTrace}");
                ErrorMessage = $"Error exporting to Excel: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task ExportToCsvAsync()
        {
            try
            {
                if (_reportExportBuilder == null)
                {
                    ErrorMessage = "Report export service is not available.";
                    return;
                }

                var dialog = new SaveFileDialog
                {
                    Filter = "CSV Files (*.csv)|*.csv",
                    DefaultExt = "csv",
                    FileName = $"{SelectedReportType}Transactions_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                    if (dialog.ShowDialog() == true)
                {
                    IsBusy = true;
                    var rows = await _reportExportBuilder.BuildExportRowsAsync(
                        SelectedReportType,
                        _allInventoryTransactions,
                        _allFinancialTransactions,
                        _allWageTransactions,
                        _allUnifiedTransactions);
                    await _exportService.ExportToCsvAsync(rows, dialog.FileName);
                    ErrorMessage = "Export to CSV completed successfully!";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error exporting to CSV: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Export row building moved to IReportExportBuilder service

        private void CalculateTotals()
        {
            TotalInventoryAmount = _allInventoryTransactions.Sum(t => t.TotalAmount);
            TotalFinancialAmount = _allFinancialTransactions.Sum(t => t.Amount);
            TotalWagesAmount = _allWageTransactions.Sum(t => t.NetAmount);
            
            TotalAmount = SelectedReportType switch
            {
                ReportType.All => TotalInventoryAmount + TotalFinancialAmount + TotalWagesAmount,
                ReportType.Inventory => TotalInventoryAmount,
                ReportType.Financial => TotalFinancialAmount,
                ReportType.Wages => TotalWagesAmount,
                _ => 0
            };

            // Compute Debit/Credit totals based on current filtered collections
            decimal debit = 0m;
            decimal credit = 0m;

            switch (SelectedReportType)
            {
                case ReportType.All:
                    debit = _allUnifiedTransactions.Where(t => t.DebitCredit == "Debit").Sum(t => t.Amount);
                    credit = _allUnifiedTransactions.Where(t => t.DebitCredit == "Credit").Sum(t => t.Amount);
                    break;
                case ReportType.Inventory:
                    debit = _allInventoryTransactions.Where(t => t.DebitCredit == "Debit").Sum(t => t.TotalAmount);
                    credit = _allInventoryTransactions.Where(t => t.DebitCredit == "Credit").Sum(t => t.TotalAmount);
                    break;
                case ReportType.Financial:
                    debit = _allFinancialTransactions.Where(t => t.DebitCredit == "Debit").Sum(t => t.Amount);
                    credit = _allFinancialTransactions.Where(t => t.DebitCredit == "Credit").Sum(t => t.Amount);
                    break;
                case ReportType.Wages:
                    debit = _allWageTransactions.Where(t => t.DebitCredit == "Debit").Sum(t => t.NetAmount);
                    credit = _allWageTransactions.Where(t => t.DebitCredit == "Credit").Sum(t => t.NetAmount);
                    break;
            }

            TotalDebit = debit;
            TotalCredit = credit;
            TotalDifference = credit - debit;

            TransactionCount = SelectedReportType switch
            {
                ReportType.All => _allUnifiedTransactions.Count,
                ReportType.Inventory => _allInventoryTransactions.Count,
                ReportType.Financial => _allFinancialTransactions.Count,
                ReportType.Wages => _allWageTransactions.Count,
                _ => 0
            };
        }

        private void UpdatePaginatedData()
        {
            switch (SelectedReportType)
            {
                case ReportType.All:
                    TotalRecords = _allUnifiedTransactions.Count;
                    TotalPages = (int)Math.Ceiling((double)TotalRecords / PageSize);
                    if (TotalPages == 0) TotalPages = 1;
                    
                    PaginatedAllTransactions.Clear();
                    var allPageData = _allUnifiedTransactions
                        .Skip((CurrentPage - 1) * PageSize)
                        .Take(PageSize);
                    foreach (var item in allPageData)
                        PaginatedAllTransactions.Add(item);
                    break;

                case ReportType.Inventory:
                    TotalRecords = _allInventoryTransactions.Count;
                    TotalPages = (int)Math.Ceiling((double)TotalRecords / PageSize);
                    if (TotalPages == 0) TotalPages = 1;
                    
                    PaginatedTransactions.Clear();
                    var inventoryPageData = _allInventoryTransactions
                        .Skip((CurrentPage - 1) * PageSize)
                        .Take(PageSize);
                    foreach (var item in inventoryPageData)
                        PaginatedTransactions.Add(item);
                    break;

                case ReportType.Financial:
                    TotalRecords = _allFinancialTransactions.Count;
                    TotalPages = (int)Math.Ceiling((double)TotalRecords / PageSize);
                    if (TotalPages == 0) TotalPages = 1;
                    
                    PaginatedFinancialTransactions.Clear();
                    var financialPageData = _allFinancialTransactions
                        .Skip((CurrentPage - 1) * PageSize)
                        .Take(PageSize);
                    foreach (var item in financialPageData)
                        PaginatedFinancialTransactions.Add(item);
                    break;

                case ReportType.Wages:
                    TotalRecords = _allWageTransactions.Count;
                    TotalPages = (int)Math.Ceiling((double)TotalRecords / PageSize);
                    if (TotalPages == 0) TotalPages = 1;
                    
                    PaginatedWageTransactions.Clear();
                    var wagePageData = _allWageTransactions
                        .Skip((CurrentPage - 1) * PageSize)
                        .Take(PageSize);
                    foreach (var item in wagePageData)
                        PaginatedWageTransactions.Add(item);
                    break;
            }

            OnPropertyChanged(nameof(CurrentTransactions));
            OnPropertyChanged(nameof(CanGoToPreviousPage));
            OnPropertyChanged(nameof(CanGoToNextPage));
            GoToPreviousPageCommand.NotifyCanExecuteChanged();
            GoToNextPageCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand(CanExecute = nameof(CanGoToPreviousPage))]
        private void GoToPreviousPage()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
            }
        }

        [RelayCommand(CanExecute = nameof(CanGoToNextPage))]
        private void GoToNextPage()
        {
            if (CurrentPage < TotalPages)
            {
            CurrentPage++;
            }
        }

        [RelayCommand]
        private void GoToFirstPage()
        {
            CurrentPage = 1;
        }

        [RelayCommand]
        private void GoToLastPage()
        {
            CurrentPage = TotalPages;
        }

        public async Task InitializeAsync()
        {
            await LoadDataAsync();
        }
    }
}

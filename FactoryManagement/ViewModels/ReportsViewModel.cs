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
    // Unified view model for displaying all transaction types in a single grid
    public class UnifiedTransactionViewModel
    {
        public string Category { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty; // Item name, Party name, or Worker name
        public string? ItemName { get; set; }
        public string? PartyName { get; set; }
        public string? WorkerName { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? Rate { get; set; }
        public decimal Amount { get; set; }
        public string? AdditionalInfo { get; set; } // For extra details like days worked, interest, etc.
        public string? Notes { get; set; }
    }

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
                        ReportType.All => AllTransactions,
                        ReportType.Inventory => Transactions,
                        ReportType.Financial => FinancialTransactions,
                        ReportType.Wages => WageTransactions,
                        _ => Transactions
                    };
                }
            }
        private readonly ITransactionService _transactionService;
        private readonly IItemService _itemService;
        private readonly IPartyService _partyService;
        private readonly IExportService _exportService;
        private readonly FinancialTransactionService _financialService;
        private readonly IWageService _wageService;

        [ObservableProperty]
        private ObservableCollection<Transaction> _transactions = new();

        [ObservableProperty]
        private ObservableCollection<FinancialTransaction> _financialTransactions = new();

        [ObservableProperty]
        private ObservableCollection<WageTransaction> _wageTransactions = new();

        [ObservableProperty]
        private ObservableCollection<UnifiedTransactionViewModel> _allTransactions = new();

        [ObservableProperty]
        private ObservableCollection<Item> _items = new();

        [ObservableProperty]
        private ObservableCollection<Party> _parties = new();

        [ObservableProperty]
        private ObservableCollection<Worker> _workers = new();

        [ObservableProperty]
        private Item? _selectedItem;

        [ObservableProperty]
        private Party? _selectedParty;

        [ObservableProperty]
        private Worker? _selectedWorker;

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
            FinancialTransactionService financialService,
            IWageService wageService)
        {
            _transactionService = transactionService;
            _itemService = itemService;
            _partyService = partyService;
            _exportService = exportService;
            _financialService = financialService;
            _wageService = wageService;
        }

        partial void OnSelectedReportTypeChanged(ReportType value)
        {
            // Clear filters when changing report type
            SelectedItem = null;
            SelectedParty = null;
            SelectedWorker = null;
            
            _ = LoadReportDataAsync();
            OnPropertyChanged(nameof(CurrentTransactions));
            OnPropertyChanged(nameof(IsAllView));
            OnPropertyChanged(nameof(IsInventoryView));
            OnPropertyChanged(nameof(IsFinancialView));
            OnPropertyChanged(nameof(IsWagesView));
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

        partial void OnStartDateChanged(DateTime value)
        {
            if (SelectedReportType != ReportType.All)
            {
                _ = ApplyFiltersAsync();
            }
        }

        partial void OnEndDateChanged(DateTime value)
        {
            if (SelectedReportType != ReportType.All)
            {
                _ = ApplyFiltersAsync();
            }
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

                var workers = await _wageService.GetAllWorkersAsync();
                Workers.Clear();
                foreach (var worker in workers)
                    Workers.Add(worker);

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
                
                // Load all transaction types
                var inventoryTransactions = await _transactionService.GetAllTransactionsAsync();
                var financialTransactions = await _financialService.GetAllFinancialTransactionsAsync();
                var wageTransactions = await _wageService.GetTransactionsByDateRangeAsync(DateTime.MinValue, DateTime.MaxValue);
                
                // Update individual collections
                Transactions.Clear();
                foreach (var t in inventoryTransactions)
                    Transactions.Add(t);
                    
                FinancialTransactions.Clear();
                foreach (var t in financialTransactions)
                    FinancialTransactions.Add(t);
                    
                WageTransactions.Clear();
                foreach (var t in wageTransactions)
                    WageTransactions.Add(t);
                
                // Combine all transactions into unified view models
                AllTransactions.Clear();
                
                // Add inventory transactions
                foreach (var t in inventoryTransactions)
                {
                    AllTransactions.Add(new UnifiedTransactionViewModel
                    {
                        Category = "Inventory",
                        TransactionId = t.TransactionId.ToString(),
                        TransactionDate = t.TransactionDate,
                        TransactionType = t.TransactionType.ToString(),
                        Description = $"{t.Item?.ItemName ?? "N/A"} - {t.Party?.Name ?? "N/A"}",
                        ItemName = t.Item?.ItemName,
                        PartyName = t.Party?.Name,
                        WorkerName = null,
                        Quantity = t.Quantity,
                        Rate = t.PricePerUnit,
                        Amount = t.TotalAmount,
                        AdditionalInfo = t.Quantity > 0 ? $"{t.Quantity:N2} units @ ₹{t.PricePerUnit:N2}" : null,
                        Notes = t.Notes
                    });
                }
                
                // Add financial transactions
                foreach (var t in financialTransactions)
                {
                    AllTransactions.Add(new UnifiedTransactionViewModel
                    {
                        Category = "Financial",
                        TransactionId = t.FinancialTransactionId.ToString(),
                        TransactionDate = t.TransactionDate,
                        TransactionType = t.TransactionType.ToString(),
                        Description = t.Party?.Name ?? "N/A",
                        ItemName = null,
                        PartyName = t.Party?.Name,
                        WorkerName = null,
                        Quantity = null,
                        Rate = t.InterestRate > 0 ? t.InterestRate : null,
                        Amount = t.Amount,
                        AdditionalInfo = t.InterestRate > 0 ? $"Interest: {t.InterestRate:N2}% (₹{t.InterestAmount:N2})" : null,
                        Notes = t.Notes
                    });
                }
                
                // Add wage transactions
                foreach (var t in wageTransactions)
                {
                    AllTransactions.Add(new UnifiedTransactionViewModel
                    {
                        Category = "Wages",
                        TransactionId = t.WageTransactionId.ToString(),
                        TransactionDate = t.TransactionDate,
                        TransactionType = t.TransactionType.ToString(),
                        Description = t.Worker?.Name ?? "N/A",
                        ItemName = null,
                        PartyName = null,
                        WorkerName = t.Worker?.Name,
                        Quantity = t.DaysWorked > 0 ? t.DaysWorked : (t.HoursWorked > 0 ? t.HoursWorked : null),
                        Rate = t.Rate,
                        Amount = t.NetAmount,
                        AdditionalInfo = t.DaysWorked > 0 ? $"{t.DaysWorked:N1} days @ ₹{t.Rate:N2}" : 
                                        (t.HoursWorked > 0 ? $"{t.HoursWorked:N1} hrs @ ₹{t.Rate:N2}" : null),
                        Notes = t.Notes
                    });
                }
                
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

        private async Task ApplyInventoryFiltersAsync()
        {
            IEnumerable<Transaction> transactions = await _transactionService.GetAllTransactionsAsync();

            // Apply item filter
            if (SelectedItem != null)
            {
                transactions = transactions.Where(t => t.ItemId == SelectedItem.ItemId);
            }

            // Apply party filter
            if (SelectedParty != null)
            {
                transactions = transactions.Where(t => t.PartyId == SelectedParty.PartyId);
            }

            // Apply date range filter
            transactions = transactions.Where(t => t.TransactionDate >= StartDate && t.TransactionDate <= EndDate);

            Transactions.Clear();
            foreach (var t in transactions)
                Transactions.Add(t);

            UpdateReportTitle();
        }

        private async Task ApplyFinancialFiltersAsync()
        {
            IEnumerable<FinancialTransaction> transactions = await _financialService.GetAllFinancialTransactionsAsync();

            // Apply party filter
            if (SelectedParty != null)
            {
                transactions = transactions.Where(t => t.PartyId == SelectedParty.PartyId);
            }

            // Apply date range filter
            transactions = transactions.Where(t => t.TransactionDate >= StartDate && t.TransactionDate <= EndDate);

            FinancialTransactions.Clear();
            foreach (var t in transactions)
                FinancialTransactions.Add(t);

            UpdateReportTitle();
        }

        private async Task ApplyWageFiltersAsync()
        {
            var transactions = await _wageService.GetTransactionsByDateRangeAsync(StartDate, EndDate);

            // Apply worker filter
            if (SelectedWorker != null)
            {
                transactions = transactions.Where(t => t.WorkerId == SelectedWorker.WorkerId);
            }

            WageTransactions.Clear();
            foreach (var t in transactions)
                WageTransactions.Add(t);

            UpdateReportTitle();
        }

        private void UpdateReportTitle()
        {
            var filters = new List<string>();

            if (SelectedItem != null)
                filters.Add($"Item: {SelectedItem.ItemName}");
            if (SelectedParty != null)
                filters.Add($"Party: {SelectedParty.Name}");
            if (SelectedWorker != null)
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
            SelectedItem = null;
            SelectedParty = null;
            SelectedWorker = null;
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
                Transactions.Clear();
                foreach (var t in transList)
                    Transactions.Add(t);
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
                FinancialTransactions.Clear();
                foreach (var t in transList)
                    FinancialTransactions.Add(t);
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
                WageTransactions.Clear();
                foreach (var t in transList)
                    WageTransactions.Add(t);
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
                Transactions.Clear();
                foreach (var t in transList)
                    Transactions.Add(t);
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
                Transactions.Clear();
                foreach (var t in transList)
                    Transactions.Add(t);
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
                Transactions.Clear();
                foreach (var t in transList)
                    Transactions.Add(t);
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
                var dialog = new SaveFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx",
                    DefaultExt = "xlsx",
                    FileName = $"{SelectedReportType}Transactions_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };

                if (dialog.ShowDialog() == true)
                {
                    IsBusy = true;
                    object exportData;
                    
                    if (SelectedReportType == ReportType.All)
                    {
                        // Export combined data with category column
                        var inventoryData = Transactions.Select(t => new
                        {
                            Category = "Inventory",
                            TransactionId = t.TransactionId.ToString(),
                            ItemName = t.Item?.ItemName ?? "",
                            PartyName = t.Party?.Name ?? "",
                            WorkerName = "",
                            TransactionType = t.TransactionType.ToString(),
                            Quantity = (decimal?)t.Quantity,
                            Rate = (decimal?)t.PricePerUnit,
                            Amount = t.TotalAmount,
                            t.TransactionDate,
                            t.Notes
                        });
                        
                        var financialData = FinancialTransactions.Select(t => new
                        {
                            Category = "Financial",
                            TransactionId = t.FinancialTransactionId.ToString(),
                            ItemName = "",
                            PartyName = t.Party?.Name ?? "",
                            WorkerName = "",
                            TransactionType = t.TransactionType.ToString(),
                            Quantity = (decimal?)null,
                            Rate = (decimal?)null,
                            Amount = t.Amount,
                            t.TransactionDate,
                            t.Notes
                        });
                        
                        var wageData = WageTransactions.Select(t => new
                        {
                            Category = "Wages",
                            TransactionId = t.WageTransactionId.ToString(),
                            ItemName = "",
                            PartyName = "",
                            WorkerName = t.Worker?.Name ?? "",
                            TransactionType = t.TransactionType.ToString(),
                            Quantity = (decimal?)null,
                            Rate = (decimal?)t.Rate,
                            Amount = t.NetAmount,
                            t.TransactionDate,
                            t.Notes
                        });
                        
                        exportData = inventoryData.Concat(financialData).Concat(wageData)
                            .OrderByDescending(t => t.TransactionDate).ToList();
                    }
                    else if (SelectedReportType == ReportType.Inventory)
                    {
                        exportData = Transactions.Select(t => new
                        {
                            t.TransactionId,
                            ItemName = t.Item?.ItemName,
                            PartyName = t.Party?.Name,
                            TransactionType = t.TransactionType.ToString(),
                            t.Quantity,
                            t.PricePerUnit,
                            t.TotalAmount,
                            t.TransactionDate,
                            EnteredBy = t.User?.Username,
                            t.Notes
                        }).ToList();
                    }
                    else if (SelectedReportType == ReportType.Financial)
                    {
                        exportData = FinancialTransactions.Select(t => new
                        {
                            t.FinancialTransactionId,
                            PartyName = t.Party?.Name,
                            TransactionType = t.TransactionType.ToString(),
                            t.Amount,
                            t.InterestRate,
                            t.InterestAmount,
                            t.TransactionDate,
                            t.DueDate,
                            t.Notes
                        }).ToList();
                    }
                    else // Wages
                    {
                        exportData = WageTransactions.Select(t => new
                        {
                            t.WageTransactionId,
                            WorkerName = t.Worker?.Name,
                            TransactionType = t.TransactionType.ToString(),
                            t.DaysWorked,
                            t.HoursWorked,
                            t.Rate,
                            t.Amount,
                            t.OvertimeAmount,
                            t.AdvanceAdjusted,
                            t.Deductions,
                            t.NetAmount,
                            t.TransactionDate,
                            t.Notes
                        }).ToList();
                    }

                    await _exportService.ExportToExcelAsync((dynamic)exportData, dialog.FileName, $"{SelectedReportType}Transactions");
                    ErrorMessage = "Export to Excel completed successfully!";
                }
            }
            catch (Exception ex)
            {
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
                var dialog = new SaveFileDialog
                {
                    Filter = "CSV Files (*.csv)|*.csv",
                    DefaultExt = "csv",
                    FileName = $"{SelectedReportType}Transactions_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (dialog.ShowDialog() == true)
                {
                    IsBusy = true;
                    object exportData;
                    
                    if (SelectedReportType == ReportType.All)
                    {
                        // Export combined data with category column
                        var inventoryData = Transactions.Select(t => new
                        {
                            Category = "Inventory",
                            TransactionId = t.TransactionId.ToString(),
                            ItemName = t.Item?.ItemName ?? "",
                            PartyName = t.Party?.Name ?? "",
                            WorkerName = "",
                            TransactionType = t.TransactionType.ToString(),
                            Quantity = (decimal?)t.Quantity,
                            Rate = (decimal?)t.PricePerUnit,
                            Amount = t.TotalAmount,
                            t.TransactionDate,
                            t.Notes
                        });
                        
                        var financialData = FinancialTransactions.Select(t => new
                        {
                            Category = "Financial",
                            TransactionId = t.FinancialTransactionId.ToString(),
                            ItemName = "",
                            PartyName = t.Party?.Name ?? "",
                            WorkerName = "",
                            TransactionType = t.TransactionType.ToString(),
                            Quantity = (decimal?)null,
                            Rate = (decimal?)null,
                            Amount = t.Amount,
                            t.TransactionDate,
                            t.Notes
                        });
                        
                        var wageData = WageTransactions.Select(t => new
                        {
                            Category = "Wages",
                            TransactionId = t.WageTransactionId.ToString(),
                            ItemName = "",
                            PartyName = "",
                            WorkerName = t.Worker?.Name ?? "",
                            TransactionType = t.TransactionType.ToString(),
                            Quantity = (decimal?)null,
                            Rate = (decimal?)t.Rate,
                            Amount = t.NetAmount,
                            t.TransactionDate,
                            t.Notes
                        });
                        
                        exportData = inventoryData.Concat(financialData).Concat(wageData)
                            .OrderByDescending(t => t.TransactionDate).ToList();
                    }
                    else if (SelectedReportType == ReportType.Inventory)
                    {
                        exportData = Transactions.Select(t => new
                        {
                            t.TransactionId,
                            ItemName = t.Item?.ItemName,
                            PartyName = t.Party?.Name,
                            TransactionType = t.TransactionType.ToString(),
                            t.Quantity,
                            t.PricePerUnit,
                            t.TotalAmount,
                            t.TransactionDate,
                            EnteredBy = t.User?.Username,
                            t.Notes
                        }).ToList();
                    }
                    else if (SelectedReportType == ReportType.Financial)
                    {
                        exportData = FinancialTransactions.Select(t => new
                        {
                            t.FinancialTransactionId,
                            PartyName = t.Party?.Name,
                            TransactionType = t.TransactionType.ToString(),
                            t.Amount,
                            t.InterestRate,
                            t.InterestAmount,
                            t.TransactionDate,
                            t.DueDate,
                            t.Notes
                        }).ToList();
                    }
                    else // Wages
                    {
                        exportData = WageTransactions.Select(t => new
                        {
                            t.WageTransactionId,
                            WorkerName = t.Worker?.Name,
                            TransactionType = t.TransactionType.ToString(),
                            t.DaysWorked,
                            t.HoursWorked,
                            t.Rate,
                            t.Amount,
                            t.OvertimeAmount,
                            t.AdvanceAdjusted,
                            t.Deductions,
                            t.NetAmount,
                            t.TransactionDate,
                            t.Notes
                        }).ToList();
                    }

                    await _exportService.ExportToCsvAsync((dynamic)exportData, dialog.FileName);
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

        private void CalculateTotals()
        {
            TotalInventoryAmount = Transactions.Sum(t => t.TotalAmount);
            TotalFinancialAmount = FinancialTransactions.Sum(t => t.Amount);
            TotalWagesAmount = WageTransactions.Sum(t => t.NetAmount);
            
            TotalAmount = SelectedReportType switch
            {
                ReportType.All => TotalInventoryAmount + TotalFinancialAmount + TotalWagesAmount,
                ReportType.Inventory => TotalInventoryAmount,
                ReportType.Financial => TotalFinancialAmount,
                ReportType.Wages => TotalWagesAmount,
                _ => 0
            };

            TransactionCount = SelectedReportType switch
            {
                ReportType.All => Transactions.Count + FinancialTransactions.Count + WageTransactions.Count,
                ReportType.Inventory => Transactions.Count,
                ReportType.Financial => FinancialTransactions.Count,
                ReportType.Wages => WageTransactions.Count,
                _ => 0
            };
        }

        public async Task InitializeAsync()
        {
            await LoadDataAsync();
        }
    }
}

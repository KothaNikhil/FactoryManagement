using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FactoryManagement.Models;
using FactoryManagement.Services;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace FactoryManagement.ViewModels
{
    public enum ReportType
    {
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
        private ReportType _selectedReportType = ReportType.Inventory;

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
            _ = LoadReportDataAsync();
            OnPropertyChanged(nameof(CurrentTransactions));
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
                    
                    if (SelectedReportType == ReportType.Inventory)
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
                    
                    if (SelectedReportType == ReportType.Inventory)
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
                ReportType.Inventory => TotalInventoryAmount,
                ReportType.Financial => TotalFinancialAmount,
                ReportType.Wages => TotalWagesAmount,
                _ => 0
            };

            TransactionCount = SelectedReportType switch
            {
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

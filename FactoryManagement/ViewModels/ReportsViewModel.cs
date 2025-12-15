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
    public partial class ReportsViewModel : ViewModelBase
    {
        private readonly ITransactionService _transactionService;
        private readonly IItemService _itemService;
        private readonly IPartyService _partyService;
        private readonly IExportService _exportService;

        [ObservableProperty]
        private ObservableCollection<Transaction> _transactions = new();

        [ObservableProperty]
        private ObservableCollection<Item> _items = new();

        [ObservableProperty]
        private ObservableCollection<Party> _parties = new();

        [ObservableProperty]
        private Item? _selectedItem;

        [ObservableProperty]
        private Party? _selectedParty;

        [ObservableProperty]
        private DateTime _startDate = DateTime.Now.AddMonths(-1);

        [ObservableProperty]
        private DateTime _endDate = DateTime.Now;

        [ObservableProperty]
        private decimal _totalAmount;

        [ObservableProperty]
        private string _reportTitle = "All Transactions";

        public ReportsViewModel(
            ITransactionService transactionService,
            IItemService itemService,
            IPartyService partyService,
            IExportService exportService)
        {
            _transactionService = transactionService;
            _itemService = itemService;
            _partyService = partyService;
            _exportService = exportService;
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

                await LoadAllTransactionsAsync();
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
                ReportTitle = "All Transactions";
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
                    FileName = $"Transactions_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };

                if (dialog.ShowDialog() == true)
                {
                    IsBusy = true;
                    var exportData = Transactions.Select(t => new
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

                    await _exportService.ExportToExcelAsync(exportData, dialog.FileName, "Transactions");
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
                    FileName = $"Transactions_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (dialog.ShowDialog() == true)
                {
                    IsBusy = true;
                    var exportData = Transactions.Select(t => new
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

                    await _exportService.ExportToCsvAsync(exportData, dialog.FileName);
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
            TotalAmount = Transactions.Sum(t => t.TotalAmount);
        }

        public async Task InitializeAsync()
        {
            await LoadDataAsync();
        }
    }
}

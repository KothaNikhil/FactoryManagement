using FactoryManagement.Models;
using FactoryManagement.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FactoryManagement.ViewModels
{
    public class FinancialTransactionsViewModel : ViewModelBase
    {
        private readonly FinancialTransactionService _financialTransactionService;
        private readonly IPartyService _partyService;

        private ObservableCollection<LoanAccount> _loans;
        private ObservableCollection<FinancialTransaction> _transactions;
        private ObservableCollection<Party> _parties;
        private LoanAccount? _selectedLoan;
        private Party? _selectedParty;
        private LoanType _selectedLoanType;
        private decimal _loanAmount;
        private decimal _interestRate;
        private DateTime _startDate;
        private DateTime? _dueDate;
        private string _notes;
        private decimal _paymentAmount;
        private string _paymentNotes;
        private decimal _totalLoansGiven;
        private decimal _totalLoansTaken;
        private decimal _totalInterestReceivable;
        private decimal _totalInterestPayable;
        private string _filterStatus;
        private bool _isLoading;

        public FinancialTransactionsViewModel(
            FinancialTransactionService financialTransactionService,
            IPartyService partyService)
        {
            _financialTransactionService = financialTransactionService;
            _partyService = partyService;

            _loans = new ObservableCollection<LoanAccount>();
            _transactions = new ObservableCollection<FinancialTransaction>();
            _parties = new ObservableCollection<Party>();
            _notes = string.Empty;
            _paymentNotes = string.Empty;
            _filterStatus = "All";
            _startDate = DateTime.Now;

            // Commands
            CreateLoanCommand = new RelayCommand(() => CreateLoanAsync().GetAwaiter().GetResult(), () => CanCreateLoan());
            RecordPaymentCommand = new RelayCommand(() => RecordPaymentAsync().GetAwaiter().GetResult(), () => CanRecordPayment());
            RefreshCommand = new RelayCommand(() => LoadDataAsync().GetAwaiter().GetResult());
            UpdateInterestCommand = new RelayCommand(() => UpdateInterestAsync().GetAwaiter().GetResult(), () => SelectedLoan != null);
            FilterLoansCommand = new RelayCommand(() => FilterLoansAsync().GetAwaiter().GetResult());
            LoadedCommand = new RelayCommand(() => OnLoadedAsync().GetAwaiter().GetResult());
        }

        public ICommand LoadedCommand { get; }

        private async Task OnLoadedAsync()
        {
            await LoadDataAsync();
        }

        #region Properties

        public ObservableCollection<LoanAccount> Loans
        {
            get => _loans;
            set
            {
                _loans = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<FinancialTransaction> Transactions
        {
            get => _transactions;
            set
            {
                _transactions = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Party> Parties
        {
            get => _parties;
            set
            {
                _parties = value;
                OnPropertyChanged();
            }
        }

        public LoanAccount? SelectedLoan
        {
            get => _selectedLoan;
            set
            {
                _selectedLoan = value;
                OnPropertyChanged();
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
                Task.Run(async () => await LoadTransactionsForSelectedLoanAsync());
            }
        }

        public Party? SelectedParty
        {
            get => _selectedParty;
            set
            {
                _selectedParty = value;
                System.Diagnostics.Debug.WriteLine($"SelectedParty changed to: {value?.Name ?? "null"}");
                OnPropertyChanged();
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }

        public LoanType SelectedLoanType
        {
            get => _selectedLoanType;
            set
            {
                _selectedLoanType = value;
                OnPropertyChanged();
            }
        }

        public decimal LoanAmount
        {
            get => _loanAmount;
            set
            {
                _loanAmount = value;
                System.Diagnostics.Debug.WriteLine($"LoanAmount changed to: {value}");
                OnPropertyChanged();
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }

        public decimal InterestRate
        {
            get => _interestRate;
            set
            {
                _interestRate = value;
                OnPropertyChanged();
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }

        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                _startDate = value;
                OnPropertyChanged();
            }
        }

        public DateTime? DueDate
        {
            get => _dueDate;
            set
            {
                _dueDate = value;
                OnPropertyChanged();
            }
        }

        public string Notes
        {
            get => _notes;
            set
            {
                _notes = value;
                OnPropertyChanged();
            }
        }

        public decimal PaymentAmount
        {
            get => _paymentAmount;
            set
            {
                _paymentAmount = value;
                OnPropertyChanged();
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }

        public string PaymentNotes
        {
            get => _paymentNotes;
            set
            {
                _paymentNotes = value;
                OnPropertyChanged();
            }
        }

        public decimal TotalLoansGiven
        {
            get => _totalLoansGiven;
            set
            {
                _totalLoansGiven = value;
                OnPropertyChanged();
            }
        }

        public decimal TotalLoansTaken
        {
            get => _totalLoansTaken;
            set
            {
                _totalLoansTaken = value;
                OnPropertyChanged();
            }
        }

        public decimal TotalInterestReceivable
        {
            get => _totalInterestReceivable;
            set
            {
                _totalInterestReceivable = value;
                OnPropertyChanged();
            }
        }

        public decimal TotalInterestPayable
        {
            get => _totalInterestPayable;
            set
            {
                _totalInterestPayable = value;
                OnPropertyChanged();
            }
        }

        public string FilterStatus
        {
            get => _filterStatus;
            set
            {
                _filterStatus = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public Array LoanTypes => Enum.GetValues(typeof(LoanType));
        public Array LoanStatuses => Enum.GetValues(typeof(LoanStatus));

        #endregion

        #region Commands

        public ICommand CreateLoanCommand { get; }
        public ICommand RecordPaymentCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand UpdateInterestCommand { get; }
        public ICommand FilterLoansCommand { get; }

        #endregion

        #region Methods

        private async Task LoadDataAsync()
        {
            IsLoading = true;
            try
            {
                var loans = await _financialTransactionService.GetAllLoansAsync();
                var parties = await _partyService.GetAllPartiesAsync();
                var summary = await _financialTransactionService.GetFinancialSummaryAsync();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Loans.Clear();
                    foreach (var loan in loans.OrderByDescending(l => l.CreatedDate))
                    {
                        Loans.Add(loan);
                    }

                    Parties.Clear();
                    foreach (var party in parties.OrderBy(p => p.Name))
                    {
                        Parties.Add(party);
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"Loaded {Parties.Count} parties");

                    TotalLoansGiven = summary["TotalLoansGiven"];
                    TotalLoansTaken = summary["TotalLoansTaken"];
                    TotalInterestReceivable = summary["TotalInterestReceivable"];
                    TotalInterestPayable = summary["TotalInterestPayable"];
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadTransactionsForSelectedLoanAsync()
        {
            if (SelectedLoan == null)
            {
                Application.Current.Dispatcher.Invoke(() => Transactions.Clear());
                return;
            }

            try
            {
                var transactions = await _financialTransactionService.GetTransactionsByLoanAsync(SelectedLoan.LoanAccountId);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Transactions.Clear();
                    foreach (var transaction in transactions.OrderByDescending(t => t.TransactionDate))
                    {
                        Transactions.Add(transaction);
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading transactions: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanCreateLoan()
        {
            var canCreate = SelectedParty != null && LoanAmount > 0 && InterestRate >= 0;
            System.Diagnostics.Debug.WriteLine($"CanCreateLoan: {canCreate} (Party: {SelectedParty != null}, Amount: {LoanAmount}, Rate: {InterestRate})");
            return canCreate;
        }

        private async Task CreateLoanAsync()
        {
            System.Diagnostics.Debug.WriteLine("CreateLoanAsync called!");
            
            if (!CanCreateLoan())
                return;

            try
            {
                var loan = new LoanAccount
                {
                    PartyId = SelectedParty!.PartyId,
                    LoanType = SelectedLoanType,
                    OriginalAmount = LoanAmount,
                    InterestRate = InterestRate,
                    StartDate = StartDate,
                    DueDate = DueDate,
                    Notes = Notes,
                    CreatedBy = 1 // TODO: Get from current user
                };

                await _financialTransactionService.CreateLoanAsync(loan);
                
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("Loan created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                });

                // Clear form
                SelectedParty = null;
                LoanAmount = 0;
                InterestRate = 0;
                StartDate = DateTime.Now;
                DueDate = null;
                Notes = string.Empty;

                await LoadDataAsync();
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating loan: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanRecordPayment()
        {
            return SelectedLoan != null && PaymentAmount > 0;
        }

        private async Task RecordPaymentAsync()
        {
            if (!CanRecordPayment())
                return;

            try
            {
                await _financialTransactionService.RecordPaymentAsync(
                    SelectedLoan!.LoanAccountId,
                    PaymentAmount,
                    1, // TODO: Get from current user
                    PaymentNotes
                );

                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("Payment recorded successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                });

                PaymentAmount = 0;
                PaymentNotes = string.Empty;

                await LoadDataAsync();
                await LoadTransactionsForSelectedLoanAsync();
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error recording payment: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task UpdateInterestAsync()
        {
            if (SelectedLoan == null)
            {
                MessageBox.Show("Please select a loan first.", "No Loan Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"Updating interest for loan {SelectedLoan.LoanAccountId}");
                
                await _financialTransactionService.UpdateLoanInterestAsync(SelectedLoan.LoanAccountId);
                
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Interest updated successfully!\n\nInterest has been calculated and added to the loan balance.", 
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                });

                await LoadDataAsync();
                await LoadTransactionsForSelectedLoanAsync();
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
            catch (InvalidOperationException ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"{ex.Message}", "Cannot Update Interest", MessageBoxButton.OK, MessageBoxImage.Information);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating interest: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task FilterLoansAsync()
        {
            IsLoading = true;
            try
            {
                IEnumerable<LoanAccount> filteredLoans;

                if (FilterStatus == "All")
                {
                    filteredLoans = await _financialTransactionService.GetAllLoansAsync();
                }
                else if (Enum.TryParse<LoanStatus>(FilterStatus, out var status))
                {
                    var allLoans = await _financialTransactionService.GetAllLoansAsync();
                    filteredLoans = allLoans.Where(l => l.Status == status);
                }
                else
                {
                    filteredLoans = await _financialTransactionService.GetAllLoansAsync();
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Loans.Clear();
                    foreach (var loan in filteredLoans.OrderByDescending(l => l.CreatedDate))
                    {
                        Loans.Add(loan);
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error filtering loans: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion
    }
}

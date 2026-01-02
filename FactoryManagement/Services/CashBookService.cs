using FactoryManagement.Data.Repositories;
using FactoryManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FactoryManagement.Services
{
    /// <summary>
    /// Service interface for cash book operations
    /// Business logic layer for cash tracking, reconciliation, and reporting
    /// </summary>
    public interface ICashBookService
    {
        // Core CRUD operations
        Task<CashBalance?> GetByDateAsync(DateTime date);
        Task<IEnumerable<CashBalance>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<CashBalance>> GetAllAsync();
        Task<CashBalance?> GetLatestAsync();
        Task<CashBalance> CreateAsync(CashBalance cashBalance);
        Task<bool> UpdateAsync(CashBalance cashBalance);
        Task<bool> DeleteAsync(int id);

        // Business operations
        /// <summary>
        /// Initialize cash book with opening balance (first time setup)
        /// </summary>
        Task<CashBalance> SetOpeningBalanceAsync(DateTime date, decimal openingBalance, int userId, string notes = "");

        /// <summary>
        /// Create or update daily cash balance record with calculated values from transactions
        /// </summary>
        Task<CashBalance> CreateOrUpdateDailyCashRecordAsync(DateTime date);

        /// <summary>
        /// Perform end-of-day reconciliation
        /// </summary>
        Task<CashBalance> ReconcileCashAsync(DateTime date, decimal actualCashCounted, int userId, string? discrepancyReason = null, string? notes = null);

        /// <summary>
        /// Get calculated cash flow for a specific date
        /// </summary>
        Task<CashFlowSummary> GetCashFlowForDateAsync(DateTime date);

        /// <summary>
        /// Get cash flow summary for date range
        /// </summary>
        Task<CashFlowSummary> GetCashFlowSummaryAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get current cash in hand
        /// </summary>
        Task<decimal> GetCurrentCashInHandAsync();

        /// <summary>
        /// Get unreconciled days
        /// </summary>
        Task<IEnumerable<CashBalance>> GetUnreconciledDaysAsync();

        /// <summary>
        /// Get days with discrepancies
        /// </summary>
        Task<IEnumerable<CashBalance>> GetDaysWithDiscrepanciesAsync();

        /// <summary>
        /// Get total discrepancy for a period
        /// </summary>
        Task<decimal> GetTotalDiscrepancyAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Check if cash book has been initialized (has any records)
        /// </summary>
        Task<bool> IsInitializedAsync();

        /// <summary>
        /// Get expected closing balance for today based on transactions
        /// </summary>
        Task<decimal> GetExpectedClosingBalanceAsync(DateTime date);
    }

    /// <summary>
    /// Service implementation for cash book operations
    /// Coordinates between repository and transaction services for comprehensive cash management
    /// </summary>
    public class CashBookService : ICashBookService
    {
        private readonly ICashBalanceRepository _cashBalanceRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IFinancialTransactionRepository _financialTransactionRepository;
        private readonly IWageTransactionRepository _wageTransactionRepository;
        private readonly IOperationalExpenseRepository _operationalExpenseRepository;

        public CashBookService(
            ICashBalanceRepository cashBalanceRepository,
            ITransactionRepository transactionRepository,
            IFinancialTransactionRepository financialTransactionRepository,
            IWageTransactionRepository wageTransactionRepository,
            IOperationalExpenseRepository operationalExpenseRepository)
        {
            _cashBalanceRepository = cashBalanceRepository ?? throw new ArgumentNullException(nameof(cashBalanceRepository));
            _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
            _financialTransactionRepository = financialTransactionRepository ?? throw new ArgumentNullException(nameof(financialTransactionRepository));
            _wageTransactionRepository = wageTransactionRepository ?? throw new ArgumentNullException(nameof(wageTransactionRepository));
            _operationalExpenseRepository = operationalExpenseRepository ?? throw new ArgumentNullException(nameof(operationalExpenseRepository));
        }

        public Task<CashBalance?> GetByDateAsync(DateTime date)
            => _cashBalanceRepository.GetByDateAsync(date);

        public Task<IEnumerable<CashBalance>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
            => _cashBalanceRepository.GetByDateRangeAsync(startDate, endDate);

        public Task<IEnumerable<CashBalance>> GetAllAsync()
            => _cashBalanceRepository.GetAllAsync();

        public Task<CashBalance?> GetLatestAsync()
            => _cashBalanceRepository.GetLatestAsync();

        public Task<CashBalance> CreateAsync(CashBalance cashBalance)
            => _cashBalanceRepository.CreateAsync(cashBalance);

        public Task<bool> UpdateAsync(CashBalance cashBalance)
            => _cashBalanceRepository.UpdateAsync(cashBalance);

        public Task<bool> DeleteAsync(int id)
            => _cashBalanceRepository.DeleteAsync(id);

        public async Task<CashBalance> SetOpeningBalanceAsync(DateTime date, decimal openingBalance, int userId, string notes = "")
        {
            // Check if already initialized for this date
            var existing = await _cashBalanceRepository.GetByDateAsync(date);
            if (existing != null)
                throw new InvalidOperationException($"Cash balance already exists for {date:yyyy-MM-dd}. Use update instead.");

            var cashBalance = new CashBalance
            {
                Date = date.Date,
                OpeningBalance = openingBalance,
                ExpectedClosingBalance = openingBalance, // No transactions yet
                TotalCashIn = 0,
                TotalCashOut = 0,
                Notes = string.IsNullOrEmpty(notes) ? "Opening balance entry" : notes,
                CreatedDate = DateTime.Now
            };

            return await _cashBalanceRepository.CreateAsync(cashBalance);
        }

        public async Task<CashBalance> CreateOrUpdateDailyCashRecordAsync(DateTime date)
        {
            var dateOnly = date.Date;

            // Get or create record
            var existing = await _cashBalanceRepository.GetByDateAsync(dateOnly);

            // Get opening balance (from previous day's closing or zero)
            decimal openingBalance = 0;
            var previousDay = await _cashBalanceRepository.GetByDateAsync(dateOnly.AddDays(-1));
            if (previousDay != null)
            {
                openingBalance = previousDay.IsReconciled && previousDay.ActualCashCounted.HasValue
                    ? previousDay.ActualCashCounted.Value
                    : previousDay.ExpectedClosingBalance;
            }
            else
            {
                // No previous day record - check for latest record before this date
                var latest = await _cashBalanceRepository.GetLatestAsync();
                if (latest != null && latest.Date < dateOnly)
                {
                    openingBalance = latest.IsReconciled && latest.ActualCashCounted.HasValue
                        ? latest.ActualCashCounted.Value
                        : latest.ExpectedClosingBalance;
                }
                else if (existing != null)
                {
                    // This is an existing record being refreshed, but no previous records exist
                    // Preserve the existing opening balance (don't reset to 0)
                    openingBalance = existing.OpeningBalance;
                }
            }

            // Calculate cash flow from all cash transactions for this date
            var cashFlow = await CalculateCashFlowForDateAsync(dateOnly);

            if (existing == null)
            {
                // Create new record
                var newRecord = new CashBalance
                {
                    Date = dateOnly,
                    OpeningBalance = openingBalance,
                    TotalCashIn = cashFlow.TotalCashIn,
                    TotalCashOut = cashFlow.TotalCashOut,
                    ExpectedClosingBalance = openingBalance + cashFlow.TotalCashIn - cashFlow.TotalCashOut,
                    CreatedDate = DateTime.Now
                };

                return await _cashBalanceRepository.CreateAsync(newRecord);
            }
            else
            {
                // Update existing (recalculate without changing reconciliation status)
                existing.OpeningBalance = openingBalance;
                existing.TotalCashIn = cashFlow.TotalCashIn;
                existing.TotalCashOut = cashFlow.TotalCashOut;
                existing.ExpectedClosingBalance = openingBalance + cashFlow.TotalCashIn - cashFlow.TotalCashOut;
                existing.ModifiedDate = DateTime.Now;

                // Recalculate discrepancy if already reconciled
                if (existing.IsReconciled && existing.ActualCashCounted.HasValue)
                {
                    existing.Discrepancy = existing.ActualCashCounted.Value - existing.ExpectedClosingBalance;
                }

                await _cashBalanceRepository.UpdateAsync(existing);
                return existing;
            }
        }

        public async Task<CashBalance> ReconcileCashAsync(DateTime date, decimal actualCashCounted, int userId, string? discrepancyReason = null, string? notes = null)
        {
            var dateOnly = date.Date;

            // Ensure daily record exists
            var record = await _cashBalanceRepository.GetByDateAsync(dateOnly);
            if (record == null)
            {
                record = await CreateOrUpdateDailyCashRecordAsync(dateOnly);
            }

            // Perform reconciliation
            record.ActualCashCounted = actualCashCounted;
            record.Discrepancy = actualCashCounted - record.ExpectedClosingBalance;
            record.IsReconciled = true;
            record.ReconciledBy = userId;
            record.ReconciledDate = DateTime.Now;
            record.DiscrepancyReason = discrepancyReason;
            if (!string.IsNullOrEmpty(notes))
            {
                record.Notes = notes;
            }
            record.ModifiedDate = DateTime.Now;

            await _cashBalanceRepository.UpdateAsync(record);
            return record;
        }

        public async Task<CashFlowSummary> GetCashFlowForDateAsync(DateTime date)
        {
            var dateOnly = date.Date;
            var record = await _cashBalanceRepository.GetByDateAsync(dateOnly);

            if (record != null)
            {
                return new CashFlowSummary
                {
                    OpeningBalance = record.OpeningBalance,
                    TotalCashIn = record.TotalCashIn,
                    TotalCashOut = record.TotalCashOut,
                    ExpectedClosingBalance = record.ExpectedClosingBalance
                };
            }

            // Calculate on the fly
            var flow = await CalculateCashFlowForDateAsync(dateOnly);
            
            // Get opening balance
            var previousDay = await _cashBalanceRepository.GetByDateAsync(dateOnly.AddDays(-1));
            flow.OpeningBalance = previousDay?.ExpectedClosingBalance ?? 0;
            flow.ExpectedClosingBalance = flow.OpeningBalance + flow.TotalCashIn - flow.TotalCashOut;

            return flow;
        }

        public async Task<CashFlowSummary> GetCashFlowSummaryAsync(DateTime startDate, DateTime endDate)
        {
            var records = await _cashBalanceRepository.GetByDateRangeAsync(startDate, endDate);
            var recordsList = records.ToList();

            if (recordsList.Any())
            {
                var earliest = recordsList.OrderBy(r => r.Date).First();
                var latest = recordsList.OrderByDescending(r => r.Date).First();

                return new CashFlowSummary
                {
                    OpeningBalance = earliest.OpeningBalance,
                    TotalCashIn = recordsList.Sum(r => r.TotalCashIn),
                    TotalCashOut = recordsList.Sum(r => r.TotalCashOut),
                    ExpectedClosingBalance = latest.ExpectedClosingBalance
                };
            }

            return new CashFlowSummary();
        }

        public Task<decimal> GetCurrentCashInHandAsync()
            => _cashBalanceRepository.GetCurrentCashInHandAsync();

        public Task<IEnumerable<CashBalance>> GetUnreconciledDaysAsync()
            => _cashBalanceRepository.GetUnreconciledAsync();

        public Task<IEnumerable<CashBalance>> GetDaysWithDiscrepanciesAsync()
            => _cashBalanceRepository.GetRecordsWithDiscrepanciesAsync();

        public Task<decimal> GetTotalDiscrepancyAsync(DateTime startDate, DateTime endDate)
            => _cashBalanceRepository.GetTotalDiscrepancyAsync(startDate, endDate);

        public async Task<bool> IsInitializedAsync()
        {
            var latest = await _cashBalanceRepository.GetLatestAsync();
            return latest != null;
        }

        public async Task<decimal> GetExpectedClosingBalanceAsync(DateTime date)
        {
            var record = await _cashBalanceRepository.GetByDateAsync(date);
            if (record != null)
                return record.ExpectedClosingBalance;

            // Calculate on the fly
            var flow = await GetCashFlowForDateAsync(date);
            return flow.ExpectedClosingBalance;
        }

        /// <summary>
        /// Calculate cash inflow and outflow from all cash-based transactions for a specific date
        /// </summary>
        private async Task<CashFlowSummary> CalculateCashFlowForDateAsync(DateTime date)
        {
            var dateOnly = date.Date;
            var nextDay = dateOnly.AddDays(1);

            var summary = new CashFlowSummary();

            // 1. Inventory Transactions (Buy/Sell/Wastage) - Only Cash/Bank mode
            var inventoryTransactions = await _transactionRepository.GetTransactionsByDateRangeAsync(dateOnly, nextDay.AddSeconds(-1));
            foreach (var trans in inventoryTransactions.Where(t => t.PaymentMode != PaymentMode.Loan))
            {
                if (trans.PaymentMode == PaymentMode.Cash)
                {
                    if (trans.TransactionType == TransactionType.Sell || trans.TransactionType == TransactionType.Processing)
                    {
                        summary.TotalCashIn += trans.TotalAmount; // Credit
                    }
                    else // Buy or Wastage
                    {
                        summary.TotalCashOut += trans.TotalAmount; // Debit
                    }
                }
            }

            // 2. Financial Transactions - Only Cash/Bank mode
            var financialTransactions = await _financialTransactionRepository.GetByDateRangeAsync(dateOnly, nextDay.AddSeconds(-1));
            foreach (var trans in financialTransactions.Where(t => t.PaymentMode != PaymentMode.Loan))
            {
                if (trans.PaymentMode == PaymentMode.Cash)
                {
                    // Inflows (Credits): LoanRepayment (receiving payment), InterestReceived, LoanTaken (borrowing)
                    if (trans.TransactionType == FinancialTransactionType.LoanRepayment ||
                        trans.TransactionType == FinancialTransactionType.InterestReceived ||
                        trans.TransactionType == FinancialTransactionType.LoanTaken)
                    {
                        summary.TotalCashIn += trans.Amount;
                    }
                    // Outflows (Debits): LoanGiven (lending), LoanPayment (paying back), InterestPaid
                    else if (trans.TransactionType == FinancialTransactionType.LoanGiven ||
                             trans.TransactionType == FinancialTransactionType.LoanPayment ||
                             trans.TransactionType == FinancialTransactionType.InterestPaid)
                    {
                        summary.TotalCashOut += trans.Amount;
                    }
                }
            }

            // 3. Wage Transactions - Only Cash/Bank mode
            var wageTransactions = await _wageTransactionRepository.GetByDateRangeAsync(dateOnly, nextDay.AddSeconds(-1));
            foreach (var trans in wageTransactions.Where(t => t.PaymentMode != PaymentMode.Loan))
            {
                if (trans.PaymentMode == PaymentMode.Cash)
                {
                    // Outflows: Wage payments, Advances given
                    // Inflows: Advance returned (AdvanceAdjustment when negative amount)
                    if (trans.TransactionType == WageTransactionType.AdvanceAdjustment && trans.Amount < 0)
                    {
                        summary.TotalCashIn += Math.Abs(trans.Amount);
                    }
                    else // Wage, AdvanceGiven, or positive AdvanceAdjustment
                    {
                        summary.TotalCashOut += trans.Amount;
                    }
                }
            }

            // 4. Operational Expenses - Only Cash/Bank mode (all are outflows)
            var operationalExpenses = await _operationalExpenseRepository.GetByDateRangeAsync(dateOnly, nextDay.AddSeconds(-1));
            foreach (var expense in operationalExpenses.Where(e => e.PaymentMode != PaymentMode.Loan))
            {
                if (expense.PaymentMode == PaymentMode.Cash)
                {
                    summary.TotalCashOut += expense.Amount;
                }
            }

            return summary;
        }
    }
}

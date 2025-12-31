using FactoryManagement.Data;
using FactoryManagement.Data.Repositories;
using FactoryManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FactoryManagement.Services
{
    public interface IFinancialTransactionService
    {
        Task<IEnumerable<LoanAccount>> GetAllLoansAsync();
        Task<LoanAccount> CreateLoanAsync(LoanAccount loan, PaymentMode paymentMode);
        Task<FinancialTransaction> RecordPaymentAsync(int loanAccountId, decimal paymentAmount, PaymentMode paymentMode, int userId, string notes = "");
        Task UpdateLoanInterestAsync(int loanAccountId);
        Task<IEnumerable<FinancialTransaction>> GetTransactionsByLoanAsync(int loanAccountId);
        Task<IEnumerable<FinancialTransaction>> GetAllFinancialTransactionsAsync();
        Task DeleteLoanAsync(int loanAccountId);
        Task DeleteFinancialTransactionAsync(int transactionId);
        Task<decimal> GetTotalLoansGivenOutstandingAsync();
        Task<decimal> GetTotalLoansTakenOutstandingAsync();
        Task<Dictionary<string, decimal>> GetFinancialSummaryAsync();
        Task<LoanAccount> RestoreLoanAsync(LoanAccount snapshot, IEnumerable<FinancialTransaction> transactions);
        Task RestoreFinancialTransactionAsync(FinancialTransaction tx);
        Task<LoanAccount?> GetLoanWithTransactionsAsync(int loanAccountId);
        Task<IEnumerable<LoanAccount>> GetOverdueLoansAsync();
    }

    public class FinancialTransactionService : IFinancialTransactionService
    {
        private readonly IFinancialTransactionRepository _financialTransactionRepository;
        private readonly ILoanAccountRepository _loanAccountRepository;
        private readonly FactoryDbContext _context;

        public FinancialTransactionService(
            IFinancialTransactionRepository financialTransactionRepository,
            ILoanAccountRepository loanAccountRepository,
            FactoryDbContext context)
        {
            _financialTransactionRepository = financialTransactionRepository;
            _loanAccountRepository = loanAccountRepository;
            _context = context;
        }

        // Loan Account Operations
        public async Task<LoanAccount> CreateLoanAsync(LoanAccount loan, PaymentMode paymentMode)
        {
            // Fetch party to capture name
            var party = await _context.Parties.FindAsync(loan.PartyId);
            loan.PartyName = party?.Name ?? string.Empty;
            
            // Initialize outstanding amounts
            loan.OutstandingPrincipal = loan.OriginalAmount;
            loan.OutstandingInterest = 0;
            loan.TotalOutstanding = loan.OriginalAmount;
            loan.Status = LoanStatus.Active;
            loan.CreatedDate = DateTime.Now;

            await _loanAccountRepository.AddAsync(loan);
            await _context.SaveChangesAsync();

            // Create initial financial transaction
            var initialTransaction = new FinancialTransaction
            {
                PartyId = loan.PartyId,
                PartyName = party?.Name ?? string.Empty,
                TransactionType = loan.LoanType == LoanType.Given ? FinancialTransactionType.LoanGiven : FinancialTransactionType.LoanTaken,
                Amount = loan.OriginalAmount,
                InterestRate = loan.InterestRate,
                TransactionDate = loan.StartDate,
                DueDate = loan.DueDate,
                LinkedLoanAccountId = loan.LoanAccountId,
                EnteredBy = loan.CreatedBy,
                Notes = $"Initial loan: {loan.Notes}",
                CreatedDate = DateTime.Now,
                PaymentMode = paymentMode
            };

            await _financialTransactionRepository.AddAsync(initialTransaction);
            await _context.SaveChangesAsync();

            return loan;
        }

        public async Task<FinancialTransaction> RecordPaymentAsync(int loanAccountId, decimal paymentAmount, PaymentMode paymentMode, int userId, string notes = "")
        {
            var loanAccount = await _loanAccountRepository.GetWithTransactionsAsync(loanAccountId);
            
            // Validate loan and payment
            if (loanAccount == null)
                throw new InvalidOperationException("Loan account not found");

            await ValidateLoanForPaymentAsync(loanAccount, paymentAmount);

            // Try to calculate interest up to current date (but don't fail if already calculated today)
            try
            {
                await UpdateLoanInterestAsync(loanAccountId);
                
                // Reload after interest update
                loanAccount = await _loanAccountRepository.GetWithTransactionsAsync(loanAccountId);
                if (loanAccount == null)
                    throw new InvalidOperationException("Loan account not found after interest update");
            }
            catch (InvalidOperationException)
            {
                // Interest already calculated today or no interest to calculate - that's fine, continue with payment
                // Ensure loanAccount is not null after catch
                if (loanAccount == null)
                    throw new InvalidOperationException("Loan account not found");
            }

            // Create payment transaction
            var transactionType = loanAccount.LoanType == LoanType.Given
                ? FinancialTransactionType.LoanRepayment
                : FinancialTransactionType.LoanPayment;

            var paymentParty = loanAccount.Party ?? await _context.Parties.FindAsync(loanAccount.PartyId);
            var transaction = new FinancialTransaction
            {
                PartyId = loanAccount.PartyId,
                PartyName = paymentParty?.Name ?? string.Empty,
                TransactionType = transactionType,
                Amount = paymentAmount,
                TransactionDate = DateTime.Now,
                LinkedLoanAccountId = loanAccountId,
                EnteredBy = userId,
                Notes = notes,
                CreatedDate = DateTime.Now,
                PaymentMode = paymentMode
            };

            await _financialTransactionRepository.AddAsync(transaction);

            // Allocate payment to loan (interest first, then principal)
            AllocatePaymentToLoan(loanAccount, paymentAmount);

            loanAccount.ModifiedDate = DateTime.Now;
            await _loanAccountRepository.UpdateAsync(loanAccount);
            await _context.SaveChangesAsync();

            return transaction;
        }

        /// <summary>
        /// Validates loan and payment conditions before processing payment.
        /// </summary>
        private async Task ValidateLoanForPaymentAsync(LoanAccount loanAccount, decimal paymentAmount)
        {
            if (loanAccount == null)
                throw new InvalidOperationException("Loan account not found");

            if (loanAccount.Status == LoanStatus.Closed)
                throw new InvalidOperationException("Cannot record payment for closed loan");

            if (paymentAmount <= 0)
                throw new InvalidOperationException("Payment amount must be greater than zero");

            if (paymentAmount > loanAccount.TotalOutstanding)
                throw new InvalidOperationException($"Payment amount ({paymentAmount:C}) exceeds outstanding amount ({loanAccount.TotalOutstanding:C})");

            await Task.CompletedTask;
        }

        /// <summary>
        /// Allocates payment amount to interest first, then to principal.
        /// Returns the updated loan account with recalculated outstanding amounts and status.
        /// </summary>
        private void AllocatePaymentToLoan(LoanAccount loanAccount, decimal paymentAmount)
        {
            decimal remainingPayment = paymentAmount;

            // First pay off interest
            if (loanAccount.OutstandingInterest > 0)
            {
                decimal interestPayment = Math.Min(remainingPayment, loanAccount.OutstandingInterest);
                loanAccount.OutstandingInterest -= interestPayment;
                remainingPayment -= interestPayment;
            }

            // Then pay off principal
            if (remainingPayment > 0)
            {
                loanAccount.OutstandingPrincipal -= remainingPayment;
            }

            // Recalculate totals
            loanAccount.TotalOutstanding = loanAccount.OutstandingPrincipal + loanAccount.OutstandingInterest;

            // Update loan status based on outstanding amount
            if (loanAccount.TotalOutstanding <= 0)
            {
                loanAccount.Status = LoanStatus.Closed;
            }
            else if (loanAccount.TotalOutstanding < loanAccount.OriginalAmount)
            {
                loanAccount.Status = LoanStatus.PartiallyPaid;
            }
        }

        /// <summary>
        /// Calculates interest accrual for a loan and creates interest transaction.
        /// Returns the accrued interest amount.
        /// </summary>
        private async Task<decimal> CalculateAndAccrueInterestAsync(LoanAccount loanAccount)
        {
            // Find last interest calculation date
            var lastInterestTransaction = loanAccount.Transactions
                .Where(t => t.TransactionType == FinancialTransactionType.InterestReceived ||
                           t.TransactionType == FinancialTransactionType.InterestPaid)
                .OrderByDescending(t => t.TransactionDate)
                .FirstOrDefault();

            var lastCalculationDate = lastInterestTransaction?.TransactionDate ?? loanAccount.StartDate;
            var today = DateTime.Now;

            // Calculate days since last calculation
            var daysSinceLastCalculation = (today - lastCalculationDate).Days;

            if (daysSinceLastCalculation <= 0)
            {
                throw new InvalidOperationException($"No interest to calculate. Last calculation was on {lastCalculationDate:yyyy-MM-dd}. Interest can only be calculated once per day.");
            }

            // Simple interest calculation: (Principal * Rate * Time) / 100
            // Time in years = Days / 365
            decimal interestAccrued = (loanAccount.OutstandingPrincipal * loanAccount.InterestRate * daysSinceLastCalculation) / (365 * 100);

            if (interestAccrued <= 0)
            {
                throw new InvalidOperationException("No interest to calculate. Outstanding principal is zero or interest rate is zero.");
            }

            // Create interest transaction
            var transactionType = loanAccount.LoanType == LoanType.Given
                ? FinancialTransactionType.InterestReceived
                : FinancialTransactionType.InterestPaid;

            var interestParty = loanAccount.Party ?? await _context.Parties.FindAsync(loanAccount.PartyId);
            var interestTransaction = new FinancialTransaction
            {
                PartyId = loanAccount.PartyId,
                PartyName = interestParty?.Name ?? string.Empty,
                TransactionType = transactionType,
                Amount = 0, // Interest doesn't involve cash flow yet
                InterestAmount = interestAccrued,
                InterestRate = loanAccount.InterestRate,
                TransactionDate = today,
                LinkedLoanAccountId = loanAccount.LoanAccountId,
                EnteredBy = loanAccount.CreatedBy,
                Notes = $"Interest accrued for {daysSinceLastCalculation} days",
                CreatedDate = DateTime.Now
            };

            await _financialTransactionRepository.AddAsync(interestTransaction);
            await _context.SaveChangesAsync();

            return interestAccrued;
        }

        public async Task UpdateLoanInterestAsync(int loanAccountId)
        {
            var loanAccount = await _loanAccountRepository.GetWithTransactionsAsync(loanAccountId);
            if (loanAccount == null || loanAccount.Status == LoanStatus.Closed)
                return;

            // Calculate and accrue interest
            decimal interestAccrued = await CalculateAndAccrueInterestAsync(loanAccount);

            // Update loan account
            loanAccount.OutstandingInterest += interestAccrued;
            loanAccount.TotalOutstanding = loanAccount.OutstandingPrincipal + loanAccount.OutstandingInterest;

            // Check if loan is overdue
            var today = DateTime.Now;
            if (loanAccount.DueDate.HasValue && today > loanAccount.DueDate.Value && loanAccount.Status == LoanStatus.Active)
            {
                loanAccount.Status = LoanStatus.Overdue;
            }

            loanAccount.ModifiedDate = DateTime.Now;
            await _loanAccountRepository.UpdateAsync(loanAccount);
            await _context.SaveChangesAsync();
        }

        // Query Operations
        public async Task<IEnumerable<LoanAccount>> GetAllLoansAsync()
        {
            return await _loanAccountRepository.GetAllAsync();
        }

        public async Task<IEnumerable<LoanAccount>> GetLoansByTypeAsync(LoanType loanType)
        {
            return await _loanAccountRepository.GetByLoanTypeAsync(loanType);
        }

        public async Task<IEnumerable<LoanAccount>> GetLoansByPartyAsync(int partyId)
        {
            return await _loanAccountRepository.GetByPartyIdAsync(partyId);
        }

        public async Task<IEnumerable<LoanAccount>> GetActiveLoansByPartyAsync(int partyId)
        {
            return await _loanAccountRepository.GetActiveLoansByPartyAsync(partyId);
        }

        public async Task<IEnumerable<LoanAccount>> GetOverdueLoansAsync()
        {
            return await _loanAccountRepository.GetOverdueLoansAsync();
        }

        public async Task<LoanAccount?> GetLoanWithTransactionsAsync(int loanAccountId)
        {
            return await _loanAccountRepository.GetWithTransactionsAsync(loanAccountId);
        }

        public async Task<IEnumerable<FinancialTransaction>> GetTransactionsByLoanAsync(int loanAccountId)
        {
            return await _financialTransactionRepository.GetByLoanAccountIdAsync(loanAccountId);
        }

        public async Task<IEnumerable<FinancialTransaction>> GetAllFinancialTransactionsAsync()
        {
            return await _financialTransactionRepository.GetAllAsync();
        }

        // Delete Operations
        public async Task DeleteLoanAsync(int loanAccountId)
        {
            var loan = await _loanAccountRepository.GetWithTransactionsAsync(loanAccountId);
            if (loan == null) return;

            // Do not delete historical entries in bulk. Require explicit deletion per transaction.
            if (loan.Transactions != null && loan.Transactions.Any())
            {
                throw new InvalidOperationException("Loan account cannot be deleted because it has related transactions. Delete individual transactions if truly intended, or close the loan.");
            }

            // Allow deletion only when there are no transactions (should be rare)
            _context.LoanAccounts.Remove(loan);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Reverses the impact of a transaction on its linked loan account.
        /// Handles interest reversals and payment reversals appropriately.
        /// </summary>
        private async Task ReverseTransactionImpactAsync(FinancialTransaction tx, LoanAccount loan)
        {
            switch (tx.TransactionType)
            {
                case FinancialTransactionType.InterestReceived:
                case FinancialTransactionType.InterestPaid:
                    // Reverse interest accrual
                    if (tx.InterestAmount.HasValue && tx.InterestAmount.Value > 0)
                    {
                        loan.OutstandingInterest = Math.Max(0, loan.OutstandingInterest - tx.InterestAmount.Value);
                        loan.TotalOutstanding = loan.OutstandingPrincipal + loan.OutstandingInterest;
                    }
                    break;

                case FinancialTransactionType.LoanRepayment:
                case FinancialTransactionType.LoanPayment:
                    // Reverse payment - re-add payment amount back to principal
                    loan.OutstandingPrincipal += tx.Amount;
                    loan.TotalOutstanding = loan.OutstandingPrincipal + loan.OutstandingInterest;

                    // Update status based on totals
                    loan.Status = loan.TotalOutstanding <= 0 ? LoanStatus.Closed : 
                                  (loan.TotalOutstanding < loan.OriginalAmount ? LoanStatus.PartiallyPaid : LoanStatus.Active);
                    break;

                case FinancialTransactionType.LoanGiven:
                case FinancialTransactionType.LoanTaken:
                    // Initial loan entry is tied to the loan account lifecycle; prefer deleting via DeleteLoanAsync
                    break;
            }

            await Task.CompletedTask;
        }

        public async Task DeleteFinancialTransactionAsync(int transactionId)
        {
            var tx = await _financialTransactionRepository.GetByIdAsync(transactionId);
            if (tx == null) return;

            // Adjust linked loan totals when applicable
            if (tx.LinkedLoanAccountId.HasValue)
            {
                var loan = await _loanAccountRepository.GetByIdAsync(tx.LinkedLoanAccountId.Value);
                if (loan != null)
                {
                    // Reverse the transaction's impact on the loan
                    await ReverseTransactionImpactAsync(tx, loan);

                    loan.ModifiedDate = DateTime.Now;
                    await _loanAccountRepository.UpdateAsync(loan);
                }
            }

            await _financialTransactionRepository.DeleteAsync(tx);
            await _context.SaveChangesAsync();
        }

        // Summary Operations
        public async Task<decimal> GetTotalLoansGivenOutstandingAsync()
        {
            return await _loanAccountRepository.GetTotalOutstandingByTypeAsync(LoanType.Given);
        }

        public async Task<decimal> GetTotalLoansTakenOutstandingAsync()
        {
            return await _loanAccountRepository.GetTotalOutstandingByTypeAsync(LoanType.Taken);
        }

        public async Task<Dictionary<string, decimal>> GetFinancialSummaryAsync()
        {
            var summary = new Dictionary<string, decimal>
            {
                ["TotalLoansGiven"] = await _loanAccountRepository.GetTotalOutstandingByTypeAsync(LoanType.Given),
                ["TotalLoansTaken"] = await _loanAccountRepository.GetTotalOutstandingByTypeAsync(LoanType.Taken),
                ["TotalInterestReceivable"] = 0,
                ["TotalInterestPayable"] = 0
            };

            var activeLoansGiven = await _loanAccountRepository.GetByLoanTypeAsync(LoanType.Given);
            var activeLoansTaken = await _loanAccountRepository.GetByLoanTypeAsync(LoanType.Taken);

            summary["TotalInterestReceivable"] = activeLoansGiven
                .Where(l => l.Status != LoanStatus.Closed)
                .Sum(l => l.OutstandingInterest);

            summary["TotalInterestPayable"] = activeLoansTaken
                .Where(l => l.Status != LoanStatus.Closed)
                .Sum(l => l.OutstandingInterest);

            return summary;
        }

        // Restore Operations
        public async Task<LoanAccount> RestoreLoanAsync(LoanAccount snapshot, IEnumerable<FinancialTransaction> transactions)
        {
            // Recreate loan account
            var loan = new LoanAccount
            {
                PartyId = snapshot.PartyId,
                PartyName = snapshot.PartyName,
                LoanType = snapshot.LoanType,
                OriginalAmount = snapshot.OriginalAmount,
                InterestRate = snapshot.InterestRate,
                StartDate = snapshot.StartDate,
                DueDate = snapshot.DueDate,
                OutstandingPrincipal = snapshot.OutstandingPrincipal,
                OutstandingInterest = snapshot.OutstandingInterest,
                TotalOutstanding = snapshot.TotalOutstanding,
                Status = snapshot.Status,
                CreatedBy = snapshot.CreatedBy,
                Notes = snapshot.Notes,
                CreatedDate = DateTime.Now
            };

            await _loanAccountRepository.AddAsync(loan);
            await _context.SaveChangesAsync();

            // Restore transactions linked to new loan id and adjust totals appropriately
            foreach (var tx in transactions.OrderBy(t => t.TransactionDate))
            {
                var newTx = new FinancialTransaction
                {
                    PartyId = tx.PartyId,
                    PartyName = tx.PartyName,
                    TransactionType = tx.TransactionType,
                    Amount = tx.Amount,
                    InterestRate = tx.InterestRate,
                    InterestAmount = tx.InterestAmount,
                    TransactionDate = tx.TransactionDate,
                    DueDate = tx.DueDate,
                    LinkedLoanAccountId = loan.LoanAccountId,
                    EnteredBy = tx.EnteredBy,
                    Notes = tx.Notes,
                    CreatedDate = DateTime.Now
                };
                await RestoreFinancialTransactionAsync(newTx);
            }

            return loan;
        }

        public async Task RestoreFinancialTransactionAsync(FinancialTransaction tx)
        {
            // Adjust loan totals first when applicable
            if (tx.LinkedLoanAccountId.HasValue)
            {
                var loan = await _loanAccountRepository.GetByIdAsync(tx.LinkedLoanAccountId.Value);
                if (loan != null)
                {
                    switch (tx.TransactionType)
                    {
                        case FinancialTransactionType.InterestReceived:
                        case FinancialTransactionType.InterestPaid:
                            if (tx.InterestAmount.HasValue && tx.InterestAmount.Value > 0)
                            {
                                loan.OutstandingInterest += tx.InterestAmount.Value;
                                loan.TotalOutstanding = loan.OutstandingPrincipal + loan.OutstandingInterest;
                            }
                            break;
                        case FinancialTransactionType.LoanRepayment:
                        case FinancialTransactionType.LoanPayment:
                            // Apply payment against outstanding principal
                            loan.OutstandingPrincipal = Math.Max(0, loan.OutstandingPrincipal - tx.Amount);
                            loan.TotalOutstanding = loan.OutstandingPrincipal + loan.OutstandingInterest;
                            // Update status
                            loan.Status = loan.TotalOutstanding <= 0 ? LoanStatus.Closed : (loan.TotalOutstanding < loan.OriginalAmount ? LoanStatus.PartiallyPaid : LoanStatus.Active);
                            break;
                        case FinancialTransactionType.LoanGiven:
                        case FinancialTransactionType.LoanTaken:
                            // Initial loan entries are created via CreateLoanAsync
                            break;
                    }
                    loan.ModifiedDate = DateTime.Now;
                    await _loanAccountRepository.UpdateAsync(loan);
                }
            }

            await _financialTransactionRepository.AddAsync(tx);
            await _context.SaveChangesAsync();
        }
    }
}

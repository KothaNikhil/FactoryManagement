using FactoryManagement.Data;
using FactoryManagement.Data.Repositories;
using FactoryManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FactoryManagement.Services
{
    public class FinancialTransactionService
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
        public async Task<LoanAccount> CreateLoanAsync(LoanAccount loan)
        {
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
                TransactionType = loan.LoanType == LoanType.Given ? FinancialTransactionType.LoanGiven : FinancialTransactionType.LoanTaken,
                Amount = loan.OriginalAmount,
                InterestRate = loan.InterestRate,
                TransactionDate = loan.StartDate,
                DueDate = loan.DueDate,
                LinkedLoanAccountId = loan.LoanAccountId,
                EnteredBy = loan.CreatedBy,
                Notes = $"Initial loan: {loan.Notes}",
                CreatedDate = DateTime.Now
            };

            await _financialTransactionRepository.AddAsync(initialTransaction);
            await _context.SaveChangesAsync();

            return loan;
        }

        public async Task<FinancialTransaction> RecordPaymentAsync(int loanAccountId, decimal paymentAmount, int userId, string notes = "")
        {
            var loanAccount = await _loanAccountRepository.GetWithTransactionsAsync(loanAccountId);
            if (loanAccount == null)
                throw new InvalidOperationException("Loan account not found");

            if (loanAccount.Status == LoanStatus.Closed)
                throw new InvalidOperationException("Cannot record payment for closed loan");

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

            if (paymentAmount > loanAccount.TotalOutstanding)
                throw new InvalidOperationException($"Payment amount ({paymentAmount:C}) exceeds outstanding amount ({loanAccount.TotalOutstanding:C})");

            // Create payment transaction
            var transactionType = loanAccount.LoanType == LoanType.Given
                ? FinancialTransactionType.LoanRepayment
                : FinancialTransactionType.LoanPayment;

            var transaction = new FinancialTransaction
            {
                PartyId = loanAccount.PartyId,
                TransactionType = transactionType,
                Amount = paymentAmount,
                TransactionDate = DateTime.Now,
                LinkedLoanAccountId = loanAccountId,
                EnteredBy = userId,
                Notes = notes,
                CreatedDate = DateTime.Now
            };

            await _financialTransactionRepository.AddAsync(transaction);

            // Update loan account outstanding amounts
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

            loanAccount.TotalOutstanding = loanAccount.OutstandingPrincipal + loanAccount.OutstandingInterest;

            // Update loan status
            if (loanAccount.TotalOutstanding <= 0)
            {
                loanAccount.Status = LoanStatus.Closed;
            }
            else if (loanAccount.TotalOutstanding < loanAccount.OriginalAmount)
            {
                loanAccount.Status = LoanStatus.PartiallyPaid;
            }

            loanAccount.ModifiedDate = DateTime.Now;
            await _loanAccountRepository.UpdateAsync(loanAccount);
            await _context.SaveChangesAsync();

            return transaction;
        }

        public async Task UpdateLoanInterestAsync(int loanAccountId)
        {
            var loanAccount = await _loanAccountRepository.GetWithTransactionsAsync(loanAccountId);
            if (loanAccount == null || loanAccount.Status == LoanStatus.Closed)
                return;

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

            var interestTransaction = new FinancialTransaction
            {
                PartyId = loanAccount.PartyId,
                TransactionType = transactionType,
                Amount = 0, // Interest doesn't involve cash flow yet
                InterestAmount = interestAccrued,
                InterestRate = loanAccount.InterestRate,
                TransactionDate = today,
                LinkedLoanAccountId = loanAccountId,
                EnteredBy = loanAccount.CreatedBy,
                Notes = $"Interest accrued for {daysSinceLastCalculation} days",
                CreatedDate = DateTime.Now
            };

            await _financialTransactionRepository.AddAsync(interestTransaction);

            // Update loan account
            loanAccount.OutstandingInterest += interestAccrued;
            loanAccount.TotalOutstanding = loanAccount.OutstandingPrincipal + loanAccount.OutstandingInterest;

            // Check if loan is overdue
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
    }
}

using FactoryManagement.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FactoryManagement.Data.Repositories
{
    public interface ILoanAccountRepository : IRepository<LoanAccount>
    {
        Task<IEnumerable<LoanAccount>> GetByPartyIdAsync(int partyId);
        Task<IEnumerable<LoanAccount>> GetByLoanTypeAsync(LoanType loanType);
        Task<IEnumerable<LoanAccount>> GetByStatusAsync(LoanStatus status);
        Task<IEnumerable<LoanAccount>> GetActiveLoansByPartyAsync(int partyId);
        Task<IEnumerable<LoanAccount>> GetOverdueLoansAsync();
        Task<decimal> GetTotalOutstandingByTypeAsync(LoanType loanType);
        Task<LoanAccount?> GetWithTransactionsAsync(int loanAccountId);
    }

    public class LoanAccountRepository : Repository<LoanAccount>, ILoanAccountRepository
    {
        public LoanAccountRepository(FactoryDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<LoanAccount>> GetByPartyIdAsync(int partyId)
        {
            return await _context.LoanAccounts
                .Include(la => la.Party)
                .Include(la => la.User)
                .Where(la => la.PartyId == partyId)
                .OrderByDescending(la => la.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<LoanAccount>> GetByLoanTypeAsync(LoanType loanType)
        {
            return await _context.LoanAccounts
                .Include(la => la.Party)
                .Include(la => la.User)
                .Where(la => la.LoanType == loanType)
                .OrderByDescending(la => la.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<LoanAccount>> GetByStatusAsync(LoanStatus status)
        {
            return await _context.LoanAccounts
                .Include(la => la.Party)
                .Include(la => la.User)
                .Where(la => la.Status == status)
                .OrderByDescending(la => la.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<LoanAccount>> GetActiveLoansByPartyAsync(int partyId)
        {
            return await _context.LoanAccounts
                .Include(la => la.Party)
                .Include(la => la.User)
                .Where(la => la.PartyId == partyId && la.Status == LoanStatus.Active)
                .OrderByDescending(la => la.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<LoanAccount>> GetOverdueLoansAsync()
        {
            var today = DateTime.Now.Date;
            return await _context.LoanAccounts
                .Include(la => la.Party)
                .Include(la => la.User)
                .Where(la => la.Status == LoanStatus.Active && la.DueDate.HasValue && la.DueDate.Value < today)
                .OrderBy(la => la.DueDate)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalOutstandingByTypeAsync(LoanType loanType)
        {
            var sum = await _context.LoanAccounts
                .Where(la => la.LoanType == loanType && 
                            (la.Status == LoanStatus.Active || la.Status == LoanStatus.PartiallyPaid || la.Status == LoanStatus.Overdue))
                .SumAsync(la => (double?)la.TotalOutstanding);
            return (decimal)(sum ?? 0);
        }

        public async Task<LoanAccount?> GetWithTransactionsAsync(int loanAccountId)
        {
            return await _context.LoanAccounts
                .Include(la => la.Party)
                .Include(la => la.User)
                .Include(la => la.Transactions.OrderByDescending(t => t.TransactionDate))
                .FirstOrDefaultAsync(la => la.LoanAccountId == loanAccountId);
        }
    }
}

using FactoryManagement.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FactoryManagement.Data.Repositories
{
    public interface IFinancialTransactionRepository : IRepository<FinancialTransaction>
    {
        Task<IEnumerable<FinancialTransaction>> GetByPartyIdAsync(int partyId);
        Task<IEnumerable<FinancialTransaction>> GetByLoanAccountIdAsync(int loanAccountId);
        Task<IEnumerable<FinancialTransaction>> GetByTypeAsync(FinancialTransactionType type);
        Task<IEnumerable<FinancialTransaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<decimal> GetTotalAmountByTypeAsync(FinancialTransactionType type);
        Task<decimal> GetTotalAmountByPartyAndTypeAsync(int partyId, FinancialTransactionType type);
    }

    public class FinancialTransactionRepository : Repository<FinancialTransaction>, IFinancialTransactionRepository
    {
        public FinancialTransactionRepository(FactoryDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<FinancialTransaction>> GetByPartyIdAsync(int partyId)
        {
            return await _context.FinancialTransactions
                .Include(ft => ft.Party)
                .Include(ft => ft.User)
                .Include(ft => ft.LinkedLoanAccount)
                .Where(ft => ft.PartyId == partyId)
                .OrderByDescending(ft => ft.TransactionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<FinancialTransaction>> GetByLoanAccountIdAsync(int loanAccountId)
        {
            return await _context.FinancialTransactions
                .Include(ft => ft.Party)
                .Include(ft => ft.User)
                .Where(ft => ft.LinkedLoanAccountId == loanAccountId)
                .OrderByDescending(ft => ft.TransactionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<FinancialTransaction>> GetByTypeAsync(FinancialTransactionType type)
        {
            return await _context.FinancialTransactions
                .Include(ft => ft.Party)
                .Include(ft => ft.User)
                .Include(ft => ft.LinkedLoanAccount)
                .Where(ft => ft.TransactionType == type)
                .OrderByDescending(ft => ft.TransactionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<FinancialTransaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.FinancialTransactions
                .Include(ft => ft.Party)
                .Include(ft => ft.User)
                .Include(ft => ft.LinkedLoanAccount)
                .Where(ft => ft.TransactionDate >= startDate && ft.TransactionDate <= endDate)
                .OrderByDescending(ft => ft.TransactionDate)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalAmountByTypeAsync(FinancialTransactionType type)
        {
            var sum = await _context.FinancialTransactions
                .Where(ft => ft.TransactionType == type)
                .SumAsync(ft => (double?)ft.Amount);
            return (decimal)(sum ?? 0);
        }

        public async Task<decimal> GetTotalAmountByPartyAndTypeAsync(int partyId, FinancialTransactionType type)
        {
            var sum = await _context.FinancialTransactions
                .Where(ft => ft.PartyId == partyId && ft.TransactionType == type)
                .SumAsync(ft => (double?)ft.Amount);
            return (decimal)(sum ?? 0);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using FactoryManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FactoryManagement.Data.Repositories
{
    public interface IBalanceHistoryRepository : IRepository<BalanceHistory>
    {
        Task<List<BalanceHistory>> GetByAccountIdAsync(int accountId);
        Task<List<BalanceHistory>> GetByDateRangeAsync(int accountId, DateTime fromDate, DateTime toDate);
        Task<List<BalanceHistory>> GetRecentAsync(int count = 10);
    }

    public class BalanceHistoryRepository : Repository<BalanceHistory>, IBalanceHistoryRepository
    {
        public BalanceHistoryRepository(FactoryDbContext context) : base(context)
        {
        }

        public async Task<List<BalanceHistory>> GetByAccountIdAsync(int accountId)
        {
            return await _dbSet
                .Include(h => h.Account)
                .Include(h => h.User)
                .Where(h => h.AccountId == accountId)
                .OrderByDescending(h => h.TransactionDate)
                .ToListAsync();
        }

        public async Task<List<BalanceHistory>> GetByDateRangeAsync(int accountId, DateTime fromDate, DateTime toDate)
        {
            return await _dbSet
                .Include(h => h.Account)
                .Include(h => h.User)
                .Where(h => h.AccountId == accountId && 
                           h.TransactionDate >= fromDate && 
                           h.TransactionDate <= toDate)
                .OrderByDescending(h => h.TransactionDate)
                .ToListAsync();
        }

        public async Task<List<BalanceHistory>> GetRecentAsync(int count = 10)
        {
            return await _dbSet
                .Include(h => h.Account)
                .Include(h => h.User)
                .OrderByDescending(h => h.TransactionDate)
                .Take(count)
                .ToListAsync();
        }
    }
}

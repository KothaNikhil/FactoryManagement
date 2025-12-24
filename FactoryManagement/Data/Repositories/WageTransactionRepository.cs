using FactoryManagement.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FactoryManagement.Data.Repositories
{
    public interface IWageTransactionRepository : IRepository<WageTransaction>
    {
        Task<IEnumerable<WageTransaction>> GetByWorkerIdAsync(int workerId);
        Task<IEnumerable<WageTransaction>> GetByTypeAsync(WageTransactionType type);
        Task<IEnumerable<WageTransaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<WageTransaction>> GetByWorkerAndDateRangeAsync(int workerId, DateTime startDate, DateTime endDate);
        Task<decimal> GetTotalAmountByTypeAsync(WageTransactionType type);
        Task<decimal> GetTotalAmountByWorkerAsync(int workerId);
        Task<decimal> GetTotalWagesPaidAsync();
        Task<decimal> GetTotalAdvancesGivenAsync();
    }

    public class WageTransactionRepository : Repository<WageTransaction>, IWageTransactionRepository
    {
        public WageTransactionRepository(FactoryDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<WageTransaction>> GetByWorkerIdAsync(int workerId)
        {
            return await _context.WageTransactions
                .Include(wt => wt.Worker)
                .Where(wt => wt.WorkerId == workerId)
                .OrderByDescending(wt => wt.TransactionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<WageTransaction>> GetByTypeAsync(WageTransactionType type)
        {
            return await _context.WageTransactions
                .Include(wt => wt.Worker)
                .Where(wt => wt.TransactionType == type)
                .OrderByDescending(wt => wt.TransactionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<WageTransaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.WageTransactions
                .Include(wt => wt.Worker)
                .Where(wt => wt.TransactionDate >= startDate && wt.TransactionDate <= endDate)
                .OrderByDescending(wt => wt.TransactionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<WageTransaction>> GetByWorkerAndDateRangeAsync(int workerId, DateTime startDate, DateTime endDate)
        {
            return await _context.WageTransactions
                .Include(wt => wt.Worker)
                .Where(wt => wt.WorkerId == workerId && 
                            wt.TransactionDate >= startDate && 
                            wt.TransactionDate <= endDate)
                .OrderByDescending(wt => wt.TransactionDate)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalAmountByTypeAsync(WageTransactionType type)
        {
            var sum = await _context.WageTransactions
                .Where(wt => wt.TransactionType == type)
                .SumAsync(wt => (double?)wt.NetAmount);
            return (decimal)(sum ?? 0);
        }

        public async Task<decimal> GetTotalAmountByWorkerAsync(int workerId)
        {
            var sum = await _context.WageTransactions
                .Where(wt => wt.WorkerId == workerId)
                .SumAsync(wt => (double?)wt.NetAmount);
            return (decimal)(sum ?? 0);
        }

        public async Task<decimal> GetTotalWagesPaidAsync()
        {
            var sum = await _context.WageTransactions
                .Where(wt => wt.TransactionType == WageTransactionType.DailyWage ||
                            wt.TransactionType == WageTransactionType.HourlyWage ||
                            wt.TransactionType == WageTransactionType.MonthlyWage ||
                            wt.TransactionType == WageTransactionType.OvertimePay ||
                            wt.TransactionType == WageTransactionType.Bonus)
                .SumAsync(wt => (double?)wt.NetAmount);
            return (decimal)(sum ?? 0);
        }

        public async Task<decimal> GetTotalAdvancesGivenAsync()
        {
            var sum = await _context.WageTransactions
                .Where(wt => wt.TransactionType == WageTransactionType.AdvanceGiven)
                .SumAsync(wt => (double?)wt.Amount);
            return (decimal)(sum ?? 0);
        }
    }
}

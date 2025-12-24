using FactoryManagement.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FactoryManagement.Data.Repositories
{
    public interface IWorkerRepository : IRepository<Worker>
    {
        Task<IEnumerable<Worker>> GetByStatusAsync(WorkerStatus status);
        Task<Worker?> GetWithTransactionsAsync(int workerId);
        Task<IEnumerable<Worker>> GetActiveWorkersAsync();
        Task<decimal> GetTotalAdvancesByWorkerAsync(int workerId);
        Task<decimal> GetTotalWagesPaidByWorkerAsync(int workerId);
    }

    public class WorkerRepository : Repository<Worker>, IWorkerRepository
    {
        public WorkerRepository(FactoryDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Worker>> GetByStatusAsync(WorkerStatus status)
        {
            return await _context.Workers
                .Where(w => w.Status == status)
                .OrderBy(w => w.Name)
                .ToListAsync();
        }

        public async Task<Worker?> GetWithTransactionsAsync(int workerId)
        {
            return await _context.Workers
                .Include(w => w.WageTransactions)
                .FirstOrDefaultAsync(w => w.WorkerId == workerId);
        }

        public async Task<IEnumerable<Worker>> GetActiveWorkersAsync()
        {
            return await _context.Workers
                .Where(w => w.Status == WorkerStatus.Active)
                .OrderBy(w => w.Name)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalAdvancesByWorkerAsync(int workerId)
        {
            var sum = await _context.WageTransactions
                .Where(wt => wt.WorkerId == workerId && wt.TransactionType == WageTransactionType.AdvanceGiven)
                .SumAsync(wt => (double?)wt.Amount);
            return (decimal)(sum ?? 0);
        }

        public async Task<decimal> GetTotalWagesPaidByWorkerAsync(int workerId)
        {
            var sum = await _context.WageTransactions
                .Where(wt => wt.WorkerId == workerId && 
                            (wt.TransactionType == WageTransactionType.DailyWage ||
                             wt.TransactionType == WageTransactionType.HourlyWage ||
                             wt.TransactionType == WageTransactionType.MonthlyWage))
                .SumAsync(wt => (double?)wt.NetAmount);
            return (decimal)(sum ?? 0);
        }
    }
}

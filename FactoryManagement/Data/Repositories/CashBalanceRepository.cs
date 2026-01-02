using FactoryManagement.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FactoryManagement.Data.Repositories
{
    /// <summary>
    /// Repository implementation for cash balance operations
    /// Provides data access for daily cash tracking and reconciliation
    /// </summary>
    public class CashBalanceRepository : ICashBalanceRepository
    {
        private readonly FactoryDbContext _context;

        public CashBalanceRepository(FactoryDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc/>
        public async Task<CashBalance?> GetByDateAsync(DateTime date)
        {
            try
            {
                var dateOnly = date.Date; // Normalize to date only (remove time component)
                return await _context.CashBalances
                    .Include(cb => cb.ReconciledByUser)
                    .FirstOrDefaultAsync(cb => cb.Date.Date == dateOnly);
            }
            catch (Exception ex)
            {
                // Log exception in production
                throw new InvalidOperationException($"Error retrieving cash balance for date {date:yyyy-MM-dd}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<CashBalance>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var startDateOnly = startDate.Date;
                var endDateOnly = endDate.Date;

                return await _context.CashBalances
                    .Include(cb => cb.ReconciledByUser)
                    .Where(cb => cb.Date.Date >= startDateOnly && cb.Date.Date <= endDateOnly)
                    .OrderByDescending(cb => cb.Date)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving cash balances for date range {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<CashBalance>> GetAllAsync()
        {
            try
            {
                return await _context.CashBalances
                    .Include(cb => cb.ReconciledByUser)
                    .OrderByDescending(cb => cb.Date)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error retrieving all cash balances", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<CashBalance?> GetLatestAsync()
        {
            try
            {
                return await _context.CashBalances
                    .Include(cb => cb.ReconciledByUser)
                    .OrderByDescending(cb => cb.Date)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error retrieving latest cash balance", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<CashBalance?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.CashBalances
                    .Include(cb => cb.ReconciledByUser)
                    .FirstOrDefaultAsync(cb => cb.CashBalanceId == id);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving cash balance with ID {id}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<CashBalance>> GetUnreconciledAsync()
        {
            try
            {
                return await _context.CashBalances
                    .Include(cb => cb.ReconciledByUser)
                    .Where(cb => !cb.IsReconciled)
                    .OrderByDescending(cb => cb.Date)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error retrieving unreconciled cash balances", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<CashBalance>> GetRecordsWithDiscrepanciesAsync()
        {
            try
            {
                return await _context.CashBalances
                    .Include(cb => cb.ReconciledByUser)
                    .Where(cb => cb.IsReconciled && cb.Discrepancy != 0)
                    .OrderByDescending(cb => cb.Date)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error retrieving cash balances with discrepancies", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<CashBalance> CreateAsync(CashBalance cashBalance)
        {
            if (cashBalance == null)
                throw new ArgumentNullException(nameof(cashBalance));

            try
            {
                cashBalance.CreatedDate = DateTime.Now;
                cashBalance.Date = cashBalance.Date.Date; // Ensure date only

                _context.CashBalances.Add(cashBalance);
                await _context.SaveChangesAsync();

                return cashBalance;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error creating cash balance record", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateAsync(CashBalance cashBalance)
        {
            if (cashBalance == null)
                throw new ArgumentNullException(nameof(cashBalance));

            try
            {
                var existingRecord = await _context.CashBalances.FindAsync(cashBalance.CashBalanceId);
                if (existingRecord == null)
                    return false;

                cashBalance.ModifiedDate = DateTime.Now;
                _context.Entry(existingRecord).CurrentValues.SetValues(cashBalance);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error updating cash balance with ID {cashBalance.CashBalanceId}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var cashBalance = await _context.CashBalances.FindAsync(id);
                if (cashBalance == null)
                    return false;

                _context.CashBalances.Remove(cashBalance);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error deleting cash balance with ID {id}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> ExistsForDateAsync(DateTime date)
        {
            try
            {
                var dateOnly = date.Date;
                return await _context.CashBalances.AnyAsync(cb => cb.Date.Date == dateOnly);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error checking existence of cash balance for date {date:yyyy-MM-dd}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<decimal> GetTotalDiscrepancyAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var startDateOnly = startDate.Date;
                var endDateOnly = endDate.Date;

                var records = await _context.CashBalances
                    .Where(cb => cb.Date.Date >= startDateOnly && cb.Date.Date <= endDateOnly && cb.IsReconciled)
                    .ToListAsync();

                return records.Sum(cb => cb.Discrepancy ?? 0);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error calculating total discrepancy for date range {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<decimal> GetCurrentCashInHandAsync()
        {
            try
            {
                var latest = await GetLatestAsync();
                if (latest == null)
                    return 0m;

                // If reconciled, use actual cash counted; otherwise use expected closing
                return latest.IsReconciled && latest.ActualCashCounted.HasValue
                    ? latest.ActualCashCounted.Value
                    : latest.ExpectedClosingBalance;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error retrieving current cash in hand", ex);
            }
        }
    }
}

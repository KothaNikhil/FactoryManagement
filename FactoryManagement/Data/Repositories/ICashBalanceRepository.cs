using FactoryManagement.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FactoryManagement.Data.Repositories
{
    /// <summary>
    /// Repository interface for cash balance operations
    /// Handles CRUD operations for daily cash tracking and reconciliation
    /// </summary>
    public interface ICashBalanceRepository
    {
        /// <summary>
        /// Get cash balance record for a specific date
        /// </summary>
        /// <param name="date">Date to retrieve balance for</param>
        /// <returns>Cash balance record or null if not found</returns>
        Task<CashBalance?> GetByDateAsync(DateTime date);

        /// <summary>
        /// Get cash balance records within a date range
        /// </summary>
        /// <param name="startDate">Start date (inclusive)</param>
        /// <param name="endDate">End date (inclusive)</param>
        /// <returns>List of cash balance records</returns>
        Task<IEnumerable<CashBalance>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get all cash balance records ordered by date descending
        /// </summary>
        /// <returns>All cash balance records</returns>
        Task<IEnumerable<CashBalance>> GetAllAsync();

        /// <summary>
        /// Get the most recent cash balance record (latest date)
        /// Used to get previous day's closing balance for opening balance
        /// </summary>
        /// <returns>Most recent cash balance or null if none exists</returns>
        Task<CashBalance?> GetLatestAsync();

        /// <summary>
        /// Get cash balance record by ID
        /// </summary>
        /// <param name="id">Cash balance ID</param>
        /// <returns>Cash balance record or null if not found</returns>
        Task<CashBalance?> GetByIdAsync(int id);

        /// <summary>
        /// Get unreconciled cash balance records
        /// </summary>
        /// <returns>List of unreconciled records</returns>
        Task<IEnumerable<CashBalance>> GetUnreconciledAsync();

        /// <summary>
        /// Get records with discrepancies (for reporting/analysis)
        /// </summary>
        /// <returns>List of records with cash discrepancies</returns>
        Task<IEnumerable<CashBalance>> GetRecordsWithDiscrepanciesAsync();

        /// <summary>
        /// Create a new cash balance record
        /// </summary>
        /// <param name="cashBalance">Cash balance entity to create</param>
        /// <returns>Created cash balance with ID</returns>
        Task<CashBalance> CreateAsync(CashBalance cashBalance);

        /// <summary>
        /// Update an existing cash balance record
        /// </summary>
        /// <param name="cashBalance">Cash balance entity to update</param>
        /// <returns>True if update successful</returns>
        Task<bool> UpdateAsync(CashBalance cashBalance);

        /// <summary>
        /// Delete a cash balance record
        /// </summary>
        /// <param name="id">ID of record to delete</param>
        /// <returns>True if deletion successful</returns>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Check if a cash balance record exists for a specific date
        /// </summary>
        /// <param name="date">Date to check</param>
        /// <returns>True if record exists</returns>
        Task<bool> ExistsForDateAsync(DateTime date);

        /// <summary>
        /// Get total cash discrepancies for a date range (for reporting)
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>Total discrepancy amount (can be positive or negative)</returns>
        Task<decimal> GetTotalDiscrepancyAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get current cash in hand (latest closing balance or actual cash)
        /// </summary>
        /// <returns>Current cash amount</returns>
        Task<decimal> GetCurrentCashInHandAsync();
    }
}

using FactoryManagement.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FactoryManagement.Services
{
    public interface ICashAccountService
    {
        // Account Management
        Task<CashAccount> CreateAccountAsync(CashAccount account);
        Task<CashAccount?> GetAccountByIdAsync(int accountId);
        Task<CashAccount?> GetAccountByTypeAsync(AccountType accountType);
        Task<List<CashAccount>> GetAllActiveAccountsAsync();
        Task<CashAccount> UpdateAccountAsync(CashAccount account);
        
        // Balance Operations
        Task<decimal> GetCurrentBalanceAsync(AccountType accountType);
        Task<decimal> GetTotalBalanceAsync(); // Cash + Bank
        Task UpdateBalanceAsync(int accountId, decimal amount, BalanceChangeType changeType, 
            string notes, int userId, int? relatedTransactionId = null);
        
        // Balance History
        Task<List<BalanceHistory>> GetBalanceHistoryAsync(int accountId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<BalanceHistory> CreateBalanceHistoryAsync(BalanceHistory history);
        
        // Reports
        Task<Dictionary<string, decimal>> GetAccountSummaryAsync();
        Task<List<BalanceHistory>> GetRecentBalanceChangesAsync(int count = 10);
    }
}

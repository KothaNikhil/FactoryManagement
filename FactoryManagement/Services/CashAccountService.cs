using FactoryManagement.Data;
using FactoryManagement.Data.Repositories;
using FactoryManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FactoryManagement.Services
{
    public class CashAccountService : ICashAccountService
    {
        private readonly IRepository<CashAccount> _cashAccountRepository;
        private readonly IRepository<BalanceHistory> _balanceHistoryRepository;
        private readonly FactoryDbContext _context;

        public CashAccountService(
            IRepository<CashAccount> cashAccountRepository,
            IRepository<BalanceHistory> balanceHistoryRepository,
            FactoryDbContext context)
        {
            _cashAccountRepository = cashAccountRepository;
            _balanceHistoryRepository = balanceHistoryRepository;
            _context = context;
        }

        public async Task<CashAccount> CreateAccountAsync(CashAccount account)
        {
            account.CurrentBalance = account.OpeningBalance;
            account.CreatedDate = DateTime.Now;
            
            var createdAccount = await _cashAccountRepository.AddAsync(account);

            // Create opening balance history entry
            var history = new BalanceHistory
            {
                AccountId = createdAccount.AccountId,
                ChangeType = BalanceChangeType.OpeningBalance,
                PreviousBalance = 0,
                ChangeAmount = account.OpeningBalance,
                NewBalance = account.OpeningBalance,
                TransactionDate = account.OpeningDate,
                Notes = $"Opening balance for {account.AccountName}",
                EnteredBy = account.CreatedBy,
                CreatedDate = DateTime.Now
            };

            await _balanceHistoryRepository.AddAsync(history);

            return createdAccount;
        }

        public async Task<CashAccount?> GetAccountByIdAsync(int accountId)
        {
            return await _cashAccountRepository.GetByIdAsync(accountId);
        }

        public async Task<CashAccount?> GetAccountByTypeAsync(AccountType accountType)
        {
            var accounts = await _cashAccountRepository.FindAsync(a => a.AccountType == accountType && a.IsActive);
            return accounts.FirstOrDefault();
        }

        public async Task<List<CashAccount>> GetAllActiveAccountsAsync()
        {
            var accounts = await _cashAccountRepository.FindAsync(a => a.IsActive);
            return accounts.OrderBy(a => a.AccountName).ToList();
        }

        public async Task<CashAccount> UpdateAccountAsync(CashAccount account)
        {
            account.ModifiedDate = DateTime.Now;
            await _cashAccountRepository.UpdateAsync(account);
            return account;
        }

        public async Task<decimal> GetCurrentBalanceAsync(AccountType accountType)
        {
            var account = await GetAccountByTypeAsync(accountType);
            return account?.CurrentBalance ?? 0;
        }

        public async Task<decimal> GetTotalBalanceAsync()
        {
            var cashBalance = await GetCurrentBalanceAsync(AccountType.Cash);
            var bankBalance = await GetCurrentBalanceAsync(AccountType.Bank);
            return cashBalance + bankBalance;
        }

        public async Task UpdateBalanceAsync(int accountId, decimal amount, 
            BalanceChangeType changeType, string notes, int userId, 
            int? relatedTransactionId = null)
        {
            var account = await _cashAccountRepository.GetByIdAsync(accountId);
            if (account == null)
                throw new InvalidOperationException($"Account {accountId} not found");

            var previousBalance = account.CurrentBalance;
            account.CurrentBalance += amount;
            account.ModifiedDate = DateTime.Now;

            var history = new BalanceHistory
            {
                AccountId = accountId,
                ChangeType = changeType,
                PreviousBalance = previousBalance,
                ChangeAmount = amount,
                NewBalance = account.CurrentBalance,
                TransactionDate = DateTime.Now,
                Notes = notes,
                TransactionId = relatedTransactionId,
                EnteredBy = userId,
                CreatedDate = DateTime.Now
            };

            await _cashAccountRepository.UpdateAsync(account);
            await _balanceHistoryRepository.AddAsync(history);
        }

        public async Task<List<BalanceHistory>> GetBalanceHistoryAsync(
            int accountId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = await _balanceHistoryRepository.FindAsync(h => h.AccountId == accountId);

            if (fromDate.HasValue)
                query = query.Where(h => h.TransactionDate >= fromDate.Value).ToList();
            
            if (toDate.HasValue)
                query = query.Where(h => h.TransactionDate <= toDate.Value).ToList();

            return query.OrderByDescending(h => h.TransactionDate).ToList();
        }

        public async Task<BalanceHistory> CreateBalanceHistoryAsync(BalanceHistory history)
        {
            return await _balanceHistoryRepository.AddAsync(history);
        }

        public async Task<Dictionary<string, decimal>> GetAccountSummaryAsync()
        {
            var summary = new Dictionary<string, decimal>();
            
            var accounts = await GetAllActiveAccountsAsync();

            foreach (var account in accounts)
            {
                summary[account.AccountName] = account.CurrentBalance;
            }

            summary["Total"] = accounts.Sum(a => a.CurrentBalance);
            
            return summary;
        }

        public async Task<List<BalanceHistory>> GetRecentBalanceChangesAsync(int count = 10)
        {
            var histories = await _balanceHistoryRepository.FindAsync(h => true);
            return histories
                .OrderByDescending(h => h.TransactionDate)
                .Take(count)
                .ToList();
        }
    }
}

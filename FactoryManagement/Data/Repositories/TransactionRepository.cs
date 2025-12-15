using FactoryManagement.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FactoryManagement.Data.Repositories
{
    public interface ITransactionRepository : IRepository<Transaction>
    {
        Task<IEnumerable<Transaction>> GetTransactionsByItemIdAsync(int itemId);
        Task<IEnumerable<Transaction>> GetTransactionsByPartyIdAsync(int partyId);
        Task<IEnumerable<Transaction>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Transaction>> GetTransactionsByTypeAsync(TransactionType type);
        Task<IEnumerable<Transaction>> GetAllWithDetailsAsync();
    }

    public class TransactionRepository : Repository<Transaction>, ITransactionRepository
    {
        public TransactionRepository(FactoryDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByItemIdAsync(int itemId)
        {
            return await _dbSet
                .Include(t => t.Item)
                .Include(t => t.Party)
                .Include(t => t.User)
                .Where(t => t.ItemId == itemId)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByPartyIdAsync(int partyId)
        {
            return await _dbSet
                .Include(t => t.Item)
                .Include(t => t.Party)
                .Include(t => t.User)
                .Where(t => t.PartyId == partyId)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Include(t => t.Item)
                .Include(t => t.Party)
                .Include(t => t.User)
                .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByTypeAsync(TransactionType type)
        {
            return await _dbSet
                .Include(t => t.Item)
                .Include(t => t.Party)
                .Include(t => t.User)
                .Where(t => t.TransactionType == type)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetAllWithDetailsAsync()
        {
            return await _dbSet
                .Include(t => t.Item)
                .Include(t => t.Party)
                .Include(t => t.User)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }
    }
}

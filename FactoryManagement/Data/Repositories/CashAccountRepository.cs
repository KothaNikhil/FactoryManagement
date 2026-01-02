using Microsoft.EntityFrameworkCore;
using FactoryManagement.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FactoryManagement.Data.Repositories
{
    public interface ICashAccountRepository : IRepository<CashAccount>
    {
        Task<CashAccount?> GetByAccountTypeAsync(AccountType accountType);
        Task<List<CashAccount>> GetAllActiveAccountsAsync();
    }

    public class CashAccountRepository : Repository<CashAccount>, ICashAccountRepository
    {
        public CashAccountRepository(FactoryDbContext context) : base(context)
        {
        }

        public async Task<CashAccount?> GetByAccountTypeAsync(AccountType accountType)
        {
            return await _dbSet
                .FirstOrDefaultAsync(a => a.AccountType == accountType && a.IsActive);
        }

        public async Task<List<CashAccount>> GetAllActiveAccountsAsync()
        {
            return await _dbSet
                .Where(a => a.IsActive)
                .OrderBy(a => a.AccountName)
                .ToListAsync();
        }
    }
}

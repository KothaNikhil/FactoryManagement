using Microsoft.EntityFrameworkCore;
using FactoryManagement.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FactoryManagement.Data.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<IEnumerable<User>> GetActiveUsersAsync();
        Task<User?> GetByUsernameAsync(string username);
    }

    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(FactoryDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<User>> GetActiveUsersAsync()
        {
            return await _dbSet.Where(u => u.IsActive).OrderBy(u => u.Username).ToListAsync();
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Username == username);
        }
    }
}

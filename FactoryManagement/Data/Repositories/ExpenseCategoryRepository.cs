using FactoryManagement.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FactoryManagement.Data.Repositories
{
    public interface IExpenseCategoryRepository : IRepository<ExpenseCategory>
    {
        Task<ExpenseCategory?> GetByCategoryNameAsync(string categoryName);
        Task<bool> CategoryExistsAsync(string categoryName);
        Task<bool> CategoryExistsAsync(string categoryName, int excludeId);
    }

    public class ExpenseCategoryRepository : Repository<ExpenseCategory>, IExpenseCategoryRepository
    {
        public ExpenseCategoryRepository(FactoryDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<ExpenseCategory>> GetAllAsync()
        {
            return await _context.ExpenseCategories
                .Include(ec => ec.Creator)
                .Where(ec => !ec.IsDeleted)
                .OrderBy(ec => ec.CategoryName)
                .ToListAsync();
        }

        public async Task<ExpenseCategory?> GetByCategoryNameAsync(string categoryName)
        {
            return await _context.ExpenseCategories
                .Include(ec => ec.Creator)
                .Where(ec => !ec.IsDeleted)
                .FirstOrDefaultAsync(ec => ec.CategoryName == categoryName);
        }

        public async Task<bool> CategoryExistsAsync(string categoryName)
        {
            return await _context.ExpenseCategories
                .Where(ec => !ec.IsDeleted)
                .AnyAsync(ec => ec.CategoryName == categoryName);
        }

        public async Task<bool> CategoryExistsAsync(string categoryName, int excludeId)
        {
            return await _context.ExpenseCategories
                .Where(ec => !ec.IsDeleted)
                .AnyAsync(ec => ec.CategoryName == categoryName && ec.ExpenseCategoryId != excludeId);
        }
    }
}

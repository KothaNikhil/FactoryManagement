using FactoryManagement.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FactoryManagement.Data.Repositories
{
    public interface IOperationalExpenseRepository : IRepository<OperationalExpense>
    {
        Task<IEnumerable<OperationalExpense>> GetByCategoryIdAsync(int categoryId);
        Task<IEnumerable<OperationalExpense>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<decimal> GetTotalAmountByCategoryAsync(int categoryId);
        Task<decimal> GetTotalAmountByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<Dictionary<int, decimal>> GetExpensesByCategoryAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<OperationalExpense>> GetUnapprovedExpensesAsync();
        Task<decimal> GetTotalExpensesAsync();
        Task<decimal> GetMonthlyExpensesAsync(int year, int month);
        Task<decimal> GetYearlyExpensesAsync(int year);
    }

    public class OperationalExpenseRepository : Repository<OperationalExpense>, IOperationalExpenseRepository
    {
        public OperationalExpenseRepository(FactoryDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<OperationalExpense>> GetAllAsync()
        {
            return await _context.OperationalExpenses
                .Include(oe => oe.ExpenseCategory)
                .Include(oe => oe.SpentByUser)
                .Include(oe => oe.User)
                .Include(oe => oe.Approver)
                .Include(oe => oe.Item)
                .OrderByDescending(oe => oe.ExpenseDate)
                .ThenByDescending(oe => oe.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<OperationalExpense>> GetByCategoryIdAsync(int categoryId)
        {
            return await _context.OperationalExpenses
                .Include(oe => oe.ExpenseCategory)
                .Include(oe => oe.SpentByUser)
                .Include(oe => oe.User)
                .Where(oe => oe.ExpenseCategoryId == categoryId)
                .OrderByDescending(oe => oe.ExpenseDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<OperationalExpense>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.OperationalExpenses
                .Include(oe => oe.ExpenseCategory)
                .Include(oe => oe.SpentByUser)
                .Include(oe => oe.User)
                .Where(oe => oe.ExpenseDate >= startDate && oe.ExpenseDate <= endDate)
                .OrderByDescending(oe => oe.ExpenseDate)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalAmountByCategoryAsync(int categoryId)
        {
            var total = await _context.OperationalExpenses
                .Where(oe => oe.ExpenseCategoryId == categoryId)
                .SumAsync(oe => (double)oe.Amount);
            return (decimal)total;
        }

        public async Task<decimal> GetTotalAmountByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var total = await _context.OperationalExpenses
                .Where(oe => oe.ExpenseDate >= startDate && oe.ExpenseDate <= endDate)
                .SumAsync(oe => (double)oe.Amount);
            return (decimal)total;
        }

        public async Task<Dictionary<int, decimal>> GetExpensesByCategoryAsync(
            DateTime? startDate = null, 
            DateTime? endDate = null)
        {
            var query = _context.OperationalExpenses.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(oe => oe.ExpenseDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(oe => oe.ExpenseDate <= endDate.Value);

            return await query
                .GroupBy(oe => oe.ExpenseCategoryId)
                .Select(g => new { CategoryId = g.Key, Total = g.Sum(oe => (double)oe.Amount) })
                .ToDictionaryAsync(x => x.CategoryId, x => (decimal)x.Total);
        }

        public async Task<IEnumerable<OperationalExpense>> GetUnapprovedExpensesAsync()
        {
            return await _context.OperationalExpenses
                .Include(oe => oe.ExpenseCategory)
                .Include(oe => oe.SpentByUser)
                .Include(oe => oe.User)
                .Where(oe => !oe.IsApproved)
                .OrderByDescending(oe => oe.ExpenseDate)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalExpensesAsync()
        {
            var total = await _context.OperationalExpenses
                .SumAsync(oe => (double?)oe.Amount);
            return (decimal)(total ?? 0);
        }

        public async Task<decimal> GetMonthlyExpensesAsync(int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            
            var total = await _context.OperationalExpenses
                .Where(oe => oe.ExpenseDate >= startDate && oe.ExpenseDate <= endDate)
                .SumAsync(oe => (double?)oe.Amount);
            
            return (decimal)(total ?? 0);
        }

        public async Task<decimal> GetYearlyExpensesAsync(int year)
        {
            var startDate = new DateTime(year, 1, 1);
            var endDate = new DateTime(year, 12, 31);
            
            var total = await _context.OperationalExpenses
                .Where(oe => oe.ExpenseDate >= startDate && oe.ExpenseDate <= endDate)
                .SumAsync(oe => (double?)oe.Amount);
            
            return (decimal)(total ?? 0);
        }
    }
}

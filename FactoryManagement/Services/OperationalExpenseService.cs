using FactoryManagement.Data.Repositories;
using FactoryManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FactoryManagement.Services
{
    public interface IOperationalExpenseService
    {
        // CRUD Operations
        Task<OperationalExpense> CreateExpenseAsync(OperationalExpense expense);
        Task<OperationalExpense?> GetExpenseByIdAsync(int id);
        Task<IEnumerable<OperationalExpense>> GetAllExpensesAsync();
        Task<OperationalExpense> UpdateExpenseAsync(OperationalExpense expense);
        Task<bool> DeleteExpenseAsync(int id);

        // Query Operations
        Task<IEnumerable<OperationalExpense>> GetExpensesByCategoryAsync(int categoryId);
        Task<IEnumerable<OperationalExpense>> GetExpensesByDateRangeAsync(DateTime startDate, DateTime endDate);

        // Aggregation Operations
        Task<decimal> GetTotalExpensesAsync();
        Task<decimal> GetTotalExpensesByCategoryAsync(int categoryId);
        Task<decimal> GetTotalExpensesByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<Dictionary<int, decimal>> GetExpenseBreakdownAsync(DateTime? startDate = null, DateTime? endDate = null);

        // Dashboard Metrics
        Task<decimal> GetMonthlyExpensesAsync(int year, int month);
        Task<decimal> GetYearlyExpensesAsync(int year);
        Task<Dictionary<ExpenseCategory, decimal>> GetTopExpenseCategoriesAsync(int topCount = 5);

        // Approval Workflow (future enhancement)
        Task<bool> ApproveExpenseAsync(int expenseId, int approvedBy);
        Task<IEnumerable<OperationalExpense>> GetUnapprovedExpensesAsync();
    }

    public class OperationalExpenseService : IOperationalExpenseService
    {
        private readonly IOperationalExpenseRepository _expenseRepository;
        private readonly IExpenseCategoryRepository _categoryRepository;

        public OperationalExpenseService(
            IOperationalExpenseRepository expenseRepository,
            IExpenseCategoryRepository categoryRepository)
        {
            _expenseRepository = expenseRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<OperationalExpense> CreateExpenseAsync(OperationalExpense expense)
        {
            // Business logic validation
            if (expense.Amount <= 0)
                throw new ArgumentException("Expense amount must be greater than zero.");

            // Validate category exists and is not deleted
            var category = await _categoryRepository.GetByIdAsync(expense.ExpenseCategoryId);
            if (category == null || category.IsDeleted)
                throw new ArgumentException("Invalid expense category.");

            expense.CreatedDate = DateTime.Now;
            return await _expenseRepository.AddAsync(expense);
        }

        public async Task<OperationalExpense?> GetExpenseByIdAsync(int id)
        {
            return await _expenseRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<OperationalExpense>> GetAllExpensesAsync()
        {
            return await _expenseRepository.GetAllAsync();
        }

        public async Task<OperationalExpense> UpdateExpenseAsync(OperationalExpense expense)
        {
            // Validate category exists and is not deleted
            var category = await _categoryRepository.GetByIdAsync(expense.ExpenseCategoryId);
            if (category == null || category.IsDeleted)
                throw new ArgumentException("Invalid expense category.");

            expense.ModifiedDate = DateTime.Now;
            await _expenseRepository.UpdateAsync(expense);
            return expense;
        }

        public async Task<bool> DeleteExpenseAsync(int id)
        {
            var expense = await _expenseRepository.GetByIdAsync(id);
            if (expense == null) return false;
            
            await _expenseRepository.DeleteAsync(expense);
            return true;
        }

        public async Task<IEnumerable<OperationalExpense>> GetExpensesByCategoryAsync(int categoryId)
        {
            return await _expenseRepository.GetByCategoryIdAsync(categoryId);
        }

        public async Task<IEnumerable<OperationalExpense>> GetExpensesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _expenseRepository.GetByDateRangeAsync(startDate, endDate);
        }

        public async Task<decimal> GetTotalExpensesAsync()
        {
            return await _expenseRepository.GetTotalExpensesAsync();
        }

        public async Task<decimal> GetTotalExpensesByCategoryAsync(int categoryId)
        {
            return await _expenseRepository.GetTotalAmountByCategoryAsync(categoryId);
        }

        public async Task<decimal> GetTotalExpensesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _expenseRepository.GetTotalAmountByDateRangeAsync(startDate, endDate);
        }

        public async Task<Dictionary<int, decimal>> GetExpenseBreakdownAsync(
            DateTime? startDate = null, 
            DateTime? endDate = null)
        {
            return await _expenseRepository.GetExpensesByCategoryAsync(startDate, endDate);
        }

        public async Task<decimal> GetMonthlyExpensesAsync(int year, int month)
        {
            return await _expenseRepository.GetMonthlyExpensesAsync(year, month);
        }

        public async Task<decimal> GetYearlyExpensesAsync(int year)
        {
            return await _expenseRepository.GetYearlyExpensesAsync(year);
        }

        public async Task<Dictionary<ExpenseCategory, decimal>> GetTopExpenseCategoriesAsync(int topCount = 5)
        {
            var breakdown = await GetExpenseBreakdownAsync();
            var categories = await _categoryRepository.GetAllAsync();
            
            var result = new Dictionary<ExpenseCategory, decimal>();
            
            foreach (var kvp in breakdown.OrderByDescending(x => x.Value).Take(topCount))
            {
                var category = categories.FirstOrDefault(c => c.ExpenseCategoryId == kvp.Key);
                if (category != null)
                {
                    result[category] = kvp.Value;
                }
            }
            
            return result;
        }

        public async Task<bool> ApproveExpenseAsync(int expenseId, int approvedBy)
        {
            var expense = await _expenseRepository.GetByIdAsync(expenseId);
            if (expense == null) return false;

            expense.IsApproved = true;
            expense.ApprovedBy = approvedBy;
            expense.ModifiedDate = DateTime.Now;

            await _expenseRepository.UpdateAsync(expense);
            return true;
        }

        public async Task<IEnumerable<OperationalExpense>> GetUnapprovedExpensesAsync()
        {
            return await _expenseRepository.GetUnapprovedExpensesAsync();
        }
    }
}

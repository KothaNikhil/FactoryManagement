using FactoryManagement.Data.Repositories;
using FactoryManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FactoryManagement.Services
{
    public interface IExpenseCategoryService
    {
        // CRUD Operations
        Task<ExpenseCategory> CreateCategoryAsync(ExpenseCategory category);
        Task<ExpenseCategory?> GetCategoryByIdAsync(int id);
        Task<IEnumerable<ExpenseCategory>> GetAllCategoriesAsync();
        Task<ExpenseCategory> UpdateCategoryAsync(ExpenseCategory category);
        Task<bool> DeleteCategoryAsync(int id);
        
        // Query Operations
        Task<ExpenseCategory?> GetCategoryByNameAsync(string categoryName);
        Task<bool> CategoryExistsAsync(string categoryName);
        Task<bool> CategoryExistsAsync(string categoryName, int excludeId);
        
        // Business Logic
        Task<bool> CanDeleteCategoryAsync(int categoryId);
    }

    public class ExpenseCategoryService : IExpenseCategoryService
    {
        private readonly IExpenseCategoryRepository _categoryRepository;
        private readonly IOperationalExpenseRepository _expenseRepository;

        public ExpenseCategoryService(
            IExpenseCategoryRepository categoryRepository,
            IOperationalExpenseRepository expenseRepository)
        {
            _categoryRepository = categoryRepository;
            _expenseRepository = expenseRepository;
        }

        public async Task<ExpenseCategory> CreateCategoryAsync(ExpenseCategory category)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(category.CategoryName))
                throw new ArgumentException("Category name is required.");

            if (await _categoryRepository.CategoryExistsAsync(category.CategoryName))
                throw new InvalidOperationException($"Category '{category.CategoryName}' already exists.");

            category.CreatedDate = DateTime.Now;

            return await _categoryRepository.AddAsync(category);
        }

        public async Task<ExpenseCategory?> GetCategoryByIdAsync(int id)
        {
            return await _categoryRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<ExpenseCategory>> GetAllCategoriesAsync()
        {
            return await _categoryRepository.GetAllAsync();
        }

        public async Task<ExpenseCategory> UpdateCategoryAsync(ExpenseCategory category)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(category.CategoryName))
                throw new ArgumentException("Category name is required.");

            if (await _categoryRepository.CategoryExistsAsync(category.CategoryName, category.ExpenseCategoryId))
                throw new InvalidOperationException($"Category '{category.CategoryName}' already exists.");

            category.ModifiedDate = DateTime.Now;
            await _categoryRepository.UpdateAsync(category);
            return category;
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) return false;

            // Soft delete: mark as deleted instead of removing from database
            // This preserves referential integrity with existing expenses
            category.IsDeleted = true;
            category.ModifiedDate = DateTime.Now;
            await _categoryRepository.UpdateAsync(category);
            return true;
        }

        public async Task<ExpenseCategory?> GetCategoryByNameAsync(string categoryName)
        {
            return await _categoryRepository.GetByCategoryNameAsync(categoryName);
        }

        public async Task<bool> CategoryExistsAsync(string categoryName)
        {
            return await _categoryRepository.CategoryExistsAsync(categoryName);
        }

        public async Task<bool> CategoryExistsAsync(string categoryName, int excludeId)
        {
            return await _categoryRepository.CategoryExistsAsync(categoryName, excludeId);
        }

        public async Task<bool> CanDeleteCategoryAsync(int categoryId)
        {
            var expenses = await _expenseRepository.GetByCategoryIdAsync(categoryId);
            return !expenses.Any();
        }
    }
}

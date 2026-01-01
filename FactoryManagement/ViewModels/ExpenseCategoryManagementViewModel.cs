using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FactoryManagement.Models;
using FactoryManagement.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace FactoryManagement.ViewModels
{
    /// <summary>
    /// ViewModel for managing expense categories (CRUD operations)
    /// </summary>
    public partial class ExpenseCategoryManagementViewModel : PaginationViewModel
    {
        private readonly IExpenseCategoryService _categoryService;

        [ObservableProperty]
        private ObservableCollection<ExpenseCategory> _allCategories = new();

        [ObservableProperty]
        private ObservableCollection<ExpenseCategory> _paginatedCategories = new();

        [ObservableProperty]
        private ExpenseCategory? _selectedCategory;

        [ObservableProperty]
        private string _categoryName = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private bool _isEditMode = false;

        [ObservableProperty]
        private int _editingCategoryId;

        [ObservableProperty]
        private string _searchText = string.Empty;

        public int CurrentUserId { get; set; } = 1; // Will be set from MainWindow

        public ExpenseCategoryManagementViewModel(IExpenseCategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task InitializeAsync()
        {
            await LoadCategoriesAsync();
        }

        [RelayCommand]
        private async Task LoadCategoriesAsync()
        {
            if (IsBusy) return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                
                // Apply filters
                var filtered = categories.AsEnumerable();

                if (!string.IsNullOrWhiteSpace(SearchText))
                    filtered = filtered.Where(c => 
                        c.CategoryName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                        (c.Description?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));

                AllCategories = new ObservableCollection<ExpenseCategory>(filtered);
                UpdatePaginatedData();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading categories: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        protected override void UpdatePaginatedData()
        {
            CalculatePagination(AllCategories, DefaultPageSize);
            PaginatedCategories.Clear();
            foreach (var category in GetPagedItems(AllCategories, DefaultPageSize))
            {
                PaginatedCategories.Add(category);
            }
        }

        [RelayCommand]
        private async Task SaveCategoryAsync()
        {
            if (IsBusy) return;

            // Validation
            if (string.IsNullOrWhiteSpace(CategoryName))
            {
                ErrorMessage = "Category name is required.";
                return;
            }

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                if (IsEditMode)
                {
                    // Update existing category
                    var category = await _categoryService.GetCategoryByIdAsync(EditingCategoryId);
                    if (category != null)
                    {
                        category.CategoryName = CategoryName;
                        category.Description = Description;

                        await _categoryService.UpdateCategoryAsync(category);
                    }
                }
                else
                {
                    // Create new category
                    var category = new ExpenseCategory
                    {
                        CategoryName = CategoryName,
                        Description = Description,
                        CreatedBy = CurrentUserId
                    };

                    await _categoryService.CreateCategoryAsync(category);
                }

                // Reset form and reload
                ClearForm();
                await LoadCategoriesAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error saving category: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void EditCategory(ExpenseCategory? category)
        {
            if (category == null) return;

            IsEditMode = true;
            EditingCategoryId = category.ExpenseCategoryId;
            CategoryName = category.CategoryName;
            Description = category.Description ?? string.Empty;
            SelectedCategory = category;
        }

        [RelayCommand]
        private async Task DeleteCategoryAsync(ExpenseCategory? category)
        {
            if (category == null || IsBusy) return;

            // Confirm deletion
            var result = System.Windows.MessageBox.Show(
                $"Are you sure you want to delete category '{category.CategoryName}'?\n\nNote: Categories with existing expenses cannot be deleted.",
                "Confirm Deletion",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

            if (result != System.Windows.MessageBoxResult.Yes) return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                await _categoryService.DeleteCategoryAsync(category.ExpenseCategoryId);
                await LoadCategoriesAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error deleting category: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void CancelEdit()
        {
            ClearForm();
        }

        [RelayCommand]
        private void NewCategory()
        {
            ClearForm();
        }

        private void ClearForm()
        {
            IsEditMode = false;
            EditingCategoryId = 0;
            CategoryName = string.Empty;
            Description = string.Empty;
            SelectedCategory = null;
        }

        partial void OnSearchTextChanged(string value)
        {
            _ = LoadCategoriesAsync();
        }
    }
}

using FactoryManagement.Models;
using FactoryManagement.Services;
using System;
using System.Windows;
using System.Windows.Input;

namespace FactoryManagement.Views
{
    public partial class QuickAddExpenseCategoryDialog : Window
    {
        private readonly IExpenseCategoryService _categoryService;
        public ExpenseCategory? NewCategory { get; private set; }

        public QuickAddExpenseCategoryDialog(IExpenseCategoryService categoryService)
        {
            InitializeComponent();
            _categoryService = categoryService;
            CategoryNameTextBox.Focus();
            this.PreviewKeyDown += QuickAddExpenseCategoryDialog_PreviewKeyDown;
        }

        private void QuickAddExpenseCategoryDialog_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                CancelButton_Click(null!, null!);
                e.Handled = true;
            }
        }

        private async void AddCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate
                if (string.IsNullOrWhiteSpace(CategoryNameTextBox.Text))
                {
                    ShowError("Category name is required");
                    return;
                }

                // Check if category already exists
                if (await _categoryService.CategoryExistsAsync(CategoryNameTextBox.Text.Trim()))
                {
                    ShowError("A category with this name already exists");
                    return;
                }

                // Create category
                var category = new ExpenseCategory
                {
                    CategoryName = CategoryNameTextBox.Text.Trim(),
                    Description = DescriptionTextBox.Text.Trim(),
                    CreatedBy = 1, // Default user - will be updated if user context is available
                    CreatedDate = DateTime.Now,
                    IsDeleted = false
                };

                // Save to database
                NewCategory = await _categoryService.CreateCategoryAsync(category);

                // Close dialog with success
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                ShowError($"Error adding category: {ex.Message}");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ShowError(string message)
        {
            ErrorTextBlock.Text = message;
            ErrorTextBlock.Visibility = Visibility.Visible;
        }
    }
}

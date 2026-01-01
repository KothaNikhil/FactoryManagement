using System.Windows.Controls;
using FactoryManagement.ViewModels;
using FactoryManagement.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace FactoryManagement.Views
{
    public partial class OperationalExpensesView : UserControl
    {
        public OperationalExpensesView()
        {
            InitializeComponent();
        }

        private async void QuickAddCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get services from DI
                var app = (App)Application.Current;
                var serviceProvider = app.GetType()
                    .GetField("_serviceProvider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                    .GetValue(app) as System.IServiceProvider;

                if (serviceProvider == null)
                    return;

                var categoryService = serviceProvider.GetRequiredService<IExpenseCategoryService>();

                // Open dialog
                var dialog = new QuickAddExpenseCategoryDialog(categoryService);
                var result = dialog.ShowDialog();

                if (result == true && dialog.NewCategory != null)
                {
                    // Refresh the categories list in the view model
                    if (DataContext is OperationalExpensesViewModel viewModel)
                    {
                        await viewModel.LoadDataCommand.ExecuteAsync(null);
                        
                        // Select the newly added category
                        viewModel.SelectedCategory = dialog.NewCategory;
                        viewModel.ErrorMessage = "✓ Category added successfully!";
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error adding category: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

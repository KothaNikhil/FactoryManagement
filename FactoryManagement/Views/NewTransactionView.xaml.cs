using System.Windows.Controls;
using FactoryManagement.ViewModels;
using FactoryManagement.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace FactoryManagement.Views
{
    public partial class NewTransactionView : UserControl
    {
        public NewTransactionView()
        {
            InitializeComponent();
        }

        private async void QuickAddPartyButton_Click(object sender, RoutedEventArgs e)
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

                var partyService = serviceProvider.GetRequiredService<IPartyService>();

                // Open dialog
                var dialog = new QuickAddPartyDialog(partyService);
                var result = dialog.ShowDialog();

                if (result == true && dialog.NewParty != null)
                {
                    // Refresh the parties list in the view model
                    if (DataContext is NewTransactionViewModel viewModel)
                    {
                        await viewModel.LoadDataCommand.ExecuteAsync(null);
                        
                        // Select the newly added party
                        viewModel.SelectedParty = dialog.NewParty;
                        viewModel.ErrorMessage = "✓ Party added successfully!";
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error adding party: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NumericTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                // Clear if the value is "0" or "0.0" etc
                if (decimal.TryParse(textBox.Text, out decimal value) && value == 0)
                {
                    textBox.Text = string.Empty;
                    textBox.SelectAll();
                }
                else
                {
                    textBox.SelectAll();
                }
            }
        }

        private void RecentTransactions_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is DataGrid dataGrid && dataGrid.SelectedItem != null)
            {
                if (DataContext is NewTransactionViewModel viewModel)
                {
                    viewModel.EditTransactionCommand.Execute(dataGrid.SelectedItem);
                }
            }
        }
    }
}


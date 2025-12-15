using System.Windows.Controls;
using FactoryManagement.ViewModels;
using FactoryManagement.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace FactoryManagement.Views
{
    public partial class TransactionEntryView : UserControl
    {
        public TransactionEntryView()
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
                    if (DataContext is TransactionEntryViewModel viewModel)
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
    }
}

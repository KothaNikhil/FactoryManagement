using FactoryManagement.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace FactoryManagement.Views
{
    /// <summary>
    /// Interaction logic for CashBookView.xaml
    /// </summary>
    public partial class CashBookView : UserControl
    {
        public CashBookView()
        {
            InitializeComponent();
            Loaded += CashBookView_Loaded;
        }

        private async void CashBookView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is CashBookViewModel viewModel)
            {
                await viewModel.InitializeAsync();
            }
        }
    }
}

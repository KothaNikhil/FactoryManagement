using FactoryManagement.ViewModels;
using System.Windows.Controls;

namespace FactoryManagement.Views
{
    /// <summary>
    /// Interaction logic for CashAccountsView.xaml
    /// </summary>
    public partial class CashAccountsView : UserControl
    {
        public CashAccountsView()
        {
            InitializeComponent();
            Loaded += async (s, e) =>
            {
                if (DataContext is CashAccountsViewModel viewModel)
                {
                    await viewModel.InitializeAsync();
                }
            };
        }
    }
}

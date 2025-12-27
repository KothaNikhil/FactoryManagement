using System.Windows.Controls;

namespace FactoryManagement.Views
{
    public partial class PayrollManagementView : UserControl
    {
        public PayrollManagementView()
        {
            InitializeComponent();
            Loaded += async (s, e) =>
            {
                if (DataContext is ViewModels.PayrollManagementViewModel viewModel)
                {
                    await viewModel.InitializeAsync();
                }
            };
        }
    }
}



using System.Windows.Controls;

namespace FactoryManagement.Views
{
    public partial class WagesManagementView : UserControl
    {
        public WagesManagementView()
        {
            InitializeComponent();
            Loaded += async (s, e) =>
            {
                if (DataContext is ViewModels.WagesManagementViewModel viewModel)
                {
                    await viewModel.InitializeAsync();
                }
            };
        }
    }
}

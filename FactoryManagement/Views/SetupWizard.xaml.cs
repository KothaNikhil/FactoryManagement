using System.Windows;
using FactoryManagement.ViewModels;

namespace FactoryManagement.Views
{
    public partial class SetupWizard : Window
    {
        public SetupWizard()
        {
            InitializeComponent();
        }

        public SetupWizard(SetupWizardViewModel viewModel) : this()
        {
            DataContext = viewModel;
            
            // Set up password callback so ViewModel can access PasswordBox values
            viewModel.GetPasswordsCallback = () => (PasswordBox.Password, PasswordConfirmBox.Password);
            
            // Set up window close callback
            viewModel.CloseWindowCallback = () =>
            {
                DialogResult = true;
                Close();
            };
        }
    }
}

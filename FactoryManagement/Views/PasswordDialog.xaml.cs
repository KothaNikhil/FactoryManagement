using System.Windows;
using System.Windows.Input;

namespace FactoryManagement.Views
{
    public partial class PasswordDialog : Window
    {
        public PasswordDialog()
        {
            InitializeComponent();
            Loaded += (s, e) => PasswordBox.Focus();
        }

        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var viewModel = DataContext as ViewModels.PasswordDialogViewModel;
                if (viewModel?.OkCommand.CanExecute(null) == true)
                {
                    viewModel.OkCommand.Execute(null);
                }
            }
        }

        public string Password => PasswordBox.Password;
        public string ConfirmPassword => ConfirmPasswordBox.Password;
    }
}

using FactoryManagement.ViewModels;
using System.Windows;

namespace FactoryManagement.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UserComboBox.Focus();
        }
    }
}

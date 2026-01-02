using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace FactoryManagement.ViewModels
{
    public partial class PasswordDialogViewModel : ViewModelBase
    {
        private readonly Window _window;

        [ObservableProperty]
        private string _title = "Password Required";

        [ObservableProperty]
        private string _message = "Enter your password";

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _isSetupMode;

        public bool IsConfirmed { get; private set; }

        public PasswordDialogViewModel(Window window, bool isSetupMode = false)
        {
            _window = window;
            _isSetupMode = isSetupMode;

            if (isSetupMode)
            {
                Title = "Set Admin Password";
                Message = "This is the first time setup. Please set a password for the Admin user.";
            }
        }

        [RelayCommand]
        private void Ok()
        {
            ErrorMessage = string.Empty;

            var passwordDialog = _window as Views.PasswordDialog;
            if (passwordDialog == null)
                return;

            var password = passwordDialog.Password;

            if (string.IsNullOrWhiteSpace(password))
            {
                ErrorMessage = "Password cannot be empty.";
                return;
            }

            if (IsSetupMode)
            {
                var confirmPassword = passwordDialog.ConfirmPassword;
                
                if (password != confirmPassword)
                {
                    ErrorMessage = "Passwords do not match. Please try again.";
                    return;
                }

                if (password.Length < 4)
                {
                    ErrorMessage = "Password must be at least 4 characters long.";
                    return;
                }
            }

            IsConfirmed = true;
            _window.DialogResult = true;
            _window.Close();
        }

        [RelayCommand]
        private void Cancel()
        {
            IsConfirmed = false;
            _window.DialogResult = false;
            _window.Close();
        }
    }
}

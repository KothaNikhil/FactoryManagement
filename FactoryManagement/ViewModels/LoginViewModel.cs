using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FactoryManagement.Models;
using FactoryManagement.Services;
using FactoryManagement.Helpers;
using FactoryManagement.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FactoryManagement.ViewModels
{
    public partial class LoginViewModel : ViewModelBase
    {
        private readonly IUserService _userService;
        private readonly Window _window;

        [ObservableProperty]
        private ObservableCollection<User> _activeUsers = new();

        [ObservableProperty]
        private User? _selectedUser;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _isLoading;

        public User? LoggedInUser { get; private set; }

        public LoginViewModel(IUserService userService, Window window)
        {
            _userService = userService;
            _window = window;
            _ = LoadActiveUsersAsync();
        }

        private async Task LoadActiveUsersAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var users = await _userService.GetActiveUsersAsync();
                
                ActiveUsers.Clear();
                foreach (var user in users.OrderBy(u => u.Username))
                {
                    ActiveUsers.Add(user);
                }

                if (ActiveUsers.Count == 0)
                {
                    ErrorMessage = "No active users found. Please contact the administrator.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading users: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task LoginAsync()
        {
            FileLogger.Log($"[LoginAsync] Started with SelectedUser: {SelectedUser?.Username} (Role: {SelectedUser?.Role})");
            ErrorMessage = string.Empty;

            if (SelectedUser == null)
            {
                ErrorMessage = "Please select a user to login.";
                return;
            }

            // Check if this is an admin user and requires password
            if (PasswordHelper.IsAdminRole(SelectedUser.Role))
            {
                FileLogger.Log($"[LoginAsync] Admin user selected: {SelectedUser.Username}");
                // Check if password is set for admin
                if (string.IsNullOrEmpty(SelectedUser.PasswordHash))
                {
                    FileLogger.Log($"[LoginAsync] Admin has no password, showing setup dialog");
                    // First time setup - ask to set password
                    var setupDialog = new PasswordDialog();
                    var setupViewModel = new PasswordDialogViewModel(setupDialog, isSetupMode: true);
                    setupDialog.DataContext = setupViewModel;

                    if (setupDialog.ShowDialog() == true && setupViewModel.IsConfirmed)
                    {
                        var password = setupDialog.Password;
                        SelectedUser.PasswordHash = PasswordHelper.HashPassword(password);
                        await _userService.UpdateUserAsync(SelectedUser);
                        
                        FileLogger.Log($"[LoginAsync] Admin password setup complete, logging in");
                        LoggedInUser = SelectedUser;
                        _window.DialogResult = true;
                        _window.Close();
                    }
                    return;
                }

                // Verify password
                FileLogger.Log($"[LoginAsync] Admin has password, showing verification dialog");
                var passwordDialog = new PasswordDialog();
                var passwordViewModel = new PasswordDialogViewModel(passwordDialog, isSetupMode: false);
                passwordViewModel.Message = $"Enter password for {SelectedUser.Username}";
                passwordDialog.DataContext = passwordViewModel;

                if (passwordDialog.ShowDialog() == true && passwordViewModel.IsConfirmed)
                {
                    var enteredPassword = passwordDialog.Password;
                    
                    if (!PasswordHelper.VerifyPassword(enteredPassword, SelectedUser.PasswordHash))
                    {
                        ErrorMessage = "Incorrect password. Please try again.";
                        FileLogger.Log($"[LoginAsync] Admin password incorrect");
                        return;
                    }

                    FileLogger.Log($"[LoginAsync] Admin password verified, logging in");
                    LoggedInUser = SelectedUser;
                    _window.DialogResult = true;
                    _window.Close();
                }
            }
            else
            {
                // Non-admin users don't need password
                FileLogger.Log($"[LoginAsync] Non-admin user selected: {SelectedUser.Username}, no password needed");
                LoggedInUser = SelectedUser;
                _window.DialogResult = true;
                _window.Close();
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            LoggedInUser = null;
            _window.DialogResult = false;
            _window.Close();
        }
    }
}

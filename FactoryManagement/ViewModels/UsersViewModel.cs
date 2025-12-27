using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FactoryManagement.Models;
using FactoryManagement.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FactoryManagement.ViewModels
{
    public partial class UsersViewModel : ViewModelBase
    {
        private readonly IUserService _userService;

        [ObservableProperty]
        private ObservableCollection<User> _users = new();

        [ObservableProperty]
        private User? _selectedUser;

        [ObservableProperty]
        private bool _isEditing;

        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _role = string.Empty;

        [ObservableProperty]
        private bool _isActive = true;

        [ObservableProperty]
        private string _searchText = string.Empty;

        private int? _editingUserId;
        
        public Func<Task>? UserListChangedCallback { get; set; }

        public UsersViewModel(IUserService userService)
        {
            _userService = userService;
        }

        public async Task InitializeAsync()
        {
            await LoadUsersAsync();
        }

        [RelayCommand]
        private async Task LoadUsersAsync()
        {
            try
            {
                IsBusy = true;
                var users = await _userService.GetAllUsersAsync();
                
                Users.Clear();
                foreach (var user in users.OrderBy(u => u.Username))
                {
                    Users.Add(user);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading users: {ex.Message}";
                MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void NewUser()
        {
            ClearForm();
            IsEditing = false;
            _editingUserId = null;
        }

        [RelayCommand]
        private void EditUser(User? user)
        {
            if (user == null) return;

            _editingUserId = user.UserId;
            Username = user.Username;
            Role = user.Role;
            IsActive = user.IsActive;
            IsEditing = true;
        }

        [RelayCommand]
        private async Task SaveUserAsync()
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(Username))
                {
                    MessageBox.Show("Username is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(Role))
                {
                    MessageBox.Show("Role is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Check for duplicate username
                var usernameExists = await _userService.UsernameExistsAsync(Username, _editingUserId);
                if (usernameExists)
                {
                    MessageBox.Show("This username already exists. Please choose a different username.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                IsBusy = true;

                if (IsEditing && _editingUserId.HasValue)
                {
                    // Update existing user
                    var user = await _userService.GetUserByIdAsync(_editingUserId.Value);
                    if (user != null)
                    {
                        user.Username = Username.Trim();
                        user.Role = Role.Trim();
                        user.IsActive = IsActive;
                        await _userService.UpdateUserAsync(user);
                        MessageBox.Show("User updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    // Create new user
                    var user = new User
                    {
                        Username = Username.Trim(),
                        Role = Role.Trim(),
                        IsActive = IsActive
                    };
                    await _userService.CreateUserAsync(user);
                    MessageBox.Show("User created successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                await LoadUsersAsync();
                ClearForm();
                
                // Notify that users have changed
                if (UserListChangedCallback != null)
                {
                    await UserListChangedCallback.Invoke();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error saving user: {ex.Message}";
                MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task DeleteUserAsync(User? user)
        {
            if (user == null) return;

            // Prevent deletion of Guest user
            if (user.Username.Equals("Guest", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(
                    "The Guest user cannot be deleted as it is a system default user.",
                    "Cannot Delete",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete user '{user.Username}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                IsBusy = true;
                await _userService.DeleteUserAsync(user.UserId);
                MessageBox.Show("User deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadUsersAsync();
                
                if (_editingUserId == user.UserId)
                {
                    ClearForm();
                }
                
                // Notify that users have changed
                if (UserListChangedCallback != null)
                {
                    await UserListChangedCallback.Invoke();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error deleting user: {ex.Message}";
                MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void CancelEdit()
        {
            ClearForm();
        }

        private void ClearForm()
        {
            Username = string.Empty;
            Role = string.Empty;
            IsActive = true;
            IsEditing = false;
            _editingUserId = null;
            ErrorMessage = string.Empty;
        }

        partial void OnSearchTextChanged(string value)
        {
            // Filter logic can be implemented here if needed
        }
    }
}

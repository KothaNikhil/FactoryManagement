using FactoryManagement.Data.Repositories;
using FactoryManagement.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FactoryManagement.Services
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<IEnumerable<User>> GetActiveUsersAsync();
        Task<User?> GetUserByIdAsync(int userId);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User> CreateUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(int userId);
        Task<bool> UsernameExistsAsync(string username, int? excludeUserId = null);
    }

    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllAsync();
        }

        public async Task<IEnumerable<User>> GetActiveUsersAsync()
        {
            return await _userRepository.GetActiveUsersAsync();
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _userRepository.GetByIdAsync(userId);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _userRepository.GetByUsernameAsync(username);
        }

        public async Task<User> CreateUserAsync(User user)
        {
            user.CreatedDate = DateTime.Now;
            user.ModifiedDate = null;
            return await _userRepository.AddAsync(user);
        }

        public async Task UpdateUserAsync(User user)
        {
            user.ModifiedDate = DateTime.Now;
            
            // Prevent deactivating the Admin user
            var existing = await _userRepository.GetByIdAsync(user.UserId);
            if (existing != null)
            {
                // Check if trying to deactivate an Admin user
                if (existing.IsActive && !user.IsActive && 
                    (existing.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase) || 
                     existing.Role.Equals("Administrator", StringComparison.OrdinalIgnoreCase)))
                {
                    throw new InvalidOperationException("The Admin user cannot be deactivated. The system must have at least one active Admin user to function properly.");
                }
                
                // Prevent deactivating the last active user
                if (existing.IsActive && !user.IsActive)
                {
                    var activeCount = await _userRepository.CountAsync(u => u.IsActive && u.UserId != user.UserId);
                    if (activeCount == 0)
                    {
                        throw new InvalidOperationException("At least one active user must remain in the system.");
                    }
                }
            }

            await _userRepository.UpdateAsync(user);
        }

        public async Task DeleteUserAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null)
            {
                // CRITICAL: Prevent deletion of Admin user (system cannot function without an admin)
                if (user.Role != null && (user.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase) || user.Role.Equals("Administrator", StringComparison.OrdinalIgnoreCase)))
                {
                    throw new InvalidOperationException("The Admin user cannot be deleted. The system must have at least one Admin user to function properly.");
                }

                // Do not allow deleting (deactivating) the last active user
                if (user.IsActive)
                {
                    var activeCount = await _userRepository.CountAsync(u => u.IsActive);
                    if (activeCount <= 1)
                    {
                        throw new InvalidOperationException("Cannot delete the last active user. At least one active user must remain.");
                    }
                }

                // Soft delete: keep user row so related transactions remain
                user.IsActive = false;
                user.ModifiedDate = DateTime.Now;
                await _userRepository.UpdateAsync(user);
            }
        }

        public async Task<bool> UsernameExistsAsync(string username, int? excludeUserId = null)
        {
            var existingUser = await _userRepository.GetByUsernameAsync(username);
            if (existingUser == null)
                return false;

            if (excludeUserId.HasValue && existingUser.UserId == excludeUserId.Value)
                return false;

            return true;
        }
    }
}

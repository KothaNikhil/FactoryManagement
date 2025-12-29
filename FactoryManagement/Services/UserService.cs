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
            await _userRepository.UpdateAsync(user);
        }

        public async Task DeleteUserAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null)
            {
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

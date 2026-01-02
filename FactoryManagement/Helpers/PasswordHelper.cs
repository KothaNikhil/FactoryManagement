using System;
using System.Security.Cryptography;
using System.Text;

namespace FactoryManagement.Helpers
{
    public static class PasswordHelper
    {
        /// <summary>
        /// Computes SHA256 hash of the password
        /// </summary>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return string.Empty;

            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Verifies if the provided password matches the stored hash
        /// </summary>
        public static bool VerifyPassword(string password, string? storedHash)
        {
            if (string.IsNullOrEmpty(storedHash))
                return false;

            var hashOfInput = HashPassword(password);
            return hashOfInput.Equals(storedHash, StringComparison.Ordinal);
        }

        /// <summary>
        /// Checks if a user is an admin based on their role
        /// </summary>
        public static bool IsAdminRole(string? role)
        {
            if (string.IsNullOrEmpty(role))
                return false;

            return role.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                   role.Equals("Administrator", StringComparison.OrdinalIgnoreCase);
        }
    }
}

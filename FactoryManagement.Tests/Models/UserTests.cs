using FactoryManagement.Models;
using System;
using Xunit;

namespace FactoryManagement.Tests.Models
{
    public class UserTests
    {
        [Fact]
        public void User_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var user = new User();

            // Assert
            Assert.Equal(0, user.UserId);
            Assert.Equal(string.Empty, user.Username);
            Assert.Equal(string.Empty, user.Role);
            Assert.True(user.IsActive);
            Assert.True((DateTime.Now - user.CreatedDate).TotalSeconds < 1);
            Assert.Null(user.ModifiedDate);
        }

        [Theory]
        [InlineData("Admin")]
        [InlineData("Manager")]
        [InlineData("Operator")]
        public void User_ShouldAcceptDifferentRoles(string role)
        {
            // Arrange & Act
            var user = new User { Role = role };

            // Assert
            Assert.Equal(role, user.Role);
        }
    }
}

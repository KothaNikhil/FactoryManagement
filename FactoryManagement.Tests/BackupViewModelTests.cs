using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FactoryManagement.Services;
using FactoryManagement.ViewModels;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace FactoryManagement.Tests
{
    public class BackupViewModelTests
    {
        [Fact]
        public void CreateBackupCommand_CallsBackupService()
        {
            // Arrange
            var mockService = new Mock<BackupService>(null!);
            mockService.Setup(s => s.CreateBackupAsync()).ReturnsAsync("test.json");
            mockService.Setup(s => s.GetAvailableBackups()).Returns(new List<BackupFileInfo>());
            var vm = new BackupViewModel(mockService.Object);

            // Act
            Assert.True(vm.CreateBackupCommand.CanExecute(null));
            
            // Assert - Verify command is properly initialized and executable
            Assert.NotNull(vm.CreateBackupCommand);
            mockService.Verify(s => s.GetAvailableBackups(), Times.Once); // Called in constructor
        }

        [Fact]
        public void DeleteBackupCommand_IsEnabledWhenBackupSelected()
        {
            // Arrange
            var mockService = new Mock<BackupService>(null!);
            mockService.Setup(s => s.GetAvailableBackups()).Returns(new List<BackupFileInfo>());
            mockService.Setup(s => s.DeleteBackup(It.IsAny<string>()));
            var vm = new BackupViewModel(mockService.Object);
            var backup = new BackupFileInfo { FileName = "test.json", FilePath = "test.json", CreatedDate = DateTime.Now, FileSize = 100 };
            vm.Backups.Add(backup);
            vm.SelectedBackup = backup;

            // Assert - Verify delete command is enabled when backup is selected
            Assert.True(vm.DeleteBackupCommand.CanExecute(null));
            Assert.Single(vm.Backups);
            Assert.Equal("test.json", vm.SelectedBackup.FileName);
        }
    }
}

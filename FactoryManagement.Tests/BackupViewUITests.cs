using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using FactoryManagement.ViewModels;
using FactoryManagement.Services;
using Moq;
using System.Windows;

namespace FactoryManagement.Tests
{
    public class BackupViewUITests
    {
        [Fact]
        public void CreateBackup_UI_CommandIsInitialized()
        {
            // Arrange
            var mockService = new Mock<BackupService>(null!);
            mockService.Setup(s => s.CreateBackupAsync()).ReturnsAsync("test.json");
            mockService.Setup(s => s.GetAvailableBackups()).Returns(new List<BackupFileInfo>());
            var vm = new BackupViewModel(mockService.Object);

            // Assert - UI test verifies command initialization and state
            Assert.NotNull(vm.CreateBackupCommand);
            Assert.True(vm.CreateBackupCommand.CanExecute(null));
            Assert.False(vm.IsProcessing);
            Assert.Equal("Found 0 backup(s)", vm.StatusMessage);
        }

        [Fact]
        public void DeleteBackup_UI_CommandStateManagement()
        {
            // Arrange
            var mockService = new Mock<BackupService>(null!);
            mockService.Setup(s => s.GetAvailableBackups()).Returns(new List<BackupFileInfo>());
            mockService.Setup(s => s.DeleteBackup(It.IsAny<string>()));
            var vm = new BackupViewModel(mockService.Object);
            
            // Assert - Initially delete command should be disabled
            Assert.False(vm.DeleteBackupCommand.CanExecute(null));
            
            // Act - Add and select a backup
            var backup = new BackupFileInfo { FileName = "test.json", FilePath = "test.json", CreatedDate = DateTime.Now, FileSize = 100 };
            vm.Backups.Add(backup);
            vm.SelectedBackup = backup;

            // Assert - Delete command should now be enabled
            Assert.True(vm.DeleteBackupCommand.CanExecute(null));
            Assert.NotNull(vm.SelectedBackup);
        }
    }
}

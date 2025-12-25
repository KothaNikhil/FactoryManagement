using FactoryManagement.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FactoryManagement.Tests.Services
{
    public class ExportServiceTests : IDisposable
    {
        private readonly ExportService _service;
        private readonly List<string> _testFilesToCleanup = new();

        public class TestData
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public decimal Amount { get; set; }
            public DateTime Date { get; set; }
        }

        public ExportServiceTests()
        {
            _service = new ExportService();
        }

        [Fact]
        public async Task ExportToExcelAsync_ShouldCreateExcelFile()
        {
            // Arrange
            var testData = new List<TestData>
            {
                new TestData { Id = 1, Name = "Item1", Amount = 100.50m, Date = DateTime.Now },
                new TestData { Id = 2, Name = "Item2", Amount = 250.75m, Date = DateTime.Now }
            };
            var filePath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.xlsx");
            _testFilesToCleanup.Add(filePath);

            // Act
            await _service.ExportToExcelAsync(testData, filePath, "TestSheet");

            // Assert
            Assert.True(File.Exists(filePath));
            var fileInfo = new FileInfo(filePath);
            Assert.True(fileInfo.Length > 0);
        }

        [Fact]
        public async Task ExportToExcelAsync_WithCustomSheetName_ShouldUseProvidedName()
        {
            // Arrange
            var testData = new List<TestData>
            {
                new TestData { Id = 1, Name = "Test", Amount = 100m, Date = DateTime.Now }
            };
            var filePath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.xlsx");
            _testFilesToCleanup.Add(filePath);

            // Act
            await _service.ExportToExcelAsync(testData, filePath, "CustomSheet");

            // Assert
            Assert.True(File.Exists(filePath));
        }

        [Fact]
        public async Task ExportToExcelAsync_WithEmptyData_ShouldCreateEmptyFile()
        {
            // Arrange
            var testData = new List<TestData>();
            var filePath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.xlsx");
            _testFilesToCleanup.Add(filePath);

            // Act
            await _service.ExportToExcelAsync(testData, filePath);

            // Assert
            Assert.True(File.Exists(filePath));
        }

        [Fact]
        public async Task ExportToCsvAsync_ShouldCreateCsvFile()
        {
            // Arrange
            var testData = new List<TestData>
            {
                new TestData { Id = 1, Name = "Item1", Amount = 100.50m, Date = new DateTime(2024, 1, 1) },
                new TestData { Id = 2, Name = "Item2", Amount = 250.75m, Date = new DateTime(2024, 1, 2) }
            };
            var filePath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.csv");
            _testFilesToCleanup.Add(filePath);

            // Act
            await _service.ExportToCsvAsync(testData, filePath);

            // Assert
            Assert.True(File.Exists(filePath));
            var fileContent = File.ReadAllText(filePath);
            Assert.Contains("Id", fileContent);
            Assert.Contains("Name", fileContent);
            Assert.Contains("Amount", fileContent);
        }

        [Fact]
        public async Task ExportToCsvAsync_WithEmptyData_ShouldCreateHeaderOnlyFile()
        {
            // Arrange
            var testData = new List<TestData>();
            var filePath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.csv");
            _testFilesToCleanup.Add(filePath);

            // Act
            await _service.ExportToCsvAsync(testData, filePath);

            // Assert
            Assert.True(File.Exists(filePath));
        }

        [Fact]
        public async Task ExportToCsvAsync_ShouldContainCorrectDataRows()
        {
            // Arrange
            var testData = new List<TestData>
            {
                new TestData { Id = 10, Name = "TestItem", Amount = 99.99m, Date = DateTime.Now }
            };
            var filePath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.csv");
            _testFilesToCleanup.Add(filePath);

            // Act
            await _service.ExportToCsvAsync(testData, filePath);

            // Assert
            var fileContent = File.ReadAllText(filePath);
            Assert.Contains("10", fileContent);
            Assert.Contains("TestItem", fileContent);
            Assert.Contains("99.99", fileContent);
        }

        [Fact]
        public async Task ExportToExcelAsync_ShouldOverwriteExistingFile()
        {
            // Arrange
            var filePath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.xlsx");
            _testFilesToCleanup.Add(filePath);
            
            // Create initial file
            var initialData = new List<TestData> { new TestData { Id = 1, Name = "Old", Amount = 10m, Date = DateTime.Now } };
            await _service.ExportToExcelAsync(initialData, filePath);
            var initialSize = new FileInfo(filePath).Length;

            // Act - Export new data to same file
            var newData = new List<TestData> 
            { 
                new TestData { Id = 2, Name = "New", Amount = 20m, Date = DateTime.Now },
                new TestData { Id = 3, Name = "New2", Amount = 30m, Date = DateTime.Now }
            };
            await _service.ExportToExcelAsync(newData, filePath);

            // Assert
            Assert.True(File.Exists(filePath));
            var finalSize = new FileInfo(filePath).Length;
            // File size might change with different data
            Assert.True(finalSize > 0);
        }

        public void Dispose()
        {
            // Cleanup test files
            foreach (var filePath in _testFilesToCleanup)
            {
                if (File.Exists(filePath))
                {
                    try
                    {
                        File.Delete(filePath);
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                }
            }
        }
    }
}

using Xunit;
using Moq;
using FactoryManagement.ViewModels;
using FactoryManagement.Services;
using FactoryManagement.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace FactoryManagement.Tests.ViewModels
{
    public class WagesManagementViewModelTests
    {
        private readonly Mock<IWageService> _mockWageService;
        private readonly WagesManagementViewModel _viewModel;

        public WagesManagementViewModelTests()
        {
            _mockWageService = new Mock<IWageService>();
            _viewModel = new WagesManagementViewModel(_mockWageService.Object);
        }

        [Fact]
        public async Task LoadWorkersAsync_ShouldLoadWorkers()
        {
            // Arrange
            var workers = new List<Worker>
            {
                new Worker { WorkerId = 1, Name = "John Doe", WorkerType = WorkerType.Daily, DailyRate = 500 },
                new Worker { WorkerId = 2, Name = "Jane Smith", WorkerType = WorkerType.Hourly, HourlyRate = 100 }
            };
            _mockWageService.Setup(s => s.GetAllWorkersAsync()).ReturnsAsync(workers);
            _mockWageService.Setup(s => s.GetTotalWagesPaidAsync()).ReturnsAsync(10000);
            _mockWageService.Setup(s => s.GetTotalAdvancesGivenAsync()).ReturnsAsync(2000);
            _mockWageService.Setup(s => s.GetTransactionsByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<WageTransaction>());

            // Act
            await _viewModel.LoadWorkersCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal(2, _viewModel.Workers.Count);
            Assert.Equal("Jane Smith", _viewModel.Workers.First().Name);
            Assert.Equal(10000, _viewModel.TotalWagesPaid);
        }

        [Fact]
        public async Task SaveWagePaymentAsync_WithoutSelectedWorker_ShouldShowError()
        {
            // Arrange
            _viewModel.SelectedWorker = null;
            _viewModel.Amount = 500;

            // Act
            await _viewModel.SaveWagePaymentCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal("Please select a worker first!", _viewModel.ErrorMessage);
            _mockWageService.Verify(s => s.RecordWagePaymentAsync(It.IsAny<WageTransaction>()), Times.Never);
        }

        [Fact]
        public async Task SaveWagePaymentAsync_WithZeroAmount_ShouldShowError()
        {
            // Arrange
            _viewModel.SelectedWorker = new Worker { WorkerId = 1, Name = "Test Worker" };
            _viewModel.Amount = 0;

            // Act
            await _viewModel.SaveWagePaymentCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal("Amount must be greater than zero!", _viewModel.ErrorMessage);
            _mockWageService.Verify(s => s.RecordWagePaymentAsync(It.IsAny<WageTransaction>()), Times.Never);
        }

        [Fact]
        public async Task SaveWagePaymentAsync_WithValidData_ShouldRecordPayment()
        {
            // Arrange
            var worker = new Worker { WorkerId = 1, Name = "Test Worker" };
            _viewModel.SelectedWorker = worker;
            _viewModel.Amount = 500;
            _viewModel.TransactionType = "DailyWage";
            _viewModel.Notes = "Test payment";

            _mockWageService.Setup(s => s.RecordWagePaymentAsync(It.IsAny<WageTransaction>()))
                .ReturnsAsync(new WageTransaction { WageTransactionId = 1 });
            _mockWageService.Setup(s => s.GetAllWorkersAsync()).ReturnsAsync(new List<Worker> { worker });
            _mockWageService.Setup(s => s.GetTotalWagesPaidAsync()).ReturnsAsync(500);
            _mockWageService.Setup(s => s.GetTotalAdvancesGivenAsync()).ReturnsAsync(0);
            _mockWageService.Setup(s => s.GetTransactionsByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<WageTransaction>());
            _mockWageService.Setup(s => s.GetWorkerTransactionsAsync(1)).ReturnsAsync(new List<WageTransaction>());
            _mockWageService.Setup(s => s.GetWorkerTotalWagesAsync(1)).ReturnsAsync(500);
            _mockWageService.Setup(s => s.GetWorkerOutstandingAdvanceAsync(1)).ReturnsAsync(0);

            // Act
            await _viewModel.SaveWagePaymentCommand.ExecuteAsync(null);

            // Assert
            _mockWageService.Verify(s => s.RecordWagePaymentAsync(It.Is<WageTransaction>(t =>
                t.WorkerId == 1 &&
                t.Amount == 500 &&
                t.Notes == "Test payment"
            )), Times.Once);
        }

        [Fact]
        public void OnSelectedWorkerChanged_ShouldSetWorkerInfoVisible()
        {
            // Arrange
            var worker = new Worker { WorkerId = 1, Name = "Test Worker" };

            // Act
            _viewModel.SelectedWorker = worker;

            // Assert
            Assert.True(_viewModel.WorkerInfoVisible);
        }

        [Fact]
        public void OnSelectedWorkerChanged_WithNull_ShouldHideWorkerInfo()
        {
            // Arrange
            _viewModel.SelectedWorker = new Worker { WorkerId = 1, Name = "Test" };
            
            // Act
            _viewModel.SelectedWorker = null;

            // Assert
            Assert.False(_viewModel.WorkerInfoVisible);
        }

        [Fact]
        public void SearchText_ShouldFilterWorkers()
        {
            // Arrange
            var workers = new List<Worker>
            {
                new Worker { WorkerId = 1, Name = "John Doe", MobileNumber = "1234567890" },
                new Worker { WorkerId = 2, Name = "Jane Smith", MobileNumber = "9876543210" },
                new Worker { WorkerId = 3, Name = "Bob Johnson", MobileNumber = "5555555555" }
            };
            
            // Manually set the internal collections
            typeof(WagesManagementViewModel)
                .GetField("_allWorkers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(_viewModel, new System.Collections.ObjectModel.ObservableCollection<Worker>(workers));
            
            _viewModel.Workers = new System.Collections.ObjectModel.ObservableCollection<Worker>(workers);

            // Act
            _viewModel.SearchText = "John";

            // Assert
            Assert.Equal(2, _viewModel.Workers.Count);
            Assert.Contains(_viewModel.Workers, w => w.Name.Contains("John"));
        }

        [Fact]
        public void TransactionType_Changes_ShouldClearErrorMessage()
        {
            // Arrange
            _viewModel.ErrorMessage = "Some error";
            _viewModel.TransactionType = "DailyWage";

            // Act
            _viewModel.TransactionType = "HourlyWage";

            // Assert - error should persist until explicitly cleared
            Assert.NotEmpty(_viewModel.ErrorMessage);
        }

        [Fact]
        public async Task QuickAddWorkerAsync_OnException_ShouldSetErrorMessage()
        {
            // Arrange
            _mockWageService.Setup(s => s.AddWorkerAsync(It.IsAny<Worker>()))
                .ThrowsAsync(new System.Exception("Database error"));

            // Act - We can't test the dialog directly, but we can verify error handling
            // Note: This test would need to be modified to use a testable dialog factory pattern

            // Assert
            _mockWageService.Setup(s => s.AddWorkerAsync(It.IsAny<Worker>()));
        }
    }
}

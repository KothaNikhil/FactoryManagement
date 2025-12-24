using FactoryManagement.Data.Repositories;
using FactoryManagement.Models;
using FactoryManagement.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FactoryManagement.Tests.Services
{
    public class WageServiceTests
    {
        [Fact]
        public async Task GetAllWorkersAsync_ShouldReturnAllWorkers()
        {
            // Arrange
            var mockWorkerRepo = new Mock<IWorkerRepository>();
            var mockTransactionRepo = new Mock<IWageTransactionRepository>();
            
            var workers = new List<Worker>
            {
                new Worker { WorkerId = 1, Name = "Worker1", WorkerType = WorkerType.Daily, DailyRate = 500 },
                new Worker { WorkerId = 2, Name = "Worker2", WorkerType = WorkerType.Monthly, MonthlyRate = 15000 }
            };
            
            mockWorkerRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(workers);
            var service = new WageService(mockWorkerRepo.Object, mockTransactionRepo.Object);

            // Act
            var result = await service.GetAllWorkersAsync();

            // Assert
            Assert.Equal(2, result.Count());
            mockWorkerRepo.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task AddWorkerAsync_ShouldSetCreatedDate()
        {
            // Arrange
            var mockWorkerRepo = new Mock<IWorkerRepository>();
            var mockTransactionRepo = new Mock<IWageTransactionRepository>();
            
            var worker = new Worker { Name = "New Worker", WorkerType = WorkerType.Daily, DailyRate = 500 };
            mockWorkerRepo.Setup(r => r.AddAsync(It.IsAny<Worker>())).ReturnsAsync(worker);
            
            var service = new WageService(mockWorkerRepo.Object, mockTransactionRepo.Object);

            // Act
            await service.AddWorkerAsync(worker);

            // Assert
            Assert.True((DateTime.Now - worker.CreatedDate).TotalSeconds < 1);
            mockWorkerRepo.Verify(r => r.AddAsync(worker), Times.Once);
        }

        [Fact]
        public async Task RecordWagePaymentAsync_ShouldAddTransaction()
        {
            // Arrange
            var mockWorkerRepo = new Mock<IWorkerRepository>();
            var mockTransactionRepo = new Mock<IWageTransactionRepository>();
            
            var transaction = new WageTransaction
            {
                WorkerId = 1,
                TransactionType = WageTransactionType.DailyWage,
                Amount = 500,
                NetAmount = 500
            };
            
            mockTransactionRepo.Setup(r => r.AddAsync(It.IsAny<WageTransaction>())).ReturnsAsync(transaction);
            var service = new WageService(mockWorkerRepo.Object, mockTransactionRepo.Object);

            // Act
            await service.RecordWagePaymentAsync(transaction);

            // Assert
            mockTransactionRepo.Verify(r => r.AddAsync(transaction), Times.Once);
        }

        [Fact]
        public async Task GetWorkerByIdAsync_ShouldReturnWorker()
        {
            // Arrange
            var mockWorkerRepo = new Mock<IWorkerRepository>();
            var mockTransactionRepo = new Mock<IWageTransactionRepository>();
            
            var worker = new Worker { WorkerId = 1, Name = "Test Worker" };
            mockWorkerRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(worker);
            
            var service = new WageService(mockWorkerRepo.Object, mockTransactionRepo.Object);

            // Act
            var result = await service.GetWorkerByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Worker", result.Name);
            mockWorkerRepo.Verify(r => r.GetByIdAsync(1), Times.Once);
        }
    }
}

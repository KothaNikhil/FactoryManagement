using FactoryManagement.Models;
using System;
using Xunit;

namespace FactoryManagement.Tests.Models
{
    public class WorkerTests
    {
        [Fact]
        public void Worker_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var worker = new Worker();

            // Assert
            Assert.Equal(0, worker.WorkerId);
            Assert.Equal(string.Empty, worker.Name);
            Assert.Equal(string.Empty, worker.MobileNumber);
            Assert.Equal(string.Empty, worker.Address);
            Assert.Equal(WorkerType.Daily, worker.WorkerType);
            Assert.Equal(WorkerStatus.Active, worker.Status);
            Assert.Equal(0, worker.DailyRate);
            Assert.Equal(0, worker.HourlyRate);
            Assert.Equal(0, worker.MonthlyRate);
        }

        [Theory]
        [InlineData(WorkerType.Daily)]
        [InlineData(WorkerType.Hourly)]
        [InlineData(WorkerType.Monthly)]
        [InlineData(WorkerType.Contractor)]
        public void Worker_AllWorkerTypes_ShouldBeValid(WorkerType workerType)
        {
            // Arrange & Act
            var worker = new Worker { WorkerType = workerType };

            // Assert
            Assert.Equal(workerType, worker.WorkerType);
        }

        [Theory]
        [InlineData(WorkerStatus.Active)]
        [InlineData(WorkerStatus.Inactive)]
        [InlineData(WorkerStatus.OnLeave)]
        [InlineData(WorkerStatus.Terminated)]
        public void Worker_AllStatuses_ShouldBeValid(WorkerStatus status)
        {
            // Arrange & Act
            var worker = new Worker { Status = status };

            // Assert
            Assert.Equal(status, worker.Status);
        }

        [Fact]
        public void Worker_Properties_ShouldRetainValues()
        {
            // Arrange
            var worker = new Worker
            {
                Name = "John Doe",
                MobileNumber = "9876543210",
                Address = "123 Main St",
                WorkerType = WorkerType.Daily,
                DailyRate = 500m
            };

            // Assert
            Assert.Equal("John Doe", worker.Name);
            Assert.Equal("9876543210", worker.MobileNumber);
            Assert.Equal("123 Main St", worker.Address);
            Assert.Equal(WorkerType.Daily, worker.WorkerType);
            Assert.Equal(500m, worker.DailyRate);
        }
    }
}

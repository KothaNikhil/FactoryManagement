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
            Assert.Equal(WorkerStatus.Active, worker.Status);
            Assert.Equal(0, worker.Rate);
            Assert.Equal(0, worker.DailyRate);
            Assert.Equal(0, worker.HourlyRate);
            Assert.Equal(0, worker.MonthlyRate);
            Assert.Equal(0, worker.TotalAdvance);
            Assert.Equal(0, worker.TotalWagesPaid);
        }

        [Fact]
        public void Worker_SetRate_ShouldStoreCorrectly()
        {
            // Arrange
            var worker = new Worker();
            decimal expectedRate = 500.50m;

            // Act
            worker.Rate = expectedRate;

            // Assert
            Assert.Equal(expectedRate, worker.Rate);
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
                Rate = 500m,
                DailyRate = 500m
            };

            // Assert
            Assert.Equal("John Doe", worker.Name);
            Assert.Equal("9876543210", worker.MobileNumber);
            Assert.Equal("123 Main St", worker.Address);
            Assert.Equal(500m, worker.Rate);
            Assert.Equal(500m, worker.DailyRate);
        }
    }
}

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
    public class PartyServiceTests
    {
        [Fact]
        public async Task GetAllPartiesAsync_ShouldReturnAllParties()
        {
            // Arrange
            var mockRepo = new Mock<IRepository<Party>>();
            var parties = new List<Party>
            {
                new Party { PartyId = 1, Name = "Party1", PartyType = PartyType.Buyer },
                new Party { PartyId = 2, Name = "Party2", PartyType = PartyType.Seller }
            };
            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(parties);
            var service = new PartyService(mockRepo.Object);

            // Act
            var result = await service.GetAllPartiesAsync();

            // Assert
            Assert.Equal(2, result.Count());
            mockRepo.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task AddPartyAsync_ShouldSetCreatedDate()
        {
            // Arrange
            var mockRepo = new Mock<IRepository<Party>>();
            var party = new Party { Name = "New Party", PartyType = PartyType.Buyer };
            mockRepo.Setup(r => r.AddAsync(It.IsAny<Party>())).ReturnsAsync(party);
            var service = new PartyService(mockRepo.Object);

            // Act
            await service.AddPartyAsync(party);

            // Assert
            Assert.True((DateTime.Now - party.CreatedDate).TotalSeconds < 1);
            mockRepo.Verify(r => r.AddAsync(party), Times.Once);
        }
    }
}

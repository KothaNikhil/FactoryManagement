using FactoryManagement.Models;
using System;
using Xunit;

namespace FactoryManagement.Tests.Models
{
    public class PartyTests
    {
        [Fact]
        public void Party_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var party = new Party();

            // Assert
            Assert.Equal(0, party.PartyId);
            Assert.Equal(string.Empty, party.Name);
            Assert.Equal(string.Empty, party.MobileNumber);
            Assert.Equal(string.Empty, party.Place);
            Assert.Equal(PartyType.Buyer, party.PartyType);
            Assert.True((DateTime.Now - party.CreatedDate).TotalSeconds < 1);
            Assert.Null(party.ModifiedDate);
        }

        [Theory]
        [InlineData(PartyType.Buyer)]
        [InlineData(PartyType.Seller)]
        [InlineData(PartyType.Both)]
        [InlineData(PartyType.Lender)]
        [InlineData(PartyType.Borrower)]
        [InlineData(PartyType.Financial)]
        public void Party_AllPartyTypes_ShouldBeValid(PartyType partyType)
        {
            // Arrange & Act
            var party = new Party { PartyType = partyType };

            // Assert
            Assert.Equal(partyType, party.PartyType);
        }
    }
}

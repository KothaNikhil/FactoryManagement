using FactoryManagement.Data;
using FactoryManagement.Data.Repositories;
using FactoryManagement.Models;
using FactoryManagement.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FactoryManagement.Tests.Services
{
    public class FinancialTransactionServiceTests : IDisposable
    {
        private readonly DbContextOptions<FactoryDbContext> _options;
        private readonly FactoryDbContext _context;
        private readonly FinancialTransactionService _service;
        private readonly FinancialTransactionRepository _financialTransactionRepo;
        private readonly LoanAccountRepository _loanAccountRepo;

        public FinancialTransactionServiceTests()
        {
            // Create in-memory database
            _options = new DbContextOptionsBuilder<FactoryDbContext>()
                .UseInMemoryDatabase(databaseName: $"FinancialTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new FactoryDbContext(_options);
            
            // Create repositories - using concrete classes not interfaces
            _financialTransactionRepo = new FinancialTransactionRepository(_context);
            _loanAccountRepo = new LoanAccountRepository(_context);
            
            // Create service with real DbContext and repositories
            _service = new FinancialTransactionService(_financialTransactionRepo, _loanAccountRepo, _context);

            // Seed test data
            SeedTestData();
        }

        private void SeedTestData()
        {
            var party = new Party
            {
                PartyId = 1,
                Name = "Test Party",
                MobileNumber = "1234567890",
                Place = "Test City",
                PartyType = PartyType.Buyer
            };
            _context.Parties.Add(party);
            _context.SaveChanges();
        }

        [Fact]
        public async Task CreateLoanAsync_LoanGiven_ShouldCreateCorrectLoanAndTransaction()
        {
            // Arrange
            var loan = new LoanAccount
            {
                PartyId = 1,
                LoanType = LoanType.Given,
                OriginalAmount = 10000,
                InterestRate = 5.5m,
                StartDate = DateTime.Now,
                DueDate = DateTime.Now.AddMonths(6),
                CreatedBy = 1
            };

            // Act
            var result = await _service.CreateLoanAsync(loan);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10000, result.OriginalAmount);
            Assert.Equal(10000, result.OutstandingPrincipal);
            Assert.Equal(0, result.OutstandingInterest);
            Assert.Equal(10000, result.TotalOutstanding);
            Assert.Equal(LoanStatus.Active, result.Status);
            
            // Verify financial transaction was created - query directly from context
            var transactions = await _context.FinancialTransactions.ToListAsync();
            Assert.Single(transactions);
            Assert.Equal(FinancialTransactionType.LoanGiven, transactions.First().TransactionType);
        }

        [Fact]
        public async Task CreateLoanAsync_LoanTaken_ShouldCreateCorrectLoanAndTransaction()
        {
            // Arrange
            var loan = new LoanAccount
            {
                PartyId = 1,
                LoanType = LoanType.Taken,
                OriginalAmount = 5000,
                InterestRate = 3.0m,
                StartDate = DateTime.Now,
                DueDate = DateTime.Now.AddMonths(3),
                CreatedBy = 1
            };

            // Act
            var result = await _service.CreateLoanAsync(loan);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5000, result.OriginalAmount);
            Assert.Equal(LoanStatus.Active, result.Status);
            
            // Verify financial transaction was created - query directly from context
            var transactions = await _context.FinancialTransactions.ToListAsync();
            Assert.Single(transactions);
            Assert.Equal(FinancialTransactionType.LoanTaken, transactions.First().TransactionType);
        }

        [Fact]
        public async Task GetAllLoansAsync_ShouldReturnAllLoans()
        {
            // Arrange
            var loan1 = new LoanAccount
            {
                PartyId = 1,
                LoanType = LoanType.Given,
                OriginalAmount = 10000,
                InterestRate = 5.5m,
                StartDate = DateTime.Now,
                CreatedBy = 1
            };
            var loan2 = new LoanAccount
            {
                PartyId = 1,
                LoanType = LoanType.Taken,
                OriginalAmount = 5000,
                InterestRate = 3.0m,
                StartDate = DateTime.Now,
                CreatedBy = 1
            };

            await _service.CreateLoanAsync(loan1);
            await _service.CreateLoanAsync(loan2);

            // Act
            var result = await _service.GetAllLoansAsync();

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetLoansByTypeAsync_ShouldFilterByLoanType()
        {
            // Arrange
            var loanGiven = new LoanAccount
            {
                PartyId = 1,
                LoanType = LoanType.Given,
                OriginalAmount = 10000,
                InterestRate = 5.5m,
                StartDate = DateTime.Now,
                CreatedBy = 1
            };
            var loanTaken = new LoanAccount
            {
                PartyId = 1,
                LoanType = LoanType.Taken,
                OriginalAmount = 5000,
                InterestRate = 3.0m,
                StartDate = DateTime.Now,
                CreatedBy = 1
            };

            await _service.CreateLoanAsync(loanGiven);
            await _service.CreateLoanAsync(loanTaken);

            // Act
            var givenLoans = await _context.LoanAccounts.Where(l => l.LoanType == LoanType.Given).ToListAsync();
            var takenLoans = await _context.LoanAccounts.Where(l => l.LoanType == LoanType.Taken).ToListAsync();

            // Assert
            Assert.Single(givenLoans);
            Assert.Single(takenLoans);
            Assert.Equal(LoanType.Given, givenLoans.First().LoanType);
            Assert.Equal(LoanType.Taken, takenLoans.First().LoanType);
        }

        [Fact]
        public async Task GetLoansByPartyAsync_ShouldFilterByParty()
        {
            // Arrange - Add another party
            var party2 = new Party
            {
                PartyId = 2,
                Name = "Another Party",
                MobileNumber = "9876543210",
                Place = "Another City",
                PartyType = PartyType.Seller
            };
            _context.Parties.Add(party2);
            await _context.SaveChangesAsync();

            var loan1 = new LoanAccount
            {
                PartyId = 1,
                LoanType = LoanType.Given,
                OriginalAmount = 10000,
                InterestRate = 5.5m,
                StartDate = DateTime.Now,
                CreatedBy = 1
            };
            var loan2 = new LoanAccount
            {
                PartyId = 2,
                LoanType = LoanType.Given,
                OriginalAmount = 5000,
                InterestRate = 3.0m,
                StartDate = DateTime.Now,
                CreatedBy = 1
            };

            await _service.CreateLoanAsync(loan1);
            await _service.CreateLoanAsync(loan2);

            // Act
            var party1Loans = await _context.LoanAccounts.Where(l => l.PartyId == 1).ToListAsync();
            var party2Loans = await _context.LoanAccounts.Where(l => l.PartyId == 2).ToListAsync();

            // Assert
            Assert.Single(party1Loans);
            Assert.Single(party2Loans);
            Assert.Equal(1, party1Loans.First().PartyId);
            Assert.Equal(2, party2Loans.First().PartyId);
        }

        [Fact]
        public async Task GetAllFinancialTransactionsAsync_ShouldReturnAllTransactions()
        {
            // Arrange
            var loan1 = new LoanAccount
            {
                PartyId = 1,
                LoanType = LoanType.Given,
                OriginalAmount = 10000,
                InterestRate = 5.5m,
                StartDate = DateTime.Now,
                CreatedBy = 1
            };
            var loan2 = new LoanAccount
            {
                PartyId = 1,
                LoanType = LoanType.Taken,
                OriginalAmount = 5000,
                InterestRate = 3.0m,
                StartDate = DateTime.Now,
                CreatedBy = 1
            };

            await _service.CreateLoanAsync(loan1);
            await _service.CreateLoanAsync(loan2);

            // Act
            var result = await _context.FinancialTransactions.ToListAsync();

            // Assert - Each loan creates one initial transaction
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task CreateLoanAsync_ShouldInitializeOutstandingAmountsCorrectly()
        {
            // Arrange
            var loan = new LoanAccount
            {
                PartyId = 1,
                LoanType = LoanType.Given,
                OriginalAmount = 15000,
                InterestRate = 4.5m,
                StartDate = DateTime.Now,
                DueDate = DateTime.Now.AddYears(1),
                CreatedBy = 1
            };

            // Act
            var result = await _service.CreateLoanAsync(loan);

            // Assert - Outstanding amounts should be initialized
            Assert.Equal(15000, result.OutstandingPrincipal);
            Assert.Equal(0, result.OutstandingInterest);
            Assert.Equal(15000, result.TotalOutstanding);
            Assert.Equal(LoanStatus.Active, result.Status);
            Assert.NotEqual(default(DateTime), result.CreatedDate);
        }

        public void Dispose()
        {
            _context?.Database.EnsureDeleted();
            _context?.Dispose();
        }
    }
}

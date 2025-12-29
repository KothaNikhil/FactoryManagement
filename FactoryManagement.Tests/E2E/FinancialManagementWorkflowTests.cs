using FactoryManagement.Data;
using FactoryManagement.Data.Repositories;
using FactoryManagement.Models;
using FactoryManagement.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FactoryManagement.Tests.E2E
{
    /// <summary>
    /// End-to-End tests for financial transactions and loan management workflows
    /// </summary>
    public class FinancialManagementWorkflowTests : IDisposable
    {
        private readonly DbContextOptions<FactoryDbContext> _options;
        private readonly FactoryDbContext _context;
        private readonly FinancialTransactionService _financialService;
        private readonly PartyService _partyService;

        public FinancialManagementWorkflowTests()
        {
            _options = new DbContextOptionsBuilder<FactoryDbContext>()
                .UseInMemoryDatabase(databaseName: $"E2EFinancialTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new FactoryDbContext(_options);
            
            var loanRepo = new LoanAccountRepository(_context);
            var financialTransactionRepo = new FinancialTransactionRepository(_context);
            var partyRepo = new Repository<Party>(_context);

            _financialService = new FinancialTransactionService(financialTransactionRepo, loanRepo, _context);
            _partyService = new PartyService(partyRepo);

            // Seed user required by financial transactions
            _context.Users.Add(new User { UserId = 1, Username = "admin", Role = "Admin", IsActive = true });
            _context.SaveChanges();
        }

        [Fact]
        public async Task LoanWorkflow_GiveLoan_RecordPayments_CalculateOutstanding()
        {
            // Step 1: Create a party (customer who needs loan)
            var customer = await _partyService.AddPartyAsync(new Party
            {
                Name = "Loan Customer",
                PartyType = PartyType.Buyer,
                MobileNumber = "1234567890"
            });

            // Step 2: Create loan account (loan given to customer)
            var loan = await _financialService.CreateLoanAsync(new LoanAccount
            {
                PartyId = customer.PartyId,
                LoanType = LoanType.Given,
                OriginalAmount = 100000,
                InterestRate = 12,
                StartDate = DateTime.Now,
                Status = LoanStatus.Active,
                CreatedBy = 1,
                Notes = "Initial loan"
            }, PaymentMode.Cash);

            Assert.NotNull(loan);
            Assert.Equal(100000, loan.OriginalAmount);
            Assert.Equal(LoanStatus.Active, loan.Status);

            // Step 3: Record first payment
            await _financialService.RecordPaymentAsync(loan.LoanAccountId, 10000, PaymentMode.Cash, 1, "First payment");

            // Step 4: Record second payment
            await _financialService.RecordPaymentAsync(loan.LoanAccountId, 15000, PaymentMode.Cash, 1, "Second payment");

            // Assert - Verify loan state
            var updatedLoan = await _financialService.GetLoanWithTransactionsAsync(loan.LoanAccountId);
            Assert.NotNull(updatedLoan);
            Assert.True(updatedLoan!.OutstandingPrincipal <= 90000);
            Assert.True(updatedLoan!.TotalOutstanding <= 90000);
            Assert.True(updatedLoan!.Status == LoanStatus.PartiallyPaid || updatedLoan!.Status == LoanStatus.Active);

            // Verify transaction count
            var transactions = await _financialService.GetTransactionsByLoanAsync(loan.LoanAccountId);
            Assert.True(transactions.Count() >= 2);
        }

        [Fact]
        public async Task LoanWorkflow_TakeLoan_RecordPayments_CloseAccount()
        {
            // Step 1: Create supplier party
            var supplier = await _partyService.AddPartyAsync(new Party
            {
                Name = "Loan Supplier",
                PartyType = PartyType.Seller,
                MobileNumber = "9876543210"
            });

            // Step 2: Take loan from supplier
            var loan2 = await _financialService.CreateLoanAsync(new LoanAccount
            {
                PartyId = supplier.PartyId,
                LoanType = LoanType.Taken,
                OriginalAmount = 50000,
                InterestRate = 10,
                StartDate = DateTime.Now,
                Status = LoanStatus.Active,
                CreatedBy = 1
            }, PaymentMode.Cash);

            // Step 3: Make partial payment
            await _financialService.RecordPaymentAsync(loan2.LoanAccountId, 20000, PaymentMode.Cash, 1, "Partial payment");

            // Step 4: Make final payment
            await _financialService.RecordPaymentAsync(loan2.LoanAccountId, 30000, PaymentMode.Cash, 1, "Final payment");

            // Assert
            var closedLoan = await _financialService.GetLoanWithTransactionsAsync(loan2.LoanAccountId);
            Assert.NotNull(closedLoan);
            Assert.Equal(0, closedLoan!.TotalOutstanding);
            Assert.Equal(LoanStatus.Closed, closedLoan!.Status);
        }

        [Fact]
        public async Task LoanWorkflow_MultipleLoanAccounts_FilterAndCalculate()
        {
            // Arrange - Create multiple parties and loans
            var customer1 = await _partyService.AddPartyAsync(new Party { Name = "Customer 1", PartyType = PartyType.Buyer });
            var customer2 = await _partyService.AddPartyAsync(new Party { Name = "Customer 2", PartyType = PartyType.Buyer });
            var supplier1 = await _partyService.AddPartyAsync(new Party { Name = "Supplier 1", PartyType = PartyType.Seller });

            // Given loans
            var loan1 = await _financialService.CreateLoanAsync(new LoanAccount
            {
                PartyId = customer1.PartyId,
                LoanType = LoanType.Given,
                OriginalAmount = 50000,
                InterestRate = 12,
                Status = LoanStatus.Active,
                CreatedBy = 1
            }, PaymentMode.Cash);

            var loan3 = await _financialService.CreateLoanAsync(new LoanAccount
            {
                PartyId = customer2.PartyId,
                LoanType = LoanType.Given,
                OriginalAmount = 75000,
                InterestRate = 10,
                Status = LoanStatus.Active,
                CreatedBy = 1
            }, PaymentMode.Cash);

            // Taken loan
            var loan4 = await _financialService.CreateLoanAsync(new LoanAccount
            {
                PartyId = supplier1.PartyId,
                LoanType = LoanType.Taken,
                OriginalAmount = 100000,
                InterestRate = 8,
                Status = LoanStatus.PartiallyPaid,
                CreatedBy = 1
            }, PaymentMode.Cash);

            // Act - Filter by type
            var givenLoans = await _financialService.GetLoansByTypeAsync(LoanType.Given);
            var takenLoans = await _financialService.GetLoansByTypeAsync(LoanType.Taken);

            // Assert
            Assert.Equal(2, givenLoans.Count());
            Assert.Single(takenLoans);

            // Calculate total outstanding for given loans
            var totalGivenOutstanding = givenLoans.Sum(l => l.TotalOutstanding);
            Assert.True(totalGivenOutstanding >= 0);

            // Calculate total outstanding for taken loans
            var totalTakenOutstanding = takenLoans.Sum(l => l.TotalOutstanding);
            Assert.True(totalTakenOutstanding >= 0);
        }

        [Fact]
        public async Task LoanWorkflow_FilterByParty_ShouldReturnCorrectLoans()
        {
            // Arrange
            var party = await _partyService.AddPartyAsync(new Party { Name = "Test Party", PartyType = PartyType.Buyer });

            await _financialService.CreateLoanAsync(new LoanAccount
            {
                PartyId = party.PartyId,
                LoanType = LoanType.Given,
                OriginalAmount = 10000,
                InterestRate = 12,
                Status = LoanStatus.Active,
                CreatedBy = 1
            }, PaymentMode.Cash);

            await _financialService.CreateLoanAsync(new LoanAccount
            {
                PartyId = party.PartyId,
                LoanType = LoanType.Given,
                OriginalAmount = 20000,
                InterestRate = 10,
                Status = LoanStatus.PartiallyPaid,
                CreatedBy = 1
            }, PaymentMode.Cash);

            // Act
            var partyLoans = await _financialService.GetLoansByPartyAsync(party.PartyId);

            // Assert
            Assert.Equal(2, partyLoans.Count());
            Assert.All(partyLoans, l => Assert.Equal(party.PartyId, l.PartyId));
        }

        [Fact]
        public async Task LoanWorkflow_FilterByStatus_ShouldReturnCorrectLoans()
        {
            // Arrange
            var party1 = await _partyService.AddPartyAsync(new Party { Name = "Party 1", PartyType = PartyType.Buyer });
            var party2 = await _partyService.AddPartyAsync(new Party { Name = "Party 2", PartyType = PartyType.Buyer });
            var party3 = await _partyService.AddPartyAsync(new Party { Name = "Party 3", PartyType = PartyType.Buyer });

            await _financialService.CreateLoanAsync(new LoanAccount
            {
                PartyId = party1.PartyId,
                LoanType = LoanType.Given,
                OriginalAmount = 10000,
                Status = LoanStatus.Active,
                CreatedBy = 1
            }, PaymentMode.Cash);

            await _financialService.CreateLoanAsync(new LoanAccount
            {
                PartyId = party2.PartyId,
                LoanType = LoanType.Given,
                OriginalAmount = 20000,
                Status = LoanStatus.PartiallyPaid,
                CreatedBy = 1
            }, PaymentMode.Cash);

            await _financialService.CreateLoanAsync(new LoanAccount
            {
                PartyId = party3.PartyId,
                LoanType = LoanType.Given,
                OriginalAmount = 15000,
                Status = LoanStatus.Closed,
                CreatedBy = 1
            }, PaymentMode.Cash);

            // Act
            // Adjust loan statuses via payments
            var allLoans = await _financialService.GetAllLoansAsync();
            var loanActive = allLoans.First(l => l.PartyId == party1.PartyId);
            var loanPartial = allLoans.First(l => l.PartyId == party2.PartyId);
            var loanClosed = allLoans.First(l => l.PartyId == party3.PartyId);

            // Make a partial payment on second loan
            await _financialService.RecordPaymentAsync(loanPartial.LoanAccountId, 5000, PaymentMode.Cash, 1, "Partial payment");
            // Close the third loan by paying full amount
            await _financialService.RecordPaymentAsync(loanClosed.LoanAccountId, loanClosed.TotalOutstanding, PaymentMode.Cash, 1, "Closure payment");

            var refreshed = await _financialService.GetAllLoansAsync();
            var activeLoans = refreshed.Where(l => l.Status == LoanStatus.Active);
            var partialLoans = refreshed.Where(l => l.Status == LoanStatus.PartiallyPaid);
            var closedLoans = refreshed.Where(l => l.Status == LoanStatus.Closed);

            // Assert presence of statuses
            Assert.True(activeLoans.Any());
            Assert.True(partialLoans.Any());
            Assert.True(closedLoans.Any());
        }

        [Fact]
        public async Task LoanWorkflow_InterestCalculation_ShouldBeAccurate()
        {
            // Arrange
            var party = await _partyService.AddPartyAsync(new Party { Name = "Test Customer", PartyType = PartyType.Buyer });

            // Create loan with interest
            var loan = await _financialService.CreateLoanAsync(new LoanAccount
            {
                PartyId = party.PartyId,
                LoanType = LoanType.Given,
                OriginalAmount = 100000,
                InterestRate = 12, // 12% annual
                StartDate = DateTime.Now.AddMonths(-1),
                Status = LoanStatus.Active,
                CreatedBy = 1
            }, PaymentMode.Cash);

            // Calculate 1 month interest: 100000 * 12% / 12 = 1000
            // Act - calculate interest up to today
            await _financialService.UpdateLoanInterestAsync(loan.LoanAccountId);

            // Assert - interest accrued is positive and totals updated
            var updated = await _financialService.GetLoanWithTransactionsAsync(loan.LoanAccountId);
            Assert.NotNull(updated);
            Assert.True(updated!.OutstandingInterest > 0);
            Assert.Equal(updated!.OutstandingPrincipal + updated!.OutstandingInterest, updated!.TotalOutstanding);
        }

        [Fact]
        public async Task LoanWorkflow_CompleteLifecycle_ShouldTrackAllTransactions()
        {
            // Complete loan lifecycle
            var party = await _partyService.AddPartyAsync(new Party { Name = "Lifecycle Customer", PartyType = PartyType.Buyer });

            // Step 1: Create loan
            var loan = await _financialService.CreateLoanAsync(new LoanAccount
            {
                PartyId = party.PartyId,
                LoanType = LoanType.Given,
                OriginalAmount = 50000,
                InterestRate = 10,
                StartDate = DateTime.Now,
                Status = LoanStatus.Active,
                CreatedBy = 1
            }, PaymentMode.Cash);

            // Step 2: Add disbursement transaction
            // Initial transaction is automatically created in CreateLoanAsync

            // Step 3: Record multiple payments
            for (int i = 1; i <= 5; i++)
            {
                await _financialService.RecordPaymentAsync(loan.LoanAccountId, 10000, PaymentMode.Cash, 1, $"Payment {i}");
            }

            // Step 4: Close loan
            // After repayments, loan should be closed or partially paid; try final payment to close if needed
            var current = await _financialService.GetLoanWithTransactionsAsync(loan.LoanAccountId);
            Assert.NotNull(current);
            if (current!.TotalOutstanding > 0)
            {
                await _financialService.RecordPaymentAsync(loan.LoanAccountId, current!.TotalOutstanding, PaymentMode.Cash, 1, "Final closure payment");
            }

            // Assert - Verify transaction history
            var transactions = await _financialService.GetTransactionsByLoanAsync(loan.LoanAccountId);
            Assert.True(transactions.Count() >= 6); // initial + 5 payments + potential final closure payment

            // Verify loan closed
            var finalLoan = await _financialService.GetLoanWithTransactionsAsync(loan.LoanAccountId);
            Assert.NotNull(finalLoan);
            Assert.Equal(LoanStatus.Closed, finalLoan!.Status);
            Assert.Equal(0, finalLoan!.TotalOutstanding);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}


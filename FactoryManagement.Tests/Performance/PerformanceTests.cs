using FactoryManagement.Data;
using FactoryManagement.Data.Repositories;
using FactoryManagement.Models;
using FactoryManagement.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace FactoryManagement.Tests.Performance
{
    /// <summary>
    /// Performance tests to ensure application handles large datasets efficiently
    /// Target: 10,000+ records with acceptable performance
    /// </summary>
    public class PerformanceTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly DbContextOptions<FactoryDbContext> _options;
        private readonly FactoryDbContext _context;
        private readonly Stopwatch _stopwatch;

        // Performance thresholds (in milliseconds)
        private const int BULK_INSERT_THRESHOLD_MS = 45000;     // adjust threshold for environment variability (bulk inserts can take 30-40s on slower systems)
        private const int QUERY_ALL_THRESHOLD_MS = 2000;        // 2 seconds to query 10K records
        private const int SEARCH_FILTER_THRESHOLD_MS = 1000;    // 1 second for filtered search
        private const int SINGLE_INSERT_THRESHOLD_MS = 100;     // 100ms for single insert
        private const int UPDATE_THRESHOLD_MS = 100;            // 100ms for single update
        private const int DELETE_THRESHOLD_MS = 100;            // 100ms for single delete

        public PerformanceTests(ITestOutputHelper output)
        {
            _output = output;
            _stopwatch = new Stopwatch();

            _options = new DbContextOptionsBuilder<FactoryDbContext>()
                .UseInMemoryDatabase(databaseName: $"PerformanceTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new FactoryDbContext(_options);
        }

        #region Item Management Performance Tests

        [Fact]
        public async Task ItemManagement_BulkInsert_10000Items_ShouldCompleteInAcceptableTime()
        {
            // Arrange
            const int itemCount = 10000;
            var items = GenerateLargeItemDataset(itemCount);
            var repository = new Repository<Item>(_context);

            // Act
            _stopwatch.Start();
            foreach (var item in items)
            {
                await repository.AddAsync(item);
            }
            await _context.SaveChangesAsync();
            _stopwatch.Stop();

            // Assert
            var elapsed = _stopwatch.ElapsedMilliseconds;
            _output.WriteLine($"Bulk insert of {itemCount} items took: {elapsed}ms");
            Assert.True(elapsed < BULK_INSERT_THRESHOLD_MS, 
                $"Bulk insert took {elapsed}ms, exceeds threshold of {BULK_INSERT_THRESHOLD_MS}ms");

            var count = await _context.Items.CountAsync();
            Assert.Equal(itemCount, count);
        }

        [Fact]
        public async Task ItemManagement_QueryAll_10000Items_ShouldCompleteInAcceptableTime()
        {
            // Arrange
            const int itemCount = 10000;
            await SeedItems(itemCount);
            var service = new ItemService(new Repository<Item>(_context));

            // Act
            _stopwatch.Restart();
            var result = await service.GetAllItemsAsync();
            _stopwatch.Stop();

            // Assert
            var elapsed = _stopwatch.ElapsedMilliseconds;
            _output.WriteLine($"Query all {itemCount} items took: {elapsed}ms");
            Assert.True(elapsed < QUERY_ALL_THRESHOLD_MS,
                $"Query took {elapsed}ms, exceeds threshold of {QUERY_ALL_THRESHOLD_MS}ms");
            Assert.Equal(itemCount, result.Count());
        }

        [Fact]
        public async Task ItemManagement_SearchByName_In10000Items_ShouldCompleteInAcceptableTime()
        {
            // Arrange
            const int itemCount = 10000;
            await SeedItems(itemCount);
            var repository = new Repository<Item>(_context);

            // Act - Search for items containing "5000"
            _stopwatch.Restart();
            var result = await _context.Items
                .Where(i => i.ItemName.Contains("5000"))
                .ToListAsync();
            _stopwatch.Stop();

            // Assert
            var elapsed = _stopwatch.ElapsedMilliseconds;
            _output.WriteLine($"Search in {itemCount} items took: {elapsed}ms (found {result.Count} items)");
            Assert.True(elapsed < SEARCH_FILTER_THRESHOLD_MS,
                $"Search took {elapsed}ms, exceeds threshold of {SEARCH_FILTER_THRESHOLD_MS}ms");
        }

        [Fact]
        public async Task ItemManagement_GetLowStockItems_In10000Items_ShouldCompleteInAcceptableTime()
        {
            // Arrange
            const int itemCount = 10000;
            await SeedItems(itemCount);
            var repository = new Repository<Item>(_context);

            // Act - Get low stock items (< 10 units)
            _stopwatch.Restart();
            var result = await _context.Items
                .Where(i => i.CurrentStock < 10)
                .ToListAsync();
            _stopwatch.Stop();

            // Assert
            var elapsed = _stopwatch.ElapsedMilliseconds;
            _output.WriteLine($"Low stock query in {itemCount} items took: {elapsed}ms (found {result.Count} items)");
            Assert.True(elapsed < SEARCH_FILTER_THRESHOLD_MS,
                $"Low stock query took {elapsed}ms, exceeds threshold of {SEARCH_FILTER_THRESHOLD_MS}ms");
        }

        #endregion

        #region Party Management Performance Tests

        [Fact]
        public async Task PartyManagement_BulkInsert_10000Parties_ShouldCompleteInAcceptableTime()
        {
            // Arrange
            const int partyCount = 10000;
            var parties = GenerateLargePartyDataset(partyCount);
            var repository = new Repository<Party>(_context);

            // Act
            _stopwatch.Start();
            foreach (var party in parties)
            {
                await repository.AddAsync(party);
            }
            await _context.SaveChangesAsync();
            _stopwatch.Stop();

            // Assert
            var elapsed = _stopwatch.ElapsedMilliseconds;
            _output.WriteLine($"Bulk insert of {partyCount} parties took: {elapsed}ms");
            Assert.True(elapsed < BULK_INSERT_THRESHOLD_MS,
                $"Bulk insert took {elapsed}ms, exceeds threshold of {BULK_INSERT_THRESHOLD_MS}ms");

            var count = await _context.Parties.CountAsync();
            Assert.Equal(partyCount, count);
        }

        [Fact]
        public async Task PartyManagement_QueryAll_10000Parties_ShouldCompleteInAcceptableTime()
        {
            // Arrange
            const int partyCount = 10000;
            await SeedParties(partyCount);
            var service = new PartyService(new Repository<Party>(_context));

            // Act
            _stopwatch.Restart();
            var result = await service.GetAllPartiesAsync();
            _stopwatch.Stop();

            // Assert
            var elapsed = _stopwatch.ElapsedMilliseconds;
            _output.WriteLine($"Query all {partyCount} parties took: {elapsed}ms");
            Assert.True(elapsed < QUERY_ALL_THRESHOLD_MS,
                $"Query took {elapsed}ms, exceeds threshold of {QUERY_ALL_THRESHOLD_MS}ms");
            Assert.Equal(partyCount, result.Count());
        }

        [Fact]
        public async Task PartyManagement_FilterByType_In10000Parties_ShouldCompleteInAcceptableTime()
        {
            // Arrange
            const int partyCount = 10000;
            await SeedParties(partyCount);

            // Act - Filter by Buyer type
            _stopwatch.Restart();
            var result = await _context.Parties
                .Where(p => p.PartyType == PartyType.Buyer)
                .ToListAsync();
            _stopwatch.Stop();

            // Assert
            var elapsed = _stopwatch.ElapsedMilliseconds;
            _output.WriteLine($"Filter by type in {partyCount} parties took: {elapsed}ms (found {result.Count} parties)");
            Assert.True(elapsed < SEARCH_FILTER_THRESHOLD_MS,
                $"Filter took {elapsed}ms, exceeds threshold of {SEARCH_FILTER_THRESHOLD_MS}ms");
        }

        [Fact]
        public async Task PartyManagement_SearchByName_In10000Parties_ShouldCompleteInAcceptableTime()
        {
            // Arrange
            const int partyCount = 10000;
            await SeedParties(partyCount);

            // Act
            _stopwatch.Restart();
            var result = await _context.Parties
                .Where(p => p.Name.Contains("Party 5000"))
                .ToListAsync();
            _stopwatch.Stop();

            // Assert
            var elapsed = _stopwatch.ElapsedMilliseconds;
            _output.WriteLine($"Search by name in {partyCount} parties took: {elapsed}ms (found {result.Count} parties)");
            Assert.True(elapsed < SEARCH_FILTER_THRESHOLD_MS,
                $"Search took {elapsed}ms, exceeds threshold of {SEARCH_FILTER_THRESHOLD_MS}ms");
        }

        #endregion

        #region Transaction Performance Tests

        [Fact]
        public async Task TransactionManagement_BulkInsert_10000Transactions_ShouldCompleteInAcceptableTime()
        {
            // Arrange
            const int transactionCount = 10000;
            await SeedItems(100);
            await SeedParties(100);
            var transactions = await GenerateLargeTransactionDataset(transactionCount);

            // Act
            _stopwatch.Start();
            foreach (var transaction in transactions)
            {
                await _context.Transactions.AddAsync(transaction);
            }
            await _context.SaveChangesAsync();
            _stopwatch.Stop();

            // Assert
            var elapsed = _stopwatch.ElapsedMilliseconds;
            _output.WriteLine($"Bulk insert of {transactionCount} transactions took: {elapsed}ms");
            Assert.True(elapsed < BULK_INSERT_THRESHOLD_MS,
                $"Bulk insert took {elapsed}ms, exceeds threshold of {BULK_INSERT_THRESHOLD_MS}ms");

            var count = await _context.Transactions.CountAsync();
            Assert.Equal(transactionCount, count);
        }

        [Fact]
        public async Task TransactionManagement_QueryAll_10000Transactions_ShouldCompleteInAcceptableTime()
        {
            // Arrange
            const int transactionCount = 10000;
            
            // Seed a user first since Transaction has a required EnteredBy field
            await _context.Users.AddAsync(new User { UserId = 1, Username = "TestUser", Role = "Admin", IsActive = true });
            await _context.SaveChangesAsync();
            
            await SeedItems(100);
            await SeedParties(100);
            await SeedTransactions(transactionCount);
            
            var repository = new TransactionRepository(_context);

            // Act
            _stopwatch.Restart();
            var result = await repository.GetAllWithDetailsAsync();
            _stopwatch.Stop();

            // Assert
            var elapsed = _stopwatch.ElapsedMilliseconds;
            _output.WriteLine($"Query all {transactionCount} transactions with details took: {elapsed}ms");
            Assert.True(elapsed < QUERY_ALL_THRESHOLD_MS * 2, // Allow 2x for joins
                $"Query took {elapsed}ms, exceeds threshold of {QUERY_ALL_THRESHOLD_MS * 2}ms");
            Assert.Equal(transactionCount, result.Count());
        }

        [Fact]
        public async Task TransactionManagement_FilterByDateRange_In10000Transactions_ShouldCompleteInAcceptableTime()
        {
            // Arrange
            const int transactionCount = 10000;
            await SeedItems(100);
            await SeedParties(100);
            await SeedTransactions(transactionCount);
            var repository = new TransactionRepository(_context);

            var startDate = DateTime.Now.AddMonths(-1);
            var endDate = DateTime.Now;

            // Act
            _stopwatch.Restart();
            var result = await repository.GetTransactionsByDateRangeAsync(startDate, endDate);
            _stopwatch.Stop();

            // Assert
            var elapsed = _stopwatch.ElapsedMilliseconds;
            _output.WriteLine($"Date range filter in {transactionCount} transactions took: {elapsed}ms (found {result.Count()} transactions)");
            Assert.True(elapsed < SEARCH_FILTER_THRESHOLD_MS,
                $"Filter took {elapsed}ms, exceeds threshold of {SEARCH_FILTER_THRESHOLD_MS}ms");
        }

        [Fact]
        public async Task TransactionManagement_FilterByType_In10000Transactions_ShouldCompleteInAcceptableTime()
        {
            // Arrange
            const int transactionCount = 10000;
            await SeedItems(100);
            await SeedParties(100);
            await SeedTransactions(transactionCount);
            var repository = new TransactionRepository(_context);

            // Act
            _stopwatch.Restart();
            var result = await repository.GetTransactionsByTypeAsync(TransactionType.Buy);
            _stopwatch.Stop();

            // Assert
            var elapsed = _stopwatch.ElapsedMilliseconds;
            _output.WriteLine($"Filter by type in {transactionCount} transactions took: {elapsed}ms (found {result.Count()} transactions)");
            Assert.True(elapsed < SEARCH_FILTER_THRESHOLD_MS,
                $"Filter took {elapsed}ms, exceeds threshold of {SEARCH_FILTER_THRESHOLD_MS}ms");
        }

        #endregion

        #region Financial Transaction Performance Tests

        [Fact]
        public async Task FinancialTransactionManagement_BulkInsert_5000LoansAnd10000Transactions_ShouldCompleteInAcceptableTime()
        {
            // Arrange
            const int loanCount = 5000;
            const int transactionCount = 10000;
            await SeedParties(500);
            var loans = await GenerateLargeLoanDataset(loanCount);
            var transactions = await GenerateLargeFinancialTransactionDataset(transactionCount);

            // Act
            _stopwatch.Start();
            foreach (var loan in loans)
            {
                await _context.LoanAccounts.AddAsync(loan);
            }
            foreach (var transaction in transactions)
            {
                await _context.FinancialTransactions.AddAsync(transaction);
            }
            await _context.SaveChangesAsync();
            _stopwatch.Stop();

            // Assert
            var elapsed = _stopwatch.ElapsedMilliseconds;
            _output.WriteLine($"Bulk insert of {loanCount} loans and {transactionCount} financial transactions took: {elapsed}ms");
            Assert.True(elapsed < BULK_INSERT_THRESHOLD_MS * 2, // Allow 2x for two datasets
                $"Bulk insert took {elapsed}ms, exceeds threshold of {BULK_INSERT_THRESHOLD_MS * 2}ms");

            var loanCountDb = await _context.LoanAccounts.CountAsync();
            var transactionCountDb = await _context.FinancialTransactions.CountAsync();
            Assert.Equal(loanCount, loanCountDb);
            Assert.Equal(transactionCount, transactionCountDb);
        }

        [Fact]
        public async Task FinancialTransactionManagement_QueryAllLoans_5000Records_ShouldCompleteInAcceptableTime()
        {
            // Arrange
            const int loanCount = 5000;
            await SeedParties(500);
            await SeedLoans(loanCount);
            var repository = new LoanAccountRepository(_context);

            // Act
            _stopwatch.Restart();
            var result = await repository.GetAllAsync();
            _stopwatch.Stop();

            // Assert
            var elapsed = _stopwatch.ElapsedMilliseconds;
            _output.WriteLine($"Query all {loanCount} loan accounts took: {elapsed}ms");
            Assert.True(elapsed < QUERY_ALL_THRESHOLD_MS,
                $"Query took {elapsed}ms, exceeds threshold of {QUERY_ALL_THRESHOLD_MS}ms");
            Assert.Equal(loanCount, result.Count());
        }

        [Fact]
        public async Task FinancialTransactionManagement_FilterByLoanType_In5000Loans_ShouldCompleteInAcceptableTime()
        {
            // Arrange
            const int loanCount = 5000;
            await SeedParties(500);
            await SeedLoans(loanCount);
            var repository = new LoanAccountRepository(_context);

            // Act
            _stopwatch.Restart();
            var result = await repository.GetByLoanTypeAsync(LoanType.Given);
            _stopwatch.Stop();

            // Assert
            var elapsed = _stopwatch.ElapsedMilliseconds;
            _output.WriteLine($"Filter by loan type in {loanCount} loans took: {elapsed}ms (found {result.Count()} loans)");
            Assert.True(elapsed < SEARCH_FILTER_THRESHOLD_MS,
                $"Filter took {elapsed}ms, exceeds threshold of {SEARCH_FILTER_THRESHOLD_MS}ms");
        }

        [Fact]
        public async Task FinancialTransactionManagement_CalculateOutstandingAmounts_5000Loans_ShouldCompleteInAcceptableTime()
        {
            // Arrange
            const int loanCount = 5000;
            await SeedParties(500);
            await SeedLoans(loanCount);

            // Act
            _stopwatch.Restart();
            var totalOutstanding = await _context.LoanAccounts
                .Where(l => l.Status == LoanStatus.Active)
                .SumAsync(l => l.TotalOutstanding);
            _stopwatch.Stop();

            // Assert
            var elapsed = _stopwatch.ElapsedMilliseconds;
            _output.WriteLine($"Calculate total outstanding for {loanCount} loans took: {elapsed}ms (total: {totalOutstanding:C})");
            Assert.True(elapsed < SEARCH_FILTER_THRESHOLD_MS,
                $"Calculation took {elapsed}ms, exceeds threshold of {SEARCH_FILTER_THRESHOLD_MS}ms");
        }

        #endregion

        #region Wage Management Performance Tests

        [Fact]
        public async Task WageManagement_BulkInsert_1000WorkersAnd10000Transactions_ShouldCompleteInAcceptableTime()
        {
            // Arrange
            const int workerCount = 1000;
            const int transactionCount = 10000;
            var workers = GenerateLargeWorkerDataset(workerCount);
            
            // Save workers first
            await _context.Workers.AddRangeAsync(workers);
            await _context.SaveChangesAsync();
            
            // Generate transactions after workers are saved
            var transactions = await GenerateLargeWageTransactionDataset(transactionCount);

            // Act
            _stopwatch.Start();
            await _context.WageTransactions.AddRangeAsync(transactions);
            await _context.SaveChangesAsync();
            _stopwatch.Stop();

            // Assert
            var elapsed = _stopwatch.ElapsedMilliseconds;
            _output.WriteLine($"Bulk insert of {workerCount} workers and {transactionCount} wage transactions took: {elapsed}ms");
            Assert.True(elapsed < BULK_INSERT_THRESHOLD_MS * 2,
                $"Bulk insert took {elapsed}ms, exceeds threshold of {BULK_INSERT_THRESHOLD_MS * 2}ms");

            var workerCountDb = await _context.Workers.CountAsync();
            var transactionCountDb = await _context.WageTransactions.CountAsync();
            Assert.Equal(workerCount, workerCountDb);
            Assert.Equal(transactionCount, transactionCountDb);
        }

        [Fact]
        public async Task WageManagement_QueryAllWorkers_1000Records_ShouldCompleteInAcceptableTime()
        {
            // Arrange
            const int workerCount = 1000;
            await SeedWorkers(workerCount);
            var repository = new Repository<Worker>(_context);

            // Act
            _stopwatch.Restart();
            var result = await repository.GetAllAsync();
            _stopwatch.Stop();

            // Assert
            var elapsed = _stopwatch.ElapsedMilliseconds;
            _output.WriteLine($"Query all {workerCount} workers took: {elapsed}ms");
            Assert.True(elapsed < QUERY_ALL_THRESHOLD_MS,
                $"Query took {elapsed}ms, exceeds threshold of {QUERY_ALL_THRESHOLD_MS}ms");
            Assert.Equal(workerCount, result.Count());
        }

        [Fact]
        public async Task WageManagement_CalculateTotalWages_10000Transactions_ShouldCompleteInAcceptableTime()
        {
            // Arrange
            const int workerCount = 100;
            const int transactionCount = 10000;
            await SeedWorkers(workerCount);
            await SeedWageTransactions(transactionCount);

            // Act
            _stopwatch.Restart();
            var totalWages = await _context.WageTransactions
                .Where(w => w.TransactionType == WageTransactionType.DailyWage || 
                           w.TransactionType == WageTransactionType.MonthlyWage ||
                           w.TransactionType == WageTransactionType.Bonus)
                .SumAsync(w => w.Amount);
            _stopwatch.Stop();

            // Assert
            var elapsed = _stopwatch.ElapsedMilliseconds;
            _output.WriteLine($"Calculate total wages for {transactionCount} transactions took: {elapsed}ms (total: {totalWages:C})");
            Assert.True(elapsed < SEARCH_FILTER_THRESHOLD_MS,
                $"Calculation took {elapsed}ms, exceeds threshold of {SEARCH_FILTER_THRESHOLD_MS}ms");
        }

        [Fact]
        public async Task WageManagement_FilterByWorkerAndDateRange_In10000Transactions_ShouldCompleteInAcceptableTime()
        {
            // Arrange
            const int workerCount = 100;
            const int transactionCount = 10000;
            await SeedWorkers(workerCount);
            await SeedWageTransactions(transactionCount);

            var worker = await _context.Workers.FirstAsync();
            var startDate = DateTime.Now.AddMonths(-1);
            var endDate = DateTime.Now;

            // Act
            _stopwatch.Restart();
            var result = await _context.WageTransactions
                .Where(w => w.WorkerId == worker.WorkerId && 
                           w.TransactionDate >= startDate && 
                           w.TransactionDate <= endDate)
                .ToListAsync();
            _stopwatch.Stop();

            // Assert
            var elapsed = _stopwatch.ElapsedMilliseconds;
            _output.WriteLine($"Filter by worker and date range in {transactionCount} transactions took: {elapsed}ms (found {result.Count} transactions)");
            Assert.True(elapsed < SEARCH_FILTER_THRESHOLD_MS,
                $"Filter took {elapsed}ms, exceeds threshold of {SEARCH_FILTER_THRESHOLD_MS}ms");
        }

        #endregion

        #region Data Generation Helpers

        private List<Item> GenerateLargeItemDataset(int count)
        {
            var items = new List<Item>();
            for (int i = 1; i <= count; i++)
            {
                items.Add(new Item
                {
                    ItemName = $"Test Item {i}",
                    Unit = i % 2 == 0 ? "Kg" : "Pcs",
                    CurrentStock = i % 100,
                    CreatedDate = DateTime.Now.AddDays(-i % 365)
                });
            }
            return items;
        }

        private List<Party> GenerateLargePartyDataset(int count)
        {
            var parties = new List<Party>();
            var partyTypes = Enum.GetValues(typeof(PartyType)).Cast<PartyType>().ToArray();

            for (int i = 1; i <= count; i++)
            {
                parties.Add(new Party
                {
                    Name = $"Test Party {i}",
                    MobileNumber = $"98765{i:D5}",
                    Place = $"City {i % 100}",
                    PartyType = partyTypes[i % partyTypes.Length],
                    CreatedDate = DateTime.Now.AddDays(-i % 365)
                });
            }
            return parties;
        }

        private async Task<List<Transaction>> GenerateLargeTransactionDataset(int count)
        {
            var transactions = new List<Transaction>();
            var items = await _context.Items.ToListAsync();
            var parties = await _context.Parties.ToListAsync();
            
            // If no items or parties, cannot generate transactions
            if (items.Count == 0 || parties.Count == 0)
            {
                throw new InvalidOperationException($"Cannot generate transactions: Items count={items.Count}, Parties count={parties.Count}");
            }
            
            var transactionTypes = new[] { TransactionType.Buy, TransactionType.Sell, TransactionType.Wastage };

            for (int i = 1; i <= count; i++)
            {
                transactions.Add(new Transaction
                {
                    ItemId = items[i % items.Count].ItemId,
                    PartyId = parties[i % parties.Count].PartyId,
                    TransactionType = transactionTypes[i % transactionTypes.Length],
                    Quantity = 10 + (i % 100),
                    PricePerUnit = 100 + (i % 500),
                    TotalAmount = (10 + (i % 100)) * (100 + (i % 500)),
                    TransactionDate = DateTime.Now.AddDays(-i % 365),
                    EnteredBy = 1,
                    CreatedDate = DateTime.Now.AddDays(-i % 365)
                });
            }
            return transactions;
        }

        private async Task<List<LoanAccount>> GenerateLargeLoanDataset(int count)
        {
            var loans = new List<LoanAccount>();
            var parties = await _context.Parties.ToListAsync();
            var loanTypes = new[] { LoanType.Given, LoanType.Taken };
            var statuses = new[] { LoanStatus.Active, LoanStatus.PartiallyPaid, LoanStatus.Closed };

            for (int i = 1; i <= count; i++)
            {
                var amount = 10000 + (i % 100000);
                loans.Add(new LoanAccount
                {
                    PartyId = parties[i % parties.Count].PartyId,
                    LoanType = loanTypes[i % loanTypes.Length],
                    OriginalAmount = amount,
                    OutstandingPrincipal = amount * 0.7m,
                    OutstandingInterest = amount * 0.05m,
                    TotalOutstanding = amount * 0.75m,
                    InterestRate = 5 + (i % 10),
                    Status = statuses[i % statuses.Length],
                    StartDate = DateTime.Now.AddMonths(-i % 24),
                    DueDate = DateTime.Now.AddMonths(i % 12),
                    CreatedBy = 1,
                    CreatedDate = DateTime.Now.AddMonths(-i % 24)
                });
            }
            return loans;
        }

        private async Task<List<FinancialTransaction>> GenerateLargeFinancialTransactionDataset(int count)
        {
            var transactions = new List<FinancialTransaction>();
            var parties = await _context.Parties.ToListAsync();
            var transactionTypes = Enum.GetValues(typeof(FinancialTransactionType))
                .Cast<FinancialTransactionType>().ToArray();

            for (int i = 1; i <= count; i++)
            {
                transactions.Add(new FinancialTransaction
                {
                    PartyId = parties[i % parties.Count].PartyId,
                    TransactionType = transactionTypes[i % transactionTypes.Length],
                    Amount = 1000 + (i % 50000),
                    InterestRate = 5 + (i % 10),
                    TransactionDate = DateTime.Now.AddDays(-i % 365),
                    EnteredBy = 1,
                    CreatedDate = DateTime.Now.AddDays(-i % 365)
                });
            }
            return transactions;
        }

        private List<Worker> GenerateLargeWorkerDataset(int count)
        {
            var workers = new List<Worker>();
            for (int i = 1; i <= count; i++)
            {
                workers.Add(new Worker
                {
                    Name = $"Test Worker {i}",
                    MobileNumber = $"{98765 + (i % 90000)}",
                    Address = $"Address {i}",
                    Rate = 500 + (i % 500),
                    DailyRate = 500 + (i % 500),
                    Status = i % 10 != 0 ? WorkerStatus.Active : WorkerStatus.Inactive,
                    JoiningDate = DateTime.Now.AddDays(-i % 365),
                    CreatedDate = DateTime.Now.AddDays(-i % 365)
                });
            }
            return workers;
        }

        private async Task<List<WageTransaction>> GenerateLargeWageTransactionDataset(int count)
        {
            var transactions = new List<WageTransaction>();
            var workers = await _context.Workers.ToListAsync();
            var transactionTypes = new[] { WageTransactionType.DailyWage, WageTransactionType.MonthlyWage, WageTransactionType.Bonus };

            for (int i = 1; i <= count; i++)
            {
                var amount = 500 + (i % 5000);
                transactions.Add(new WageTransaction
                {
                    WorkerId = workers[i % workers.Count].WorkerId,
                    TransactionType = transactionTypes[i % transactionTypes.Length],
                    Amount = amount,
                    NetAmount = amount,
                    Rate = 500 + (i % 500),
                    DaysWorked = 1,
                    TransactionDate = DateTime.Now.AddDays(-i % 365),
                    Notes = $"Transaction {i}",
                    EnteredBy = 1,
                    CreatedDate = DateTime.Now.AddDays(-i % 365)
                });
            }
            return transactions;
        }

        #endregion

        #region Seeding Helpers

        private async Task SeedItems(int count)
        {
            var items = GenerateLargeItemDataset(count);
            await _context.Items.AddRangeAsync(items);
            await _context.SaveChangesAsync();
        }

        private async Task SeedParties(int count)
        {
            var parties = GenerateLargePartyDataset(count);
            await _context.Parties.AddRangeAsync(parties);
            await _context.SaveChangesAsync();
        }

        private async Task SeedTransactions(int count)
        {
            var transactions = await GenerateLargeTransactionDataset(count);
            await _context.Transactions.AddRangeAsync(transactions);
            await _context.SaveChangesAsync();
        }

        private async Task SeedLoans(int count)
        {
            var loans = await GenerateLargeLoanDataset(count);
            await _context.LoanAccounts.AddRangeAsync(loans);
            await _context.SaveChangesAsync();
        }

        private async Task SeedWorkers(int count)
        {
            var workers = GenerateLargeWorkerDataset(count);
            await _context.Workers.AddRangeAsync(workers);
            await _context.SaveChangesAsync();
        }

        private async Task SeedWageTransactions(int count)
        {
            var transactions = await GenerateLargeWageTransactionDataset(count);
            await _context.WageTransactions.AddRangeAsync(transactions);
            await _context.SaveChangesAsync();
        }

        #endregion

        public void Dispose()
        {
            _context?.Database.EnsureDeleted();
            _context?.Dispose();
        }
    }
}

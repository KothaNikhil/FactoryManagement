using FactoryManagement.Data.Repositories;
using FactoryManagement.Models;
using FactoryManagement.Services;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FactoryManagement.Tests.TestHelpers
{
    /// <summary>
    /// Factory class for creating common mock objects used across tests
    /// </summary>
    public static class MockFactories
    {
        /// <summary>
        /// Creates a mock Item Service with preset data
        /// </summary>
        public static Mock<IItemService> CreateMockItemService(List<Item>? items = null)
        {
            var mock = new Mock<IItemService>();
            var testItems = items ?? new List<Item>
            {
                new Item { ItemId = 1, ItemName = "Item1", CurrentStock = 100, Unit = "kg" },
                new Item { ItemId = 2, ItemName = "Item2", CurrentStock = 50, Unit = "pcs" }
            };

            mock.Setup(s => s.GetAllItemsAsync()).ReturnsAsync(testItems);
            mock.Setup(s => s.GetItemByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => testItems.FirstOrDefault(i => i.ItemId == id));

            return mock;
        }

        /// <summary>
        /// Creates a mock Party Service with preset data
        /// </summary>
        public static Mock<IPartyService> CreateMockPartyService(List<Party>? parties = null)
        {
            var mock = new Mock<IPartyService>();
            var testParties = parties ?? new List<Party>
            {
                new Party { PartyId = 1, Name = "Party1", PartyType = PartyType.Buyer },
                new Party { PartyId = 2, Name = "Party2", PartyType = PartyType.Seller }
            };

            mock.Setup(s => s.GetAllPartiesAsync()).ReturnsAsync(testParties);
            mock.Setup(s => s.GetPartyByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => testParties.FirstOrDefault(p => p.PartyId == id));

            return mock;
        }

        /// <summary>
        /// Creates a mock Transaction Service with preset data
        /// </summary>
        public static Mock<ITransactionService> CreateMockTransactionService(List<Transaction>? transactions = null)
        {
            var mock = new Mock<ITransactionService>();
            var testTransactions = transactions ?? new List<Transaction>
            {
                new Transaction { TransactionId = 1, ItemId = 1, Quantity = 10, TransactionType = TransactionType.Buy },
                new Transaction { TransactionId = 2, ItemId = 2, Quantity = 5, TransactionType = TransactionType.Sell }
            };

            mock.Setup(s => s.GetAllTransactionsAsync()).ReturnsAsync(testTransactions);
            mock.Setup(s => s.GetTransactionByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => testTransactions.FirstOrDefault(t => t.TransactionId == id));
            mock.Setup(s => s.GetRecentTransactionsAsync(It.IsAny<int>()))
                .ReturnsAsync((int count) => testTransactions.OrderByDescending(t => t.TransactionDate).Take(count));

            return mock;
        }

        /// <summary>
        /// Creates a mock Wage Service with preset data
        /// </summary>
        public static Mock<IWageService> CreateMockWageService(List<Worker>? workers = null)
        {
            var mock = new Mock<IWageService>();
            var testWorkers = workers ?? new List<Worker>
            {
                new Worker { WorkerId = 1, Name = "Worker1", Rate = 500, Status = WorkerStatus.Active },
                new Worker { WorkerId = 2, Name = "Worker2", Rate = 600, Status = WorkerStatus.Active }
            };

            mock.Setup(s => s.GetAllWorkersAsync()).ReturnsAsync(testWorkers);
            mock.Setup(s => s.GetWorkerByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => testWorkers.FirstOrDefault(w => w.WorkerId == id));
            mock.Setup(s => s.GetActiveWorkersAsync())
                .ReturnsAsync(testWorkers.Where(w => w.Status == WorkerStatus.Active));

            return mock;
        }

        /// <summary>
        /// Creates a mock Item Repository
        /// </summary>
        public static Mock<IRepository<Item>> CreateMockItemRepository(List<Item>? items = null)
        {
            var mock = new Mock<IRepository<Item>>();
            var testItems = items ?? new List<Item>
            {
                new Item { ItemId = 1, ItemName = "Item1", CurrentStock = 100, Unit = "kg" }
            };

            mock.Setup(r => r.GetAllAsync()).ReturnsAsync(testItems);
            mock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => testItems.FirstOrDefault(i => i.ItemId == id));

            return mock;
        }

        /// <summary>
        /// Creates a mock Party Repository
        /// </summary>
        public static Mock<IRepository<Party>> CreateMockPartyRepository(List<Party>? parties = null)
        {
            var mock = new Mock<IRepository<Party>>();
            var testParties = parties ?? new List<Party>
            {
                new Party { PartyId = 1, Name = "Party1", PartyType = PartyType.Both }
            };

            mock.Setup(r => r.GetAllAsync()).ReturnsAsync(testParties);
            mock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => testParties.FirstOrDefault(p => p.PartyId == id));

            return mock;
        }

        /// <summary>
        /// Creates a mock User Repository
        /// </summary>
        public static Mock<IRepository<User>> CreateMockUserRepository(List<User>? users = null)
        {
            var mock = new Mock<IRepository<User>>();
            var testUsers = users ?? new List<User>
            {
                new User { UserId = 1, Username = "admin", Role = "Admin", IsActive = true },
                new User { UserId = 2, Username = "user", Role = "User", IsActive = true }
            };

            mock.Setup(r => r.GetAllAsync()).ReturnsAsync(testUsers);
            mock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => testUsers.FirstOrDefault(u => u.UserId == id));

            return mock;
        }

        /// <summary>
        /// Creates a mock Export Service
        /// </summary>
        public static Mock<IExportService> CreateMockExportService()
        {
            var mock = new Mock<IExportService>();
            
            mock.Setup(s => s.ExportToExcelAsync(
                It.IsAny<IEnumerable<object>>(), 
                It.IsAny<string>(), 
                It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            mock.Setup(s => s.ExportToCsvAsync(
                It.IsAny<IEnumerable<object>>(), 
                It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            return mock;
        }

        /// <summary>
        /// Creates a mock Financial Transaction Service
        /// </summary>
        public static Mock<FinancialTransactionService> CreateMockFinancialTransactionService()
        {
            var mockFinancialRepo = new Mock<IFinancialTransactionRepository>();
            var mockLoanRepo = new Mock<ILoanAccountRepository>();
            var mockContext = new Mock<FactoryManagement.Data.FactoryDbContext>();

            var mock = new Mock<FinancialTransactionService>(
                mockFinancialRepo.Object,
                mockLoanRepo.Object,
                mockContext.Object);

            mock.Setup(s => s.GetAllLoansAsync())
                .ReturnsAsync(new List<LoanAccount>());
            mock.Setup(s => s.GetAllFinancialTransactionsAsync())
                .ReturnsAsync(new List<FinancialTransaction>());

            return mock;
        }

        /// <summary>
        /// Creates a mock Transaction Repository
        /// </summary>
        public static Mock<ITransactionRepository> CreateMockTransactionRepository(List<Transaction>? transactions = null)
        {
            var mock = new Mock<ITransactionRepository>();
            var testTransactions = transactions ?? new List<Transaction>();

            mock.Setup(r => r.GetAllAsync()).ReturnsAsync(testTransactions);
            mock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => testTransactions.FirstOrDefault(t => t.TransactionId == id));

            return mock;
        }

        /// <summary>
        /// Creates a mock Worker Repository
        /// </summary>
        public static Mock<IWorkerRepository> CreateMockWorkerRepository(List<Worker>? workers = null)
        {
            var mock = new Mock<IWorkerRepository>();
            var testWorkers = workers ?? new List<Worker>();

            mock.Setup(r => r.GetAllAsync()).ReturnsAsync(testWorkers);
            mock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => testWorkers.FirstOrDefault(w => w.WorkerId == id));

            return mock;
        }

        /// <summary>
        /// Creates a mock Wage Transaction Repository
        /// </summary>
        public static Mock<IWageTransactionRepository> CreateMockWageTransactionRepository(List<WageTransaction>? transactions = null)
        {
            var mock = new Mock<IWageTransactionRepository>();
            var testTransactions = transactions ?? new List<WageTransaction>();

            mock.Setup(r => r.GetAllAsync()).ReturnsAsync(testTransactions);
            mock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => testTransactions.FirstOrDefault(t => t.WageTransactionId == id));

            return mock;
        }
    }
}

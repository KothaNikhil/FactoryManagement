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
    /// End-to-End tests for wage management and payroll workflows
    /// </summary>
    public class WageManagementWorkflowTests : IDisposable
    {
        private readonly DbContextOptions<FactoryDbContext> _options;
        private readonly FactoryDbContext _context;
        private readonly WageService _wageService;

        public WageManagementWorkflowTests()
        {
            _options = new DbContextOptionsBuilder<FactoryDbContext>()
                .UseInMemoryDatabase(databaseName: $"E2EWageTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new FactoryDbContext(_options);
            
            var workerRepo = new WorkerRepository(_context);
            var wageTransactionRepo = new WageTransactionRepository(_context);

            _wageService = new WageService(workerRepo, wageTransactionRepo);
        }

        [Fact]
        public async Task WageWorkflow_AddWorker_RecordDailyWage_CalculateTotal()
        {
            // Step 1: Add new worker
            var worker = await _wageService.AddWorkerAsync(new Worker
            {
                Name = "John Doe",
                MobileNumber = "1234567890",
                Address = "Worker Address",
                Rate = 500,
                DailyRate = 500,
                Status = WorkerStatus.Active,
                JoiningDate = DateTime.Now
            });

            Assert.NotNull(worker);
            Assert.Equal("John Doe", worker.Name);
            Assert.Equal(500, worker.Rate);

            // Step 2: Record daily wage payment
            var wageTransaction = await _wageService.RecordWagePaymentAsync(new WageTransaction
            {
                WorkerId = worker.WorkerId,
                TransactionType = WageTransactionType.DailyWage,
                Amount = 500,
                NetAmount = 500,
                Rate = 500,
                DaysWorked = 1,
                TransactionDate = DateTime.Now,
                Notes = "Day 1 wage",
                EnteredBy = 1
            });

            Assert.NotNull(wageTransaction);
            Assert.Equal(500, wageTransaction.Amount);

            // Step 3: Record multiple days
            for (int i = 2; i <= 5; i++)
            {
                await _wageService.RecordWagePaymentAsync(new WageTransaction
                {
                    WorkerId = worker.WorkerId,
                    TransactionType = WageTransactionType.DailyWage,
                    Amount = 500,
                    NetAmount = 500,
                    Rate = 500,
                    DaysWorked = 1,
                    TransactionDate = DateTime.Now.AddDays(i - 1),
                    Notes = $"Day {i} wage",
                    EnteredBy = 1
                });
            }

            // Assert - Calculate total wages
            var allTransactions = await _wageService.GetAllWageTransactionsAsync();
            var workerTransactions = allTransactions.Where(t => t.WorkerId == worker.WorkerId).ToList();
            Assert.Equal(5, workerTransactions.Count);
            
            var totalWages = workerTransactions.Sum(t => t.NetAmount);
            Assert.Equal(2500, totalWages); // 5 days * 500
        }

        [Fact]
        public async Task WageWorkflow_MonthlyWage_WithBonus_ShouldCalculateCorrectly()
        {
            // Step 1: Add monthly worker
            var worker = await _wageService.AddWorkerAsync(new Worker
            {
                Name = "Jane Smith",
                MobileNumber = "9876543210",
                Rate = 25000,
                MonthlyRate = 25000,
                Status = WorkerStatus.Active,
                JoiningDate = DateTime.Now.AddMonths(-1)
            });

            // Step 2: Record monthly salary
            await _wageService.RecordWagePaymentAsync(new WageTransaction
            {
                WorkerId = worker.WorkerId,
                TransactionType = WageTransactionType.MonthlyWage,
                Amount = 25000,
                NetAmount = 25000,
                Rate = 25000,
                TransactionDate = DateTime.Now,
                Notes = "Monthly salary",
                EnteredBy = 1
            });

            // Step 3: Record performance bonus
            await _wageService.RecordWagePaymentAsync(new WageTransaction
            {
                WorkerId = worker.WorkerId,
                TransactionType = WageTransactionType.Bonus,
                Amount = 5000,
                NetAmount = 5000,
                TransactionDate = DateTime.Now,
                Notes = "Performance bonus",
                EnteredBy = 1
            });

            // Assert
            var transactions = await _wageService.GetWorkerTransactionsAsync(worker.WorkerId);
            Assert.Equal(2, transactions.Count());
            
            var totalEarnings = transactions.Sum(t => t.NetAmount);
            Assert.Equal(30000, totalEarnings); // 25000 + 5000
        }

        [Fact]
        public async Task WageWorkflow_AdvanceGiven_AdvanceAdjustment_ShouldTrackCorrectly()
        {
            // Step 1: Add worker
            var worker = await _wageService.AddWorkerAsync(new Worker
            {
                Name = "Mike Johnson",
                Rate = 600,
                DailyRate = 600,
                Status = WorkerStatus.Active,
                JoiningDate = DateTime.Now
            });

            // Step 2: Give advance
            await _wageService.RecordAdvanceAsync(worker.WorkerId, 5000, "Advance payment");

            // Step 3: Record daily wages for a month (25 working days)
            for (int i = 1; i <= 25; i++)
            {
                await _wageService.RecordWagePaymentAsync(new WageTransaction
                {
                    WorkerId = worker.WorkerId,
                    TransactionType = WageTransactionType.DailyWage,
                    Amount = 600,
                    NetAmount = 600,
                    Rate = 600,
                    DaysWorked = 1,
                    TransactionDate = DateTime.Now.AddDays(i),
                    EnteredBy = 1
                });
            }

            // Step 4: Adjust advance
            await _wageService.RecordWagePaymentAsync(new WageTransaction
            {
                WorkerId = worker.WorkerId,
                TransactionType = WageTransactionType.AdvanceAdjustment,
                Amount = 5000,
                NetAmount = -5000, // Deduction
                TransactionDate = DateTime.Now.AddDays(30),
                Notes = "Advance adjustment",
                EnteredBy = 1
            });

            // Assert
            var transactions = await _wageService.GetWorkerTransactionsAsync(worker.WorkerId);
            
            var totalWages = transactions
                .Where(t => t.TransactionType == WageTransactionType.DailyWage)
                .Sum(t => t.NetAmount);
            Assert.Equal(15000, totalWages); // 25 * 600

            var advanceGiven = transactions
                .Where(t => t.TransactionType == WageTransactionType.AdvanceGiven)
                .Sum(t => t.NetAmount);
            Assert.Equal(5000, advanceGiven);

            var advanceAdjusted = transactions
                .Where(t => t.TransactionType == WageTransactionType.AdvanceAdjustment)
                .Sum(t => t.NetAmount);
            Assert.Equal(-5000, advanceAdjusted);

            var netPayable = totalWages - advanceGiven + advanceAdjusted;
            Assert.Equal(5000, netPayable); // 15000 - 5000 - 5000
        }

        [Fact]
        public async Task WageWorkflow_MultipleWorkers_CalculatePayroll()
        {
            // Arrange - Create multiple workers
            var worker1 = await _wageService.AddWorkerAsync(new Worker
            {
                Name = "Worker 1",
                Rate = 500,
                DailyRate = 500,
                Status = WorkerStatus.Active
            });

            var worker2 = await _wageService.AddWorkerAsync(new Worker
            {
                Name = "Worker 2",
                Rate = 600,
                DailyRate = 600,
                Status = WorkerStatus.Active
            });

            var worker3 = await _wageService.AddWorkerAsync(new Worker
            {
                Name = "Worker 3",
                Rate = 30000,
                MonthlyRate = 30000,
                Status = WorkerStatus.Active
            });

            // Record wages
            // Worker 1 - 20 days
            for (int i = 0; i < 20; i++)
            {
                await _wageService.RecordWagePaymentAsync(new WageTransaction
                {
                    WorkerId = worker1.WorkerId,
                    TransactionType = WageTransactionType.DailyWage,
                    Amount = 500,
                    NetAmount = 500,
                    Rate = 500,
                    DaysWorked = 1,
                    TransactionDate = DateTime.Now.AddDays(i),
                    EnteredBy = 1
                });
            }

            // Worker 2 - 22 days
            for (int i = 0; i < 22; i++)
            {
                await _wageService.RecordWagePaymentAsync(new WageTransaction
                {
                    WorkerId = worker2.WorkerId,
                    TransactionType = WageTransactionType.DailyWage,
                    Amount = 600,
                    NetAmount = 600,
                    Rate = 600,
                    DaysWorked = 1,
                    TransactionDate = DateTime.Now.AddDays(i),
                    EnteredBy = 1
                });
            }

            // Worker 3 - Monthly salary
            await _wageService.RecordWagePaymentAsync(new WageTransaction
            {
                WorkerId = worker3.WorkerId,
                TransactionType = WageTransactionType.MonthlyWage,
                Amount = 30000,
                NetAmount = 30000,
                Rate = 30000,
                TransactionDate = DateTime.Now,
                EnteredBy = 1
            });

            // Assert - Calculate total payroll
            var allTransactions = await _wageService.GetAllWageTransactionsAsync();
            
            var worker1Total = allTransactions.Where(t => t.WorkerId == worker1.WorkerId).Sum(t => t.NetAmount);
            var worker2Total = allTransactions.Where(t => t.WorkerId == worker2.WorkerId).Sum(t => t.NetAmount);
            var worker3Total = allTransactions.Where(t => t.WorkerId == worker3.WorkerId).Sum(t => t.NetAmount);

            Assert.Equal(10000, worker1Total); // 20 * 500
            Assert.Equal(13200, worker2Total); // 22 * 600
            Assert.Equal(30000, worker3Total);

            var totalPayroll = worker1Total + worker2Total + worker3Total;
            Assert.Equal(53200, totalPayroll);
        }

        [Fact]
        public async Task WageWorkflow_FilterByWorker_ShouldReturnCorrectTransactions()
        {
            // Arrange
            var worker1 = await _wageService.AddWorkerAsync(new Worker { Name = "Worker A", Rate = 500, Status = WorkerStatus.Active });
            var worker2 = await _wageService.AddWorkerAsync(new Worker { Name = "Worker B", Rate = 600, Status = WorkerStatus.Active });

            // Add transactions for both workers
            await _wageService.RecordWagePaymentAsync(new WageTransaction
            {
                WorkerId = worker1.WorkerId,
                TransactionType = WageTransactionType.DailyWage,
                Amount = 500,
                NetAmount = 500,
                Rate = 500,
                DaysWorked = 1,
                TransactionDate = DateTime.Now,
                EnteredBy = 1
            });

            await _wageService.RecordWagePaymentAsync(new WageTransaction
            {
                WorkerId = worker2.WorkerId,
                TransactionType = WageTransactionType.DailyWage,
                Amount = 600,
                NetAmount = 600,
                Rate = 600,
                DaysWorked = 1,
                TransactionDate = DateTime.Now,
                EnteredBy = 1
            });

            // Act
            var worker1Transactions = await _wageService.GetWorkerTransactionsAsync(worker1.WorkerId);
            var worker2Transactions = await _wageService.GetWorkerTransactionsAsync(worker2.WorkerId);

            // Assert
            Assert.Single(worker1Transactions);
            Assert.Single(worker2Transactions);
            Assert.Equal(500, worker1Transactions.First().Amount);
            Assert.Equal(600, worker2Transactions.First().Amount);
        }

        [Fact]
        public async Task WageWorkflow_WorkerStatus_ShouldTrackActiveInactive()
        {
            // Arrange - Create workers with different statuses
            var activeWorker = await _wageService.AddWorkerAsync(new Worker
            {
                Name = "Active Worker",
                Rate = 500,
                Status = WorkerStatus.Active,
                JoiningDate = DateTime.Now
            });

            var inactiveWorker = await _wageService.AddWorkerAsync(new Worker
            {
                Name = "Inactive Worker",
                Rate = 600,
                Status = WorkerStatus.Inactive,
                JoiningDate = DateTime.Now.AddMonths(-6)
            });

            var onLeaveWorker = await _wageService.AddWorkerAsync(new Worker
            {
                Name = "On Leave Worker",
                Rate = 550,
                Status = WorkerStatus.OnLeave,
                JoiningDate = DateTime.Now.AddMonths(-3)
            });

            // Act - Get all workers
            var allWorkers = await _wageService.GetAllWorkersAsync();

            // Assert
            Assert.Contains(allWorkers, w => w.Status == WorkerStatus.Active);
            Assert.Contains(allWorkers, w => w.Status == WorkerStatus.Inactive);
            Assert.Contains(allWorkers, w => w.Status == WorkerStatus.OnLeave);

            // Verify specific workers
            var active = await _wageService.GetWorkerByIdAsync(activeWorker.WorkerId);
            Assert.NotNull(active);
            Assert.Equal(WorkerStatus.Active, active!.Status);
        }

        [Fact]
        public async Task WageWorkflow_SearchWorkers_ShouldFindByName()
        {
            // Arrange
            await _wageService.AddWorkerAsync(new Worker { Name = "John Smith", Rate = 500, Status = WorkerStatus.Active });
            await _wageService.AddWorkerAsync(new Worker { Name = "John Doe", Rate = 600, Status = WorkerStatus.Active });
            await _wageService.AddWorkerAsync(new Worker { Name = "Jane Doe", Rate = 550, Status = WorkerStatus.Active });

            // Act
            var johnResults = (await _wageService.GetAllWorkersAsync()).Where(w => w.Name.Contains("John", StringComparison.OrdinalIgnoreCase));
            var doeResults = (await _wageService.GetAllWorkersAsync()).Where(w => w.Name.Contains("Doe", StringComparison.OrdinalIgnoreCase));

            // Assert
            Assert.Equal(2, johnResults.Count()); // John Smith and John Doe
            Assert.Equal(2, doeResults.Count());   // John Doe and Jane Doe
        }

        [Fact]
        public async Task WageWorkflow_CompletePayrollCycle_ShouldHandleAllScenarios()
        {
            // Complete payroll cycle for a month
            var worker = await _wageService.AddWorkerAsync(new Worker
            {
                Name = "Complete Cycle Worker",
                Rate = 700,
                DailyRate = 700,
                Status = WorkerStatus.Active,
                JoiningDate = DateTime.Now.AddMonths(-1)
            });

            // Give advance at month start
            await _wageService.RecordAdvanceAsync(worker.WorkerId, 3000, "Month start advance");

            // Record daily wages (25 working days)
            for (int i = 1; i <= 25; i++)
            {
                await _wageService.RecordWagePaymentAsync(new WageTransaction
                {
                    WorkerId = worker.WorkerId,
                    TransactionType = WageTransactionType.DailyWage,
                    Amount = 700,
                    NetAmount = 700,
                    Rate = 700,
                    DaysWorked = 1,
                    TransactionDate = DateTime.Now.AddDays(i),
                    EnteredBy = 1
                });
            }

            // Add overtime for 3 days
            await _wageService.RecordWagePaymentAsync(new WageTransaction
            {
                WorkerId = worker.WorkerId,
                TransactionType = WageTransactionType.OvertimePay,
                Amount = 500,
                NetAmount = 500,
                TransactionDate = DateTime.Now.AddDays(15),
                Notes = "Overtime payment",
                EnteredBy = 1
            });

            // Add bonus
            await _wageService.RecordWagePaymentAsync(new WageTransaction
            {
                WorkerId = worker.WorkerId,
                TransactionType = WageTransactionType.Bonus,
                Amount = 2000,
                NetAmount = 2000,
                TransactionDate = DateTime.Now.AddDays(30),
                Notes = "Performance bonus",
                EnteredBy = 1
            });

            // Adjust advance
            await _wageService.RecordWagePaymentAsync(new WageTransaction
            {
                WorkerId = worker.WorkerId,
                TransactionType = WageTransactionType.AdvanceAdjustment,
                Amount = 3000,
                NetAmount = -3000,
                TransactionDate = DateTime.Now.AddDays(30),
                Notes = "Advance deduction",
                EnteredBy = 1
            });

            // Assert - Calculate final payable amount
            var transactions = await _wageService.GetWorkerTransactionsAsync(worker.WorkerId);
            
            var dailyWages = transactions.Where(t => t.TransactionType == WageTransactionType.DailyWage).Sum(t => t.NetAmount);
            var overtime = transactions.Where(t => t.TransactionType == WageTransactionType.OvertimePay).Sum(t => t.NetAmount);
            var bonus = transactions.Where(t => t.TransactionType == WageTransactionType.Bonus).Sum(t => t.NetAmount);
            var advance = transactions.Where(t => t.TransactionType == WageTransactionType.AdvanceGiven).Sum(t => t.NetAmount);
            var deduction = transactions.Where(t => t.TransactionType == WageTransactionType.AdvanceAdjustment).Sum(t => t.NetAmount);

            Assert.Equal(17500, dailyWages); // 25 * 700
            Assert.Equal(500, overtime);
            Assert.Equal(2000, bonus);
            Assert.Equal(3000, advance);
            Assert.Equal(-3000, deduction);

            var netPayable = dailyWages + overtime + bonus - advance + deduction;
            Assert.Equal(14000, netPayable); // 17500 + 500 + 2000 - 3000 - 3000
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}

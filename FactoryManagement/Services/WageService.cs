using FactoryManagement.Data.Repositories;
using FactoryManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FactoryManagement.Services
{
    public interface IWageService
    {
        // Worker Operations
        Task<IEnumerable<Worker>> GetAllWorkersAsync();
        Task<IEnumerable<Worker>> GetActiveWorkersAsync();
        Task<Worker?> GetWorkerByIdAsync(int id);
        Task<Worker> AddWorkerAsync(Worker worker);
        Task UpdateWorkerAsync(Worker worker);
        Task DeleteWorkerAsync(int id);

        // Wage Transaction Operations
        Task<WageTransaction> RecordWagePaymentAsync(WageTransaction transaction);
        Task<WageTransaction> RecordAdvanceAsync(int workerId, decimal amount, string notes);
        Task<IEnumerable<WageTransaction>> GetWorkerTransactionsAsync(int workerId);
        Task<IEnumerable<WageTransaction>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate);
        
        // Summary Operations
        Task<decimal> GetTotalWagesPaidAsync();
        Task<decimal> GetTotalAdvancesGivenAsync();
        Task<decimal> GetWorkerTotalWagesAsync(int workerId);
        Task<decimal> GetWorkerTotalAdvancesAsync(int workerId);
        Task<decimal> GetWorkerOutstandingAdvanceAsync(int workerId);
    }

    public class WageService : IWageService
    {
        private readonly IWorkerRepository _workerRepository;
        private readonly IWageTransactionRepository _wageTransactionRepository;

        public WageService(IWorkerRepository workerRepository, IWageTransactionRepository wageTransactionRepository)
        {
            _workerRepository = workerRepository;
            _wageTransactionRepository = wageTransactionRepository;
        }

        // Worker Operations
        public async Task<IEnumerable<Worker>> GetAllWorkersAsync()
        {
            return await _workerRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Worker>> GetActiveWorkersAsync()
        {
            return await _workerRepository.GetActiveWorkersAsync();
        }

        public async Task<Worker?> GetWorkerByIdAsync(int id)
        {
            return await _workerRepository.GetByIdAsync(id);
        }

        public async Task<Worker> AddWorkerAsync(Worker worker)
        {
            worker.CreatedDate = DateTime.Now;
            worker.TotalAdvance = 0;
            worker.TotalWagesPaid = 0;
            return await _workerRepository.AddAsync(worker);
        }

        public async Task UpdateWorkerAsync(Worker worker)
        {
            worker.ModifiedDate = DateTime.Now;
            await _workerRepository.UpdateAsync(worker);
        }

        public async Task DeleteWorkerAsync(int id)
        {
            var worker = await _workerRepository.GetByIdAsync(id);
            if (worker != null)
            {
                await _workerRepository.DeleteAsync(worker);
            }
        }

        // Wage Transaction Operations
        public async Task<WageTransaction> RecordWagePaymentAsync(WageTransaction transaction)
        {
            transaction.CreatedDate = DateTime.Now;

            // Calculate net amount if not provided
            if (transaction.NetAmount == 0)
            {
                transaction.NetAmount = transaction.Amount 
                    + (transaction.OvertimeAmount ?? 0) 
                    - (transaction.AdvanceAdjusted ?? 0) 
                    - (transaction.Deductions ?? 0);
            }

            // Save transaction
            var savedTransaction = await _wageTransactionRepository.AddAsync(transaction);

            // Update worker totals
            var worker = await _workerRepository.GetByIdAsync(transaction.WorkerId);
            if (worker != null)
            {
                if (transaction.TransactionType == WageTransactionType.DailyWage ||
                    transaction.TransactionType == WageTransactionType.HourlyWage ||
                    transaction.TransactionType == WageTransactionType.MonthlyWage ||
                    transaction.TransactionType == WageTransactionType.OvertimePay ||
                    transaction.TransactionType == WageTransactionType.Bonus)
                {
                    worker.TotalWagesPaid += transaction.NetAmount;
                }

                // Add to advance balance if this is an advance given
                if (transaction.TransactionType == WageTransactionType.AdvanceGiven)
                {
                    worker.TotalAdvance += transaction.Amount;
                }

                // Reduce advance balance if this is an advance returned
                if (transaction.TransactionType == WageTransactionType.AdvanceAdjustment)
                {
                    worker.TotalAdvance -= transaction.Amount;
                    if (worker.TotalAdvance < 0) worker.TotalAdvance = 0;
                }

                // Adjust advance balance from AdvanceAdjusted field (legacy)
                if (transaction.AdvanceAdjusted.HasValue && transaction.AdvanceAdjusted.Value > 0)
                {
                    worker.TotalAdvance -= transaction.AdvanceAdjusted.Value;
                    if (worker.TotalAdvance < 0) worker.TotalAdvance = 0;
                }

                worker.ModifiedDate = DateTime.Now;
                await _workerRepository.UpdateAsync(worker);
            }

            return savedTransaction;
        }

        public async Task<WageTransaction> RecordAdvanceAsync(int workerId, decimal amount, string notes)
        {
            var advance = new WageTransaction
            {
                WorkerId = workerId,
                TransactionType = WageTransactionType.AdvanceGiven,
                TransactionDate = DateTime.Now,
                Amount = amount,
                NetAmount = amount,
                Notes = notes,
                CreatedDate = DateTime.Now
            };

            var savedAdvance = await _wageTransactionRepository.AddAsync(advance);

            // Update worker's total advance
            var worker = await _workerRepository.GetByIdAsync(workerId);
            if (worker != null)
            {
                worker.TotalAdvance += amount;
                worker.ModifiedDate = DateTime.Now;
                await _workerRepository.UpdateAsync(worker);
            }

            return savedAdvance;
        }

        public async Task<IEnumerable<WageTransaction>> GetWorkerTransactionsAsync(int workerId)
        {
            return await _wageTransactionRepository.GetByWorkerIdAsync(workerId);
        }

        public async Task<IEnumerable<WageTransaction>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _wageTransactionRepository.GetByDateRangeAsync(startDate, endDate);
        }

        // Summary Operations
        public async Task<decimal> GetTotalWagesPaidAsync()
        {
            return await _wageTransactionRepository.GetTotalWagesPaidAsync();
        }

        public async Task<decimal> GetTotalAdvancesGivenAsync()
        {
            return await _wageTransactionRepository.GetTotalAdvancesGivenAsync();
        }

        public async Task<decimal> GetWorkerTotalWagesAsync(int workerId)
        {
            var transactions = await _wageTransactionRepository.GetByWorkerIdAsync(workerId);
            return transactions
                .Where(t => t.TransactionType == WageTransactionType.DailyWage ||
                           t.TransactionType == WageTransactionType.HourlyWage ||
                           t.TransactionType == WageTransactionType.MonthlyWage ||
                           t.TransactionType == WageTransactionType.OvertimePay ||
                           t.TransactionType == WageTransactionType.Bonus)
                .Sum(t => t.NetAmount);
        }

        public async Task<decimal> GetWorkerTotalAdvancesAsync(int workerId)
        {
            return await _workerRepository.GetTotalAdvancesByWorkerAsync(workerId);
        }

        public async Task<decimal> GetWorkerOutstandingAdvanceAsync(int workerId)
        {
            var worker = await _workerRepository.GetByIdAsync(workerId);
            return worker?.TotalAdvance ?? 0;
        }
    }
}

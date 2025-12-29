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
        Task<Worker> AddWorkerAsync(Worker worker, int? userId = null);
        Task UpdateWorkerAsync(Worker worker, int? userId = null);
        Task DeleteWorkerAsync(int id);

        // Wage Transaction Operations
        Task<WageTransaction> RecordWagePaymentAsync(WageTransaction transaction, int? userId = null);
        Task<WageTransaction> RecordAdvanceAsync(int workerId, decimal amount, string notes, int? userId = null);
        Task<IEnumerable<WageTransaction>> GetWorkerTransactionsAsync(int workerId);
        Task<IEnumerable<WageTransaction>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<WageTransaction>> GetAllWageTransactionsAsync();
        Task DeleteWageTransactionAsync(int wageTransactionId);
        Task RestoreWageTransactionAsync(WageTransaction transaction);
        Task UpdateWagePaymentAsync(WageTransaction updated, int? userId = null);
        
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

        public async Task<Worker> AddWorkerAsync(Worker worker, int? userId = null)
        {
            worker.CreatedDate = DateTime.Now;
            worker.CreatedByUserId = userId;
            worker.TotalAdvance = 0;
            worker.TotalWagesPaid = 0;
            return await _workerRepository.AddAsync(worker);
        }

        public async Task UpdateWorkerAsync(Worker worker, int? userId = null)
        {
            worker.ModifiedDate = DateTime.Now;
            worker.ModifiedByUserId = userId;
            await _workerRepository.UpdateAsync(worker);
        }

        public async Task DeleteWorkerAsync(int id)
        {
            var worker = await _workerRepository.GetByIdAsync(id);
            if (worker != null)
            {
                // Soft delete: mark worker inactive to retain wage history
                worker.Status = WorkerStatus.Inactive;
                worker.LeavingDate = DateTime.Now;
                worker.ModifiedDate = DateTime.Now;
                await _workerRepository.UpdateAsync(worker);
            }
        }

        // Wage Transaction Operations
        public async Task<WageTransaction> RecordWagePaymentAsync(WageTransaction transaction, int? userId = null)
        {
            transaction.CreatedDate = DateTime.Now;
            if (userId.HasValue)
            {
                transaction.EnteredBy = userId.Value;
            }

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

        public async Task<WageTransaction> RecordAdvanceAsync(int workerId, decimal amount, string notes, int? userId = null)
        {
            var advance = new WageTransaction
            {
                WorkerId = workerId,
                TransactionType = WageTransactionType.AdvanceGiven,
                TransactionDate = DateTime.Now,
                Amount = amount,
                NetAmount = amount,
                Notes = notes,
                CreatedDate = DateTime.Now,
                EnteredBy = userId ?? 1
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

        public async Task<IEnumerable<WageTransaction>> GetAllWageTransactionsAsync()
        {
            return await _wageTransactionRepository.GetAllAsync();
        }

        public async Task DeleteWageTransactionAsync(int wageTransactionId)
        {
            var tx = await _wageTransactionRepository.GetByIdAsync(wageTransactionId);
            if (tx == null) return;

            var worker = await _workerRepository.GetByIdAsync(tx.WorkerId);
            if (worker != null)
            {
                // Reverse worker totals based on transaction type
                switch (tx.TransactionType)
                {
                    case WageTransactionType.DailyWage:
                    case WageTransactionType.HourlyWage:
                    case WageTransactionType.MonthlyWage:
                    case WageTransactionType.OvertimePay:
                    case WageTransactionType.Bonus:
                        worker.TotalWagesPaid = Math.Max(0, worker.TotalWagesPaid - tx.NetAmount);
                        break;
                    case WageTransactionType.AdvanceGiven:
                        worker.TotalAdvance = Math.Max(0, worker.TotalAdvance - tx.Amount);
                        break;
                    case WageTransactionType.AdvanceAdjustment:
                        // Deleting an advance adjustment re-adds the amount to outstanding advance
                        worker.TotalAdvance += tx.Amount;
                        break;
                }
                worker.ModifiedDate = DateTime.Now;
                await _workerRepository.UpdateAsync(worker);
            }

            await _wageTransactionRepository.DeleteAsync(tx);
        }

        public async Task RestoreWageTransactionAsync(WageTransaction transaction)
        {
            // Update worker totals forward based on transaction type
            var worker = await _workerRepository.GetByIdAsync(transaction.WorkerId);
            if (worker != null)
            {
                switch (transaction.TransactionType)
                {
                    case WageTransactionType.DailyWage:
                    case WageTransactionType.HourlyWage:
                    case WageTransactionType.MonthlyWage:
                    case WageTransactionType.OvertimePay:
                    case WageTransactionType.Bonus:
                        worker.TotalWagesPaid += transaction.NetAmount;
                        break;
                    case WageTransactionType.AdvanceGiven:
                        worker.TotalAdvance += transaction.Amount;
                        break;
                    case WageTransactionType.AdvanceAdjustment:
                        worker.TotalAdvance = Math.Max(0, worker.TotalAdvance - transaction.Amount);
                        break;
                }
                worker.ModifiedDate = DateTime.Now;
                await _workerRepository.UpdateAsync(worker);
            }

            await _wageTransactionRepository.AddAsync(transaction);
        }

        public async Task UpdateWagePaymentAsync(WageTransaction updated, int? userId = null)
        {
            var existing = await _wageTransactionRepository.GetByIdAsync(updated.WageTransactionId);
            if (existing == null)
            {
                throw new InvalidOperationException("Transaction not found for update.");
            }

            // Ensure timestamps and ownership
            updated.CreatedDate = existing.CreatedDate;
            updated.ModifiedDate = DateTime.Now;
            updated.TransactionDate = existing.TransactionDate;
            updated.EnteredBy = existing.EnteredBy;

            // Recalculate net amount if not provided
            if (updated.NetAmount == 0)
            {
                updated.NetAmount = updated.Amount
                    + (updated.OvertimeAmount ?? 0)
                    - (updated.AdvanceAdjusted ?? 0)
                    - (updated.Deductions ?? 0);
            }

            // If worker changed, reverse on old worker and apply on new worker
            if (existing.WorkerId != updated.WorkerId || existing.TransactionType != updated.TransactionType || existing.Amount != updated.Amount || existing.NetAmount != updated.NetAmount)
            {
                var oldWorker = await _workerRepository.GetByIdAsync(existing.WorkerId);
                if (oldWorker != null)
                {
                    switch (existing.TransactionType)
                    {
                        case WageTransactionType.DailyWage:
                        case WageTransactionType.HourlyWage:
                        case WageTransactionType.MonthlyWage:
                        case WageTransactionType.OvertimePay:
                        case WageTransactionType.Bonus:
                            oldWorker.TotalWagesPaid = Math.Max(0, oldWorker.TotalWagesPaid - existing.NetAmount);
                            break;
                        case WageTransactionType.AdvanceGiven:
                            oldWorker.TotalAdvance = Math.Max(0, oldWorker.TotalAdvance - existing.Amount);
                            break;
                        case WageTransactionType.AdvanceAdjustment:
                            oldWorker.TotalAdvance += existing.Amount;
                            break;
                    }
                    // Legacy field reversal
                    if (existing.AdvanceAdjusted.HasValue && existing.AdvanceAdjusted.Value > 0)
                    {
                        oldWorker.TotalAdvance += existing.AdvanceAdjusted.Value;
                    }
                    oldWorker.ModifiedDate = DateTime.Now;
                    await _workerRepository.UpdateAsync(oldWorker);
                }

                var newWorker = await _workerRepository.GetByIdAsync(updated.WorkerId);
                if (newWorker != null)
                {
                    switch (updated.TransactionType)
                    {
                        case WageTransactionType.DailyWage:
                        case WageTransactionType.HourlyWage:
                        case WageTransactionType.MonthlyWage:
                        case WageTransactionType.OvertimePay:
                        case WageTransactionType.Bonus:
                            newWorker.TotalWagesPaid += updated.NetAmount;
                            break;
                        case WageTransactionType.AdvanceGiven:
                            newWorker.TotalAdvance += updated.Amount;
                            break;
                        case WageTransactionType.AdvanceAdjustment:
                            newWorker.TotalAdvance = Math.Max(0, newWorker.TotalAdvance - updated.Amount);
                            break;
                    }
                    // Legacy field application
                    if (updated.AdvanceAdjusted.HasValue && updated.AdvanceAdjusted.Value > 0)
                    {
                        newWorker.TotalAdvance = Math.Max(0, newWorker.TotalAdvance - updated.AdvanceAdjusted.Value);
                    }
                    newWorker.ModifiedDate = DateTime.Now;
                    await _workerRepository.UpdateAsync(newWorker);
                }
            }

            await _wageTransactionRepository.UpdateAsync(updated);
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

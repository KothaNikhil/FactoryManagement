using FactoryManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FactoryManagement.Services
{
    // Unified view model for displaying all transaction types in a single grid
    public class UnifiedTransactionViewModel
    {
        public string Category { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public string DebitCredit { get; set; } = string.Empty; // "Debit" or "Credit"
        public string Description { get; set; } = string.Empty; // Item name, Party name, or Worker name
        public string? Item { get; set; } // Item name for display
        public string? Name { get; set; } // Party or Worker name for display
        public string? ItemName { get; set; }
        public string? PartyName { get; set; }
        public string? WorkerName { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? Rate { get; set; }
        public decimal Amount { get; set; }
        public string? AdditionalInfo { get; set; } // For extra details like days worked, interest, etc.
        public string? Notes { get; set; }
        public string? EnteredBy { get; set; } // Username of the person who entered the transaction
        
        // Processing-specific fields
        public string? InputItemName { get; set; }
        public decimal? InputQuantity { get; set; }
        public decimal? ConversionRate { get; set; }
    }

    /// <summary>
    /// Service for creating unified transaction views from multiple transaction sources
    /// </summary>
    public interface IUnifiedTransactionService
    {
        Task<List<UnifiedTransactionViewModel>> GetAllUnifiedTransactionsAsync(int? limit = null);
    }

    public class UnifiedTransactionService : IUnifiedTransactionService
    {
        private readonly ITransactionService _transactionService;
        private readonly FinancialTransactionService _financialService;
        private readonly IWageService _wageService;

        public UnifiedTransactionService(
            ITransactionService transactionService,
            FinancialTransactionService financialService,
            IWageService wageService)
        {
            _transactionService = transactionService;
            _financialService = financialService;
            _wageService = wageService;
        }

        /// <summary>
        /// Gets all transactions from all sources combined into a unified list
        /// </summary>
        /// <param name="limit">Optional limit on the number of transactions to return (most recent first)</param>
        /// <returns>List of unified transaction view models</returns>
        public async Task<List<UnifiedTransactionViewModel>> GetAllUnifiedTransactionsAsync(int? limit = null)
        {
            var unifiedTransactions = new List<UnifiedTransactionViewModel>();

            // Load all transaction types
            var inventoryTransactions = await _transactionService.GetAllTransactionsAsync();
            var financialTransactions = await _financialService.GetAllFinancialTransactionsAsync();
            var wageTransactions = await _wageService.GetTransactionsByDateRangeAsync(DateTime.MinValue, DateTime.MaxValue);

            // Add inventory transactions
            foreach (var t in inventoryTransactions)
            {
                var description = t.TransactionType == TransactionType.Processing
                    ? $"{t.InputItem?.ItemName ?? "N/A"} → {t.Item?.ItemName ?? "N/A"} ({t.Party?.Name ?? "N/A"})"
                    : $"{t.Item?.ItemName ?? "N/A"} - {t.Party?.Name ?? "N/A"}";
                
                var additionalInfo = t.TransactionType == TransactionType.Processing
                    ? $"Input: {t.InputQuantity:N2} → Output: {t.Quantity:N2}"
                    : (t.Quantity > 0 ? $"{t.Quantity:N2} units @ ₹{t.PricePerUnit:N2}" : null);

                // Buy/Wastage = Debit (money out), Sell/Processing = Credit (money in)
                var debitCredit = (t.TransactionType == TransactionType.Sell || t.TransactionType == TransactionType.Processing) ? "Credit" : "Debit";

                unifiedTransactions.Add(new UnifiedTransactionViewModel
                {
                    Category = "Inventory",
                    TransactionId = t.TransactionId.ToString(),
                    TransactionDate = t.TransactionDate,
                    TransactionType = t.TransactionType.ToString(),
                    DebitCredit = debitCredit,
                    Description = description,
                    Item = t.Item?.ItemName,
                    Name = t.Party?.Name,
                    ItemName = t.Item?.ItemName,
                    PartyName = t.Party?.Name,
                    WorkerName = null,
                    Quantity = t.Quantity,
                    Rate = t.PricePerUnit,
                    Amount = t.TotalAmount,
                    AdditionalInfo = additionalInfo,
                    Notes = t.Notes,
                    EnteredBy = t.User?.Username,
                    InputItemName = t.InputItem?.ItemName,
                    InputQuantity = t.InputQuantity,
                    ConversionRate = null
                });
            }
            
            // Add financial transactions
            foreach (var t in financialTransactions)
            {
                // LoanGiven/LoanPayment/InterestPaid = Debit (money out)
                // LoanTaken/LoanRepayment/InterestReceived = Credit (money in)
                var debitCredit = t.TransactionType == FinancialTransactionType.LoanGiven ||
                                 t.TransactionType == FinancialTransactionType.LoanPayment ||
                                 t.TransactionType == FinancialTransactionType.InterestPaid
                                 ? "Debit" : "Credit";

                unifiedTransactions.Add(new UnifiedTransactionViewModel
                {
                    Category = "Financial",
                    TransactionId = t.FinancialTransactionId.ToString(),
                    TransactionDate = t.TransactionDate,
                    TransactionType = t.TransactionType.ToString(),
                    DebitCredit = debitCredit,
                    Description = t.Party?.Name ?? "N/A",
                    Item = null,
                    Name = t.Party?.Name,
                    ItemName = null,
                    PartyName = t.Party?.Name,
                    WorkerName = null,
                    Quantity = null,
                    Rate = t.InterestRate > 0 ? t.InterestRate : null,
                    Amount = t.Amount,
                    AdditionalInfo = t.InterestRate > 0 ? $"Interest: {t.InterestRate:N2}% (₹{t.InterestAmount:N2})" : null,
                    Notes = t.Notes,
                    EnteredBy = t.User?.Username
                });
            }
            
            // Add wage transactions
            foreach (var t in wageTransactions)
            {
                // AdvanceGiven = Debit (money out to worker)
                // AdvanceAdjustment/Deduction = Credit (money recovered/deducted)
                // All other wage types (DailyWage, HourlyWage, MonthlyWage, OvertimePay, Bonus) = Debit (money paid)
                var debitCredit = t.TransactionType == WageTransactionType.AdvanceAdjustment ||
                                 t.TransactionType == WageTransactionType.Deduction
                                 ? "Credit" : "Debit";

                unifiedTransactions.Add(new UnifiedTransactionViewModel
                {
                    Category = "Wages",
                    TransactionId = t.WageTransactionId.ToString(),
                    TransactionDate = t.TransactionDate,
                    TransactionType = t.TransactionType.ToString(),
                    DebitCredit = debitCredit,
                    Description = t.Worker?.Name ?? "N/A",
                    Item = null,
                    Name = t.Worker?.Name,
                    ItemName = null,
                    PartyName = null,
                    WorkerName = t.Worker?.Name,
                    Quantity = t.DaysWorked > 0 ? t.DaysWorked : (t.HoursWorked > 0 ? t.HoursWorked : null),
                    Rate = t.Rate,
                    Amount = t.NetAmount,
                    AdditionalInfo = t.DaysWorked > 0 ? $"{t.DaysWorked:N1} days @ ₹{t.Rate:N2}" : 
                                    (t.HoursWorked > 0 ? $"{t.HoursWorked:N1} hrs @ ₹{t.Rate:N2}" : null),
                    Notes = t.Notes,
                    EnteredBy = t.User?.Username
                });
            }

            // Sort by date descending and apply limit if specified
            var sortedTransactions = unifiedTransactions
                .OrderByDescending(t => t.TransactionDate)
                .ToList();

            return limit.HasValue ? sortedTransactions.Take(limit.Value).ToList() : sortedTransactions;
        }
    }
}

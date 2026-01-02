using FactoryManagement.Models;
using FactoryManagement.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FactoryManagement.Services
{
    public interface ITransactionService
    {
        Task<IEnumerable<Transaction>> GetAllTransactionsAsync();
        Task<Transaction?> GetTransactionByIdAsync(int id);
        Task<Transaction> AddTransactionAsync(Transaction transaction);
        Task UpdateTransactionAsync(Transaction transaction);
        Task UpdateTransactionWithStockAsync(Transaction updated);
        Task DeleteTransactionAsync(int id);
        Task<IEnumerable<Transaction>> GetTransactionsByItemAsync(int itemId);
        Task<IEnumerable<Transaction>> GetTransactionsByPartyAsync(int partyId);
        Task<IEnumerable<Transaction>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Transaction>> GetTransactionsByTypeAsync(TransactionType type);
        Task<IEnumerable<Transaction>> GetRecentTransactionsAsync(int count);
    }

    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IItemService _itemService;
        private readonly ICashAccountService _cashAccountService;

        public TransactionService(
            ITransactionRepository transactionRepository, 
            IItemService itemService,
            ICashAccountService cashAccountService)
        {
            _transactionRepository = transactionRepository;
            _itemService = itemService;
            _cashAccountService = cashAccountService;
        }

        public async Task<IEnumerable<Transaction>> GetAllTransactionsAsync()
        {
            return await _transactionRepository.GetAllWithDetailsAsync();
        }

        public async Task<Transaction?> GetTransactionByIdAsync(int id)
        {
            return await _transactionRepository.GetByIdAsync(id);
        }

        public async Task<Transaction> AddTransactionAsync(Transaction transaction)
        {
            transaction.CreatedDate = DateTime.Now;
            transaction.TotalAmount = transaction.Quantity * transaction.PricePerUnit;
            
            var result = await _transactionRepository.AddAsync(transaction);

            // Stock updates: processing is service-only, no inventory changes; also skip if ItemId is null
            if (transaction.TransactionType != TransactionType.Processing && transaction.ItemId.HasValue)
            {
                await _itemService.UpdateStockAsync(transaction.ItemId.Value, transaction.Quantity, transaction.TransactionType);
            }

            // Update cash balance based on transaction type and payment mode
            await UpdateBalanceForTransactionAsync(transaction);
            
            return result;
        }

        public async Task UpdateTransactionAsync(Transaction transaction)
        {
            transaction.TotalAmount = transaction.Quantity * transaction.PricePerUnit;
            await _transactionRepository.UpdateAsync(transaction);
        }

        public async Task UpdateTransactionWithStockAsync(Transaction updated)
        {
            var existing = await _transactionRepository.GetByIdAsync(updated.TransactionId);
            if (existing == null)
            {
                throw new InvalidOperationException("Transaction not found for update.");
            }

            // Reverse stock from existing transaction when applicable
            if (existing.TransactionType != TransactionType.Processing && existing.ItemId.HasValue)
            {
                var reverseType = existing.TransactionType switch
                {
                    TransactionType.Buy => TransactionType.Sell,
                    TransactionType.Sell => TransactionType.Buy,
                    TransactionType.Wastage => TransactionType.Buy,
                    _ => TransactionType.Buy
                };
                await _itemService.UpdateStockAsync(existing.ItemId.Value, existing.Quantity, reverseType);
            }

            // Persist updated values and compute total
            updated.TotalAmount = updated.Quantity * updated.PricePerUnit;
            await _transactionRepository.UpdateAsync(updated);

            // Apply stock from updated transaction when applicable
            if (updated.TransactionType != TransactionType.Processing && updated.ItemId.HasValue)
            {
                await _itemService.UpdateStockAsync(updated.ItemId.Value, updated.Quantity, updated.TransactionType);
            }
        }

        public async Task DeleteTransactionAsync(int id)
        {
            var transaction = await _transactionRepository.GetByIdAsync(id);
            if (transaction != null)
            {
                // Reverse stock update based on transaction type
                if (transaction.TransactionType != TransactionType.Processing && transaction.ItemId.HasValue)
                {
                    // Reverse regular transaction
                    var reverseType = transaction.TransactionType switch
                    {
                        TransactionType.Buy => TransactionType.Sell,
                        TransactionType.Sell => TransactionType.Buy,
                        TransactionType.Wastage => TransactionType.Buy, // wastage had reduced stock; reversal increases
                        _ => TransactionType.Buy
                    };
                    await _itemService.UpdateStockAsync(transaction.ItemId.Value, transaction.Quantity, reverseType);
                }

                // Reverse balance update
                await ReverseBalanceForTransactionAsync(transaction);
                
                await _transactionRepository.DeleteAsync(transaction);
            }
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByItemAsync(int itemId)
        {
            return await _transactionRepository.GetTransactionsByItemIdAsync(itemId);
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByPartyAsync(int partyId)
        {
            return await _transactionRepository.GetTransactionsByPartyIdAsync(partyId);
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _transactionRepository.GetTransactionsByDateRangeAsync(startDate, endDate);
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByTypeAsync(TransactionType type)
        {
            return await _transactionRepository.GetTransactionsByTypeAsync(type);
        }

        public async Task<IEnumerable<Transaction>> GetRecentTransactionsAsync(int count)
        {
            var allTransactions = await _transactionRepository.GetAllWithDetailsAsync();
            return allTransactions
                .OrderByDescending(t => t.TransactionDate)
                .ThenByDescending(t => t.TransactionId)
                .Take(count)
                .ToList();
        }

        /// <summary>
        /// Updates cash balance based on transaction type and payment mode
        /// </summary>
        private async Task UpdateBalanceForTransactionAsync(Transaction transaction)
        {
            try
            {
                // Only track cash/bank transactions
                if (transaction.PaymentMode == PaymentMode.Loan)
                    return;

                // Determine account type based on payment mode
                var accountType = transaction.PaymentMode == PaymentMode.Cash 
                    ? AccountType.Cash 
                    : AccountType.Bank;

                var account = await _cashAccountService.GetAccountByTypeAsync(accountType);
                if (account == null)
                    return; // No balance tracking if account not set up

                // Calculate balance change
                decimal balanceChange = 0;
                string notes = "";

                switch (transaction.TransactionType)
                {
                    case TransactionType.Buy:
                        balanceChange = -transaction.TotalAmount; // Money out
                        notes = $"Purchase from {transaction.PartyName} - {transaction.ItemName}";
                        break;
                    case TransactionType.Sell:
                        balanceChange = transaction.TotalAmount; // Money in
                        notes = $"Sale to {transaction.PartyName} - {transaction.ItemName}";
                        break;
                    case TransactionType.Wastage:
                        balanceChange = -transaction.TotalAmount; // Money out (loss)
                        notes = $"Wastage: {transaction.ItemName}";
                        break;
                    case TransactionType.Processing:
                        // No cash movement for processing
                        return;
                }

                // Update balance
                await _cashAccountService.UpdateBalanceAsync(
                    account.AccountId,
                    balanceChange,
                    BalanceChangeType.Transaction,
                    notes,
                    transaction.EnteredBy,
                    transaction.TransactionId);
            }
            catch (Exception ex)
            {
                // Log error but don't fail transaction
                System.Diagnostics.Debug.WriteLine($"Error updating balance for transaction: {ex.Message}");
            }
        }

        /// <summary>
        /// Reverses the cash balance for a deleted transaction
        /// </summary>
        private async Task ReverseBalanceForTransactionAsync(Transaction transaction)
        {
            try
            {
                // Only track cash/bank transactions
                if (transaction.PaymentMode == PaymentMode.Loan)
                    return;

                // Determine account type
                var accountType = transaction.PaymentMode == PaymentMode.Cash 
                    ? AccountType.Cash 
                    : AccountType.Bank;

                var account = await _cashAccountService.GetAccountByTypeAsync(accountType);
                if (account == null)
                    return;

                // Reverse the balance change
                decimal reverseAmount = 0;
                string notes = "";

                switch (transaction.TransactionType)
                {
                    case TransactionType.Buy:
                        reverseAmount = transaction.TotalAmount; // Reverse the deduction
                        notes = $"Reversed: Purchase from {transaction.PartyName}";
                        break;
                    case TransactionType.Sell:
                        reverseAmount = -transaction.TotalAmount; // Reverse the addition
                        notes = $"Reversed: Sale to {transaction.PartyName}";
                        break;
                    case TransactionType.Wastage:
                        reverseAmount = transaction.TotalAmount; // Reverse the loss
                        notes = $"Reversed: Wastage";
                        break;
                    case TransactionType.Processing:
                        return;
                }

                // Apply reversal
                await _cashAccountService.UpdateBalanceAsync(
                    account.AccountId,
                    reverseAmount,
                    BalanceChangeType.ManualAdjustment,
                    notes,
                    transaction.EnteredBy,
                    transaction.TransactionId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error reversing balance for transaction: {ex.Message}");
            }
        }
    }
}

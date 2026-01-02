using FactoryManagement.Models;
using FactoryManagement.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FactoryManagement.Services
{
    public interface IItemService
    {
        Task<IEnumerable<Item>> GetAllItemsAsync();
        Task<Item?> GetItemByIdAsync(int id);
        Task<Item> AddItemAsync(Item item, int? userId = null);
        Task UpdateItemAsync(Item item, int? userId = null, string? userRole = null);
        Task DeleteItemAsync(int id, string? userRole = null);
        Task UpdateStockAsync(int itemId, decimal quantityChange, TransactionType transactionType);
        Task UpdateStockForProcessingAsync(int inputItemId, decimal inputQuantity, int outputItemId, decimal outputQuantity);
    }

    public class ItemService : IItemService
    {
        private readonly IRepository<Item> _itemRepository;

        public ItemService(IRepository<Item> itemRepository)
        {
            _itemRepository = itemRepository;
        }

        public async Task<IEnumerable<Item>> GetAllItemsAsync()
        {
            return await _itemRepository.GetAllAsync();
        }

        public async Task<Item?> GetItemByIdAsync(int id)
        {
            return await _itemRepository.GetByIdAsync(id);
        }

        public async Task<Item> AddItemAsync(Item item, int? userId = null)
        {
            item.CreatedDate = DateTime.Now;
            item.CreatedByUserId = userId;
            return await _itemRepository.AddAsync(item);
        }

        public async Task UpdateItemAsync(Item item, int? userId = null, string? userRole = null)
        {
            // Admin-only authorization
            if (userRole != "Admin")
                throw new UnauthorizedAccessException("Only Admin users can update items.");

            item.ModifiedDate = DateTime.Now;
            item.ModifiedByUserId = userId;
            await _itemRepository.UpdateAsync(item);
        }

        public async Task DeleteItemAsync(int id, string? userRole = null)
        {
            // Admin-only authorization
            if (userRole != "Admin")
                throw new UnauthorizedAccessException("Only Admin users can delete items.");

            var item = await _itemRepository.GetByIdAsync(id);
            if (item == null)
                throw new InvalidOperationException("Item not found");

            // Allow deletion - ItemId will be set to NULL in related records, ItemName is preserved
            await _itemRepository.DeleteAsync(item);
        }

        public async Task UpdateStockAsync(int itemId, decimal quantityChange, TransactionType transactionType)
        {
            // Processing does not affect inventory; skip entirely
            if (transactionType == TransactionType.Processing)
            {
                await Task.CompletedTask;
                return;
            }

            var item = await _itemRepository.GetByIdAsync(itemId);
            if (item != null)
            {
                switch (transactionType)
                {
                    case TransactionType.Buy:
                        item.CurrentStock += quantityChange;
                        break;
                    case TransactionType.Sell:
                    case TransactionType.Wastage:
                        item.CurrentStock -= quantityChange;
                        break;
                }
                item.ModifiedDate = DateTime.Now;
                await _itemRepository.UpdateAsync(item);
            }
        }

        public async Task UpdateStockForProcessingAsync(int inputItemId, decimal inputQuantity, int outputItemId, decimal outputQuantity)
        {
            // No-op: Processing does not alter inventory in service-only model
            await Task.CompletedTask;
        }
    }
}

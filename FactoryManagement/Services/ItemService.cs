using FactoryManagement.Models;
using FactoryManagement.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FactoryManagement.Services
{
    public interface IItemService
    {
        Task<IEnumerable<Item>> GetAllItemsAsync();
        Task<Item?> GetItemByIdAsync(int id);
        Task<Item> AddItemAsync(Item item, int? userId = null);
        Task UpdateItemAsync(Item item, int? userId = null);
        Task DeleteItemAsync(int id);
        Task UpdateStockAsync(int itemId, decimal quantityChange, TransactionType transactionType);
        Task UpdateStockForProcessingAsync(int inputItemId, decimal inputQuantity, int outputItemId, decimal outputQuantity);
        
        // New methods for package management
        Task ConvertToLooseStockAsync(int itemId, int? userId = null);
        Task ConvertToPackagedStockAsync(int itemId, List<StockPackage> packages, int? userId = null);
    }

    public class ItemService : IItemService
    {
        private readonly IRepository<Item> _itemRepository;
        private readonly IRepository<StockPackage> _packageRepository;

        public ItemService(IRepository<Item> itemRepository, IRepository<StockPackage>? packageRepository = null)
        {
            _itemRepository = itemRepository;
            _packageRepository = packageRepository;
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

        public async Task UpdateItemAsync(Item item, int? userId = null)
        {
            item.ModifiedDate = DateTime.Now;
            item.ModifiedByUserId = userId;
            await _itemRepository.UpdateAsync(item);
        }

        public async Task DeleteItemAsync(int id)
        {
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

        public async Task ConvertToLooseStockAsync(int itemId, int? userId = null)
        {
            if (_packageRepository == null)
                throw new InvalidOperationException("Package repository not initialized");

            try
            {
                var allPackages = await _packageRepository.GetAllAsync();
                var itemPackages = allPackages.Where(p => p.ItemId == itemId).ToList();
                
                foreach (var package in itemPackages)
                {
                    await _packageRepository.DeleteAsync(package);
                }

                var item = await _itemRepository.GetByIdAsync(itemId);
                if (item != null)
                {
                    item.ModifiedDate = DateTime.Now;
                    item.ModifiedByUserId = userId;
                    await _itemRepository.UpdateAsync(item);
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error converting item {itemId} to loose stock", ex);
            }
        }

        public async Task ConvertToPackagedStockAsync(int itemId, List<StockPackage> packages, int? userId = null)
        {
            if (_packageRepository == null)
                throw new InvalidOperationException("Package repository not initialized");

            try
            {
                var item = await _itemRepository.GetByIdAsync(itemId);
                if (item == null)
                    throw new ArgumentException($"Item {itemId} not found", nameof(itemId));

                // Clear existing packages
                await ConvertToLooseStockAsync(itemId, userId);

                // Add new packages
                foreach (var package in packages)
                {
                    package.ItemId = itemId;
                    package.CreatedByUserId = userId;
                    package.CreatedDate = DateTime.Now;
                    await _packageRepository.AddAsync(package);
                }

                // Sync stock to total of packages
                var totalStock = packages.Sum(p => p.TotalQuantity);
                item.CurrentStock = totalStock;
                item.ModifiedDate = DateTime.Now;
                item.ModifiedByUserId = userId;
                await _itemRepository.UpdateAsync(item);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error converting item {itemId} to packaged stock", ex);
            }
        }
    }}
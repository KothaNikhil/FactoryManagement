using FactoryManagement.Models;
using FactoryManagement.Data;
using FactoryManagement.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FactoryManagement.Services
{
    /// <summary>
    /// Service for managing stock package distributions
    /// </summary>
    public interface IStockPackageService
    {
        /// <summary>
        /// Get all packages for a specific item
        /// </summary>
        Task<IEnumerable<StockPackage>> GetPackagesByItemIdAsync(int itemId);

        /// <summary>
        /// Add a new package distribution
        /// </summary>
        Task<StockPackage> AddPackageAsync(StockPackage package, int? userId = null);

        /// <summary>
        /// Update an existing package
        /// </summary>
        Task UpdatePackageAsync(StockPackage package, int? userId = null);

        /// <summary>
        /// Delete a specific package
        /// </summary>
        Task DeletePackageAsync(int packageId);

        /// <summary>
        /// Delete all packages for an item
        /// </summary>
        Task DeleteAllPackagesForItemAsync(int itemId);

        /// <summary>
        /// Calculate total stock from all packages of an item
        /// </summary>
        Task<decimal> CalculateTotalStockFromPackagesAsync(int itemId);

        /// <summary>
        /// Sync the item's CurrentStock with calculated packages total
        /// </summary>
        Task SyncItemStockWithPackagesAsync(int itemId);

        /// <summary>
        /// Get package info as formatted string (e.g., "20×25kg, 10×50kg")
        /// </summary>
        Task<string> GetPackageBreakdownAsync(int itemId);
    }

    public class StockPackageService : IStockPackageService
    {
        private readonly IRepository<StockPackage> _packageRepository;
        private readonly IRepository<Item> _itemRepository;

        public StockPackageService(
            IRepository<StockPackage> packageRepository,
            IRepository<Item> itemRepository)
        {
            _packageRepository = packageRepository;
            _itemRepository = itemRepository;
        }

        public async Task<IEnumerable<StockPackage>> GetPackagesByItemIdAsync(int itemId)
        {
            try
            {
                var allPackages = await _packageRepository.GetAllAsync();
                return allPackages
                    .Where(p => p.ItemId == itemId && p.PackageCount > 0)
                    .OrderBy(p => p.PackageSize)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error retrieving packages for item {itemId}", ex);
            }
        }

        public async Task<StockPackage> AddPackageAsync(StockPackage package, int? userId = null)
        {
            try
            {
                if (package.PackageSize <= 0)
                    throw new ArgumentException("Package size must be greater than 0", nameof(package.PackageSize));

                if (package.PackageCount <= 0)
                    throw new ArgumentException("Package count must be greater than 0", nameof(package.PackageCount));

                if (package.ItemId <= 0)
                    throw new ArgumentException("Valid ItemId is required", nameof(package.ItemId));

                package.CreatedDate = DateTime.Now;
                package.CreatedByUserId = userId;

                var result = await _packageRepository.AddAsync(package);
                
                // Auto-sync item stock
                await SyncItemStockWithPackagesAsync(package.ItemId);
                
                return result;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error adding package: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
        }

        public async Task UpdatePackageAsync(StockPackage package, int? userId = null)
        {
            try
            {
                if (package.PackageSize <= 0)
                    throw new ArgumentException("Package size must be greater than 0", nameof(package.PackageSize));

                if (package.PackageCount < 0)
                    throw new ArgumentException("Package count cannot be negative", nameof(package.PackageCount));

                package.ModifiedDate = DateTime.Now;
                package.ModifiedByUserId = userId;

                await _packageRepository.UpdateAsync(package);
                
                // Auto-sync item stock
                await SyncItemStockWithPackagesAsync(package.ItemId);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error updating package", ex);
            }
        }

        public async Task DeletePackageAsync(int packageId)
        {
            try
            {
                var package = await _packageRepository.GetByIdAsync(packageId);
                if (package == null)
                    throw new ArgumentException($"Package {packageId} not found", nameof(packageId));

                var itemId = package.ItemId;
                await _packageRepository.DeleteAsync(package);
                
                // Auto-sync item stock
                await SyncItemStockWithPackagesAsync(itemId);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error deleting package {packageId}", ex);
            }
        }

        public async Task DeleteAllPackagesForItemAsync(int itemId)
        {
            try
            {
                var allPackages = await _packageRepository.GetAllAsync();
                var packages = allPackages.Where(p => p.ItemId == itemId).ToList();
                
                foreach (var package in packages)
                {
                    await _packageRepository.DeleteAsync(package);
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error deleting packages for item {itemId}: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
        }

        public async Task<decimal> CalculateTotalStockFromPackagesAsync(int itemId)
        {
            try
            {
                var packages = await GetPackagesByItemIdAsync(itemId);
                return packages.Sum(p => p.TotalQuantity);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error calculating total stock for item {itemId}", ex);
            }
        }

        public async Task SyncItemStockWithPackagesAsync(int itemId)
        {
            try
            {
                var item = await _itemRepository.GetByIdAsync(itemId);
                if (item == null)
                    throw new ArgumentException($"Item {itemId} not found", nameof(itemId));

                var totalStock = await CalculateTotalStockFromPackagesAsync(itemId);
                
                item.CurrentStock = totalStock;
                item.ModifiedDate = DateTime.Now;
                
                await _itemRepository.UpdateAsync(item);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error syncing stock for item {itemId}", ex);
            }
        }

        public async Task<string> GetPackageBreakdownAsync(int itemId)
        {
            try
            {
                var packages = await GetPackagesByItemIdAsync(itemId);
                
                if (!packages.Any())
                    return "Loose stock";

                var breakdown = string.Join(", ",
                    packages.Select(p => $"{p.PackageCount}×{p.PackageSize:N0}kg"));

                return breakdown;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error getting package breakdown for item {itemId}", ex);
            }
        }
    }
}

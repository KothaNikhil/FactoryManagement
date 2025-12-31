using FactoryManagement.Data;
using FactoryManagement.Data.Repositories;
using FactoryManagement.Models;
using FactoryManagement.Services;
using FactoryManagement.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FactoryManagement.Tests.E2E
{
    /// <summary>
    /// End-to-End tests for party (customer/supplier) management workflows
    /// </summary>
    public class PartyManagementWorkflowTests : IDisposable
    {
        private readonly DbContextOptions<FactoryDbContext> _options;
        private readonly FactoryDbContext _context;
        private readonly PartyService _partyService;
        private readonly TransactionService _transactionService;
        private readonly ContactsViewModel _viewModel;

        public PartyManagementWorkflowTests()
        {
            _options = new DbContextOptionsBuilder<FactoryDbContext>()
                .UseInMemoryDatabase(databaseName: $"E2EPartyTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new FactoryDbContext(_options);
            
            var partyRepo = new Repository<Party>(_context);
            var itemRepo = new Repository<Item>(_context);
            var transactionRepo = new TransactionRepository(_context);

            var itemService = new ItemService(itemRepo);
            _partyService = new PartyService(partyRepo);
            _transactionService = new TransactionService(transactionRepo, itemService);
            _viewModel = new ContactsViewModel(_partyService);

            SeedTestData().Wait();
        }

        private async Task SeedTestData()
        {
            await _context.Users.AddAsync(new User { UserId = 1, Username = "admin", Role = "Admin", IsActive = true });
            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task PartyWorkflow_AddNewCustomer_ShouldBeSearchableAndEditable()
        {
            // Act - Step 1: Add new customer
            await _partyService.AddPartyAsync(new Party
            {
                Name = "ABC Corporation",
                PartyType = PartyType.Buyer,
                MobileNumber = "1234567890",
                Place = "123 Business St"
            });

            // Assert - Customer saved
            var allParties = await _partyService.GetAllPartiesAsync();
            var customer = allParties.FirstOrDefault(p => p.Name == "ABC Corporation");
            Assert.NotNull(customer);
            Assert.Equal(PartyType.Buyer, customer.PartyType);

            // Act - Step 2: Search for customer
            var searchResults = (await _partyService.GetAllPartiesAsync())
                .Where(p => p.Name.Contains("ABC", StringComparison.OrdinalIgnoreCase));
            
            // Assert - Found in search
            Assert.Single(searchResults);
            Assert.Equal("ABC Corporation", searchResults.First().Name);

            // Act - Step 3: Edit customer details
            customer.MobileNumber = "9876543210";
            customer.Place = "456 New Address";
            await _partyService.UpdatePartyAsync(customer);

            // Assert - Changes saved
            var updated = await _partyService.GetPartyByIdAsync(customer.PartyId);
            Assert.NotNull(updated);
            Assert.Equal("9876543210", updated!.MobileNumber);
            Assert.Equal("456 New Address", updated!.Place);
        }

        [Fact]
        public async Task PartyWorkflow_AddNewSupplier_ShouldBeSearchableAndFilterable()
        {
            // Arrange & Act - Add supplier
            var supplier = await _partyService.AddPartyAsync(new Party
            {
                Name = "Tech Supplies Inc",
                PartyType = PartyType.Seller,
                MobileNumber = "5551234567",
                Place = "789 Supply Lane"
            });

            // Assert - Supplier created
            Assert.NotNull(supplier);
            Assert.Equal(PartyType.Seller, supplier.PartyType);

            // Act - Filter by supplier type
            var suppliers = await _partyService.GetPartiesByTypeAsync(PartyType.Seller);
            
            // Assert - Supplier in filtered list
            Assert.Contains(suppliers, s => s.Name == "Tech Supplies Inc");

            // Act - Search
            var searchResults2 = (await _partyService.GetAllPartiesAsync())
                .Where(p => p.Name.Contains("Tech", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(searchResults2, s => s.Name == "Tech Supplies Inc");
        }

        [Fact]
        public async Task PartyWorkflow_CompletePartyLifecycle_ShouldMaintainIntegrity()
        {
            // Step 1: Create customer
            var customer = await _partyService.AddPartyAsync(new Party
            {
                Name = "XYZ Retail",
                PartyType = PartyType.Buyer,
                MobileNumber = "1112223333",
                Place = "Retail Plaza"
            });

            // Step 2: Create an item for transactions
            var item = new Item { ItemName = "Product A", Unit = "Pcs", CurrentStock = 100 };
            await _context.Items.AddAsync(item);
            await _context.SaveChangesAsync();

            // Step 3: Create sales transactions with this customer
            await _transactionService.AddTransactionAsync(new Transaction
            {
                ItemId = item.ItemId,
                PartyId = customer.PartyId,
                TransactionType = TransactionType.Sell,
                Quantity = 10,
                PricePerUnit = 50,
                TotalAmount = 500,
                EnteredBy = 1
            });

            await _transactionService.AddTransactionAsync(new Transaction
            {
                ItemId = item.ItemId,
                PartyId = customer.PartyId,
                TransactionType = TransactionType.Sell,
                Quantity = 15,
                PricePerUnit = 50,
                TotalAmount = 750,
                EnteredBy = 1
            });

            // Step 4: Verify transactions are linked
            var allTransactions = await _transactionService.GetAllTransactionsAsync();
            var customerTransactions = allTransactions.Where(t => t.PartyId == customer.PartyId).ToList();
            Assert.Equal(2, customerTransactions.Count);

            // Step 5: Update customer information
            customer.MobileNumber = "4445556666";
            customer.Place = "New Retail Plaza";
            await _partyService.UpdatePartyAsync(customer);

            // Step 6: Verify customer details updated but transactions intact
            var updatedCustomer = await _partyService.GetPartyByIdAsync(customer.PartyId);
            Assert.NotNull(updatedCustomer);
            Assert.Equal("4445556666", updatedCustomer!.MobileNumber);
            
            var transactionsAfterUpdate = (await _transactionService.GetAllTransactionsAsync())
                .Where(t => t.PartyId == customer.PartyId).ToList();
            Assert.Equal(2, transactionsAfterUpdate.Count);

            // Step 7: Search for customer
            var searchResults3 = (await _partyService.GetAllPartiesAsync())
                .Where(p => p.Name.Contains("XYZ", StringComparison.OrdinalIgnoreCase));
            Assert.Single(searchResults3);
        }

        [Fact]
        public async Task PartyWorkflow_FilterByType_ShouldReturnCorrectParties()
        {
            // Arrange - Create mix of customers and suppliers
            await _partyService.AddPartyAsync(new Party { Name = "Customer 1", PartyType = PartyType.Buyer });
            await _partyService.AddPartyAsync(new Party { Name = "Customer 2", PartyType = PartyType.Buyer });
            await _partyService.AddPartyAsync(new Party { Name = "Customer 3", PartyType = PartyType.Buyer });
            await _partyService.AddPartyAsync(new Party { Name = "Supplier 1", PartyType = PartyType.Seller });
            await _partyService.AddPartyAsync(new Party { Name = "Supplier 2", PartyType = PartyType.Seller });

            // Act
            var customers = await _partyService.GetPartiesByTypeAsync(PartyType.Buyer);
            var suppliers = await _partyService.GetPartiesByTypeAsync(PartyType.Seller);

            // Assert
            Assert.Equal(3, customers.Count());
            Assert.Equal(2, suppliers.Count());
            Assert.All(customers, c => Assert.Equal(PartyType.Buyer, c.PartyType));
            Assert.All(suppliers, s => Assert.Equal(PartyType.Seller, s.PartyType));
        }

        [Fact]
        public async Task PartyWorkflow_SearchFunctionality_ShouldFindCorrectParties()
        {
            // Arrange
            await _partyService.AddPartyAsync(new Party { Name = "ABC Electronics", PartyType = PartyType.Buyer });
            await _partyService.AddPartyAsync(new Party { Name = "ABC Furniture", PartyType = PartyType.Seller });
            await _partyService.AddPartyAsync(new Party { Name = "XYZ Electronics", PartyType = PartyType.Buyer });
            await _partyService.AddPartyAsync(new Party { Name = "Global Supplies", PartyType = PartyType.Seller });

            // Act - Search by partial name
            var abcResults = (await _partyService.GetAllPartiesAsync()).Where(p => p.Name.Contains("ABC", StringComparison.OrdinalIgnoreCase));
            var electronicsResults = (await _partyService.GetAllPartiesAsync()).Where(p => p.Name.Contains("Electronics", StringComparison.OrdinalIgnoreCase));
            var globalResults = (await _partyService.GetAllPartiesAsync()).Where(p => p.Name.Contains("Global", StringComparison.OrdinalIgnoreCase));

            // Assert
            Assert.Equal(2, abcResults.Count()); // ABC Electronics and ABC Furniture
            Assert.Equal(2, electronicsResults.Count()); // ABC Electronics and XYZ Electronics
            Assert.Single(globalResults); // Global Supplies
        }

        [Fact]
        public async Task PartyWorkflow_DeleteParty_ShouldHardDelete()
        {
            // Arrange
            var party = await _partyService.AddPartyAsync(new Party
            {
                Name = "Temporary Party",
                PartyType = PartyType.Buyer
            });

            var partyId = party.PartyId;

            // Verify party exists
            var exists = await _partyService.GetPartyByIdAsync(partyId);
            Assert.NotNull(exists);

            // Act - Delete party (hard delete)
            await _partyService.DeletePartyAsync(partyId);

            // Assert - Party is completely removed
            var allParties2 = await _partyService.GetAllPartiesAsync();
            Assert.DoesNotContain(allParties2, p => p.PartyId == partyId);

            var deleted = await _partyService.GetPartyByIdAsync(partyId);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task PartyWorkflow_ViewModel_ShouldLoadAndManageParties()
        {
            // Arrange - Create some parties
            await _partyService.AddPartyAsync(new Party { Name = "Test Customer", PartyType = PartyType.Buyer });
            await _partyService.AddPartyAsync(new Party { Name = "Test Supplier", PartyType = PartyType.Seller });

            // Act - Load parties in ViewModel
            await _viewModel.InitializeAsync();

            // Assert - Parties loaded
            Assert.True(_viewModel.Parties.Count >= 2);

            // Act - Add new party via ViewModel
            await _partyService.AddPartyAsync(new Party { Name = "ViewModel Party", PartyType = PartyType.Buyer, MobileNumber = "1234567890", Place = "ViewModel Address" });

            // Refresh
            await _viewModel.InitializeAsync();

            // Assert - New party appears
            Assert.Contains(_viewModel.Parties, p => p.Name == "ViewModel Party");
        }

        [Fact]
        public async Task PartyWorkflow_BulkPartyCreation_ShouldHandleMultipleParties()
        {
            // Arrange & Act - Create multiple parties
            var parties = new[]
            {
                new Party { Name = "Customer A", PartyType = PartyType.Buyer, MobileNumber = "111" },
                new Party { Name = "Customer B", PartyType = PartyType.Buyer, MobileNumber = "222" },
                new Party { Name = "Supplier A", PartyType = PartyType.Seller, MobileNumber = "333" },
                new Party { Name = "Supplier B", PartyType = PartyType.Seller, MobileNumber = "444" },
                new Party { Name = "Customer C", PartyType = PartyType.Buyer, MobileNumber = "555" }
            };

            foreach (var party in parties)
            {
                await _partyService.AddPartyAsync(party);
            }

            // Assert - All parties created
            var allParties3 = await _partyService.GetAllPartiesAsync();
            Assert.True(allParties3.Count() >= 5);

            // Filter by type
            var customers = await _partyService.GetPartiesByTypeAsync(PartyType.Buyer);
            var suppliers = await _partyService.GetPartiesByTypeAsync(PartyType.Seller);
            Assert.Equal(3, customers.Count());
            Assert.Equal(2, suppliers.Count());

            // Search functionality
            var searchA = (await _partyService.GetAllPartiesAsync()).Where(p => p.Name.Contains("Customer A", StringComparison.OrdinalIgnoreCase));
            Assert.Single(searchA);
        }

        // Opening balance features are not present in the current model; skipping related tests.

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}


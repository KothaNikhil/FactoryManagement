using FactoryManagement.Models;
using FactoryManagement.Data.Repositories;
using FactoryManagement.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FactoryManagement.Services
{
    public interface IPartyService
    {
        Task<IEnumerable<Party>> GetAllPartiesAsync();
        Task<IEnumerable<Party>> GetPartiesByTypeAsync(PartyType type);
        Task<Party?> GetPartyByIdAsync(int id);
        Task<Party> AddPartyAsync(Party party, int? userId = null);
        Task UpdatePartyAsync(Party party, int? userId = null);
        Task DeletePartyAsync(int id);
    }

    public class PartyService : IPartyService
    {
        private readonly IRepository<Party> _partyRepository;
        private readonly FactoryDbContext _context;

        public PartyService(IRepository<Party> partyRepository, FactoryDbContext context)
        {
            _partyRepository = partyRepository;
            _context = context;
        }

        public async Task<IEnumerable<Party>> GetAllPartiesAsync()
        {
            return await _partyRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Party>> GetPartiesByTypeAsync(PartyType type)
        {
            return await _partyRepository.FindAsync(p => p.PartyType == type || p.PartyType == PartyType.Both);
        }

        public async Task<Party?> GetPartyByIdAsync(int id)
        {
            return await _partyRepository.GetByIdAsync(id);
        }

        public async Task<Party> AddPartyAsync(Party party, int? userId = null)
        {
            party.CreatedDate = DateTime.Now;
            party.CreatedByUserId = userId;
            return await _partyRepository.AddAsync(party);
        }

        public async Task UpdatePartyAsync(Party party, int? userId = null)
        {
            party.ModifiedDate = DateTime.Now;
            party.ModifiedByUserId = userId;
            await _partyRepository.UpdateAsync(party);
        }

        public async Task DeletePartyAsync(int id)
        {
            try
            {
                var party = await _partyRepository.GetByIdAsync(id);
                if (party == null)
                    throw new InvalidOperationException("Party not found");

                // Check for active/open loan accounts
                var hasActiveLoans = await _context.LoanAccounts
                    .AnyAsync(l => l.PartyId == id && l.Status == LoanStatus.Active);

                if (hasActiveLoans)
                {
                    throw new InvalidOperationException($"Cannot delete '{party.Name}'. This party has active loan accounts. Please close all loans before deleting the party.");
                }

                // Allow deletion - PartyId will be set to NULL in related records, PartyName is preserved
                await _partyRepository.DeleteAsync(party);
                await _context.SaveChangesAsync();
            }
            catch (InvalidOperationException)
            {
                throw; // Re-throw business logic exceptions
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Cannot delete the party due to database constraints. The party may still have related records. Inner error: {ex.InnerException?.Message}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error deleting party: {ex.Message}");
            }
        }
    }
}

using FactoryManagement.Models;
using FactoryManagement.Data.Repositories;
using System;
using System.Collections.Generic;
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

        public PartyService(IRepository<Party> partyRepository)
        {
            _partyRepository = partyRepository;
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
            var party = await _partyRepository.GetByIdAsync(id);
            if (party != null)
            {
                await _partyRepository.DeleteAsync(party);
            }
        }
    }
}

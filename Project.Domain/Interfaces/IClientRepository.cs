using Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Interfaces
{
    public interface IClientRepository
    {
        Task<Client> GetByIdAsync(int id);
        Task<Client> GetByIdentificationAsync(string identification);
        Task<IEnumerable<Client>> GetAllAsync(int pageNumber, int pageSize, string searchTerm = null);
        Task AddAsync(Client client);
        Task UpdateAsync(Client client);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(string identification);
        Task<int> CountAsync(string searchTerm = null);
    }
}

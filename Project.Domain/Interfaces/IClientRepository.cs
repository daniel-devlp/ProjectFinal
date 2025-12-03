using Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Interfaces
{
    public interface IClientRepository : IRepository<Client>
    {
        Task<Client?> GetByIdentificationAsync(string identificationNumber);
        Task<(IEnumerable<Client> Items, int TotalCount)> GetPagedAsync(
            int pageNumber, int pageSize, string searchTerm = null);
            
        // ✅ Métodos para borrado lógico
        Task<(IEnumerable<Client> Items, int TotalCount)> GetPagedIncludingDeletedAsync(
            int pageNumber, int pageSize, string searchTerm = null);
        Task<IEnumerable<Client>> GetDeletedClientsAsync();
        Task<Client?> GetByIdIncludingDeletedAsync(int id);
        Task<bool> ExistsByIdentificationAsync(string identificationNumber, int? excludeClientId = null);
        Task<bool> ExistsByEmailAsync(string email, int? excludeClientId = null);
        Task<int> GetActiveCountAsync();
        Task<int> GetTotalCountAsync();
        Task RestoreAsync(int id);
    }
}

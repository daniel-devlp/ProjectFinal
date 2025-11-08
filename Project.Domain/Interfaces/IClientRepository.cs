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
        Task<Client> GetByIdentificationAsync(string identificationNumber);
        Task<(IEnumerable<Client> Items, int TotalCount)> GetPagedAsync(
            int pageNumber, int pageSize, string searchTerm = null);
    }
}

using Microsoft.EntityFrameworkCore;
using Project.Domain.Entities;
using Project.Domain.Interfaces;
using Project.Infrastructure.Frameworks.EntityFramework;

namespace Project.Infrastructure.Repositories
{
    public class ClientRepository : Repository<Client>, IClientRepository
    {
        public ClientRepository(ApplicationDBContext context) : base(context)
        {
        }

        public async Task<Client> GetByIdentificationAsync(string identificationNumber)
        {
            return await _dbSet.FirstOrDefaultAsync(c => 
                c.IdentificationNumber == identificationNumber);
        }

        public async Task<(IEnumerable<Client> Items, int TotalCount)> GetPagedAsync(
            int pageNumber, int pageSize, string searchTerm = null)
        {
            var query = _dbSet.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(c =>
                    c.FirstName.ToLower().Contains(term) ||
                    c.LastName.ToLower().Contains(term) ||
                    c.IdentificationNumber.Contains(term) ||
                    (c.Email != null && c.Email.ToLower().Contains(term)));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(c => c.FirstName)
                .ThenBy(c => c.LastName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return (items, totalCount);
        }
    }
}

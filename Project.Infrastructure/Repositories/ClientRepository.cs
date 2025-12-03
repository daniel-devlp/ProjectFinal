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

        public async Task<Client?> GetByIdentificationAsync(string identificationNumber)
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

        // Métodos específicos para borrado lógico
        public async Task<(IEnumerable<Client> Items, int TotalCount)> GetPagedIncludingDeletedAsync(
            int pageNumber, int pageSize, string searchTerm = null)
        {
            // Ignorar el filtro global para mostrar también los eliminados
            var query = _dbSet.IgnoreQueryFilters().AsQueryable();

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
                .OrderBy(c => c.IsActive ? 0 : 1) // Activos primero
                .ThenBy(c => c.FirstName)
                .ThenBy(c => c.LastName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<IEnumerable<Client>> GetDeletedClientsAsync()
        {
            return await _dbSet
                .IgnoreQueryFilters()
                .Where(c => !c.IsActive)
                .OrderByDescending(c => c.DeletedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Client?> GetByIdIncludingDeletedAsync(int id)
        {
            return await _dbSet
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.ClientId == id);
        }

        public async Task<bool> ExistsByIdentificationAsync(string identificationNumber, int? excludeClientId = null)
        {
            var query = _dbSet.IgnoreQueryFilters()
                .Where(c => c.IdentificationNumber == identificationNumber);
            
            if (excludeClientId.HasValue)
            {
                query = query.Where(c => c.ClientId != excludeClientId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> ExistsByEmailAsync(string email, int? excludeClientId = null)
        {
            var query = _dbSet.IgnoreQueryFilters()
                .Where(c => c.Email == email);
      
            if (excludeClientId.HasValue)
            {
                query = query.Where(c => c.ClientId != excludeClientId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<int> GetActiveCountAsync()
        {
            return await _dbSet.CountAsync(); // El filtro global ya excluye los eliminados
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _dbSet.IgnoreQueryFilters().CountAsync();
        }

        public async Task RestoreAsync(int id)
        {
            var client = await GetByIdIncludingDeletedAsync(id);
            if (client != null && !client.IsActive)
            {
                client.Restore();
                _dbSet.Update(client);
            }
        }

        public async Task<Client?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(c => 
                c.Email == email);
        }
    }
}

using Project.Domain.Entities;
using Project.Domain.Interfaces;
using Project.Infrastructure.Frameworks.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
namespace Project.Infrastructure.Repositories
{
    public class ClientRepository : IClientRepository
    {
        private readonly ApplicationDBContext _context;

        public ClientRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<Client> GetByIdAsync(int id)
        {
            return await _context.Clients.FindAsync(id);
        }

        public async Task<Client> GetByIdentificationAsync(string identification)
        {
            return await _context.Clients.FirstOrDefaultAsync(c => c.IdentificationNumber == identification);
        }

        public async Task<IEnumerable<Client>> GetAllAsync(int pageNumber, int pageSize, string searchTerm = null)
        {
            if (pageNumber <= 0) throw new ArgumentException("Page number must be greater than zero.", nameof(pageNumber));
            if (pageSize <= 0) throw new ArgumentException("Page size must be greater than zero.", nameof(pageSize));

            var query = _context.Clients.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(c =>
                    (c.FirstName != null && c.FirstName.ToLower().Contains(term)) ||
                    (c.LastName != null && c.LastName.ToLower().Contains(term)) ||
                    (c.IdentificationNumber != null && c.IdentificationNumber.ToLower().Contains(term))
                );
            }

            return await query
                .OrderBy(c => c.FirstName)
                .ThenBy(c => c.LastName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task AddAsync(Client client)
        {
            await _context.Clients.AddAsync(client);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Client client)
        {
            _context.Clients.Update(client);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client != null)
            {
                _context.Clients.Remove(client);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(string identification)
        {
            return await _context.Clients.AnyAsync(c => c.IdentificationNumber == identification);
        }

        public async Task<int> CountAsync(string searchTerm = null)
        {
            var query = _context.Clients.AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchTerm))
                query = query.Where(c => c.FirstName.Contains(searchTerm) || c.IdentificationNumber.Contains(searchTerm));
            return await query.CountAsync();
        }
    }
}

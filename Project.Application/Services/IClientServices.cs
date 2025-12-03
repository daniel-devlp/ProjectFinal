using Project.Application.Dtos;
using Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.Services
{
    public interface IClientServices
    {
        Task<ClientDto> GetByIdAsync(int id);
        Task<ClientDto> GetByIdentificationAsync(string identification);
        Task<ClientDto?> GetByEmailAsync(string email); // ✅ Nuevo método para buscar por email
        Task<PagedResult<ClientDto>> GetAllAsync(int pageNumber, int pageSize, string searchTerm = null);
        Task AddAsync(ClientCreateDto clientDto);
        Task UpdateAsync(ClientUpdateDto clientDto);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(string identification);
        Task<int> CountAsync(string searchTerm = null);
        
        // ✅ Métodos nuevos para borrado lógico
        Task<PagedResult<ClientDto>> GetAllIncludingDeletedAsync(int pageNumber, int pageSize, string? searchTerm = null);
        Task<IEnumerable<ClientDto>> GetDeletedClientsAsync();
        Task RestoreAsync(int id);
        Task<int> CountAllAsync(string? searchTerm = null);
    }
}

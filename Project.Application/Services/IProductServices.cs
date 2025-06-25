using Project.Application.Dtos;
using Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.Services
{
    public interface IProductServices
    {
        Task<ProductDto> GetByIdAsync(int id);
        Task<ProductDto> GetByCodeAsync(string code);
        Task<PagedResult<ProductDto>> GetAllAsync(int pageNumber, int pageSize, string searchTerm = null);
        Task AddAsync(ProductCreateDto product);
        Task UpdateAsync(ProductUpdateDto product);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(string code);
        Task<int> CountAsync(string searchTerm = null);
        Task<IEnumerable<ProductDto>> GetAvailableProductsAsync(int pageNumber, int pageSize, string searchTerm = null);
        Task<bool> HasStockAsync(int productId, int quantity);
    }
}

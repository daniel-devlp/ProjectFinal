using Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Interfaces
{
    public interface IProductRepository
    {
        Task<Product> GetByIdAsync(int id);
        Task<IEnumerable<Product>> GetAllWithStockAsync(int pageNumber, int pageSize, string searchTerm = null);
        Task<IEnumerable<Product>> GetAllAsync(int pageNumber, int pageSize, string searchTerm = null);
        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(string code);
        Task<int> CountAsync(string searchTerm = null);
    }
}

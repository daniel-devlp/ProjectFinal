using Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<Product> GetByCodeAsync(string code);
        Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedAsync(
            int pageNumber, int pageSize, string searchTerm = null);
        Task<(IEnumerable<Product> Items, int TotalCount)> GetAvailableProductsAsync(
            int pageNumber, int pageSize, string searchTerm = null);
        Task<bool> HasStockAsync(int productId, int quantity);
        Task DecreaseStockAsync(int productId, int quantity);
        Task IncreaseStockAsync(int productId, int quantity);
        Task<bool> ExistsByCodeAsync(string code);
        Task<bool> ExistsByIdAsync(int productId);
    }
}

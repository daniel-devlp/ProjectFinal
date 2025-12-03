using Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<Product?> GetByCodeAsync(string code);
        Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedAsync(
            int pageNumber, int pageSize, string? searchTerm = null);
        Task<(IEnumerable<Product> Items, int TotalCount)> GetAvailableProductsAsync(
            int pageNumber, int pageSize, string? searchTerm = null);
        Task<bool> HasStockAsync(int productId, int quantity);
        Task DecreaseStockAsync(int productId, int quantity);
        Task IncreaseStockAsync(int productId, int quantity);
        Task<bool> ExistsByCodeAsync(string code, int? excludeProductId = null);
        Task<bool> ExistsByIdAsync(int productId);
        Task<int> CountAsync(Expression<Func<Product, bool>>? predicate = null);

        // ✅ Métodos para borrado lógico
        Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedIncludingDeletedAsync(
            int pageNumber, int pageSize, string? searchTerm = null);
        Task<IEnumerable<Product>> GetDeletedProductsAsync();
        Task<Product?> GetByIdIncludingDeletedAsync(int id);
        Task<int> GetActiveCountAsync();
        Task<int> GetTotalCountAsync();
        Task RestoreAsync(int id);

        // ✅ Métodos adicionales útiles
        Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold = 10);
        Task<IEnumerable<Product>> GetTopSellingProductsAsync(int count = 10);
    }
}

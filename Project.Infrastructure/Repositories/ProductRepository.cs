using Project.Domain.Entities;
using Project.Domain.Interfaces;
using Project.Infrastructure.Frameworks.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Project.Infrastructure.Repositories
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(ApplicationDBContext context) : base(context)
        {
        }

        public async Task<Product> GetByCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;
            return await _dbSet.FirstOrDefaultAsync(p => p.Code == code.Trim());
        }

        public async Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedAsync(
            int pageNumber, int pageSize, string searchTerm = null)
        {
            var query = _dbSet.AsQueryable();
            
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(p => 
                    p.Name.ToLower().Contains(term) || 
                    p.Code.ToLower().Contains(term) || 
                    p.Description.ToLower().Contains(term));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(p => p.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<(IEnumerable<Product> Items, int TotalCount)> GetAvailableProductsAsync(
            int pageNumber, int pageSize, string searchTerm = null)
        {
            var query = _dbSet.Where(p => p.Stock > 0 && p.IsActive);
            
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(p => 
                    p.Name.ToLower().Contains(term) || 
                    p.Code.ToLower().Contains(term));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(p => p.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<bool> HasStockAsync(int productId, int quantity)
        {
            var product = await _dbSet.FindAsync(productId);
            return product != null && product.Stock >= quantity && product.IsActive;
        }

        public async Task DecreaseStockAsync(int productId, int quantity)
        {
            var product = await _dbSet.FindAsync(productId);
            if (product == null) 
                throw new InvalidOperationException("Product does not exist.");
            if (product.Stock < quantity) 
                throw new InvalidOperationException("Not enough stock available.");
            
            product.Stock -= quantity;
            _dbSet.Update(product);
        }

        public async Task IncreaseStockAsync(int productId, int quantity)
        {
            var product = await _dbSet.FindAsync(productId);
            if (product == null) 
                throw new InvalidOperationException("Product does not exist.");
            
            product.Stock += quantity;
            _dbSet.Update(product);
        }

        public async Task<bool> ExistsByCodeAsync(string code)
        {
            return await _dbSet.AnyAsync(p => p.Code == code);
        }

        public async Task<bool> ExistsByIdAsync(int productId)
        {
            return await _dbSet.AnyAsync(p => p.ProductId == productId);
        }
    }
}

using Project.Domain.Entities;
using Project.Domain.Interfaces;
using Project.Infrastructure.Frameworks.EntityFramework;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Project.Infrastructure.Repositories
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(ApplicationDBContext context) : base(context)
        {
        }

        public async Task<Product?> GetByCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;
            return await _dbSet.FirstOrDefaultAsync(p => p.Code == code.Trim());
        }

        public async Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedAsync(
            int pageNumber, int pageSize, string? searchTerm = null)
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
            int pageNumber, int pageSize, string? searchTerm = null)
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
            
            product.DecreaseStock(quantity); // ✅ Usar método de dominio
            _dbSet.Update(product);
        }

        public async Task IncreaseStockAsync(int productId, int quantity)
        {
            var product = await _dbSet.FindAsync(productId);
            if (product == null) 
                throw new InvalidOperationException("Product does not exist.");
            
            product.IncreaseStock(quantity); // ✅ Usar método de dominio
            _dbSet.Update(product);
 }

        public async Task<bool> ExistsByCodeAsync(string code, int? excludeProductId = null)
        {
     var query = _dbSet.IgnoreQueryFilters() // ✅ Incluir eliminados para validar unicidad
   .Where(p => p.Code == code);
    
       if (excludeProductId.HasValue)
    {
         query = query.Where(p => p.ProductId != excludeProductId.Value);
      }

      return await query.AnyAsync();
        }

    public async Task<bool> ExistsByIdAsync(int productId)
        {
    return await _dbSet.AnyAsync(p => p.ProductId == productId);
    }

        public async Task<int> CountAsync(Expression<Func<Product, bool>>? predicate = null)
{
          if (predicate == null)
return await _dbSet.CountAsync();
 
       return await _dbSet.CountAsync(predicate);
        }

        // ✅ Métodos específicos para borrado lógico
        public async Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedIncludingDeletedAsync(
  int pageNumber, int pageSize, string? searchTerm = null)
        {
      // Ignorar el filtro global para mostrar también los eliminados
       var query = _dbSet.IgnoreQueryFilters().AsQueryable();

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
      .OrderBy(p => p.IsActive ? 0 : 1) // Activos primero
      .ThenBy(p => p.Name)
     .Skip((pageNumber - 1) * pageSize)
   .Take(pageSize)
     .AsNoTracking()
     .ToListAsync();

   return (items, totalCount);
      }

  public async Task<IEnumerable<Product>> GetDeletedProductsAsync()
   {
     return await _dbSet
       .IgnoreQueryFilters()
      .Where(p => !p.IsActive)
    .OrderByDescending(p => p.DeletedAt)
      .AsNoTracking()
      .ToListAsync();
        }

     public async Task<Product?> GetByIdIncludingDeletedAsync(int id)
        {
          return await _dbSet
     .IgnoreQueryFilters()
    .FirstOrDefaultAsync(p => p.ProductId == id);
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
      var product = await GetByIdIncludingDeletedAsync(id);
      if (product != null && !product.IsActive)
 {
   product.Restore();
   _dbSet.Update(product);
    }
        }

  public async Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold = 10)
   {
         return await _dbSet
         .Where(p => p.Stock <= threshold)
      .OrderBy(p => p.Stock)
   .AsNoTracking()
    .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetTopSellingProductsAsync(int count = 10)
        {
            // Esta consulta requeriría joins con InvoiceDetails para calcular ventas
     // Por simplicidad, retornamos productos ordenados por nombre
       return await _dbSet
         .OrderBy(p => p.Name)
       .Take(count)
     .AsNoTracking()
 .ToListAsync();
        }
    }
}

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
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDBContext _context;

        public ProductRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            return await _context.Products.FindAsync(id);
        }

        public async Task<Product> GetByCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;
            return await _context.Products.FirstOrDefaultAsync(p => p.Code == code.Trim());
        }
        public async Task<bool> ExistsByIdAsync(int productId)
        {
            return await _context.Products.AnyAsync(p => p.ProductId == productId);
        }
        public async Task<IEnumerable<Product>> GetAllAsync(int pageNumber, int pageSize, string searchTerm = null)
        {
            var query = _context.Products.AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchTerm))
                query = query.Where(p => p.Name.Contains(searchTerm) || p.Code.Contains(searchTerm) || p.Description.Contains(searchTerm));
            
            return await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetAllWithStockAsync(int pageNumber, int pageSize, string searchTerm = null)
        {
            var query = _context.Products.Where(p => p.Stock > 0);
            if (!string.IsNullOrWhiteSpace(searchTerm))
                query = query.Where(p => p.Name.Contains(searchTerm) || p.Code.Contains(searchTerm));
            return await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetAvailableProductsAsync(int pageNumber, int pageSize, string searchTerm = null)
        {
            var query = _context.Products.Where(p => p.Stock > 0);
            if (!string.IsNullOrWhiteSpace(searchTerm))
                query = query.Where(p => p.Name.Contains(searchTerm) || p.Code.Contains(searchTerm));
            return await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task AddAsync(Product product)
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(string code)
        {
            return await _context.Products.AnyAsync(p => p.Code == code);
        }

        public async Task<int> CountAsync(string searchTerm = null)
        {
            var query = _context.Products.AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchTerm))
                query = query.Where(p => p.Name.Contains(searchTerm) || p.Code.Contains(searchTerm));
            return await query.CountAsync();
        }

        public async Task<bool> HasStockAsync(int productId, int quantity)
        {
            var product = await _context.Products.FindAsync(productId);
            return product != null && product.Stock >= quantity;
        }

        public async Task DecreaseStockAsync(int productId, int quantity)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) throw new InvalidOperationException("Product does not exist.");
            if (product.Stock < quantity) throw new InvalidOperationException("Not enough stock.");
            product.Stock -= quantity;
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task IncreaseStockAsync(int productId, int quantity)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) throw new InvalidOperationException("Product does not exist.");
            product.Stock += quantity;
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }
    }
}

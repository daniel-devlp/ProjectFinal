using Microsoft.EntityFrameworkCore;
using Project.Application.Dtos;
using Project.Domain.Entities;
using Project.Domain.Interfaces;
using Project.Infrastructure.Frameworks.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.Services
{
    public class ProductService : IProductServices
    {
        private readonly IProductRepository _productRepository;
        private readonly ApplicationDBContext _context;

        public ProductService(IProductRepository productRepository,ApplicationDBContext context )
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        }

        public async Task<ProductDto> GetByIdAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Product ID must be greater than zero.", nameof(id));

            var product = await _productRepository.GetByIdAsync(id);
            return product != null ? ToProductDto(product) : null;
        }

        public async Task<ProductDto> GetByCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Product code is required.", nameof(code));

            var product = await _productRepository.GetByCodeAsync(code.Trim());
            return product != null ? ToProductDto(product) : null;
        }

        public async Task<PagedResult<ProductDto>> GetAllAsync(int pageNumber, int pageSize, string searchTerm = null)
        {
            if (pageNumber <= 0) throw new ArgumentException("Page number must be greater than zero.", nameof(pageNumber));
            if (pageSize <= 0) throw new ArgumentException("Page size must be greater than zero.", nameof(pageSize));

            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string term = searchTerm.Trim().ToLower();
                query = query.Where(p =>
                    (p.Name != null && p.Name.ToLower().Contains(term)) ||
                    (p.Code != null && p.Code.ToLower().Contains(term)) ||
                    (p.Description != null && p.Description.ToLower().Contains(term))
                );
            }

            var totalCount = await query.CountAsync();

            var products = await query
                .OrderBy(p => p.ProductId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            var productDtos = products.Select(p => new ProductDto
            {
                ProductId = p.ProductId,
                Code = p.Code,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                IsActive = p.IsActive
            }).ToList();

            return new PagedResult<ProductDto>
            {
                Items = productDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task AddAsync(ProductCreateDto productDto)
        {
            if (productDto == null) throw new ArgumentNullException(nameof(productDto));
            if (string.IsNullOrWhiteSpace(productDto.Code)) throw new ArgumentException("Product code is required.", nameof(productDto.Code));
            if (string.IsNullOrWhiteSpace(productDto.Name)) throw new ArgumentException("Product name is required.", nameof(productDto.Name));
            if (productDto.Price < 0) throw new ArgumentException("Price cannot be negative.", nameof(productDto.Price));
            if (productDto.Stock < 0) throw new ArgumentException("Stock cannot be negative.", nameof(productDto.Stock));

            // Check uniqueness
            if (await _productRepository.ExistsAsync(productDto.Code.Trim()))
                throw new InvalidOperationException("A product with this code already exists.");

            var product = new Product
            {
                Code = productDto.Code.Trim(),
                Name = productDto.Name.Trim(),
                Description = productDto.Description?.Trim(),
                Price = productDto.Price,
                Stock = productDto.Stock,
                IsActive = true
            };
            await _productRepository.AddAsync(product);
        }

        public async Task UpdateAsync(ProductUpdateDto productDto)
        {
            if (productDto == null) throw new ArgumentNullException(nameof(productDto));
            if (productDto.ProductId <= 0) throw new ArgumentException("Product ID must be greater than zero.", nameof(productDto.ProductId));
            if (string.IsNullOrWhiteSpace(productDto.Code)) throw new ArgumentException("Product code is required.", nameof(productDto.Code));
            if (string.IsNullOrWhiteSpace(productDto.Name)) throw new ArgumentException("Product name is required.", nameof(productDto.Name));
            if (productDto.Price < 0) throw new ArgumentException("Price cannot be negative.", nameof(productDto.Price));
            if (productDto.Stock < 0) throw new ArgumentException("Stock cannot be negative.", nameof(productDto.Stock));

            var existingProduct = await _productRepository.GetByIdAsync(productDto.ProductId);
            if (existingProduct == null)
                throw new InvalidOperationException("Product does not exist.");

            // If code is changed, check uniqueness
            if (!string.Equals(existingProduct.Code, productDto.Code, StringComparison.OrdinalIgnoreCase))
            {
                if (await _productRepository.ExistsAsync(productDto.Code.Trim()))
                    throw new InvalidOperationException("A product with this code already exists.");
            }

            existingProduct.Code = productDto.Code.Trim();
            existingProduct.Name = productDto.Name.Trim();
            existingProduct.Description = productDto.Description?.Trim();
            existingProduct.Price = productDto.Price;
            existingProduct.Stock = productDto.Stock;
            existingProduct.IsActive = productDto.IsActive;

            await _productRepository.UpdateAsync(existingProduct);
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Product ID must be greater than zero.", nameof(id));

            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                throw new InvalidOperationException("Product does not exist.");

            await _productRepository.DeleteAsync(id);
        }

        public Task<bool> ExistsAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Product code is required.", nameof(code));
            return _productRepository.ExistsAsync(code.Trim());
        }

        public Task<int> CountAsync(string searchTerm = null)
        {
            return _productRepository.CountAsync(searchTerm?.Trim());
        }

        public async Task<IEnumerable<ProductDto>> GetAvailableProductsAsync(int pageNumber, int pageSize, string searchTerm = null)
        {
            if (pageNumber <= 0) throw new ArgumentException("Page number must be greater than zero.", nameof(pageNumber));
            if (pageSize <= 0) throw new ArgumentException("Page size must be greater than zero.", nameof(pageSize));

            var products = await _productRepository.GetAvailableProductsAsync(pageNumber, pageSize, searchTerm?.Trim());
            return products?.Select(ToProductDto) ?? Enumerable.Empty<ProductDto>();
        }

        public Task<bool> HasStockAsync(int productId, int quantity)
        {
            if (productId <= 0) throw new ArgumentException("Product ID must be greater than zero.", nameof(productId));
            if (quantity <= 0) throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
            return _productRepository.HasStockAsync(productId, quantity);
        }

        // Mapping method
        private ProductDto ToProductDto(Product product)
        {
            if (product == null) return null;
            return new ProductDto
            {
                ProductId = product.ProductId,
                Code = product.Code,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                IsActive = product.IsActive
            };
        }
    }
}

using Project.Application.Dtos;
using Project.Domain.Entities;
using Project.Domain.Interfaces;

namespace Project.Application.Services
{
    public class ProductService : IProductServices
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<ProductDto> GetByIdAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Product ID must be greater than zero.", nameof(id));

            var product = await _unitOfWork.Products.GetByIdAsync(id);
            return product != null ? ToProductDto(product) : null;
        }

        public async Task<ProductDto> GetByCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Product code is required.", nameof(code));

            var product = await _unitOfWork.Products.GetByCodeAsync(code.Trim());
            return product != null ? ToProductDto(product) : null;
        }

        public async Task<PagedResult<ProductDto>> GetAllAsync(int pageNumber, int pageSize, string searchTerm = null)
        {
            if (pageNumber <= 0) throw new ArgumentException("Page number must be greater than zero.", nameof(pageNumber));
            if (pageSize <= 0) throw new ArgumentException("Page size must be greater than zero.", nameof(pageSize));

            var (products, totalCount) = await _unitOfWork.Products.GetPagedAsync(pageNumber, pageSize, searchTerm);

            var productDtos = products.Select(ToProductDto).ToList();

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
            if (await _unitOfWork.Products.ExistsByCodeAsync(productDto.Code.Trim()))
                throw new InvalidOperationException("A product with this code already exists.");

            var product = new Product
            {
                Code = productDto.Code.Trim(),
                Name = productDto.Name.Trim(),
                Description = productDto.Description?.Trim(),
                Price = productDto.Price,
                Stock = productDto.Stock,
                IsActive = true,
                ImageUri = productDto.ImageUri
            };

            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateAsync(ProductUpdateDto productDto)
        {
            if (productDto == null) throw new ArgumentNullException(nameof(productDto));
            if (productDto.ProductId <= 0) throw new ArgumentException("Product ID must be greater than zero.", nameof(productDto.ProductId));
            if (string.IsNullOrWhiteSpace(productDto.Code)) throw new ArgumentException("Product code is required.", nameof(productDto.Code));
            if (string.IsNullOrWhiteSpace(productDto.Name)) throw new ArgumentException("Product name is required.", nameof(productDto.Name));
            if (productDto.Price < 0) throw new ArgumentException("Price cannot be negative.", nameof(productDto.Price));
            if (productDto.Stock < 0) throw new ArgumentException("Stock cannot be negative.", nameof(productDto.Stock));

            var existingProduct = await _unitOfWork.Products.GetByIdAsync(productDto.ProductId);
            if (existingProduct == null)
                throw new InvalidOperationException("Product does not exist.");

            // If code is changed, check uniqueness
            if (!string.Equals(existingProduct.Code, productDto.Code, StringComparison.OrdinalIgnoreCase))
            {
                if (await _unitOfWork.Products.ExistsByCodeAsync(productDto.Code.Trim()))
                    throw new InvalidOperationException("A product with this code already exists.");
            }

            existingProduct.Code = productDto.Code.Trim();
            existingProduct.Name = productDto.Name.Trim();
            existingProduct.Description = productDto.Description?.Trim();
            existingProduct.Price = productDto.Price;
            existingProduct.Stock = productDto.Stock;
            existingProduct.IsActive = productDto.IsActive;

            _unitOfWork.Products.Update(existingProduct);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Product ID must be greater than zero.", nameof(id));

            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
                throw new InvalidOperationException("Product does not exist.");

            _unitOfWork.Products.Remove(product);
            await _unitOfWork.SaveChangesAsync();
        }

        public Task<bool> ExistsAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Product code is required.", nameof(code));
            return _unitOfWork.Products.ExistsByCodeAsync(code.Trim());
        }

        public async Task<int> CountAsync(string searchTerm = null)
        {
            return await _unitOfWork.Products.CountAsync(p => 
                string.IsNullOrEmpty(searchTerm) || 
                p.Name.Contains(searchTerm) || 
                p.Code.Contains(searchTerm) || 
                p.Description.Contains(searchTerm));
        }

        public async Task<IEnumerable<ProductDto>> GetAvailableProductsAsync(int pageNumber, int pageSize, string searchTerm = null)
        {
            if (pageNumber <= 0) throw new ArgumentException("Page number must be greater than zero.", nameof(pageNumber));
            if (pageSize <= 0) throw new ArgumentException("Page size must be greater than zero.", nameof(pageSize));

            var (products, _) = await _unitOfWork.Products.GetAvailableProductsAsync(pageNumber, pageSize, searchTerm?.Trim());
            return products?.Select(ToProductDto) ?? Enumerable.Empty<ProductDto>();
        }

        public Task<bool> HasStockAsync(int productId, int quantity)
        {
            if (productId <= 0) throw new ArgumentException("Product ID must be greater than zero.", nameof(productId));
            if (quantity <= 0) throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
            return _unitOfWork.Products.HasStockAsync(productId, quantity);
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

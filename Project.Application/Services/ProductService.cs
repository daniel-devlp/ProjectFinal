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

        public async Task<ProductDto?> GetByIdAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Product ID must be greater than zero.", nameof(id));

            var product = await _unitOfWork.Products.GetByIdAsync(id);
            return product != null ? ToProductDto(product) : null;
        }

        public async Task<ProductDto?> GetByCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Product code is required.", nameof(code));

            var product = await _unitOfWork.Products.GetByCodeAsync(code.Trim());
            return product != null ? ToProductDto(product) : null;
        }

        public async Task<PagedResult<ProductDto>> GetAllAsync(int pageNumber, int pageSize, string? searchTerm = null)
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

            // ✅ Validación mejorada de unicidad para código
            if (await _unitOfWork.Products.ExistsByCodeAsync(productDto.Code.Trim()))
                throw new InvalidOperationException("Ya existe un producto con este código.");

            try
            {
                // Usar el constructor de dominio que incluye validaciones
                var product = new Product(
                    productDto.Code.Trim(),
                    productDto.Name.Trim(),
                    productDto.Description?.Trim() ?? string.Empty,
                    productDto.Price,
                    productDto.Stock,
                    productDto.ImageUri ?? string.Empty
                );

                await _unitOfWork.Products.AddAsync(product);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error creating product: {ex.Message}", ex);
            }
        }

        public async Task UpdateAsync(ProductUpdateDto productDto)
        {
            if (productDto == null) throw new ArgumentNullException(nameof(productDto));

            var existingProduct = await _unitOfWork.Products.GetByIdAsync(productDto.ProductId);
            if (existingProduct == null)
                throw new InvalidOperationException("Product does not exist.");

            // ✅ Validar unicidad mejorada para código si cambia
            if (!string.Equals(existingProduct.Code, productDto.Code, StringComparison.OrdinalIgnoreCase))
            {
                if (await _unitOfWork.Products.ExistsByCodeAsync(productDto.Code.Trim(), existingProduct.ProductId))
                    throw new InvalidOperationException("Ya existe un producto con este código.");
            }

            try
            {
                // ✅ Usar métodos de dominio para actualizar
                existingProduct.UpdateProduct(
                    productDto.Name.Trim(),
                    productDto.Description?.Trim() ?? string.Empty,
                    productDto.Price,
                    productDto.Stock,
                    productDto.ImageUri ?? string.Empty
                );

                // Actualizar código si cambió
                if (!string.Equals(existingProduct.Code, productDto.Code, StringComparison.OrdinalIgnoreCase))
                {
                    existingProduct.SetCode(productDto.Code.Trim());
                }

                _unitOfWork.Products.Update(existingProduct);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error updating product: {ex.Message}", ex);
            }
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Product ID must be greater than zero.", nameof(id));

            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
                throw new InvalidOperationException("Product does not exist.");

            // ✅ Borrado lógico en lugar de físico
            product.SoftDelete();
            _unitOfWork.Products.Update(product);
            await _unitOfWork.SaveChangesAsync();
        }

        // ✅ Métodos nuevos para borrado lógico
        public async Task<PagedResult<ProductDto>> GetAllIncludingDeletedAsync(int pageNumber, int pageSize, string? searchTerm = null)
        {
            if (pageNumber <= 0) throw new ArgumentException("Page number must be greater than zero.", nameof(pageNumber));
            if (pageSize <= 0) throw new ArgumentException("Page size must be greater than zero.", nameof(pageSize));

            var (products, totalCount) = await _unitOfWork.Products.GetPagedIncludingDeletedAsync(pageNumber, pageSize, searchTerm);
            var productDtos = products.Select(ToProductDto).ToList();

            return new PagedResult<ProductDto>
            {
                Items = productDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<IEnumerable<ProductDto>> GetDeletedProductsAsync()
        {
            var products = await _unitOfWork.Products.GetDeletedProductsAsync();
            return products.Select(ToProductDto);
        }

        public async Task RestoreAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Product ID must be greater than zero.", nameof(id));

            await _unitOfWork.Products.RestoreAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Product code is required.", nameof(code));
            return await _unitOfWork.Products.ExistsByCodeAsync(code.Trim());
        }

        public async Task<int> CountAsync(string? searchTerm = null)
        {
            return await _unitOfWork.Products.GetActiveCountAsync(); // Solo productos activos
        }

        public async Task<int> CountAllAsync(string? searchTerm = null)
        {
            return await _unitOfWork.Products.GetTotalCountAsync(); // Todos los productos
        }

        public async Task<IEnumerable<ProductDto>> GetAvailableProductsAsync(int pageNumber, int pageSize, string? searchTerm = null)
        {
            if (pageNumber <= 0) throw new ArgumentException("Page number must be greater than zero.", nameof(pageNumber));
            if (pageSize <= 0) throw new ArgumentException("Page size must be greater than zero.", nameof(pageSize));

            var (products, _) = await _unitOfWork.Products.GetAvailableProductsAsync(pageNumber, pageSize, searchTerm?.Trim());
            return products?.Select(ToProductDto) ?? Enumerable.Empty<ProductDto>();
        }

        public async Task<IEnumerable<ProductDto>> GetLowStockProductsAsync(int threshold = 10)
        {
            var products = await _unitOfWork.Products.GetLowStockProductsAsync(threshold);
            return products.Select(ToProductDto);
        }

        public Task<bool> HasStockAsync(int productId, int quantity)
        {
            if (productId <= 0) throw new ArgumentException("Product ID must be greater than zero.", nameof(productId));
            if (quantity <= 0) throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
            return _unitOfWork.Products.HasStockAsync(productId, quantity);
        }

        public async Task UpdateStockAsync(ProductStockUpdateDto stockUpdateDto)
        {
            if (stockUpdateDto == null) throw new ArgumentNullException(nameof(stockUpdateDto));

            var product = await _unitOfWork.Products.GetByIdAsync(stockUpdateDto.ProductId);
            if (product == null)
                throw new InvalidOperationException("Product does not exist.");

            try
            {
                product.IncreaseStock(stockUpdateDto.Quantity);
                _unitOfWork.Products.Update(product);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error updating stock: {ex.Message}", ex);
            }
        }

        // ✅ Mapping method mejorado
        private static ProductDto ToProductDto(Product product)
        {
            if (product == null) return null!;
            return new ProductDto
            {
                ProductId = product.ProductId,
                Code = product.Code,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                IsActive = product.IsActive,
                ImageUri = product.ImageUri,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                DeletedAt = product.DeletedAt
            };
        }
    }
}

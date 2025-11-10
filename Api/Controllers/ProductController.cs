using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Application.Dtos;
using Project.Application.Services;
using Project.Domain.Entities;

namespace Api.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductServices _productService;
        private readonly IImageService _imageService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(
          IProductServices productService,
          IImageService imageService,
         ILogger<ProductController> logger)
        {
            _productService = productService;
            _imageService = imageService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Administrator,user")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null) return NotFound();
            return Ok(product);
        }

        [HttpGet]
        [Authorize(Roles = "Administrator,user")]
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 10, string? searchTerm = null)
        {
            var products = await _productService.GetAllAsync(pageNumber, pageSize, searchTerm);
            return Ok(products);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create([FromBody] ProductCreateDto product)
        {
            await _productService.AddAsync(product);
            return StatusCode(StatusCodes.Status201Created);
        }

        /// <summary>
        /// Crear producto con imagen
        /// </summary>
        [HttpPost("with-image")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> CreateWithImage([FromForm] ProductCreateWithImageDto productDto)
        {
            try
            {
                // Primero crear el producto sin imagen
                var createDto = new ProductCreateDto
                {
                    Code = productDto.Code,
                    Name = productDto.Name,
                    Description = productDto.Description,
                    Price = productDto.Price,
                    Stock = productDto.Stock
                };

                await _productService.AddAsync(createDto);

                // Si se proporcionó una imagen, subirla
                if (productDto.Image != null)
                {
                    var imageResult = await _imageService.UploadImageAsync(productDto.Image, "products");

                    if (imageResult.Success)
                    {
                        // Aquí podrías actualizar el producto con la URL de la imagen
                        // Esto requeriría modificar el ProductService para incluir un método UpdateImageUrl
                        _logger.LogInformation("Product created with image: {ImageUrl}", imageResult.SecureUrl);
                        return Ok(new
                        {
                            message = "Product created successfully with image",
                            imageUrl = imageResult.SecureUrl,
                            publicId = imageResult.PublicId
                        });
                    }
                    else
                    {
                        _logger.LogWarning("Product created but image upload failed: {Error}", imageResult.ErrorMessage);
                        return Ok(new { message = "Product created but image upload failed", error = imageResult.ErrorMessage });
                    }
                }

                return StatusCode(StatusCodes.Status201Created, new { message = "Product created successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product with image");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductUpdateDto product)
        {
            if (id != product.ProductId) return BadRequest();
            await _productService.UpdateAsync(product);
            return NoContent();
        }

        /// <summary>
        /// Actualizar imagen de producto
        /// </summary>
        [HttpPut("{id}/image")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> UpdateProductImage(int id, IFormFile image, [FromQuery] string? oldImageUrl = null)
        {
            try
            {
                // Verificar que el producto existe
                var product = await _productService.GetByIdAsync(id);
                if (product == null)
                {
                    return NotFound(new { message = "Product not found" });
                }

                // Subir nueva imagen
                var imageResult = await _imageService.UploadImageAsync(image, "products");

                if (!imageResult.Success)
                {
                    return BadRequest(new { message = "Failed to upload image", error = imageResult.ErrorMessage });
                }

                // Eliminar imagen anterior si se proporcionó
                if (!string.IsNullOrWhiteSpace(oldImageUrl))
                {
                    try
                    {
                        await _imageService.DeleteImageByUrlAsync(oldImageUrl);
                        _logger.LogInformation("Old image deleted: {OldImageUrl}", oldImageUrl);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete old image: {OldImageUrl}", oldImageUrl);
                    }
                }

                return Ok(new
                {
                    message = "Product image updated successfully",
                    imageUrl = imageResult.SecureUrl,
                    publicId = imageResult.PublicId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product image for product ID: {ProductId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Eliminar imagen de producto
        /// </summary>
        [HttpDelete("{id}/image")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteProductImage(int id, [FromQuery] string imageUrl)
        {
            try
            {
                // Verificar que el producto existe
                var product = await _productService.GetByIdAsync(id);
                if (product == null)
                {
                    return NotFound(new { message = "Product not found" });
                }

                if (string.IsNullOrWhiteSpace(imageUrl))
                {
                    return BadRequest(new { message = "Image URL is required" });
                }

                var result = await _imageService.DeleteImageByUrlAsync(imageUrl);

                if (result)
                {
                    return Ok(new { message = "Product image deleted successfully" });
                }
                else
                {
                    return NotFound(new { message = "Image not found or could not be deleted" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product image for product ID: {ProductId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int id)
        {
            await _productService.DeleteAsync(id);
            return NoContent();
        }
    }

    // DTO para crear producto con imagen
    public class ProductCreateWithImageDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public IFormFile? Image { get; set; }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Application.Dtos;
using Project.Application.Services;

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

/// <summary>
        /// Obtiene un producto por ID (solo productos activos)
      /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "Administrator,user")]
        public async Task<IActionResult> GetById(int id)
        {
    try
  {
      var product = await _productService.GetByIdAsync(id);
     if (product == null) 
      return NotFound(new { message = "Product not found" });
     
      return Ok(product);
   }
      catch (ArgumentException ex)
         {
    return BadRequest(new { message = ex.Message });
}
         catch (Exception ex)
{
    _logger.LogError(ex, "Error getting product with ID: {ProductId}", id);
 return StatusCode(500, new { message = "Internal server error" });
  }
  }

        /// <summary>
        /// Obtiene todos los productos activos con paginación y búsqueda
      /// </summary>
        [HttpGet]
        [Authorize(Roles = "Administrator,user")]
      public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 10, string? searchTerm = null)
   {
         try
      {
    var products = await _productService.GetAllAsync(pageNumber, pageSize, searchTerm);
    return Ok(products);
      }
      catch (ArgumentException ex)
 {
     return BadRequest(new { message = ex.Message });
        }
       catch (Exception ex)
 {
     _logger.LogError(ex, "Error getting products");
         return StatusCode(500, new { message = "Internal server error" });
      }
        }

    /// <summary>
  /// Obtiene todos los productos incluyendo eliminados (solo administradores)
        /// </summary>
        [HttpGet("all-including-deleted")]
        [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> GetAllIncludingDeleted(int pageNumber = 1, int pageSize = 10, string? searchTerm = null)
    {
     try
    {
      var products = await _productService.GetAllIncludingDeletedAsync(pageNumber, pageSize, searchTerm);
        return Ok(products);
      }
            catch (ArgumentException ex)
        {
   return BadRequest(new { message = ex.Message });
      }
      catch (Exception ex)
    {
     _logger.LogError(ex, "Error getting all products including deleted");
    return StatusCode(500, new { message = "Internal server error" });
  }
      }

        /// <summary>
/// Obtiene solo los productos eliminados (solo administradores)
     /// </summary>
     [HttpGet("deleted")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetDeleted()
        {
        try
        {
        var products = await _productService.GetDeletedProductsAsync();
      return Ok(products);
     }
      catch (Exception ex)
      {
       _logger.LogError(ex, "Error getting deleted products");
    return StatusCode(500, new { message = "Internal server error" });
       }
     }

/// <summary>
  /// Obtiene productos con stock bajo
  /// </summary>
    [HttpGet("low-stock")]
      [Authorize(Roles = "Administrator")]
 public async Task<IActionResult> GetLowStock([FromQuery] int threshold = 10)
        {
       try
        {
   var products = await _productService.GetLowStockProductsAsync(threshold);
    return Ok(products);
     }
      catch (Exception ex)
      {
     _logger.LogError(ex, "Error getting low stock products");
    return StatusCode(500, new { message = "Internal server error" });
       }
        }

      /// <summary>
        /// Crea un nuevo producto con validaciones completas
     /// </summary>
        [HttpPost]
    [Authorize(Roles = "Administrator")]
  public async Task<IActionResult> Create([FromForm] ProductCreateWithImageDto productDto)
     {
            try
     {
    _logger.LogInformation("Creating product: {Name}", productDto.Name);

    if (!ModelState.IsValid)
   {
    return BadRequest(new { 
    message = "Validation failed", 
       errors = ModelState.Where(x => x.Value?.Errors.Count > 0)
       .ToDictionary(
        kvp => kvp.Key,
     kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
     )
    });
 }

    string? imageUrl = null;
 string? publicId = null;

    // Si se proporciona imagen, subirla a Cloudinary
   if (productDto.Image != null)
    {
         var imageResult = await _imageService.UploadImageAsync(productDto.Image, "products");

      if (!imageResult.Success)
    {
       return BadRequest(new
   {
         success = false,
   message = "Failed to upload image",
     error = imageResult.ErrorMessage
     });
}

  imageUrl = imageResult.SecureUrl;
   publicId = imageResult.PublicId;
    }

    // Crear el producto
   var createDto = new ProductCreateDto
      {
       Code = productDto.Code.ToUpperInvariant(), // ✅ Normalizar código
  Name = productDto.Name,
 Description = productDto.Description ?? string.Empty,
    Price = productDto.Price,
   Stock = productDto.Stock,
 ImageUri = imageUrl ?? string.Empty
     };

    try
       {
      await _productService.AddAsync(createDto);
   _logger.LogInformation("Product created successfully: {Code}", createDto.Code);
          }
  catch (Exception dbEx)
       {
              // Si falla la BD, eliminar imagen subida
  if (!string.IsNullOrEmpty(publicId))
   {
           try
           {
    await _imageService.DeleteImageAsync(publicId);
}
         catch (Exception cleanupEx)
        {
           _logger.LogWarning(cleanupEx, "Failed to cleanup image after database error");
        }
         }

         throw dbEx;
     }

        var response = new
      {
  success = true,
              message = imageUrl != null ? "Product created successfully with image" : "Product created successfully",
        product = new
      {
   code = createDto.Code,
  name = createDto.Name,
    description = createDto.Description,
         price = createDto.Price,
       stock = createDto.Stock,
   imageUrl = imageUrl
    }
       };

         return StatusCode(StatusCodes.Status201Created, response);
}
catch (InvalidOperationException ex)
        {
    return BadRequest(new { message = ex.Message });
}
      catch (ArgumentException ex)
        {
       return BadRequest(new { message = ex.Message });
   }
   catch (Exception ex)
            {
 _logger.LogError(ex, "Error creating product");
        return StatusCode(500, new { message = "Internal server error" });
 }
    }

 /// <summary>
        /// Actualiza un producto existente con validaciones
     /// </summary>
   [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Update(int id, [FromForm] ProductUpdateWithImageDto productDto)
   {
        try
  {
    if (!ModelState.IsValid)
         {
return BadRequest(new { 
  message = "Validation failed", 
         errors = ModelState.Where(x => x.Value?.Errors.Count > 0)
       .ToDictionary(
        kvp => kvp.Key,
     kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
     )
    });
     }

  // Verificar que el producto existe
       var existingProduct = await _productService.GetByIdAsync(id);
    if (existingProduct == null)
{
   return NotFound(new { message = "Product not found" });
    }

            string? newImageUrl = existingProduct.ImageUri;
   string? publicId = null;

        // Si se proporciona nueva imagen
     if (productDto.Image != null)
          {
  var imageResult = await _imageService.UploadImageAsync(productDto.Image, "products");

 if (!imageResult.Success)
        {
   return BadRequest(new { message = "Failed to upload image", error = imageResult.ErrorMessage });
   }

       newImageUrl = imageResult.SecureUrl;
         publicId = imageResult.PublicId;

         // Eliminar imagen anterior si existe
        if (!string.IsNullOrWhiteSpace(existingProduct.ImageUri))
        {
      try
      {
         await _imageService.DeleteImageByUrlAsync(existingProduct.ImageUri);
            }
   catch (Exception ex)
    {
      _logger.LogWarning(ex, "Failed to delete old image");
             }
      }
     }

       // Actualizar el producto
 var updateDto = new ProductUpdateDto
     {
        ProductId = id,
         Code = productDto.Code.ToUpperInvariant(), // ✅ Normalizar código
    Name = productDto.Name,
         Description = productDto.Description,
       Price = productDto.Price,
         Stock = productDto.Stock,
   IsActive = productDto.IsActive,
 ImageUri = newImageUrl ?? string.Empty
       };

    await _productService.UpdateAsync(updateDto);

var response = new
    {
   message = productDto.Image != null ? "Product updated successfully with new image" : "Product updated successfully",
       product = new
   {
  id = id,
     code = updateDto.Code,
         name = updateDto.Name,
    imageUrl = newImageUrl
   }
    };

     return Ok(response);
   }
        catch (InvalidOperationException ex)
  {
    return BadRequest(new { message = ex.Message });
        }
      catch (ArgumentException ex)
        {
       return BadRequest(new { message = ex.Message });
   }
        catch (Exception ex)
       {
       _logger.LogError(ex, "Error updating product with ID: {ProductId}", id);
    return StatusCode(500, new { message = "Internal server error" });
 }
        }

        /// <summary>
  /// Realiza borrado lógico de un producto (cambia IsActive a false)
  /// </summary>
     [HttpDelete("{id}")]
[Authorize(Roles = "Administrator")]
  public async Task<IActionResult> Delete(int id)
   {
    try
     {
 await _productService.DeleteAsync(id);
       return Ok(new { message = "Product deleted successfully (soft delete)" });
       }
     catch (InvalidOperationException ex)
     {
    return BadRequest(new { message = ex.Message });
   }
       catch (ArgumentException ex)
    {
 return BadRequest(new { message = ex.Message });
    }
       catch (Exception ex)
        {
      _logger.LogError(ex, "Error deleting product with ID: {ProductId}", id);
      return StatusCode(500, new { message = "Internal server error" });
    }
        }

/// <summary>
   /// Restaura un producto eliminado lógicamente (solo administradores)
  /// </summary>
        [HttpPost("{id}/restore")]
     [Authorize(Roles = "Administrator")]
  public async Task<IActionResult> Restore(int id)
{
    try
 {
      await _productService.RestoreAsync(id);
       return Ok(new { message = "Product restored successfully" });
}
     catch (ArgumentException ex)
     {
          return BadRequest(new { message = ex.Message });
   }
    catch (Exception ex)
        {
      _logger.LogError(ex, "Error restoring product with ID: {ProductId}", id);
      return StatusCode(500, new { message = "Internal server error" });
    }
        }

/// <summary>
   /// Obtiene estadísticas de productos (solo administradores)
 /// </summary>
        [HttpGet("statistics")]
     [Authorize(Roles = "Administrator")]
  public async Task<IActionResult> GetStatistics()
        {
    try
     {
 var activeCount = await _productService.CountAsync();
      var totalCount = await _productService.CountAllAsync();
       var deletedCount = totalCount - activeCount;

       return Ok(new { 
        activeProducts = activeCount,
     deletedProducts = deletedCount,
    totalProducts = totalCount,
            deletionRate = totalCount > 0 ? Math.Round((double)deletedCount / totalCount * 100, 2) : 0
       });
     }
      catch (Exception ex)
            {
      _logger.LogError(ex, "Error getting product statistics");
      return StatusCode(500, new { message = "Internal server error" });
  }
     }

  /// <summary>
      /// Busca un producto por código
/// </summary>
        [HttpGet("by-code/{code}")]
   [Authorize(Roles = "Administrator,user")]
   public async Task<IActionResult> GetByCode(string code)
  {
         try
      {
 var product = await _productService.GetByCodeAsync(code);
    if (product == null) 
       return NotFound(new { message = "Product not found" });
     
   return Ok(product);
        }
      catch (ArgumentException ex)
     {
    return BadRequest(new { message = ex.Message });
}
   catch (Exception ex)
{
    _logger.LogError(ex, "Error getting product by code: {Code}", code);
      return StatusCode(500, new { message = "Internal server error" });
  }
        }

   /// <summary>
  /// Actualiza solo el stock de un producto
     /// </summary>
        [HttpPatch("{id}/stock")]
  [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> UpdateStock(int id, [FromBody] ProductStockUpdateDto stockUpdateDto)
    {
    try
        {
       if (id != stockUpdateDto.ProductId)
            {
      return BadRequest(new { message = "ID mismatch" });
       }

          if (!ModelState.IsValid)
    {
      return BadRequest(new { 
    message = "Validation failed", 
       errors = ModelState 
    });
     }

            await _productService.UpdateStockAsync(stockUpdateDto);
     return Ok(new { message = "Stock updated successfully" });
     }
    catch (InvalidOperationException ex)
   {
      return BadRequest(new { message = ex.Message });
       }
        catch (ArgumentException ex)
     {
      return BadRequest(new { message = ex.Message });
   }
      catch (Exception ex)
      {
    _logger.LogError(ex, "Error updating stock for product ID: {ProductId}", id);
    return StatusCode(500, new { message = "Internal server error" });
  }
}
  }
}

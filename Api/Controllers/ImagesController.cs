using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Application.Dtos;
using Project.Application.Services;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImagesController : ControllerBase
    {
        private readonly IImageService _imageService;
        private readonly ILogger<ImagesController> _logger;

    public ImagesController(IImageService imageService, ILogger<ImagesController> logger)
    {
            _imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));
     _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Sube una imagen a Cloudinary
        /// </summary>
  [HttpPost("upload")]
      [Authorize]
        public async Task<IActionResult> UploadImage(IFormFile file, [FromQuery] string folder = "products")
      {
        try
       {
   if (file == null || file.Length == 0)
           {
             return BadRequest(new { message = "No file provided" });
         }

              var result = await _imageService.UploadImageAsync(file, folder);
   
      if (result.Success)
       {
  return Ok(new
          {
  message = "Image uploaded successfully",
              data = result
   });
      }
                else
     {
           return BadRequest(new { message = result.ErrorMessage });
          }
 }
          catch (Exception ex)
{
                _logger.LogError(ex, "Error uploading image");
     return StatusCode(500, new { message = "Internal server error" });
            }
        }

     /// <summary>
 /// Sube múltiples imágenes a Cloudinary
    /// </summary>
  [HttpPost("upload-multiple")]
   [Authorize]
        public async Task<IActionResult> UploadMultipleImages(List<IFormFile> files, [FromQuery] string folder = "products")
        {
            try
            {
        if (files == null || !files.Any())
   {
             return BadRequest(new { message = "No files provided" });
  }

              var results = await _imageService.UploadMultipleImagesAsync(files, folder);
           
         var successCount = results.Count(r => r.Success);
var failCount = results.Count(r => !r.Success);

   return Ok(new
   {
         message = $"Upload completed. {successCount} successful, {failCount} failed",
   results = results
   });
}
     catch (Exception ex)
     {
      _logger.LogError(ex, "Error uploading multiple images");
    return StatusCode(500, new { message = "Internal server error" });
  }
        }

    /// <summary>
    /// Elimina una imagen de Cloudinary por public ID
        /// </summary>
        [HttpDelete("{publicId}")]
 [Authorize]
      public async Task<IActionResult> DeleteImage(string publicId)
        {
   try
       {
                if (string.IsNullOrWhiteSpace(publicId))
{
          return BadRequest(new { message = "Public ID is required" });
        }

   var result = await _imageService.DeleteImageAsync(publicId);
       
           if (result)
    {
        return Ok(new { message = "Image deleted successfully" });
   }
       else
     {
        return NotFound(new { message = "Image not found or could not be deleted" });
   }
         }
            catch (Exception ex)
            {
   _logger.LogError(ex, "Error deleting image with public ID: {PublicId}", publicId);
      return StatusCode(500, new { message = "Internal server error" });
 }
        }

        /// <summary>
      /// Elimina una imagen de Cloudinary por URL
        /// </summary>
   [HttpDelete("by-url")]
        [Authorize]
        public async Task<IActionResult> DeleteImageByUrl([FromBody] DeleteImageByUrlRequest request)
        {
            try
            {
  if (string.IsNullOrWhiteSpace(request.ImageUrl))
     {
 return BadRequest(new { message = "Image URL is required" });
  }

    var result = await _imageService.DeleteImageByUrlAsync(request.ImageUrl);
 
      if (result)
        {
               return Ok(new { message = "Image deleted successfully" });
      }
  else
       {
    return NotFound(new { message = "Image not found or could not be deleted" });
        }
   }
        catch (Exception ex)
     {
    _logger.LogError(ex, "Error deleting image by URL: {Url}", request.ImageUrl);
 return StatusCode(500, new { message = "Internal server error" });
      }
        }

        /// <summary>
        /// Extrae el public ID de una URL de Cloudinary
        /// </summary>
        [HttpPost("extract-public-id")]
        public IActionResult ExtractPublicId([FromBody] ExtractPublicIdRequest request)
        {
   try
  {
     if (string.IsNullOrWhiteSpace(request.ImageUrl))
     {
      return BadRequest(new { message = "Image URL is required" });
}

         var publicId = _imageService.ExtractPublicIdFromUrl(request.ImageUrl);
        
  if (string.IsNullOrWhiteSpace(publicId))
  {
 return BadRequest(new { message = "Could not extract public ID from URL" });
             }

         return Ok(new { publicId = publicId });
     }
      catch (Exception ex)
   {
           _logger.LogError(ex, "Error extracting public ID from URL: {Url}", request.ImageUrl);
                return StatusCode(500, new { message = "Internal server error" });
         }
    }
    }

    // Request DTOs
    public class DeleteImageByUrlRequest
    {
    public string ImageUrl { get; set; } = string.Empty;
    }

    public class ExtractPublicIdRequest
    {
        public string ImageUrl { get; set; } = string.Empty;
    }
}
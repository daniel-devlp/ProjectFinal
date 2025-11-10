using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Project.Application.Dtos;
using Project.Application.Services;
using Project.Application.Settings;

namespace Project.Infrastructure.Services
{
    public class ImageService : IImageService
 {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<ImageService> _logger;
  private readonly CloudinarySettings _settings;

      public ImageService(Cloudinary cloudinary, ILogger<ImageService> logger, IOptions<CloudinarySettings> settings)
        {
   _cloudinary = cloudinary ?? throw new ArgumentNullException(nameof(cloudinary));
     _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
        }

      public async Task<ImageUploadResultDto> UploadImageAsync(IFormFile file, string folder = "products")
        {
    try
     {
      if (file == null || file.Length == 0)
   {
     return new ImageUploadResultDto
            {
   Success = false,
 ErrorMessage = "No file provided or file is empty"
       };
    }

        // Validar tipo de archivo
        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
     if (!allowedTypes.Contains(file.ContentType.ToLower()))
      {
 return new ImageUploadResultDto
         {
    Success = false,
                  ErrorMessage = "Invalid file type. Only JPEG, PNG, GIF, and WebP are allowed."
                };
 }

     // Validar tamaño (máximo 10MB)
 const int maxSizeInBytes = 10 * 1024 * 1024;
     if (file.Length > maxSizeInBytes)
  {
      return new ImageUploadResultDto
      {
    Success = false,
                  ErrorMessage = "File size exceeds 10MB limit"
  };
      }

       using var stream = file.OpenReadStream();
 var uploadParams = new ImageUploadParams
   {
      File = new FileDescription(file.FileName, stream),
       Folder = folder,
        PublicId = $"{folder}_{Guid.NewGuid()}",
  Transformation = new Transformation()
       .Quality("auto")
    .FetchFormat("auto")
    .Crop("limit")
      .Width(1200)
      .Height(1200),
UseFilename = false,
    UniqueFilename = true,
    Overwrite = false
       };

             var uploadResult = await _cloudinary.UploadAsync(uploadParams);

       if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
 _logger.LogInformation("Image uploaded successfully: {PublicId}", uploadResult.PublicId);
         
     return new ImageUploadResultDto
             {
   Success = true,
         PublicId = uploadResult.PublicId,
     SecureUrl = uploadResult.SecureUrl?.ToString() ?? string.Empty,
         Url = uploadResult.Url?.ToString() ?? string.Empty,
      Format = uploadResult.Format ?? string.Empty,
      ResourceType = uploadResult.ResourceType ?? string.Empty,
        Bytes = uploadResult.Bytes,
                   Width = uploadResult.Width,
          Height = uploadResult.Height,
     CreatedAt = uploadResult.CreatedAt
           };
         }
           else
   {
  _logger.LogError("Failed to upload image: {Error}", uploadResult.Error?.Message);
       return new ImageUploadResultDto
  {
       Success = false,
   ErrorMessage = uploadResult.Error?.Message ?? "Upload failed"
 };
     }
     }
            catch (Exception ex)
    {
        _logger.LogError(ex, "Exception occurred while uploading image");
        return new ImageUploadResultDto
        {
   Success = false,
    ErrorMessage = $"Upload failed: {ex.Message}"
    };
            }
     }

     public async Task<ImageUploadResultDto> UploadImageAsync(Stream imageStream, string fileName, string folder = "products")
        {
            try
  {
if (imageStream == null || imageStream.Length == 0)
     {
    return new ImageUploadResultDto
    {
     Success = false,
     ErrorMessage = "No image stream provided or stream is empty"
 };
     }

       var uploadParams = new ImageUploadParams
      {
 File = new FileDescription(fileName, imageStream),
       Folder = folder,
      PublicId = $"{folder}_{Guid.NewGuid()}",
     Transformation = new Transformation()
 .Quality("auto")
      .FetchFormat("auto")
   .Crop("limit")
  .Width(1200)
       .Height(1200),
       UseFilename = false,
   UniqueFilename = true,
       Overwrite = false
       };

      var uploadResult = await _cloudinary.UploadAsync(uploadParams);

     if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
          {
    return new ImageUploadResultDto
     {
       Success = true,
    PublicId = uploadResult.PublicId,
      SecureUrl = uploadResult.SecureUrl?.ToString() ?? string.Empty,
    Url = uploadResult.Url?.ToString() ?? string.Empty,
  Format = uploadResult.Format ?? string.Empty,
    ResourceType = uploadResult.ResourceType ?? string.Empty,
      Bytes = uploadResult.Bytes,
          Width = uploadResult.Width,
    Height = uploadResult.Height,
  CreatedAt = uploadResult.CreatedAt
     };
                }
  else
    {
     return new ImageUploadResultDto
           {
     Success = false,
ErrorMessage = uploadResult.Error?.Message ?? "Upload failed"
  };
       }
     }
     catch (Exception ex)
  {
    _logger.LogError(ex, "Exception occurred while uploading image from stream");
        return new ImageUploadResultDto
            {
         Success = false,
       ErrorMessage = $"Upload failed: {ex.Message}"
          };
   }
        }

       public async Task<bool> DeleteImageAsync(string publicId)
        {
     try
         {
 if (string.IsNullOrWhiteSpace(publicId))
          {
    _logger.LogWarning("Attempted to delete image with empty public ID");
  return false;
    }

    var deletionParams = new DeletionParams(publicId);
   var result = await _cloudinary.DestroyAsync(deletionParams);
         
      if (result.StatusCode == System.Net.HttpStatusCode.OK && result.Result == "ok")
    {
       _logger.LogInformation("Image deleted successfully: {PublicId}", publicId);
          return true;
       }
              else
          {
     _logger.LogWarning("Failed to delete image: {PublicId}, Result: {Result}", publicId, result.Result);
          return false;
      }
            }
       catch (Exception ex)
            {
       _logger.LogError(ex, "Exception occurred while deleting image: {PublicId}", publicId);
  return false;
            }
        }

        public async Task<bool> DeleteImageByUrlAsync(string imageUrl)
     {
       try
            {
       var publicId = ExtractPublicIdFromUrl(imageUrl);
     if (string.IsNullOrWhiteSpace(publicId))
          {
           _logger.LogWarning("Could not extract public ID from URL: {Url}", imageUrl);
          return false;
          }

   return await DeleteImageAsync(publicId);
          }
       catch (Exception ex)
 {
           _logger.LogError(ex, "Exception occurred while deleting image by URL: {Url}", imageUrl);
     return false;
 }
        }

        public string ExtractPublicIdFromUrl(string imageUrl)
        {
  try
    {
      if (string.IsNullOrWhiteSpace(imageUrl))
        return string.Empty;

     // Cloudinary URL format: https://res.cloudinary.com/{cloud_name}/{resource_type}/{type}/{version}/{public_id}.{format}
     var uri = new Uri(imageUrl);
 var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length >= 4)
           {
          // Obtener el public_id (puede incluir carpetas)
                  var publicIdWithExtension = string.Join("/", segments.Skip(3));
     
      // Remover la extensión del archivo
          var lastDotIndex = publicIdWithExtension.LastIndexOf('.');
     if (lastDotIndex > 0)
       {
     return publicIdWithExtension.Substring(0, lastDotIndex);
            }
     
       return publicIdWithExtension;
  }

     return string.Empty;
    }
 catch (Exception ex)
       {
         _logger.LogError(ex, "Exception occurred while extracting public ID from URL: {Url}", imageUrl);
          return string.Empty;
            }
        }

        public async Task<IEnumerable<ImageUploadResultDto>> UploadMultipleImagesAsync(IEnumerable<IFormFile> files, string folder = "products")
        {
    var results = new List<ImageUploadResultDto>();
       
   foreach (var file in files)
         {
     var result = await UploadImageAsync(file, folder);
     results.Add(result);
         }

  return results;
        }
    }
}
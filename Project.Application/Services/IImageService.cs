using Microsoft.AspNetCore.Http;
using Project.Application.Dtos;

namespace Project.Application.Services
{
    public interface IImageService
    {
      Task<ImageUploadResultDto> UploadImageAsync(IFormFile file, string folder = "products");
        Task<ImageUploadResultDto> UploadImageAsync(Stream imageStream, string fileName, string folder = "products");
        Task<bool> DeleteImageAsync(string publicId);
        Task<bool> DeleteImageByUrlAsync(string imageUrl);
        string ExtractPublicIdFromUrl(string imageUrl);
        Task<IEnumerable<ImageUploadResultDto>> UploadMultipleImagesAsync(IEnumerable<IFormFile> files, string folder = "products");
    }
}
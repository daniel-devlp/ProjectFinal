using Microsoft.AspNetCore.Http;

namespace Project.Application.Dtos
{
  public class ImageUploadResultDto
    {
        public string PublicId { get; set; } = string.Empty;
        public string SecureUrl { get; set; } = string.Empty;
 public string Url { get; set; } = string.Empty;
        public string Format { get; set; } = string.Empty;
   public string ResourceType { get; set; } = string.Empty;
        public long Bytes { get; set; }
   public int Width { get; set; }
        public int Height { get; set; }
    public DateTime CreatedAt { get; set; }
      public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class ImageDeleteResultDto
    {
      public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
   public string PublicId { get; set; } = string.Empty;
    }

    public class ProductImageUploadDto
    {
      public int ProductId { get; set; }
        public IFormFile Image { get; set; } = null!;
    }

    public class ProductMultipleImagesUploadDto
{
        public int ProductId { get; set; }
        public IEnumerable<IFormFile> Images { get; set; } = new List<IFormFile>();
    }
}
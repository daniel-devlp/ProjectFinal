using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project.Application.Validators;

namespace Project.Application.Dtos
{
    public class ProductDto
    {
        public int ProductId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; } = true; // ✅ Campo para borrado lógico
        public string ImageUri { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } // ✅ Campos de auditoría
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }

    public class ProductCreateDto
    {
        [Required(ErrorMessage = "El código es requerido")]
        [ProductCodeFormat]
        public string Code { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 200 caracteres")]
        public string Name { get; set; } = string.Empty;
  
        [StringLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
        public string? Description { get; set; }
        
        [Required(ErrorMessage = "El precio es requerido")]
        [PositiveDecimal]
        public decimal Price { get; set; }
      
        [Required(ErrorMessage = "El stock es requerido")]
        [NonNegativeInteger]
        public int Stock { get; set; }
        
        public string ImageUri { get; set; } = string.Empty;
    }

    public class ProductUpdateDto
    {
        [Required(ErrorMessage = "El ID del producto es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del producto debe ser mayor a cero")]
        public int ProductId { get; set; }
      
        [Required(ErrorMessage = "El código es requerido")]
        [ProductCodeFormat]
        public string Code { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 200 caracteres")]
        public string Name { get; set; } = string.Empty;
 
        [StringLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
        public string? Description { get; set; }
        
        [Required(ErrorMessage = "El precio es requerido")]
        [PositiveDecimal]
        public decimal Price { get; set; }
        
        [Required(ErrorMessage = "El stock es requerido")]
        [NonNegativeInteger]
        public int Stock { get; set; }
 
        public bool IsActive { get; set; } = true;
    
        public string ImageUri { get; set; } = string.Empty;
    }

    // DTOs específicos para controllers con archivos
    public class ProductCreateWithImageDto
    {
        [Required(ErrorMessage = "El código es requerido")]
        [ProductCodeFormat]
        public string Code { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 200 caracteres")]
        public string Name { get; set; } = string.Empty;
     
        [StringLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
        public string? Description { get; set; }
        
        [Required(ErrorMessage = "El precio es requerido")]
        [PositiveDecimal]
        public decimal Price { get; set; }
        
        [Required(ErrorMessage = "El stock es requerido")]
        [NonNegativeInteger]
        public int Stock { get; set; }
        
        public IFormFile? Image { get; set; }
    }

    public class ProductUpdateWithImageDto
    {
      [Required(ErrorMessage = "El código es requerido")]
    [ProductCodeFormat]
public string Code { get; set; } = string.Empty;
    
     [Required(ErrorMessage = "El nombre es requerido")]
     [StringLength(200, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 200 caracteres")]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
    public string? Description { get; set; }
   
        [Required(ErrorMessage = "El precio es requerido")]
      [PositiveDecimal]
public decimal Price { get; set; }
        
  [Required(ErrorMessage = "El stock es requerido")]
        [NonNegativeInteger]
    public int Stock { get; set; }
        
 public bool IsActive { get; set; } = true;
      
public IFormFile? Image { get; set; }
    }

    // ✅ DTOs para gestión de stock
    public class ProductStockUpdateDto
    {
        [Required(ErrorMessage = "El ID del producto es requerido")]
        public int ProductId { get; set; }
        
        [Required(ErrorMessage = "La cantidad es requerida")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero")]
  public int Quantity { get; set; }
   
        public string? Reason { get; set; }
    }
}

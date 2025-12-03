using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project.Application.Validators;

namespace Project.Application.Dtos
{
    public class InvoiceDetailDto
    {
        public int InvoiceDetailId { get; set; }
        public int InvoiceId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
        public ProductDto? Product { get; set; }
    }

    public class InvoiceDetailCreateDto
    {
        [Required(ErrorMessage = "El ID del producto es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del producto debe ser mayor a cero")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "La cantidad es requerida")]
        [PositiveQuantity]
        public int Quantity { get; set; }

        // ✅ El precio unitario se tomará del producto actual, no del DTO
        // Esto garantiza que siempre se use el precio actual del sistema
    }

    public class InvoiceDetailUpdateDto
    {
        public int InvoiceDetailId { get; set; }

        [Required(ErrorMessage = "El ID del producto es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del producto debe ser mayor a cero")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "La cantidad es requerida")]
        [PositiveQuantity]
        public int Quantity { get; set; }

        // ✅ El precio unitario se tomará del producto actual, no del DTO
        // Esto garantiza que siempre se use el precio actual del sistema
    }
}

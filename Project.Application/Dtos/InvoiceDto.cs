using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project.Application.Validators;

namespace Project.Application.Dtos
{
    public class InvoiceDto
    {
        public int InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public int ClientId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public string Observations { get; set; } = string.Empty;
        
        // ✅ Campos para borrado lógico y auditoría
        public bool IsActive { get; set; } = true;
        public string Status { get; set; } = string.Empty;
        public string? CancelReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
      
        public ClientDto? Client { get; set; }
        public List<InvoiceDetailDto> InvoiceDetails { get; set; } = new List<InvoiceDetailDto>();
    }

    public class InvoiceCreateDto
    {
        [Required(ErrorMessage = "El ID del cliente es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del cliente debe ser mayor a cero")]
        public int ClientId { get; set; }

        [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
        public string? Observations { get; set; }

        [Required(ErrorMessage = "La factura debe contener al menos un detalle")]
        [ValidInvoiceDetails]
        public List<InvoiceDetailCreateDto> InvoiceDetails { get; set; } = new List<InvoiceDetailCreateDto>();
    }

    public class InvoiceUpdateDto
    {
        [Required(ErrorMessage = "El ID de la factura es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID de la factura debe ser mayor a cero")]
        public int InvoiceId { get; set; }

        [Required(ErrorMessage = "El ID del cliente es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del cliente debe ser mayor a cero")]
        public int ClientId { get; set; }

        [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
        public string? Observations { get; set; }

        [Required(ErrorMessage = "La factura debe contener al menos un detalle")]
        [ValidInvoiceDetails]
        public List<InvoiceDetailUpdateDto> InvoiceDetails { get; set; } = new List<InvoiceDetailUpdateDto>();
    }

    // ✅ DTOs para operaciones específicas
    public class InvoiceFinalizeDto
    {
        [Required(ErrorMessage = "El ID de la factura es requerido")]
        public int InvoiceId { get; set; }
    }

    public class InvoiceCancelDto
    {
        [Required(ErrorMessage = "El ID de la factura es requerido")]
        public int InvoiceId { get; set; }

        [Required(ErrorMessage = "La razón de cancelación es requerida")]
        [StringLength(200, ErrorMessage = "La razón no puede exceder 200 caracteres")]
        public string Reason { get; set; } = string.Empty;
    }

    public class InvoiceSearchDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? ClientId { get; set; }
        public string? Status { get; set; }
        public bool IncludeDeleted { get; set; } = false;
    }
}

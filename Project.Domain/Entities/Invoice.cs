using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Entities
{
    public class Invoice
    {
  public int InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;

        public int ClientId { get; set; }
        public string UserId { get; set; } = string.Empty;

     public DateTime IssueDate { get; set; } = DateTime.UtcNow;

     public decimal Subtotal { get; set; }
        public decimal Tax { get; set; } = 0;
        public decimal Total { get; set; }
        public string Observations { get; set; } = string.Empty;

        // ✅ Campos para borrado lógico y auditoría
        public bool IsActive { get; set; } = true;
 public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
   public DateTime? DeletedAt { get; set; }
      
        // ✅ Campos para control de estado
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
        public string? CancelReason { get; set; }

      // Navigation properties
        public virtual Client Client { get; set; } = null!;
        public virtual ICollection<InvoiceDetail> InvoiceDetails { get; set; } = new HashSet<InvoiceDetail>();

 // ✅ Constructor sin parámetros para EF
        public Invoice()
    {
   CreatedAt = DateTime.UtcNow;
     IssueDate = DateTime.UtcNow;
            IsActive = true;
 Status = InvoiceStatus.Draft;
        }

      // ✅ Constructor con validaciones básicas
        public Invoice(int clientId, string userId, string observations = "")
   {
       SetClient(clientId);
    SetUser(userId);
            Observations = observations?.Trim() ?? string.Empty;
        CreatedAt = DateTime.UtcNow;
         IssueDate = DateTime.UtcNow;
            IsActive = true;
      Status = InvoiceStatus.Draft;
     InvoiceNumber = string.Empty; // Se generará automáticamente
        }

        // ✅ Métodos para validaciones de dominio
        public void SetClient(int clientId)
        {
            if (clientId <= 0)
       throw new ArgumentException("Client ID must be greater than zero");
      ClientId = clientId;
        }

        public void SetUser(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
       throw new ArgumentException("User ID is required");
            UserId = userId.Trim();
 }

     public void SetInvoiceNumber(string invoiceNumber)
 {
         if (string.IsNullOrWhiteSpace(invoiceNumber))
      throw new ArgumentException("Invoice number is required");
    InvoiceNumber = invoiceNumber.Trim();
        }

        public void AddDetail(int productId, int quantity, decimal unitPrice)
        {
            if (Status != InvoiceStatus.Draft)
     throw new InvalidOperationException("Cannot modify a finalized invoice");

 var detail = new InvoiceDetail
    {
       ProductID = productId,
                Quantity = quantity,
        UnitPrice = unitPrice,
  Subtotal = quantity * unitPrice
};

    InvoiceDetails.Add(detail);
   RecalculateTotals();
}

     public void RemoveDetail(int productId)
        {
            if (Status != InvoiceStatus.Draft)
            throw new InvalidOperationException("Cannot modify a finalized invoice");

            var detail = InvoiceDetails.FirstOrDefault(d => d.ProductID == productId);
        if (detail != null)
            {
      InvoiceDetails.Remove(detail);
                RecalculateTotals();
            }
        }

        public void RecalculateTotals()
    {
    Subtotal = InvoiceDetails.Sum(d => d.Subtotal);
            Tax = Math.Round(Subtotal * 0.12m, 2); // 12% IVA Ecuador
      Total = Subtotal + Tax;
       UpdatedAt = DateTime.UtcNow;
        }

        public void Finalize()
   {
      if (!InvoiceDetails.Any())
throw new InvalidOperationException("Cannot finalize an invoice without details");

            Status = InvoiceStatus.Finalized;
       UpdatedAt = DateTime.UtcNow;
        }

        public void Cancel(string reason)
     {
       if (Status == InvoiceStatus.Cancelled)
          throw new InvalidOperationException("Invoice is already cancelled");

   Status = InvoiceStatus.Cancelled;
   CancelReason = reason?.Trim() ?? "Cancelled by user";
       UpdatedAt = DateTime.UtcNow;
        }

  // ✅ Método para borrado lógico
        public void SoftDelete(string reason = "")
        {
     if (Status == InvoiceStatus.Finalized)
      throw new InvalidOperationException("Cannot delete a finalized invoice. Cancel it first.");

            IsActive = false;
            DeletedAt = DateTime.UtcNow;
          UpdatedAt = DateTime.UtcNow;
            CancelReason = reason?.Trim() ?? "Deleted by user";
        }

        // ✅ Método para restaurar factura eliminada
        public void Restore()
 {
     IsActive = true;
   DeletedAt = null;
   UpdatedAt = DateTime.UtcNow;
            CancelReason = null;
        }

        // ✅ Método para verificar si se puede modificar
        public bool CanBeModified()
  {
    return IsActive && Status == InvoiceStatus.Draft;
        }
    }

    // ✅ Enum para estados de factura
    public enum InvoiceStatus
    {
        Draft = 0,// Borrador
        Finalized = 1,  // Finalizada
        Cancelled = 2,  // Cancelada
        Paid = 3        // Pagada
    }
}

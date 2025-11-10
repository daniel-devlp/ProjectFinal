// MÓDULO DE PAGOS - PREPARADO PARA IMPLEMENTACIÓN FUTURA
// Descomentar cuando sea necesario implementar sistema de pagos

/*
using Project.Domain.Entities;

namespace Project.Domain.Entities
{
public class Payment
    {
        public int PaymentId { get; set; }
   public int InvoiceId { get; set; }
        public string PaymentMethodId { get; set; } = string.Empty;
public decimal Amount { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public PaymentStatus Status { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime? ProcessedAt { get; set; }
 public string? ProcessorResponse { get; set; }
        public string? FailureReason { get; set; }

        // Navigation properties
        public virtual Invoice Invoice { get; set; } = null!;
        public virtual PaymentMethod PaymentMethod { get; set; } = null!;

        public Payment()
        {
  PaymentDate = DateTime.UtcNow;
       Status = PaymentStatus.Pending;
        }

   public Payment(int invoiceId, string paymentMethodId, decimal amount, string transactionId)
        {
            if (invoiceId <= 0)
              throw new ArgumentException("Invoice ID must be greater than zero", nameof(invoiceId));
            
            if (string.IsNullOrWhiteSpace(paymentMethodId))
      throw new ArgumentException("Payment method ID is required", nameof(paymentMethodId));
       
   if (amount <= 0)
    throw new ArgumentException("Amount must be greater than zero", nameof(amount));
            
       if (string.IsNullOrWhiteSpace(transactionId))
                throw new ArgumentException("Transaction ID is required", nameof(transactionId));

  InvoiceId = invoiceId;
   PaymentMethodId = paymentMethodId;
        Amount = amount;
   TransactionId = transactionId;
          PaymentDate = DateTime.UtcNow;
 Status = PaymentStatus.Pending;
        }

        public void MarkAsCompleted(string? processorResponse = null)
     {
       Status = PaymentStatus.Completed;
     ProcessedAt = DateTime.UtcNow;
            ProcessorResponse = processorResponse;
        FailureReason = null;
     }

        public void MarkAsFailed(string failureReason)
        {
            Status = PaymentStatus.Failed;
            ProcessedAt = DateTime.UtcNow;
            FailureReason = failureReason;
        }

   public void MarkAsRefunded()
        {
     Status = PaymentStatus.Refunded;
            ProcessedAt = DateTime.UtcNow;
 }
    }

    public class PaymentMethod
    {
        public string PaymentMethodId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public PaymentType Type { get; set; }
        public string? ProcessorConfig { get; set; } // JSON config for payment processor
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public decimal ProcessingFee { get; set; }

 // Navigation properties
  public virtual ICollection<Payment> Payments { get; set; } = new HashSet<Payment>();

      public PaymentMethod()
 {
            IsActive = true;
        }

        public PaymentMethod(string paymentMethodId, string name, PaymentType type)
        {
   if (string.IsNullOrWhiteSpace(paymentMethodId))
  throw new ArgumentException("Payment method ID is required", nameof(paymentMethodId));
 
            if (string.IsNullOrWhiteSpace(name))
    throw new ArgumentException("Name is required", nameof(name));

  PaymentMethodId = paymentMethodId;
     Name = name;
            Type = type;
          IsActive = true;
        }
    }

    public enum PaymentStatus
    {
        Pending = 0,
        Processing = 1,
Completed = 2,
        Failed = 3,
        Cancelled = 4,
     Refunded = 5
    }

    public enum PaymentType
    {
        CreditCard = 0,
        DebitCard = 1,
        BankTransfer = 2,
     PayPal = 3,
        Stripe = 4,
        Cash = 5,
        Cryptocurrency = 6
    }
}
*/
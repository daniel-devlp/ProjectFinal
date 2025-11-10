namespace Project.Application.Dtos
{
    public class ShoppingCartDto
  {
  public int CartId { get; set; }
   public string UserId { get; set; } = string.Empty;
       public int ProductId { get; set; }
      public int Quantity { get; set; }
     public decimal UnitPrice { get; set; }
       public decimal Subtotal { get; set; }
      public DateTime DateAdded { get; set; }
     public DateTime? UpdatedAt { get; set; }
      public ProductDto? Product { get; set; }
    }

    public class AddToCartDto
   {
      public int ProductId { get; set; }
     public int Quantity { get; set; }
    }

 public class UpdateCartItemDto
 {
  public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class CartSummaryDto
    {
    public List<ShoppingCartDto> Items { get; set; } = new();
       public decimal Total { get; set; }
   public int TotalItems { get; set; }
   public int UniqueProducts { get; set; }
    }

    // DTOs para módulo de pagos (comentados para implementación futura)
    /*
    public class PaymentDto
    {
        public int PaymentId { get; set; }
  public int InvoiceId { get; set; }
   public string PaymentMethodId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
      public string TransactionId { get; set; } = string.Empty;
       public string Status { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
        public DateTime? ProcessedAt { get; set; }
     public string? ProcessorResponse { get; set; }
        public string? FailureReason { get; set; }
 public PaymentMethodDto? PaymentMethod { get; set; }
 }

    public class PaymentMethodDto
    {
     public string PaymentMethodId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
   public bool IsActive { get; set; }
      public string Type { get; set; } = string.Empty;
     public decimal MinAmount { get; set; }
     public decimal MaxAmount { get; set; }
       public decimal ProcessingFee { get; set; }
    }

    public class ProcessPaymentDto
    {
      public int InvoiceId { get; set; }
     public string PaymentMethodId { get; set; } = string.Empty;
   public decimal Amount { get; set; }
     public string? AdditionalInfo { get; set; }
    }

    public class PaymentResultDto
    {
     public bool Success { get; set; }
        public string TransactionId { get; set; } = string.Empty;
  public string Status { get; set; } = string.Empty;
        public string? Message { get; set; }
      public PaymentDto? Payment { get; set; }
    }
    */
}
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

    // DTO para checkout con pago integrado
    public class CheckoutWithPaymentDto
    {
    public int ClientId { get; set; }
        public string PaymentMethodId { get; set; } = string.Empty;
  // Campos adicionales para pago móvil
     public string? CustomerPhone { get; set; }
        public string? CustomerEmail { get; set; }
  public string? DeviceId { get; set; }
        public string? CustomerNotes { get; set; }
    }

    public class CheckoutWithPaymentResultDto
    {
    public InvoiceDto Invoice { get; set; } = null!;
   public bool PaymentRequired { get; set; }
     public string PaymentMethodId { get; set; } = string.Empty;
public decimal Amount { get; set; }
  public string Message { get; set; } = string.Empty;
        public PaymentDto? Payment { get; set; }
    }

    // DTOs para módulo de pagos - ACTIVADO
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
    public InvoiceDto? Invoice { get; set; }
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
   public string? IconUrl { get; set; }
        public int DisplayOrder { get; set; }
    }

  public class ProcessPaymentDto
    {
  public int InvoiceId { get; set; }
     public string PaymentMethodId { get; set; } = string.Empty;
   public decimal Amount { get; set; }
 public string? AdditionalInfo { get; set; }
public string? CustomerNotes { get; set; }
    }

    public class PaymentResultDto
    {
     public bool Success { get; set; }
        public string TransactionId { get; set; } = string.Empty;
  public string Status { get; set; } = string.Empty;
     public string? Message { get; set; }
      public PaymentDto? Payment { get; set; }
        public string? ErrorCode { get; set; }
    }

    // DTOs específicos para móvil
    public class MobilePaymentDto
  {
    public int InvoiceId { get; set; }
   public string PaymentMethodId { get; set; } = string.Empty;
       public decimal Amount { get; set; }
        public string DeviceId { get; set; } = string.Empty;
     public string? CustomerPhone { get; set; }
        public string? CustomerEmail { get; set; }
    }

  public class PaymentHistoryDto
    {
    public List<PaymentDto> Payments { get; set; } = new();
public decimal TotalPaid { get; set; }
 public int TotalTransactions { get; set; }
  public DateTime? LastPaymentDate { get; set; }
    }

    public class RefundPaymentDto
   {
  public int PaymentId { get; set; }
      public decimal RefundAmount { get; set; }
      public string? Reason { get; set; }
     public string? AdminNotes { get; set; }
    }
}
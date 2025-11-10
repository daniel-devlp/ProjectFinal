using Project.Domain.Entities;

namespace Project.Domain.Entities
{
    public class ShoppingCart
  {
        public int CartId { get; set; }
        public string UserId { get; set; } = string.Empty;
    public int ProductId { get; set; }
        public int Quantity { get; set; }
 public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual Product Product { get; set; } = null!;

        // Constructor sin parámetros para EF
        public ShoppingCart()
    {
      DateAdded = DateTime.UtcNow;
        }

        // Constructor con validaciones de dominio
      public ShoppingCart(string userId, int productId, int quantity, decimal unitPrice)
      {
   if (string.IsNullOrWhiteSpace(userId))
      throw new ArgumentException("User ID is required", nameof(userId));
 
            if (productId <= 0)
 throw new ArgumentException("Product ID must be greater than zero", nameof(productId));
            
     if (quantity <= 0)
              throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
  
            if (unitPrice < 0)
             throw new ArgumentException("Unit price cannot be negative", nameof(unitPrice));

      UserId = userId;
            ProductId = productId;
            Quantity = quantity;
            UnitPrice = unitPrice;
            Subtotal = quantity * unitPrice;
            DateAdded = DateTime.UtcNow;
      }

      public void UpdateQuantity(int newQuantity)
   {
   if (newQuantity <= 0)
       throw new ArgumentException("Quantity must be greater than zero", nameof(newQuantity));
            
 Quantity = newQuantity;
Subtotal = Quantity * UnitPrice;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdatePrice(decimal newUnitPrice)
        {
 if (newUnitPrice < 0)
throw new ArgumentException("Unit price cannot be negative", nameof(newUnitPrice));
   
      UnitPrice = newUnitPrice;
      Subtotal = Quantity * UnitPrice;
    UpdatedAt = DateTime.UtcNow;
   }

   public decimal CalculateSubtotal() => Quantity * UnitPrice;
    }
}
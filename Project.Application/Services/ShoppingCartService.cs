using Project.Application.Dtos;
using Project.Domain.Entities;
using Project.Domain.Interfaces;

namespace Project.Application.Services
{
   public class ShoppingCartService : IShoppingCartService
    {
        private readonly IUnitOfWork _unitOfWork;

    public ShoppingCartService(IUnitOfWork unitOfWork)
      {
  _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
   }

public async Task<ShoppingCartDto> AddToCartAsync(string userId, AddToCartDto addToCartDto)
    {
  if (string.IsNullOrWhiteSpace(userId))
         throw new ArgumentException("User ID is required", nameof(userId));

  if (addToCartDto == null)
     throw new ArgumentNullException(nameof(addToCartDto));

       // Verificar si el producto existe
  var product = await _unitOfWork.Products.GetByIdAsync(addToCartDto.ProductId);
  if (product == null)
 throw new InvalidOperationException("Product not found");

            // Verificar stock disponible
      if (!await _unitOfWork.Products.HasStockAsync(addToCartDto.ProductId, addToCartDto.Quantity))
   throw new InvalidOperationException("Insufficient stock");

    // Verificar si el item ya existe en el carrito
    var existingItem = await _unitOfWork.ShoppingCart.GetCartItemAsync(userId, addToCartDto.ProductId);
      
  if (existingItem != null)
   {
   // Actualizar cantidad
   var newQuantity = existingItem.Quantity + addToCartDto.Quantity;
        
  // Verificar stock total
  if (!await _unitOfWork.Products.HasStockAsync(addToCartDto.ProductId, newQuantity))
       throw new InvalidOperationException("Insufficient stock for requested quantity");

    existingItem.UpdateQuantity(newQuantity);
 _unitOfWork.ShoppingCart.Update(existingItem);
    await _unitOfWork.SaveChangesAsync();

   return MapToDto(existingItem, product);
      }
  else
   {
      // Crear nuevo item en el carrito
     var cartItem = new ShoppingCart(userId, addToCartDto.ProductId, addToCartDto.Quantity, product.Price);
       
       await _unitOfWork.ShoppingCart.AddAsync(cartItem);
    await _unitOfWork.SaveChangesAsync();

  return MapToDto(cartItem, product);
     }
     }

     public async Task<CartSummaryDto> GetCartAsync(string userId)
      {
    if (string.IsNullOrWhiteSpace(userId))
      throw new ArgumentException("User ID is required", nameof(userId));

         var (items, total) = await _unitOfWork.ShoppingCart.GetCartSummaryAsync(userId);
        
 return new CartSummaryDto
 {
    Items = items.Select(item => MapToDto(item, item.Product)).ToList(),
      Total = total,
       TotalItems = items.Sum(i => i.Quantity),
   UniqueProducts = items.Count()
       };
        }

     public async Task<ShoppingCartDto> UpdateCartItemAsync(string userId, UpdateCartItemDto updateDto)
        {
       if (string.IsNullOrWhiteSpace(userId))
     throw new ArgumentException("User ID is required", nameof(userId));

     if (updateDto == null)
      throw new ArgumentNullException(nameof(updateDto));

  var cartItem = await _unitOfWork.ShoppingCart.GetCartItemAsync(userId, updateDto.ProductId);
  if (cartItem == null)
      throw new InvalidOperationException("Cart item not found");

      // Verificar stock disponible
 if (!await _unitOfWork.Products.HasStockAsync(updateDto.ProductId, updateDto.Quantity))
  throw new InvalidOperationException("Insufficient stock");

     cartItem.UpdateQuantity(updateDto.Quantity);
         _unitOfWork.ShoppingCart.Update(cartItem);
       await _unitOfWork.SaveChangesAsync();

  return MapToDto(cartItem, cartItem.Product);
   }

  public async Task RemoveFromCartAsync(string userId, int productId)
        {
      if (string.IsNullOrWhiteSpace(userId))
       throw new ArgumentException("User ID is required", nameof(userId));

         await _unitOfWork.ShoppingCart.RemoveCartItemAsync(userId, productId);
    await _unitOfWork.SaveChangesAsync();
   }

    public async Task ClearCartAsync(string userId)
      {
         if (string.IsNullOrWhiteSpace(userId))
 throw new ArgumentException("User ID is required", nameof(userId));

       await _unitOfWork.ShoppingCart.ClearCartAsync(userId);
     await _unitOfWork.SaveChangesAsync();
   }

        public async Task<int> GetCartItemCountAsync(string userId)
    {
   if (string.IsNullOrWhiteSpace(userId))
   throw new ArgumentException("User ID is required", nameof(userId));

     return await _unitOfWork.ShoppingCart.GetCartItemCountAsync(userId);
     }

public async Task<decimal> GetCartTotalAsync(string userId)
      {
    if (string.IsNullOrWhiteSpace(userId))
         throw new ArgumentException("User ID is required", nameof(userId));

            return await _unitOfWork.ShoppingCart.GetCartTotalAsync(userId);
        }

   public async Task<bool> CartItemExistsAsync(string userId, int productId)
  {
      if (string.IsNullOrWhiteSpace(userId))
       throw new ArgumentException("User ID is required", nameof(userId));

   return await _unitOfWork.ShoppingCart.CartItemExistsAsync(userId, productId);
        }

     public async Task<InvoiceDto> CheckoutAsync(string userId, int clientId)
  {
  if (string.IsNullOrWhiteSpace(userId))
      throw new ArgumentException("User ID is required", nameof(userId));

  if (clientId <= 0)
      throw new ArgumentException("Client ID must be greater than zero", nameof(clientId));

     await _unitOfWork.BeginTransactionAsync();
   try
       {
 // Obtener items del carrito
    var cartItems = await _unitOfWork.ShoppingCart.GetCartByUserAsync(userId);
    if (!cartItems.Any())
 throw new InvalidOperationException("Cart is empty");

 // Verificar stock de todos los productos
         foreach (var item in cartItems)
    {
     if (!await _unitOfWork.Products.HasStockAsync(item.ProductId, item.Quantity))
    throw new InvalidOperationException($"Insufficient stock for product {item.Product?.Name}");
   }

 // Crear factura
 var invoice = new Invoice
   {
   InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMddHHmmss}",
   ClientId = clientId,
   UserId = userId,
     IssueDate = DateTime.UtcNow,
   Observations = "Generated from shopping cart - Ready for payment",
    InvoiceDetails = cartItems.Select(item => new InvoiceDetail
   {
      ProductID = item.ProductId,
    Quantity = item.Quantity,
    UnitPrice = item.UnitPrice,
    Subtotal = item.Subtotal
       }).ToList()
    };

    invoice.Subtotal = invoice.InvoiceDetails.Sum(d => d.Subtotal);
   invoice.Tax = Math.Round(invoice.Subtotal * 0.12m, 2);
    invoice.Total = invoice.Subtotal + invoice.Tax;

    await _unitOfWork.Invoices.AddAsync(invoice);
 await _unitOfWork.SaveChangesAsync();

   // Decrementar stock
    foreach (var item in cartItems)
       {
  await _unitOfWork.Products.DecreaseStockAsync(item.ProductId, item.Quantity);
     }

        // Limpiar carrito
      await _unitOfWork.ShoppingCart.ClearCartAsync(userId);
     
  await _unitOfWork.SaveChangesAsync();
         await _unitOfWork.CommitTransactionAsync();

// Mapear a DTO
      return new InvoiceDto
   {
    InvoiceId = invoice.InvoiceId,
    InvoiceNumber = invoice.InvoiceNumber,
    ClientId = invoice.ClientId,
     UserId = invoice.UserId,
IssueDate = invoice.IssueDate,
 Subtotal = invoice.Subtotal,
       Tax = invoice.Tax,
      Total = invoice.Total,
  Observations = invoice.Observations,
  InvoiceDetails = invoice.InvoiceDetails.Select(d => new InvoiceDetailDto
    {
 InvoiceDetailId = d.InvoiceDetailId,
      InvoiceId = d.InvoiceID,
    ProductId = d.ProductID,
      Quantity = d.Quantity,
      UnitPrice = d.UnitPrice,
       Subtotal = d.Subtotal
 }).ToList()
  };
        }
   catch
       {
      await _unitOfWork.RollbackTransactionAsync();
  throw;
       }
   }

   // Nuevo método para checkout con pago inmediato
        public async Task<CheckoutWithPaymentResultDto> CheckoutWithPaymentAsync(string userId, int clientId, string paymentMethodId)
        {
            if (string.IsNullOrWhiteSpace(userId))
        throw new ArgumentException("User ID is required", nameof(userId));

     if (clientId <= 0)
     throw new ArgumentException("Client ID must be greater than zero", nameof(clientId));

  if (string.IsNullOrWhiteSpace(paymentMethodId))
                throw new ArgumentException("Payment method ID is required", nameof(paymentMethodId));

   // Primero crear la factura
  var invoice = await CheckoutAsync(userId, clientId);

  // Luego intentar procesar el pago (esto requeriría inyectar IPaymentService)
            // Por ahora retornamos la información para que el móvil procese el pago
  return new CheckoutWithPaymentResultDto
  {
        Invoice = invoice,
    PaymentRequired = true,
    PaymentMethodId = paymentMethodId,
 Amount = invoice.Total,
   Message = "Invoice created successfully. Proceed to payment."
            };
    }

   private static ShoppingCartDto MapToDto(ShoppingCart cartItem, Product? product)
        {
       return new ShoppingCartDto
        {
      CartId = cartItem.CartId,
    UserId = cartItem.UserId,
    ProductId = cartItem.ProductId,
     Quantity = cartItem.Quantity,
      UnitPrice = cartItem.UnitPrice,
    Subtotal = cartItem.Subtotal,
  DateAdded = cartItem.DateAdded,
    UpdatedAt = cartItem.UpdatedAt,
    Product = product != null ? new ProductDto
       {
         ProductId = product.ProductId,
    Code = product.Code,
    Name = product.Name,
    Description = product.Description,
  Price = product.Price,
       Stock = product.Stock,
  IsActive = product.IsActive
  } : null
        };
  }
  }
}
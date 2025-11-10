using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Application.Dtos;
using Project.Application.Services;
using System.Security.Claims;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requiere autenticación para todas las acciones
    public class ShoppingCartController : ControllerBase
    {
     private readonly IShoppingCartService _shoppingCartService;

  public ShoppingCartController(IShoppingCartService shoppingCartService)
{
    _shoppingCartService = shoppingCartService;
      }

     /// <summary>
        /// Obtiene el carrito del usuario actual
      /// </summary>
       [HttpGet]
   public async Task<IActionResult> GetCart()
 {
    try
      {
     var userId = GetCurrentUserId();
     var cart = await _shoppingCartService.GetCartAsync(userId);
     return Ok(cart);
   }
  catch (Exception ex)
   {
    return BadRequest(new { message = ex.Message });
 }
     }

        /// <summary>
     /// Agrega un producto al carrito
   /// </summary>
    [HttpPost("add")]
 public async Task<IActionResult> AddToCart([FromBody] AddToCartDto addToCartDto)
        {
    try
     {
   var userId = GetCurrentUserId();
     var cartItem = await _shoppingCartService.AddToCartAsync(userId, addToCartDto);
   return Ok(cartItem);
      }
    catch (Exception ex)
   {
      return BadRequest(new { message = ex.Message });
  }
        }

/// <summary>
  /// Actualiza la cantidad de un producto en el carrito
     /// </summary>
    [HttpPut("update")]
      public async Task<IActionResult> UpdateCartItem([FromBody] UpdateCartItemDto updateDto)
     {
    try
     {
     var userId = GetCurrentUserId();
    var cartItem = await _shoppingCartService.UpdateCartItemAsync(userId, updateDto);
 return Ok(cartItem);
     }
    catch (Exception ex)
  {
     return BadRequest(new { message = ex.Message });
 }
  }

  /// <summary>
/// Elimina un producto del carrito
/// </summary>
  [HttpDelete("remove/{productId}")]
    public async Task<IActionResult> RemoveFromCart(int productId)
   {
  try
     {
    var userId = GetCurrentUserId();
   await _shoppingCartService.RemoveFromCartAsync(userId, productId);
      return Ok(new { message = "Product removed from cart successfully" });
      }
   catch (Exception ex)
     {
       return BadRequest(new { message = ex.Message });
     }
  }

/// <summary>
/// Limpia todo el carrito del usuario
  /// </summary>
 [HttpDelete("clear")]
   public async Task<IActionResult> ClearCart()
  {
  try
    {
   var userId = GetCurrentUserId();
    await _shoppingCartService.ClearCartAsync(userId);
     return Ok(new { message = "Cart cleared successfully" });
       }
  catch (Exception ex)
 {
      return BadRequest(new { message = ex.Message });
    }
      }

        /// <summary>
/// Obtiene el número total de items en el carrito
   /// </summary>
      [HttpGet("count")]
       public async Task<IActionResult> GetCartItemCount()
     {
  try
 {
    var userId = GetCurrentUserId();
      var count = await _shoppingCartService.GetCartItemCountAsync(userId);
    return Ok(new { count });
      }
 catch (Exception ex)
    {
    return BadRequest(new { message = ex.Message });
  }
     }

/// <summary>
   /// Obtiene el total del carrito
 /// </summary>
     [HttpGet("total")]
 public async Task<IActionResult> GetCartTotal()
  {
       try
    {
    var userId = GetCurrentUserId();
    var total = await _shoppingCartService.GetCartTotalAsync(userId);
       return Ok(new { total });
   }
 catch (Exception ex)
 {
 return BadRequest(new { message = ex.Message });
    }
    }

    /// <summary>
    /// Verifica si un producto existe en el carrito
    /// </summary>
 [HttpGet("exists/{productId}")]
  public async Task<IActionResult> CartItemExists(int productId)
   {
 try
        {
 var userId = GetCurrentUserId();
    var exists = await _shoppingCartService.CartItemExistsAsync(userId, productId);
    return Ok(new { exists });
   }
    catch (Exception ex)
  {
  return BadRequest(new { message = ex.Message });
 }
    }

   /// <summary>
   /// Procesa el checkout (convierte carrito en factura)
   /// </summary>
  [HttpPost("checkout")]
  public async Task<IActionResult> Checkout([FromBody] CheckoutDto checkoutDto)
     {
   try
    {
      var userId = GetCurrentUserId();
 var invoice = await _shoppingCartService.CheckoutAsync(userId, checkoutDto.ClientId);
return Ok(new { 
  message = "Checkout completed successfully", 
     invoice = invoice 
 });
     }
   catch (Exception ex)
 {
       return BadRequest(new { message = ex.Message });
   }
   }

 private string GetCurrentUserId()
   {
       var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
     if (string.IsNullOrEmpty(userId))
    throw new UnauthorizedAccessException("User not authenticated");
    return userId;
      }
    }

  // DTO para checkout
  public class CheckoutDto
      {
     public int ClientId { get; set; }
        // En el futuro se pueden agregar más campos como método de pago, dirección, etc.
     /*
       public string PaymentMethodId { get; set; } = string.Empty;
  public string ShippingAddress { get; set; } = string.Empty;
      public string? Notes { get; set; }
        */
    }
}
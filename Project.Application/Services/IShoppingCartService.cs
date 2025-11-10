using Project.Application.Dtos;

namespace Project.Application.Services
{
    public interface IShoppingCartService
    {
     Task<ShoppingCartDto> AddToCartAsync(string userId, AddToCartDto addToCartDto);
        Task<CartSummaryDto> GetCartAsync(string userId);
    Task<ShoppingCartDto> UpdateCartItemAsync(string userId, UpdateCartItemDto updateDto);
     Task RemoveFromCartAsync(string userId, int productId);
   Task ClearCartAsync(string userId);
 Task<int> GetCartItemCountAsync(string userId);
  Task<decimal> GetCartTotalAsync(string userId);
  Task<bool> CartItemExistsAsync(string userId, int productId);
 Task<InvoiceDto> CheckoutAsync(string userId, int clientId);
    }
}
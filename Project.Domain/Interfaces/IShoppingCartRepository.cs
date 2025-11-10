using Project.Domain.Entities;

namespace Project.Domain.Interfaces
{
    public interface IShoppingCartRepository : IRepository<ShoppingCart>
    {
    Task<IEnumerable<ShoppingCart>> GetCartByUserAsync(string userId);
        Task<ShoppingCart> GetCartItemAsync(string userId, int productId);
      Task<bool> CartItemExistsAsync(string userId, int productId);
      Task<int> GetCartItemCountAsync(string userId);
        Task<decimal> GetCartTotalAsync(string userId);
        Task ClearCartAsync(string userId);
        Task RemoveExpiredItemsAsync(DateTime expirationDate);
   
        // Métodos específicos para gestión del carrito
      Task<(IEnumerable<ShoppingCart> Items, decimal Total)> GetCartSummaryAsync(string userId);
        Task UpdateCartItemQuantityAsync(string userId, int productId, int newQuantity);
        Task RemoveCartItemAsync(string userId, int productId);
    }
}
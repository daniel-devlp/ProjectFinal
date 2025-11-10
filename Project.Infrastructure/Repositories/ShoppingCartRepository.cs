using Microsoft.EntityFrameworkCore;
using Project.Domain.Entities;
using Project.Domain.Interfaces;
using Project.Infrastructure.Frameworks.EntityFramework;

namespace Project.Infrastructure.Repositories
{
    public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
    {
      public ShoppingCartRepository(ApplicationDBContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ShoppingCart>> GetCartByUserAsync(string userId)
   {
         return await _dbSet
          .Include(c => c.Product)
      .Where(c => c.UserId == userId)
   .OrderBy(c => c.DateAdded)
     .AsNoTracking()
 .ToListAsync();
        }

 public async Task<ShoppingCart> GetCartItemAsync(string userId, int productId)
   {
       return await _dbSet
    .Include(c => c.Product)
      .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);
        }

   public async Task<bool> CartItemExistsAsync(string userId, int productId)
        {
       return await _dbSet
 .AnyAsync(c => c.UserId == userId && c.ProductId == productId);
      }

    public async Task<int> GetCartItemCountAsync(string userId)
       {
 return await _dbSet
       .Where(c => c.UserId == userId)
 .SumAsync(c => c.Quantity);
  }

 public async Task<decimal> GetCartTotalAsync(string userId)
     {
   return await _dbSet
  .Where(c => c.UserId == userId)
 .SumAsync(c => c.Subtotal);
        }

    public async Task ClearCartAsync(string userId)
   {
          var cartItems = await _dbSet
         .Where(c => c.UserId == userId)
       .ToListAsync();

       _dbSet.RemoveRange(cartItems);
    }

        public async Task RemoveExpiredItemsAsync(DateTime expirationDate)
        {
   var expiredItems = await _dbSet
       .Where(c => c.DateAdded < expirationDate)
 .ToListAsync();

    _dbSet.RemoveRange(expiredItems);
    }

      public async Task<(IEnumerable<ShoppingCart> Items, decimal Total)> GetCartSummaryAsync(string userId)
        {
      var items = await GetCartByUserAsync(userId);
     var total = items.Sum(item => item.Subtotal);
    return (items, total);
}

        public async Task UpdateCartItemQuantityAsync(string userId, int productId, int newQuantity)
        {
            var cartItem = await GetCartItemAsync(userId, productId);
     if (cartItem != null)
     {
        cartItem.UpdateQuantity(newQuantity);
          _dbSet.Update(cartItem);
   }
        }

        public async Task RemoveCartItemAsync(string userId, int productId)
       {
          var cartItem = await GetCartItemAsync(userId, productId);
   if (cartItem != null)
   {
           _dbSet.Remove(cartItem);
   }
        }
    }
}
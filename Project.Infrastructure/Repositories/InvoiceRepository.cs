using Project.Domain.Entities;
using Project.Domain.Interfaces;
using Project.Infrastructure.Frameworks.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Project.Infrastructure.Frameworks.Identity;

namespace Project.Infrastructure.Repositories
{
    public class InvoiceRepository : Repository<Invoice>, IInvoiceRepository
    {
 private readonly UserManager<ApplicationUser> _userManager;

        public InvoiceRepository(ApplicationDBContext context, UserManager<ApplicationUser> userManager) : base(context)
        {
      _userManager = userManager;
        }

  public override async Task<Invoice?> GetByIdAsync(int id)
        {
       if (id <= 0) throw new ArgumentException("Invoice ID must be greater than zero.", nameof(id));
       return await _dbSet
                .Include(i => i.InvoiceDetails)
       .ThenInclude(d => d.Product)
   .Include(i => i.Client)
          .AsNoTracking()
        .FirstOrDefaultAsync(i => i.InvoiceId == id);
        }

      public async Task<(IEnumerable<Invoice> Items, int TotalCount)> GetPagedAsync(
   int pageNumber, int pageSize, string? searchTerm = null)
 {
     if (pageNumber <= 0) throw new ArgumentException("Page number must be greater than zero.", nameof(pageNumber));
    if (pageSize <= 0) throw new ArgumentException("Page size must be greater than zero.", nameof(pageSize));

    var query = _dbSet
   .Include(i => i.InvoiceDetails)
     .ThenInclude(d => d.Product)
        .Include(i => i.Client)
     .AsQueryable();

      if (!string.IsNullOrWhiteSpace(searchTerm))
          {
 string term = searchTerm.Trim().ToLower();
  query = query.Where(i =>
    i.UserId.ToLower().Contains(term) ||
  i.InvoiceNumber.ToLower().Contains(term) ||
      i.Client.FirstName.ToLower().Contains(term) ||
  i.Client.LastName.ToLower().Contains(term)
      );
      }

        var totalCount = await query.CountAsync();

            var items = await query
      .OrderByDescending(i => i.IssueDate)
       .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
   .AsNoTracking()
 .ToListAsync();

            return (items, totalCount);
        }

    public async Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber)
    {
      return await _dbSet
   .Include(i => i.InvoiceDetails)
           .ThenInclude(d => d.Product)
                .Include(i => i.Client)
     .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber);
        }

        public async Task<IEnumerable<Invoice>> GetByClientIdAsync(int clientId)
   {
      return await _dbSet
      .Include(i => i.InvoiceDetails)
               .ThenInclude(d => d.Product)
     .Where(i => i.ClientId == clientId)
        .OrderByDescending(i => i.IssueDate)
         .AsNoTracking()
   .ToListAsync();
  }

        public async Task<IEnumerable<Invoice>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
   {
      return await _dbSet
  .Include(i => i.InvoiceDetails)
   .ThenInclude(d => d.Product)
    .Include(i => i.Client)
          .Where(i => i.IssueDate >= startDate && i.IssueDate <= endDate)
    .OrderByDescending(i => i.IssueDate)
     .AsNoTracking()
    .ToListAsync();
        }

        // ✅ Métodos para borrado lógico
        public async Task<(IEnumerable<Invoice> Items, int TotalCount)> GetPagedIncludingDeletedAsync(
     int pageNumber, int pageSize, string? searchTerm = null)
   {
     var query = _dbSet
              .IgnoreQueryFilters()
       .Include(i => i.InvoiceDetails)
   .ThenInclude(d => d.Product)
      .Include(i => i.Client)
     .AsQueryable();

    if (!string.IsNullOrWhiteSpace(searchTerm))
{
 string term = searchTerm.Trim().ToLower();
  query = query.Where(i =>
i.UserId.ToLower().Contains(term) ||
      i.InvoiceNumber.ToLower().Contains(term) ||
    i.Client.FirstName.ToLower().Contains(term) ||
      i.Client.LastName.ToLower().Contains(term));
         }

     var totalCount = await query.CountAsync();
      var items = await query
    .OrderBy(i => i.IsActive ? 0 : 1)
        .ThenByDescending(i => i.IssueDate)
   .Skip((pageNumber - 1) * pageSize)
  .Take(pageSize)
  .AsNoTracking()
   .ToListAsync();

         return (items, totalCount);
        }

        public async Task<IEnumerable<Invoice>> GetDeletedInvoicesAsync()
    {
      return await _dbSet
       .IgnoreQueryFilters()
                .Include(i => i.InvoiceDetails)
         .ThenInclude(d => d.Product)
      .Include(i => i.Client)
 .Where(i => !i.IsActive)
  .OrderByDescending(i => i.DeletedAt)
         .AsNoTracking()
    .ToListAsync();
  }

        public async Task<Invoice?> GetByIdIncludingDeletedAsync(int id)
     {
      return await _dbSet
  .IgnoreQueryFilters()
        .Include(i => i.InvoiceDetails)
         .ThenInclude(d => d.Product)
                .Include(i => i.Client)
.FirstOrDefaultAsync(i => i.InvoiceId == id);
   }

  public async Task<int> GetActiveCountAsync()
        {
      return await _dbSet.CountAsync();
        }

        public async Task<int> GetTotalCountAsync()
        {
   return await _dbSet.IgnoreQueryFilters().CountAsync();
        }

    public async Task RestoreAsync(int id)
    {
     var invoice = await GetByIdIncludingDeletedAsync(id);
 if (invoice != null && !invoice.IsActive)
            {
          invoice.Restore();
       _dbSet.Update(invoice);
        }
        }

 // ✅ Métodos para validaciones de integridad
        public async Task<bool> ExistsByNumberAsync(string invoiceNumber)
        {
  return await _dbSet
    .IgnoreQueryFilters()
    .AnyAsync(i => i.InvoiceNumber == invoiceNumber);
 }

        public async Task<bool> ClientExistsAsync(int clientId)
      {
     return await _context.Clients.AnyAsync(c => c.ClientId == clientId);
   }

        public async Task<bool> UserExistsAsync(string userId)
        {
   var user = await _userManager.FindByIdAsync(userId);
    return user != null;
 }

    public async Task<bool> ProductExistsAsync(int productId)
        {
         return await _context.Products.AnyAsync(p => p.ProductId == productId);
        }

      // ✅ Métodos para búsquedas específicas
        public async Task<IEnumerable<Invoice>> GetByUserIdAsync(string userId)
   {
     return await _dbSet
       .Include(i => i.InvoiceDetails)
       .ThenInclude(d => d.Product)
         .Include(i => i.Client)
 .Where(i => i.UserId == userId)
        .OrderByDescending(i => i.IssueDate)
      .AsNoTracking()
    .ToListAsync();
     }

   public async Task<IEnumerable<Invoice>> GetByStatusAsync(InvoiceStatus status)
  {
  return await _dbSet
       .Include(i => i.InvoiceDetails)
         .ThenInclude(d => d.Product)
       .Include(i => i.Client)
    .Where(i => i.Status == status)
      .OrderByDescending(i => i.IssueDate)
      .AsNoTracking()
          .ToListAsync();
   }

        public async Task<IEnumerable<Invoice>> SearchAsync(DateTime? fromDate, DateTime? toDate, 
     int? clientId, InvoiceStatus? status, string? searchTerm)
        {
         var query = _dbSet
   .Include(i => i.InvoiceDetails)
      .ThenInclude(d => d.Product)
           .Include(i => i.Client)
  .AsQueryable();

 if (fromDate.HasValue)
       query = query.Where(i => i.IssueDate >= fromDate.Value);

            if (toDate.HasValue)
   query = query.Where(i => i.IssueDate <= toDate.Value);

            if (clientId.HasValue)
 query = query.Where(i => i.ClientId == clientId.Value);

     if (status.HasValue)
      query = query.Where(i => i.Status == status.Value);

   if (!string.IsNullOrWhiteSpace(searchTerm))
            {
        string term = searchTerm.Trim().ToLower();
          query = query.Where(i =>
          i.InvoiceNumber.ToLower().Contains(term) ||
i.Client.FirstName.ToLower().Contains(term) ||
     i.Client.LastName.ToLower().Contains(term));
        }

     return await query
     .OrderByDescending(i => i.IssueDate)
     .AsNoTracking()
      .ToListAsync();
        }

        // Métodos para detalle de factura
        public async Task<IEnumerable<InvoiceDetail>> GetInvoiceDetailsAsync(int invoiceId)
        {
   return await _context.InvoiceDetails
 .Where(d => d.InvoiceID == invoiceId)
        .Include(d => d.Product)
    .AsNoTracking()
  .ToListAsync();
        }

    public async Task<InvoiceDetail?> GetInvoiceDetailAsync(int invoiceId, int productId)
   {
          return await _context.InvoiceDetails
   .Include(d => d.Product)
     .AsNoTracking()
 .FirstOrDefaultAsync(d => d.InvoiceID == invoiceId && d.ProductID == productId);
        }

   public async Task AddProductToInvoiceAsync(int invoiceId, int productId, int quantity)
     {
     var invoice = await _dbSet
   .Include(i => i.InvoiceDetails)
   .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);

     var product = await _context.Products.FindAsync(productId);
   if (invoice == null) throw new InvalidOperationException("Invoice does not exist.");
            if (product == null) throw new InvalidOperationException("Product does not exist.");

 var detail = new InvoiceDetail
     {
   InvoiceID = invoiceId,
    ProductID = productId,
         Quantity = quantity,
      UnitPrice = product.Price,
      Subtotal = product.Price * quantity
   };

   invoice.InvoiceDetails.Add(detail);
       invoice.RecalculateTotals();
  }

   public async Task RemoveProductFromInvoiceAsync(int invoiceId, int productId)
    {
       var detail = await _context.InvoiceDetails
      .FirstOrDefaultAsync(d => d.InvoiceID == invoiceId && d.ProductID == productId);

   if (detail == null)
   throw new InvalidOperationException("Product does not exist in invoice.");

    _context.InvoiceDetails.Remove(detail);

 // Actualizar totales de la factura
     var invoice = await _dbSet.FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);
   if (invoice != null)
    {
             invoice.RecalculateTotals();
  }
        }

        public async Task<bool> ProductExistsInInvoiceAsync(int invoiceId, int productId)
        {
            return await _context.InvoiceDetails
      .AnyAsync(d => d.InvoiceID == invoiceId && d.ProductID == productId);
        }

        // ✅ Métodos para reportes
        public async Task<decimal> GetTotalSalesAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
  var query = _dbSet.Where(i => i.Status == InvoiceStatus.Finalized);

    if (fromDate.HasValue)
     query = query.Where(i => i.IssueDate >= fromDate.Value);

   if (toDate.HasValue)
     query = query.Where(i => i.IssueDate <= toDate.Value);

            return await query.SumAsync(i => i.Total);
        }

        public async Task<IEnumerable<Invoice>> GetTopInvoicesByAmountAsync(int count = 10)
        {
            return await _dbSet
                .Include(i => i.Client)
           .Where(i => i.Status == InvoiceStatus.Finalized)
       .OrderByDescending(i => i.Total)
    .Take(count)
      .AsNoTracking()
     .ToListAsync();
     }
    }
}

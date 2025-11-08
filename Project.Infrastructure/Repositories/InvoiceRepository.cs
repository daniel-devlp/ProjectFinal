using Project.Domain.Entities;
using Project.Domain.Interfaces;
using Project.Infrastructure.Frameworks.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Project.Infrastructure.Repositories
{
    public class InvoiceRepository : Repository<Invoice>, IInvoiceRepository
    {
        public InvoiceRepository(ApplicationDBContext context) : base(context)
        {
        }

        public override async Task<Invoice> GetByIdAsync(int id)
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
            int pageNumber, int pageSize, string searchTerm = null)
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

        public async Task<Invoice> GetByInvoiceNumberAsync(string invoiceNumber)
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

        public async Task<IEnumerable<InvoiceDetail>> GetInvoiceDetailsAsync(int invoiceId)
        {
            return await _context.InvoiceDetails
                .Where(d => d.InvoiceID == invoiceId)
                .Include(d => d.Product)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<InvoiceDetail> GetInvoiceDetailAsync(int invoiceId, int productId)
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
        }

        public async Task RemoveProductFromInvoiceAsync(int invoiceId, int productId)
        {
            var detail = await _context.InvoiceDetails
                .FirstOrDefaultAsync(d => d.InvoiceID == invoiceId && d.ProductID == productId);

            if (detail == null)
                throw new InvalidOperationException("Product does not exist in invoice.");

            _context.InvoiceDetails.Remove(detail);
        }

        public async Task<bool> ProductExistsInInvoiceAsync(int invoiceId, int productId)
        {
            return await _context.InvoiceDetails
                .AnyAsync(d => d.InvoiceID == invoiceId && d.ProductID == productId);
        }
    }
}

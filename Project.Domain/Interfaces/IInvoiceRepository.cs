using Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Interfaces
{
    public interface IInvoiceRepository : IRepository<Invoice>
    {
        Task<(IEnumerable<Invoice> Items, int TotalCount)> GetPagedAsync(
            int pageNumber, int pageSize, string searchTerm = null);
        Task<Invoice> GetByInvoiceNumberAsync(string invoiceNumber);
        Task<IEnumerable<Invoice>> GetByClientIdAsync(int clientId);
        Task<IEnumerable<Invoice>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        
        // Métodos para detalle de factura
        Task<IEnumerable<InvoiceDetail>> GetInvoiceDetailsAsync(int invoiceId);
        Task<InvoiceDetail> GetInvoiceDetailAsync(int invoiceId, int productId);
        Task AddProductToInvoiceAsync(int invoiceId, int productId, int quantity);
        Task RemoveProductFromInvoiceAsync(int invoiceId, int productId);
        Task<bool> ProductExistsInInvoiceAsync(int invoiceId, int productId);
    }
}

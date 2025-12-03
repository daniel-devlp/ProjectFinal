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
            int pageNumber, int pageSize, string? searchTerm = null);
        Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber);
        Task<IEnumerable<Invoice>> GetByClientIdAsync(int clientId);
        Task<IEnumerable<Invoice>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        
        // Métodos para borrado lógico
        Task<(IEnumerable<Invoice> Items, int TotalCount)> GetPagedIncludingDeletedAsync(
            int pageNumber, int pageSize, string? searchTerm = null);
        Task<IEnumerable<Invoice>> GetDeletedInvoicesAsync();
        Task<Invoice?> GetByIdIncludingDeletedAsync(int id);
        Task<int> GetActiveCountAsync();
        Task<int> GetTotalCountAsync();
        Task RestoreAsync(int id);

        // Métodos para validaciones de integridad
        Task<bool> ExistsByNumberAsync(string invoiceNumber);
        Task<bool> ClientExistsAsync(int clientId);
        Task<bool> UserExistsAsync(string userId);
        Task<bool> ProductExistsAsync(int productId);

        // Métodos para búsquedas específicas
        Task<IEnumerable<Invoice>> GetByUserIdAsync(string userId);
        Task<IEnumerable<Invoice>> GetByStatusAsync(InvoiceStatus status);
        Task<IEnumerable<Invoice>> SearchAsync(DateTime? fromDate, DateTime? toDate, 
            int? clientId, InvoiceStatus? status, string? searchTerm);
      
        // Métodos para detalle de factura
        Task<IEnumerable<InvoiceDetail>> GetInvoiceDetailsAsync(int invoiceId);
        Task<InvoiceDetail?> GetInvoiceDetailAsync(int invoiceId, int productId);
        Task AddProductToInvoiceAsync(int invoiceId, int productId, int quantity);
        Task RemoveProductFromInvoiceAsync(int invoiceId, int productId);
        Task<bool> ProductExistsInInvoiceAsync(int invoiceId, int productId);
   
        // Métodos para reportes
        Task<decimal> GetTotalSalesAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<IEnumerable<Invoice>> GetTopInvoicesByAmountAsync(int count = 10);
    }
}

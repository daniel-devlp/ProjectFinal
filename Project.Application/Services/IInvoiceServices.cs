using Project.Application.Dtos;
using Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.Services
{
    public interface IInvoiceServices
    {
        Task<InvoiceDto> GetByIdAsync(int id);
        Task<PagedResult<InvoiceDto>> GetAllAsync(int pageNumber, int pageSize, string searchTerm = null);
        Task AddAsync(InvoiceCreateDto invoiceDto);
        Task UpdateAsync(InvoiceUpdateDto invoiceDto);
        Task DeleteAsync(int id);
        Task<int> CountAsync(string searchTerm = null);

        // Detalle de factura (detalle)
        Task<IEnumerable<InvoiceDetailDto>> GetInvoiceDetailsAsync(int invoiceId);

        // Lógica de ventas (maestro-detalle)
        Task AddProductToInvoiceAsync(int invoiceId, int productId, int quantity);
        Task RemoveProductFromInvoiceAsync(int invoiceId, int productId);
        Task UpdateProductInInvoiceAsync(int invoiceId, int oldProductId, int newProductId, int newQuantity);

        // Validaciones de negocio
        Task<bool> ProductExistsInInvoiceAsync(int invoiceId, int productId);
        Task<bool> ValidateStockForInvoiceAsync(int invoiceId);

        // Cálculo de totales
        Task<InvoiceDto> CalculateTotalsAsync(int invoiceId);
    }
}

using Project.Application.Dtos;
using Project.Domain.Entities;

namespace Project.Application.Services
{
    public interface IInvoiceServices
    {
        Task<InvoiceDto?> GetByIdAsync(int id);
        Task<PagedResult<InvoiceDto>> GetAllAsync(int pageNumber, int pageSize, string? searchTerm = null);
        Task<InvoiceDto> AddAsync(InvoiceCreateDto invoiceDto, string currentUserId);
        Task<InvoiceDto> UpdateAsync(InvoiceUpdateDto invoiceDto);
        Task DeleteAsync(int id);
        Task<int> CountAsync(string? searchTerm = null);
        Task<IEnumerable<InvoiceDetailDto>> GetInvoiceDetailsAsync(int invoiceId);

        // ✅ Método nuevo para crear facturas asociadas automáticamente al usuario
        Task<InvoiceDto> CreateInvoiceForUserAsync(string userId, InvoiceCreateForUserDto invoiceDto);

        // ✅ Métodos nuevos para borrado lógico
        Task<PagedResult<InvoiceDto>> GetAllIncludingDeletedAsync(int pageNumber, int pageSize, string? searchTerm = null);
        Task<IEnumerable<InvoiceDto>> GetDeletedInvoicesAsync();
        Task RestoreAsync(int id);
        Task<int> CountAllAsync(string? searchTerm = null);

        // ✅ Métodos para operaciones específicas de facturas
        Task<InvoiceDto> FinalizeAsync(InvoiceFinalizeDto dto);
        Task<InvoiceDto> CancelAsync(InvoiceCancelDto dto);
        Task<IEnumerable<InvoiceDto>> SearchAsync(InvoiceSearchDto searchDto);
    }
}

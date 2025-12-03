using Project.Application.Dtos;
using Project.Domain.Entities;
using Project.Domain.Interfaces;

namespace Project.Application.Services
{
    public class InvoiceService : IInvoiceServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IInvoiceNumberService _invoiceNumberService;

        public InvoiceService(IUnitOfWork unitOfWork, IInvoiceNumberService invoiceNumberService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _invoiceNumberService = invoiceNumberService ?? throw new ArgumentNullException(nameof(invoiceNumberService));
        }

        public async Task<InvoiceDto?> GetByIdAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invoice ID must be greater than zero.", nameof(id));
            var invoice = await _unitOfWork.Invoices.GetByIdAsync(id);
            return invoice != null ? ToInvoiceDto(invoice) : null;
        }

        public async Task<PagedResult<InvoiceDto>> GetAllAsync(int pageNumber, int pageSize, string? searchTerm = null)
        {
            if (pageNumber <= 0) throw new ArgumentException("Page number must be greater than zero.", nameof(pageNumber));
            if (pageSize <= 0) throw new ArgumentException("Page size must be greater than zero.", nameof(pageSize));

            var (invoices, totalCount) = await _unitOfWork.Invoices.GetPagedAsync(pageNumber, pageSize, searchTerm);
            var invoiceDtos = invoices.Select(ToInvoiceDto).ToList();

            return new PagedResult<InvoiceDto>
            {
                Items = invoiceDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<InvoiceDto> AddAsync(InvoiceCreateDto invoiceDto, string currentUserId)
        {
            if (invoiceDto == null) throw new ArgumentNullException(nameof(invoiceDto));
            if (string.IsNullOrWhiteSpace(currentUserId)) throw new ArgumentException("User ID is required.", nameof(currentUserId));

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // ✅ Validaciones de integridad referencial
                if (!await _unitOfWork.Invoices.ClientExistsAsync(invoiceDto.ClientId))
                    throw new InvalidOperationException("El cliente especificado no existe.");

                if (!await _unitOfWork.Invoices.UserExistsAsync(currentUserId))
                    throw new InvalidOperationException("El usuario actual no existe.");

                // ✅ Validar detalles y stock
                foreach (var detail in invoiceDto.InvoiceDetails)
                {
                    if (!await _unitOfWork.Invoices.ProductExistsAsync(detail.ProductId))
                        throw new InvalidOperationException($"El producto con ID {detail.ProductId} no existe.");

                    if (!await _unitOfWork.Products.HasStockAsync(detail.ProductId, detail.Quantity))
                        throw new InvalidOperationException($"Stock insuficiente para el producto con ID {detail.ProductId}.");
                }

                // ✅ Generar número de factura automáticamente
                var invoiceNumber = await _invoiceNumberService.GenerateInvoiceNumberAsync();

                // ✅ Crear factura usando constructor de dominio
                var invoice = new Invoice(invoiceDto.ClientId, currentUserId, invoiceDto.Observations ?? "");
                invoice.SetInvoiceNumber(invoiceNumber);

                // ✅ Obtener productos y agregar detalles con precios actuales
                foreach (var detailDto in invoiceDto.InvoiceDetails)
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(detailDto.ProductId);
                    if (product != null)
                    {
                        invoice.AddDetail(detailDto.ProductId, detailDto.Quantity, product.Price);
                    }
                }

                invoice.Finalize(); // Cambiar estado a finalizada

                await _unitOfWork.Invoices.AddAsync(invoice);
                await _unitOfWork.SaveChangesAsync();

                // ✅ Decrementar stock después de guardar la factura
                foreach (var detail in invoice.InvoiceDetails)
                {
                    await _unitOfWork.Products.DecreaseStockAsync(detail.ProductID, detail.Quantity);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return ToInvoiceDto(invoice);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<InvoiceDto> UpdateAsync(InvoiceUpdateDto invoiceDto)
        {
            if (invoiceDto == null) throw new ArgumentNullException(nameof(invoiceDto));

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var existingInvoice = await _unitOfWork.Invoices.GetByIdAsync(invoiceDto.InvoiceId);
                if (existingInvoice == null)
                    throw new InvalidOperationException("La factura no existe.");

                if (!existingInvoice.CanBeModified())
                    throw new InvalidOperationException("No se puede modificar una factura finalizada o cancelada.");

                // ✅ Validaciones de integridad referencial
                if (!await _unitOfWork.Invoices.ClientExistsAsync(invoiceDto.ClientId))
                    throw new InvalidOperationException("El cliente especificado no existe.");

                // ✅ Restaurar stock de productos existentes
                foreach (var existingDetail in existingInvoice.InvoiceDetails.ToList())
                {
                    await _unitOfWork.Products.IncreaseStockAsync(existingDetail.ProductID, existingDetail.Quantity);
                    existingInvoice.RemoveDetail(existingDetail.ProductID);
                }

                // ✅ Validar nuevos productos y stock
                foreach (var detail in invoiceDto.InvoiceDetails)
                {
                    if (!await _unitOfWork.Invoices.ProductExistsAsync(detail.ProductId))
                        throw new InvalidOperationException($"El producto con ID {detail.ProductId} no existe.");

                    if (!await _unitOfWork.Products.HasStockAsync(detail.ProductId, detail.Quantity))
                        throw new InvalidOperationException($"Stock insuficiente para el producto con ID {detail.ProductId}.");
                }

                // ✅ Actualizar campos básicos
                existingInvoice.SetClient(invoiceDto.ClientId);
                existingInvoice.Observations = invoiceDto.Observations?.Trim() ?? "";

                // ✅ Agregar nuevos detalles con precios actuales
                foreach (var detailDto in invoiceDto.InvoiceDetails)
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(detailDto.ProductId);
                    if (product != null)
                    {
                        existingInvoice.AddDetail(detailDto.ProductId, detailDto.Quantity, product.Price);
                    }
                }

                _unitOfWork.Invoices.Update(existingInvoice);
                await _unitOfWork.SaveChangesAsync();

                // ✅ Decrementar stock de nuevos productos
                foreach (var detail in existingInvoice.InvoiceDetails)
                {
                    await _unitOfWork.Products.DecreaseStockAsync(detail.ProductID, detail.Quantity);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return ToInvoiceDto(existingInvoice);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invoice ID must be greater than zero.", nameof(id));
            
            var invoice = await _unitOfWork.Invoices.GetByIdAsync(id);
            if (invoice == null)
                throw new InvalidOperationException("La factura no existe.");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // ✅ Restaurar stock antes de eliminación lógica
                foreach (var detail in invoice.InvoiceDetails)
                {
                    await _unitOfWork.Products.IncreaseStockAsync(detail.ProductID, detail.Quantity);
                }

                // ✅ Borrado lógico
                invoice.SoftDelete("Eliminada por usuario");
                _unitOfWork.Invoices.Update(invoice);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        // ✅ Métodos nuevos para borrado lógico
        public async Task<PagedResult<InvoiceDto>> GetAllIncludingDeletedAsync(int pageNumber, int pageSize, string? searchTerm = null)
        {
            var (invoices, totalCount) = await _unitOfWork.Invoices.GetPagedIncludingDeletedAsync(pageNumber, pageSize, searchTerm);
            var invoiceDtos = invoices.Select(ToInvoiceDto).ToList();

            return new PagedResult<InvoiceDto>
            {
                Items = invoiceDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<IEnumerable<InvoiceDto>> GetDeletedInvoicesAsync()
        {
            var invoices = await _unitOfWork.Invoices.GetDeletedInvoicesAsync();
            return invoices.Select(ToInvoiceDto);
        }

        public async Task RestoreAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invoice ID must be greater than zero.", nameof(id));

            await _unitOfWork.Invoices.RestoreAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<InvoiceDto> FinalizeAsync(InvoiceFinalizeDto dto)
        {
            var invoice = await _unitOfWork.Invoices.GetByIdAsync(dto.InvoiceId);
            if (invoice == null)
                throw new InvalidOperationException("La factura no existe.");

            if (invoice.Status != InvoiceStatus.Draft)
                throw new InvalidOperationException("Solo se pueden finalizar facturas en borrador.");

            if (!invoice.InvoiceDetails.Any())
                throw new InvalidOperationException("No se puede finalizar una factura sin detalles.");

            invoice.Finalize();
            _unitOfWork.Invoices.Update(invoice);
            await _unitOfWork.SaveChangesAsync();

            return ToInvoiceDto(invoice);
        }

        public async Task<InvoiceDto> CancelAsync(InvoiceCancelDto dto)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var invoice = await _unitOfWork.Invoices.GetByIdAsync(dto.InvoiceId);
                if (invoice == null)
                    throw new InvalidOperationException("La factura no existe.");

                if (invoice.Status == InvoiceStatus.Cancelled)
                    throw new InvalidOperationException("La factura ya está cancelada.");

                // ✅ Restaurar stock si se cancela una factura finalizada
                if (invoice.Status == InvoiceStatus.Finalized)
                {
                    foreach (var detail in invoice.InvoiceDetails)
                    {
                        await _unitOfWork.Products.IncreaseStockAsync(detail.ProductID, detail.Quantity);
                    }
                }

                invoice.Cancel(dto.Reason);
                _unitOfWork.Invoices.Update(invoice);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return ToInvoiceDto(invoice);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<IEnumerable<InvoiceDto>> SearchAsync(InvoiceSearchDto searchDto)
        {
            var invoices = await _unitOfWork.Invoices.SearchAsync(
                searchDto.FromDate,
                searchDto.ToDate,
                searchDto.ClientId,
                Enum.TryParse<InvoiceStatus>(searchDto.Status, out var status) ? status : null,
                searchDto.SearchTerm);

            return invoices.Select(ToInvoiceDto);
        }

        public async Task<int> CountAsync(string? searchTerm = null)
        {
            return await _unitOfWork.Invoices.GetActiveCountAsync();
        }

        public async Task<int> CountAllAsync(string? searchTerm = null)
        {
            return await _unitOfWork.Invoices.GetTotalCountAsync();
        }

        public async Task<IEnumerable<InvoiceDetailDto>> GetInvoiceDetailsAsync(int invoiceId)
        {
            if (invoiceId <= 0) throw new ArgumentException("Invoice ID must be greater than zero.", nameof(invoiceId));
            var details = await _unitOfWork.Invoices.GetInvoiceDetailsAsync(invoiceId);
            return details?.Select(ToInvoiceDetailDto) ?? Enumerable.Empty<InvoiceDetailDto>();
        }

        // ✅ Método de mapeo mejorado
        private static InvoiceDto ToInvoiceDto(Invoice invoice)
        {
            if (invoice == null) return null!;

            return new InvoiceDto
            {
                InvoiceId = invoice.InvoiceId,
                InvoiceNumber = invoice.InvoiceNumber,
                ClientId = invoice.ClientId,
                UserId = invoice.UserId,
                IssueDate = invoice.IssueDate,
                Subtotal = invoice.Subtotal,
                Tax = invoice.Tax,
                Total = invoice.Total,
                Observations = invoice.Observations,
                IsActive = invoice.IsActive,
                Status = invoice.Status.ToString(),
                CancelReason = invoice.CancelReason,
                CreatedAt = invoice.CreatedAt,
                UpdatedAt = invoice.UpdatedAt,
                DeletedAt = invoice.DeletedAt,
                Client = invoice.Client != null ? new ClientDto
                {
                    ClientId = invoice.Client.ClientId,
                    IdentificationNumber = invoice.Client.IdentificationNumber,
                    IdentificationType = invoice.Client.IdentificationType,
                    FirstName = invoice.Client.FirstName,
                    LastName = invoice.Client.LastName,
                    Phone = invoice.Client.Phone,
                    Email = invoice.Client.Email,
                    Address = invoice.Client.Address,
                    IsActive = invoice.Client.IsActive,
                    CreatedAt = invoice.Client.CreatedAt,
                    UpdatedAt = invoice.Client.UpdatedAt,
                    DeletedAt = invoice.Client.DeletedAt
                } : null,
                InvoiceDetails = invoice.InvoiceDetails?.Select(ToInvoiceDetailDto).ToList() ?? new List<InvoiceDetailDto>()
            };
        }

        private static InvoiceDetailDto ToInvoiceDetailDto(InvoiceDetail detail)
        {
            if (detail == null) return null!;

            return new InvoiceDetailDto
            {
                InvoiceDetailId = detail.InvoiceDetailId,
                InvoiceId = detail.InvoiceID,
                ProductId = detail.ProductID,
                Quantity = detail.Quantity,
                UnitPrice = detail.UnitPrice,
                Subtotal = detail.Subtotal,
                Product = detail.Product != null ? new ProductDto
                {
                    ProductId = detail.Product.ProductId,
                    Code = detail.Product.Code,
                    Name = detail.Product.Name,
                    Description = detail.Product.Description,
                    Price = detail.Product.Price,
                    Stock = detail.Product.Stock,
                    IsActive = detail.Product.IsActive,
                    ImageUri = detail.Product.ImageUri,
                    CreatedAt = detail.Product.CreatedAt,
                    UpdatedAt = detail.Product.UpdatedAt,
                    DeletedAt = detail.Product.DeletedAt
                } : null
            };
        }
    }
}

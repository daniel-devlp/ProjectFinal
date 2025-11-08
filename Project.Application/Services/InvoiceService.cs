using Project.Application.Dtos;
using Project.Domain.Entities;
using Project.Domain.Interfaces;

namespace Project.Application.Services
{
    public class InvoiceService : IInvoiceServices
    {
        private readonly IUnitOfWork _unitOfWork;

        public InvoiceService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<InvoiceDto> GetByIdAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invoice ID must be greater than zero.", nameof(id));
            var invoice = await _unitOfWork.Invoices.GetByIdAsync(id);
            return invoice != null ? ToInvoiceDto(invoice) : null;
        }

        public async Task<PagedResult<InvoiceDto>> GetAllAsync(int pageNumber, int pageSize, string searchTerm = null)
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

        public async Task AddAsync(InvoiceCreateDto invoiceDto)
        {
            if (invoiceDto == null) throw new ArgumentNullException(nameof(invoiceDto));
            if (invoiceDto.ClientId <= 0) throw new ArgumentException("Client ID is required.", nameof(invoiceDto.ClientId));
            if (string.IsNullOrWhiteSpace(invoiceDto.UserId))
                throw new ArgumentException("User ID is required.", nameof(invoiceDto.UserId));
            if (invoiceDto.InvoiceDetails == null || !invoiceDto.InvoiceDetails.Any())
                throw new ArgumentException("Invoice must have at least one detail.", nameof(invoiceDto.InvoiceDetails));

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Validar stock y existencia de productos
                foreach (var detail in invoiceDto.InvoiceDetails)
                {
                    if (!await _unitOfWork.Products.ExistsAsync(p => p.ProductId == detail.ProductId))
                        throw new InvalidOperationException($"Product with ID {detail.ProductId} does not exist.");

                    if (detail.Quantity <= 0)
                        throw new ArgumentException("Quantity must be greater than zero.", nameof(detail.Quantity));

                    if (!await _unitOfWork.Products.HasStockAsync(detail.ProductId, detail.Quantity))
                        throw new InvalidOperationException($"Insufficient stock for product with ID {detail.ProductId}.");
                }

                var invoice = new Invoice
                {
                    InvoiceNumber = invoiceDto.InvoiceNumber,
                    ClientId = invoiceDto.ClientId,
                    UserId = invoiceDto.UserId,
                    IssueDate = invoiceDto.IssueDate ?? DateTime.UtcNow,
                    Observations = invoiceDto.Observations?.Trim(),
                    InvoiceDetails = invoiceDto.InvoiceDetails.Select(d => new InvoiceDetail
                    {
                        ProductID = d.ProductId,
                        Quantity = d.Quantity,
                        UnitPrice = d.UnitPrice,
                        Subtotal = d.Quantity * d.UnitPrice
                    }).ToList()
                };

                invoice.Subtotal = invoice.InvoiceDetails.Sum(d => d.Subtotal);
                invoice.Tax = Math.Round(invoice.Subtotal * 0.12m, 2);
                invoice.Total = invoice.Subtotal + invoice.Tax;

                await _unitOfWork.Invoices.AddAsync(invoice);
                await _unitOfWork.SaveChangesAsync();

                // Decrementar stock
                foreach (var detail in invoice.InvoiceDetails)
                {
                    await _unitOfWork.Products.DecreaseStockAsync(detail.ProductID, detail.Quantity);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task UpdateAsync(InvoiceUpdateDto invoiceDto)
        {
            if (invoiceDto == null) throw new ArgumentNullException(nameof(invoiceDto));
            if (invoiceDto.InvoiceId <= 0) throw new ArgumentException("Invoice ID is required.", nameof(invoiceDto.InvoiceId));

            var existingInvoice = await _unitOfWork.Invoices.GetByIdAsync(invoiceDto.InvoiceId);
            if (existingInvoice == null)
                throw new InvalidOperationException("Invoice not found.");

            // Validar detalles, existencia y stock 
            if (invoiceDto.InvoiceDetails == null || !invoiceDto.InvoiceDetails.Any())
                throw new ArgumentException("Invoice must have at least one detail.", nameof(invoiceDto.InvoiceDetails));

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Restaurar stock de los productos existentes
                foreach (var existingDetail in existingInvoice.InvoiceDetails)
                {
                    await _unitOfWork.Products.IncreaseStockAsync(existingDetail.ProductID, existingDetail.Quantity);
                }

                // Validar nuevos productos y stock
                foreach (var detail in invoiceDto.InvoiceDetails)
                {
                    if (!await _unitOfWork.Products.ExistsAsync(p => p.ProductId == detail.ProductId))
                        throw new InvalidOperationException($"Product with ID {detail.ProductId} does not exist.");

                    if (detail.Quantity <= 0)
                        throw new ArgumentException("Quantity must be greater than zero.", nameof(detail.Quantity));

                    if (!await _unitOfWork.Products.HasStockAsync(detail.ProductId, detail.Quantity))
                        throw new InvalidOperationException($"Insufficient stock for product with ID {detail.ProductId}.");
                }

                // Actualizar campos principales
                existingInvoice.InvoiceNumber = invoiceDto.InvoiceNumber;
                existingInvoice.ClientId = invoiceDto.ClientId;
                existingInvoice.UserId = invoiceDto.UserId;
                existingInvoice.IssueDate = invoiceDto.IssueDate;
                existingInvoice.Observations = invoiceDto.Observations?.Trim();

                // Limpiar detalles existentes
                existingInvoice.InvoiceDetails.Clear();

                // Agregar nuevos detalles
                foreach (var d in invoiceDto.InvoiceDetails)
                {
                    existingInvoice.InvoiceDetails.Add(new InvoiceDetail
                    {
                        ProductID = d.ProductId,
                        Quantity = d.Quantity,
                        UnitPrice = d.UnitPrice,
                        Subtotal = d.Quantity * d.UnitPrice,
                        InvoiceID = invoiceDto.InvoiceId
                    });
                }

                existingInvoice.Subtotal = existingInvoice.InvoiceDetails.Sum(d => d.Subtotal);
                existingInvoice.Tax = Math.Round(existingInvoice.Subtotal * 0.12m, 2);
                existingInvoice.Total = existingInvoice.Subtotal + existingInvoice.Tax;

                _unitOfWork.Invoices.Update(existingInvoice);
                await _unitOfWork.SaveChangesAsync();

                // Decrementar stock de nuevos productos
                foreach (var detail in existingInvoice.InvoiceDetails)
                {
                    await _unitOfWork.Products.DecreaseStockAsync(detail.ProductID, detail.Quantity);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
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
                throw new InvalidOperationException("Invoice does not exist.");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Restaurar stock antes de eliminar
                foreach (var detail in invoice.InvoiceDetails)
                {
                    await _unitOfWork.Products.IncreaseStockAsync(detail.ProductID, detail.Quantity);
                }

                _unitOfWork.Invoices.Remove(invoice);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<int> CountAsync(string searchTerm = null)
        {
            return await _unitOfWork.Invoices.CountAsync(i => 
                string.IsNullOrEmpty(searchTerm) || 
                i.InvoiceNumber.Contains(searchTerm) ||
                i.UserId.Contains(searchTerm) ||
                i.Client.FirstName.Contains(searchTerm) ||
                i.Client.LastName.Contains(searchTerm));
        }

        public async Task<IEnumerable<InvoiceDetailDto>> GetInvoiceDetailsAsync(int invoiceId)
        {
            if (invoiceId <= 0) throw new ArgumentException("Invoice ID must be greater than zero.", nameof(invoiceId));
            var details = await _unitOfWork.Invoices.GetInvoiceDetailsAsync(invoiceId);
            return details?.Select(ToInvoiceDetailDto) ?? Enumerable.Empty<InvoiceDetailDto>();
        }

        // --- Métodos transaccionales para productos en facturas ---

        public async Task AddProductToInvoiceAsync(int invoiceId, int productId, int quantity)
        {
            if (invoiceId <= 0) throw new ArgumentException("Invoice ID must be greater than zero.", nameof(invoiceId));
            if (productId <= 0) throw new ArgumentException("Product ID must be greater than zero.", nameof(productId));
            if (quantity <= 0) throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var exists = await _unitOfWork.Invoices.ProductExistsInInvoiceAsync(invoiceId, productId);
                if (exists)
                    throw new InvalidOperationException("Product already exists in the invoice.");

                if (!await _unitOfWork.Products.ExistsAsync(p => p.ProductId == productId))
                    throw new InvalidOperationException("Product does not exist.");

                if (!await _unitOfWork.Products.HasStockAsync(productId, quantity))
                    throw new InvalidOperationException("Insufficient stock for the product.");

                await _unitOfWork.Invoices.AddProductToInvoiceAsync(invoiceId, productId, quantity);
                await _unitOfWork.Products.DecreaseStockAsync(productId, quantity);
                await CalculateTotalsAsync(invoiceId);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task RemoveProductFromInvoiceAsync(int invoiceId, int productId)
        {
            if (invoiceId <= 0) throw new ArgumentException("Invoice ID must be greater than zero.", nameof(invoiceId));
            if (productId <= 0) throw new ArgumentException("Product ID must be greater than zero.", nameof(productId));

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var detail = await _unitOfWork.Invoices.GetInvoiceDetailAsync(invoiceId, productId);
                if (detail == null)
                    throw new InvalidOperationException("Product does not exist in the invoice.");

                await _unitOfWork.Invoices.RemoveProductFromInvoiceAsync(invoiceId, productId);
                await _unitOfWork.Products.IncreaseStockAsync(productId, detail.Quantity);
                await CalculateTotalsAsync(invoiceId);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task UpdateProductInInvoiceAsync(int invoiceId, int oldProductId, int newProductId, int newQuantity)
        {
            if (invoiceId <= 0) throw new ArgumentException("Invoice ID must be greater than zero.", nameof(invoiceId));
            if (oldProductId <= 0 || newProductId <= 0) throw new ArgumentException("Product IDs must be greater than zero.");
            if (newQuantity <= 0) throw new ArgumentException("Quantity must be greater than zero.", nameof(newQuantity));

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await RemoveProductFromInvoiceAsync(invoiceId, oldProductId);
                await AddProductToInvoiceAsync(invoiceId, newProductId, newQuantity);
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public Task<bool> ProductExistsInInvoiceAsync(int invoiceId, int productId)
        {
            if (invoiceId <= 0) throw new ArgumentException("Invoice ID must be greater than zero.", nameof(invoiceId));
            if (productId <= 0) throw new ArgumentException("Product ID must be greater than zero.", nameof(productId));
            return _unitOfWork.Invoices.ProductExistsInInvoiceAsync(invoiceId, productId);
        }

        public async Task<bool> ValidateStockForInvoiceAsync(int invoiceId)
        {
            if (invoiceId <= 0) throw new ArgumentException("Invoice ID must be greater than zero.", nameof(invoiceId));
            var details = await _unitOfWork.Invoices.GetInvoiceDetailsAsync(invoiceId);
            foreach (var detail in details)
            {
                var hasStock = await _unitOfWork.Products.HasStockAsync(detail.ProductID, detail.Quantity);
                if (!hasStock)
                    return false;
            }
            return true;
        }

        public async Task<InvoiceDto> CalculateTotalsAsync(int invoiceId)
        {
            if (invoiceId <= 0) throw new ArgumentException("Invoice ID must be greater than zero.", nameof(invoiceId));
            var invoice = await _unitOfWork.Invoices.GetByIdAsync(invoiceId);
            if (invoice == null)
                throw new InvalidOperationException("Invoice does not exist.");
            
            var details = await _unitOfWork.Invoices.GetInvoiceDetailsAsync(invoiceId);

            decimal subtotal = details.Sum(d => d.Quantity * d.UnitPrice);
            decimal iva = Math.Round(subtotal * 0.12m, 2);
            decimal total = subtotal + iva;

            invoice.Subtotal = subtotal;
            invoice.Tax = iva;
            invoice.Total = total;

            _unitOfWork.Invoices.Update(invoice);
            await _unitOfWork.SaveChangesAsync();

            return ToInvoiceDto(invoice);
        }

        // --- Métodos de mapeo manual ---

        private InvoiceDto ToInvoiceDto(Invoice invoice)
        {
            if (invoice == null) return null;

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
                Client = invoice.Client != null ? new ClientDto
                {
                    ClientId = invoice.Client.ClientId,
                    IdentificationNumber = invoice.Client.IdentificationNumber,
                  //  IdentificationType = invoice.Client.IdentificationType,
                    FirstName = invoice.Client.FirstName,
                    LastName = invoice.Client.LastName,
                    Phone = invoice.Client.Phone,
                    Email = invoice.Client.Email,
                    Address = invoice.Client.Address
                } : null,
                InvoiceDetails = invoice.InvoiceDetails?.Select(ToInvoiceDetailDto).ToList()
            };
        }

        private InvoiceDetailDto ToInvoiceDetailDto(InvoiceDetail d)
        {
            if (d == null) return null;
            return new InvoiceDetailDto
            {
                InvoiceDetailId = d.InvoiceDetailId,
                InvoiceId = d.InvoiceID,
                ProductId = d.ProductID,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice,
                Subtotal = d.Subtotal,
                Product = d.Product != null ? new ProductDto
                {
                    ProductId = d.Product.ProductId,
                    Code = d.Product.Code,
                    Name = d.Product.Name,
                    Description = d.Product.Description,
                    Price = d.Product.Price,
                    Stock = d.Product.Stock,
                    IsActive = d.Product.IsActive
                } : null
            };
        }
    }
}

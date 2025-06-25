using Project.Application.Dtos;
using Project.Domain.Entities;
using Project.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Project.Application.Services
{
    public class InvoiceService : IInvoiceServices
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IProductRepository _productRepository;
        private readonly Infrastructure.Frameworks.EntityFramework.ApplicationDBContext _context;

        public InvoiceService(
            IInvoiceRepository invoiceRepository,
            IProductRepository productRepository,
            Infrastructure.Frameworks.EntityFramework.ApplicationDBContext context)
        {
            _invoiceRepository = invoiceRepository ?? throw new ArgumentNullException(nameof(invoiceRepository));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<InvoiceDto> GetByIdAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invoice ID must be greater than zero.", nameof(id));
            var invoice = await _invoiceRepository.GetByIdAsync(id);
            return invoice != null ? ToInvoiceDto(invoice) : null;
        }

        public async Task<PagedResult<InvoiceDto>> GetAllAsync(int pageNumber, int pageSize, string searchTerm = null)
        {
            if (pageNumber <= 0) throw new ArgumentException("Page number must be greater than zero.", nameof(pageNumber));
            if (pageSize <= 0) throw new ArgumentException("Page size must be greater than zero.", nameof(pageSize));

            var query = _context.Invoices
                .Include(i => i.Client)
                .Include(i => i.InvoiceDetails)
                    .ThenInclude(d => d.Product)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string term = searchTerm.Trim().ToLower();
                query = query.Where(i =>
                    i.InvoiceNumber.ToLower().Contains(term) ||
                    i.UserId.ToLower().Contains(term) ||
                    i.Client.FirstName.ToLower().Contains(term) ||
                    i.Client.LastName.ToLower().Contains(term) ||
                    (i.Observations != null && i.Observations.ToLower().Contains(term)) ||
                    i.InvoiceDetails.Any(d =>
                        d.Product.Name.ToLower().Contains(term) ||
                        d.Product.Code.ToLower().Contains(term)
                    )
                );
            }

            var totalCount = await query.CountAsync();

            var invoices = await query
                .OrderByDescending(i => i.IssueDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            var invoiceDtos = invoices.Select(i => new InvoiceDto
            {
                InvoiceId = i.InvoiceId,
                InvoiceNumber = i.InvoiceNumber,
                ClientId = i.ClientId,
                UserId = i.UserId,
                IssueDate = i.IssueDate,
                Subtotal = i.Subtotal,
                Tax = i.Tax,
                Total = i.Total,
                Observations = i.Observations,
                Client = i.Client == null ? null : new ClientDto
                {
                    ClientId = i.Client.ClientId,
                    IdentificationNumber = i.Client.IdentificationNumber,
                    FirstName = i.Client.FirstName,
                    LastName = i.Client.LastName,
                    Phone = i.Client.Phone,
                    Email = i.Client.Email,
                    Address = i.Client.Address
                },
                InvoiceDetails = i.InvoiceDetails?.Select(d => new InvoiceDetailDto
                {
                    InvoiceDetailId = d.InvoiceDetailId,
                    InvoiceId = d.InvoiceID,
                    ProductId = d.ProductID,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,
                    Product = d.Product == null ? null : new ProductDto
                    {
                        ProductId = d.Product.ProductId,
                        Code = d.Product.Code,
                        Name = d.Product.Name,
                        Description = d.Product.Description,
                        Price = d.Product.Price,
                        Stock = d.Product.Stock,
                        IsActive = d.Product.IsActive
                    }
                }).ToList()
            }).ToList();

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

            // Validar stock y existencia de productos
            foreach (var detail in invoiceDto.InvoiceDetails)
            {
                if (!await _productRepository.ExistsByIdAsync(detail.ProductId))
                    throw new InvalidOperationException($"El producto con ID {detail.ProductId} no existe.");

                if (detail.Quantity <= 0)
                    throw new ArgumentException("La cantidad debe ser mayor a cero.", nameof(detail.Quantity));

                if (!await _productRepository.HasStockAsync(detail.ProductId, detail.Quantity))
                    throw new InvalidOperationException($"No hay suficiente stock para el producto con ID {detail.ProductId}.");
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

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    await _invoiceRepository.AddAsync(invoice);
                    foreach (var detail in invoice.InvoiceDetails)
                        await _productRepository.DecreaseStockAsync(detail.ProductID, detail.Quantity);
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task UpdateAsync(InvoiceUpdateDto invoiceDto)
        {
            if (invoiceDto == null) throw new ArgumentNullException(nameof(invoiceDto));
            if (invoiceDto.InvoiceId <= 0) throw new ArgumentException("Invoice ID is required.", nameof(invoiceDto.InvoiceId));

            var existingInvoice = await _invoiceRepository.GetByIdAsync(invoiceDto.InvoiceId);
            if (existingInvoice == null)
                throw new InvalidOperationException("Invoice not found.");

            // Validar detalles, existencia y stock 
            if (invoiceDto.InvoiceDetails == null || !invoiceDto.InvoiceDetails.Any())
                throw new ArgumentException("Invoice must have at least one detail.", nameof(invoiceDto.InvoiceDetails));

            foreach (var detail in invoiceDto.InvoiceDetails)
            {
                if (!await _productRepository.ExistsByIdAsync(detail.ProductId))
                    throw new InvalidOperationException($"El producto con ID {detail.ProductId} no existe.");

                if (detail.Quantity <= 0)
                    throw new ArgumentException("La cantidad debe ser mayor a cero.", nameof(detail.Quantity));

                if (!await _productRepository.HasStockAsync(detail.ProductId, detail.Quantity))
                    throw new InvalidOperationException($"No hay suficiente stock para el producto con ID {detail.ProductId}.");
            }

            // Actualiza campos principales
            existingInvoice.InvoiceNumber = invoiceDto.InvoiceNumber;
            existingInvoice.ClientId = invoiceDto.ClientId;
            existingInvoice.UserId = invoiceDto.UserId;
            existingInvoice.IssueDate = invoiceDto.IssueDate;
            existingInvoice.Observations = invoiceDto.Observations?.Trim();

            // Elimina detalles existentes de la base de datos
            _context.InvoiceDetails.RemoveRange(existingInvoice.InvoiceDetails);
            await _context.SaveChangesAsync(); // Guarda para liberar correctamente los detalles antiguos

            // Agrega los nuevos detalles a la entidad ya trackeada
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

            await _invoiceRepository.UpdateAsync(existingInvoice);
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invoice ID must be greater than zero.", nameof(id));
            var invoice = await _invoiceRepository.GetByIdAsync(id);
            if (invoice == null)
                throw new InvalidOperationException("Invoice does not exist.");
            await _invoiceRepository.DeleteAsync(id);
        }

        public Task<int> CountAsync(string searchTerm = null)
        {
            return _invoiceRepository.CountAsync(searchTerm?.Trim());
        }

        public async Task<IEnumerable<InvoiceDetailDto>> GetInvoiceDetailsAsync(int invoiceId)
        {
            if (invoiceId <= 0) throw new ArgumentException("Invoice ID must be greater than zero.", nameof(invoiceId));
            var details = await _invoiceRepository.GetInvoiceDetailsAsync(invoiceId);
            return details?.Select(ToInvoiceDetailDto) ?? Enumerable.Empty<InvoiceDetailDto>();
        }

        // --- Métodos transaccionales para productos en facturas ---

        public async Task AddProductToInvoiceAsync(int invoiceId, int productId, int quantity)
        {
            if (invoiceId <= 0) throw new ArgumentException("Invoice ID must be greater than zero.", nameof(invoiceId));
            if (productId <= 0) throw new ArgumentException("Product ID must be greater than zero.", nameof(productId));
            if (quantity <= 0) throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var exists = await _invoiceRepository.ProductExistsInInvoiceAsync(invoiceId, productId);
                    if (exists)
                        throw new InvalidOperationException("El producto ya existe en la factura.");

                    if (!await _productRepository.ExistsByIdAsync(productId))
                        throw new InvalidOperationException("El producto no existe.");

                    if (!await _productRepository.HasStockAsync(productId, quantity))
                        throw new InvalidOperationException("No hay suficiente stock para el producto.");

                    await _invoiceRepository.AddProductToInvoiceAsync(invoiceId, productId, quantity);
                    await _productRepository.DecreaseStockAsync(productId, quantity);
                    await CalculateTotalsAsync(invoiceId);

                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task RemoveProductFromInvoiceAsync(int invoiceId, int productId)
        {
            if (invoiceId <= 0) throw new ArgumentException("Invoice ID must be greater than zero.", nameof(invoiceId));
            if (productId <= 0) throw new ArgumentException("Product ID must be greater than zero.", nameof(productId));

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var detail = await _invoiceRepository.GetInvoiceDetailAsync(invoiceId, productId);
                    if (detail == null)
                        throw new InvalidOperationException("El producto no existe en la factura.");

                    await _invoiceRepository.RemoveProductFromInvoiceAsync(invoiceId, productId);
                    await _productRepository.IncreaseStockAsync(productId, detail.Quantity);
                    await CalculateTotalsAsync(invoiceId);

                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task UpdateProductInInvoiceAsync(int invoiceId, int oldProductId, int newProductId, int newQuantity)
        {
            if (invoiceId <= 0) throw new ArgumentException("Invoice ID must be greater than zero.", nameof(invoiceId));
            if (oldProductId <= 0 || newProductId <= 0) throw new ArgumentException("Product IDs must be greater than zero.");
            if (newQuantity <= 0) throw new ArgumentException("Quantity must be greater than zero.", nameof(newQuantity));

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    await RemoveProductFromInvoiceAsync(invoiceId, oldProductId);
                    await AddProductToInvoiceAsync(invoiceId, newProductId, newQuantity);

                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public Task<bool> ProductExistsInInvoiceAsync(int invoiceId, int productId)
        {
            if (invoiceId <= 0) throw new ArgumentException("Invoice ID must be greater than zero.", nameof(invoiceId));
            if (productId <= 0) throw new ArgumentException("Product ID must be greater than zero.", nameof(productId));
            return _invoiceRepository.ProductExistsInInvoiceAsync(invoiceId, productId);
        }

        public async Task<bool> ValidateStockForInvoiceAsync(int invoiceId)
        {
            if (invoiceId <= 0) throw new ArgumentException("Invoice ID must be greater than zero.", nameof(invoiceId));
            var details = await _invoiceRepository.GetInvoiceDetailsAsync(invoiceId);
            foreach (var detail in details)
            {
                var hasStock = await _productRepository.HasStockAsync(detail.ProductID, detail.Quantity);
                if (!hasStock)
                    return false;
            }
            return true;
        }

        public async Task<InvoiceDto> CalculateTotalsAsync(int invoiceId)
        {
            if (invoiceId <= 0) throw new ArgumentException("Invoice ID must be greater than zero.", nameof(invoiceId));
            var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);
            if (invoice == null)
                throw new InvalidOperationException("Invoice does not exist.");
            var details = await _invoiceRepository.GetInvoiceDetailsAsync(invoiceId);

            decimal subtotal = details.Sum(d => d.Quantity * d.UnitPrice);
            decimal iva = Math.Round(subtotal * 0.12m, 2);
            decimal total = subtotal + iva;

            invoice.Subtotal = subtotal;
            invoice.Tax = iva;
            invoice.Total = total;

            await _invoiceRepository.UpdateAsync(invoice);

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
               
                };
        }
    }
}

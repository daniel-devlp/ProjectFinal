using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Application.Dtos;
using Project.Application.Services;
using System.Security.Claims;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceServices _invoiceService;
        private readonly ILogger<InvoiceController> _logger;

        public InvoiceController(IInvoiceServices invoiceService, ILogger<InvoiceController> logger)
        {
            _invoiceService = invoiceService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene una factura por ID (solo facturas activas)
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "Administrator,user")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var invoice = await _invoiceService.GetByIdAsync(id);
                if (invoice == null) 
                    return NotFound(new { message = "Invoice not found" });
                
                return Ok(invoice);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invoice with ID: {InvoiceId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Obtiene todas las facturas activas con paginación y búsqueda
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Administrator,user")]
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 10, string? searchTerm = null)
        {
            try
            {
                var invoices = await _invoiceService.GetAllAsync(pageNumber, pageSize, searchTerm);
                return Ok(invoices);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invoices");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Obtiene todas las facturas incluyendo eliminadas (solo administradores)
        /// </summary>
        [HttpGet("all-including-deleted")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetAllIncludingDeleted(int pageNumber = 1, int pageSize = 10, string? searchTerm = null)
        {
            try
            {
                var invoices = await _invoiceService.GetAllIncludingDeletedAsync(pageNumber, pageSize, searchTerm);
                return Ok(invoices);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all invoices including deleted");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Obtiene solo las facturas eliminadas (solo administradores)
        /// </summary>
        [HttpGet("deleted")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetDeleted()
        {
            try
            {
                var invoices = await _invoiceService.GetDeletedInvoicesAsync();
                return Ok(invoices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting deleted invoices");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Crea una nueva factura con validaciones completas y número automático
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Administrator,user")]
        public async Task<IActionResult> Create([FromBody] InvoiceCreateDto invoiceDto)
        {
            try
            {
                _logger.LogInformation("Creating invoice for client: {ClientId}", invoiceDto.ClientId);

                if (!ModelState.IsValid)
                {
                    return BadRequest(new { 
                        message = "Validation failed", 
                        errors = ModelState.Where(x => x.Value?.Errors.Count > 0)
                            .ToDictionary(
                                kvp => kvp.Key,
                                kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
                            )
                    });
                }

                // ✅ Obtener usuario actual del token JWT
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized(new { message = "Usuario no autenticado" });
                }

                var createdInvoice = await _invoiceService.AddAsync(invoiceDto, currentUserId);
            
                var response = new
                {
                    success = true,
                    message = "Factura creada exitosamente",
                    invoice = createdInvoice
                };

                return StatusCode(StatusCodes.Status201Created, response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating invoice");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Actualiza una factura existente with validaciones
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Update(int id, [FromBody] InvoiceUpdateDto invoiceDto)
        {
            try
            {
                if (id != invoiceDto.InvoiceId)
                {
                    return BadRequest(new { 
                        message = "ID mismatch", 
                        details = $"URL ID ({id}) does not match body ID ({invoiceDto.InvoiceId})" 
                    });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new { 
                        message = "Validation failed", 
                        errors = ModelState.Where(x => x.Value?.Errors.Count > 0)
                            .ToDictionary(
                                kvp => kvp.Key,
                                kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
                            )
                    });
                }

                var updatedInvoice = await _invoiceService.UpdateAsync(invoiceDto);

                var response = new
                {
                    message = "Factura actualizada exitosamente",
                    invoice = updatedInvoice
                };

                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating invoice with ID: {InvoiceId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Realiza borrado lógico de una factura (cambia IsActive a false)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _invoiceService.DeleteAsync(id);
                return Ok(new { message = "Invoice deleted successfully (soft delete)" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting invoice with ID: {InvoiceId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Restaura una factura eliminada lógicamente (solo administradores)
        /// </summary>
        [HttpPost("{id}/restore")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Restore(int id)
        {
            try
            {
                await _invoiceService.RestoreAsync(id);
                return Ok(new { message = "Invoice restored successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring invoice with ID: {InvoiceId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Finaliza una factura (cambia de borrador a finalizada)
        /// </summary>
        [HttpPost("{id}/finalize")]
        [Authorize(Roles = "Administrator,user")]
        public async Task<IActionResult> Finalize(int id)
        {
            try
            {
                var dto = new InvoiceFinalizeDto { InvoiceId = id };
                var finalizedInvoice = await _invoiceService.FinalizeAsync(dto);
                
                return Ok(new { 
                    message = "Invoice finalized successfully",
                    invoice = finalizedInvoice
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finalizing invoice with ID: {InvoiceId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Cancela una factura con razón
        /// </summary>
        [HttpPost("{id}/cancel")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Cancel(int id, [FromBody] InvoiceCancelDto cancelDto)
        {
            try
            {
                if (id != cancelDto.InvoiceId)
                {
                    return BadRequest(new { message = "ID mismatch" });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new { 
                        message = "Validation failed", 
                        errors = ModelState 
                    });
                }

                var cancelledInvoice = await _invoiceService.CancelAsync(cancelDto);
                
                return Ok(new { 
                    message = "Invoice cancelled successfully",
                    invoice = cancelledInvoice
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling invoice with ID: {InvoiceId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Búsqueda avanzada de facturas
        /// </summary>
        [HttpPost("search")]
        [Authorize(Roles = "Administrator,user")]
        public async Task<IActionResult> Search([FromBody] InvoiceSearchDto searchDto)
        {
            try
            {
                var invoices = await _invoiceService.SearchAsync(searchDto);
                return Ok(invoices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching invoices");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Obtiene estadísticas de facturas (solo administradores)
        /// </summary>
        [HttpGet("statistics")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var activeCount = await _invoiceService.CountAsync();
                var totalCount = await _invoiceService.CountAllAsync();
                var deletedCount = totalCount - activeCount;

                return Ok(new { 
                    activeInvoices = activeCount,
                    deletedInvoices = deletedCount,
                    totalInvoices = totalCount,
                    deletionRate = totalCount > 0 ? Math.Round((double)deletedCount / totalCount * 100, 2) : 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invoice statistics");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Obtiene los detalles de una factura específica
        /// </summary>
        [HttpGet("{id}/details")]
        [Authorize(Roles = "Administrator,user")]
        public async Task<IActionResult> GetInvoiceDetails(int id)
        {
            try
            {
                var details = await _invoiceService.GetInvoiceDetailsAsync(id);
                
                return Ok(details);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invoice details for ID: {InvoiceId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
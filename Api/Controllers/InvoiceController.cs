using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Si usas DbContext directamente
using Project.Application.Dtos;
using Project.Application.Services;
using Project.Domain.Entities;
using Project.Infrastructure.Frameworks.EntityFramework;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceServices _invoiceService;
        private readonly ApplicationDBContext _dbContext; // O tu contexto específico

        public InvoiceController(IInvoiceServices invoiceService, ApplicationDBContext dbContext)
        {
            _invoiceService = invoiceService;
            _dbContext = dbContext;
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Administrator,user")]
        public async Task<IActionResult> GetById(int id)
        {
            var invoice = await _invoiceService.GetByIdAsync(id);
            if (invoice == null) return NotFound();
            return Ok(invoice);
        }

        [HttpGet]
        [Authorize(Roles = "Administrator,user")]
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 10, string? searchTerm = null)
        {
            var invoices = await _invoiceService.GetAllAsync(pageNumber, pageSize, searchTerm);
            return Ok(invoices);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator,user")]
        public async Task<IActionResult> Create([FromBody] InvoiceCreateDto invoiceDto)
        {
            if (invoiceDto?.InvoiceDetails == null || !invoiceDto.InvoiceDetails.Any())
                return BadRequest("La factura debe contener al menos un detalle.");

            // Validación: Verificar existencia del cliente
            var clientExists = await _dbContext.Set<Client>().AnyAsync(c => c.ClientId == invoiceDto.ClientId);
            if (!clientExists)
                return BadRequest("El cliente especificado no existe.");

            try
            {
                await _invoiceService.AddAsync(invoiceDto);
                return StatusCode(201);
            }
            catch (DbUpdateException ex)
            {
                // Manejo específico para errores de base de datos
                return BadRequest("Error de base de datos: " + ex.InnerException?.Message ?? ex.Message);
            }
            catch (System.Exception ex)
            {
                return BadRequest("Error inesperado: " + ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Update(int id, [FromBody] InvoiceUpdateDto invoiceDto)
        {
            if (id != invoiceDto.InvoiceId) return BadRequest("El ID de la URL no coincide con el de la factura.");

            if (invoiceDto?.InvoiceDetails == null || !invoiceDto.InvoiceDetails.Any())
                return BadRequest("La factura debe contener al menos un detalle.");

            try
            {
                await _invoiceService.UpdateAsync(invoiceDto);
                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                return BadRequest("Error de base de datos: " + ex.InnerException?.Message ?? ex.Message);
            }
            catch (System.Exception ex)
            {
                return BadRequest("Error inesperado: " + ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _invoiceService.DeleteAsync(id);
                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                return BadRequest("Error de base de datos: " + ex.InnerException?.Message ?? ex.Message);
            }
            catch (System.Exception ex)
            {
                return BadRequest("Error inesperado: " + ex.Message);
            }
        }

        [HttpGet("{invoiceId}/details")]
        [Authorize(Roles = "Administrator,user")]
        public async Task<IActionResult> GetDetails(int invoiceId)
        {
            var details = await _invoiceService.GetInvoiceDetailsAsync(invoiceId);
            return Ok(details);
        }

        [HttpPost("{invoiceId}/add-product")]
        [Authorize(Roles = "Administrator,user")]
        public async Task<IActionResult> AddProduct(int invoiceId, [FromBody] InvoiceDetailCreateDto detailDto)
        {
            if (detailDto == null || detailDto.ProductId <= 0 || detailDto.Quantity <= 0)
                return BadRequest("Datos de producto inválidos.");

            try
            {
                await _invoiceService.AddProductToInvoiceAsync(invoiceId, detailDto.ProductId, detailDto.Quantity);
                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                return BadRequest("Error de base de datos: " + ex.InnerException?.Message ?? ex.Message);
            }
            catch (System.Exception ex)
            {
                return BadRequest("Error inesperado: " + ex.Message);
            }
        }

        [HttpDelete("{invoiceId}/remove-product/{productId}")]
        [Authorize(Roles = "Administrator,user")]
        public async Task<IActionResult> RemoveProduct(int invoiceId, int productId)
        {
            try
            {
                await _invoiceService.RemoveProductFromInvoiceAsync(invoiceId, productId);
                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                return BadRequest("Error de base de datos: " + ex.InnerException?.Message ?? ex.Message);
            }
            catch (System.Exception ex)
            {
                return BadRequest("Error inesperado: " + ex.Message);
            }
        }
    }
}
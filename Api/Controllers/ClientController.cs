using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Application.Dtos;
using Project.Application.Services;
using Project.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientController : ControllerBase
    {
        private readonly IClientServices _clientService;
        private readonly ILogger<ClientController> _logger;

        public ClientController(IClientServices clientService, ILogger<ClientController> logger)
        {
            _clientService = clientService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene un cliente por ID (solo clientes activos)
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "Administrator,user")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var client = await _clientService.GetByIdAsync(id);
                if (client == null) 
                    return NotFound(new { message = "Client not found" });
     
                return Ok(client);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting client with ID: {ClientId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Obtiene todos los clientes activos con paginación
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Administrator,user")]
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 10, string? searchTerm = null)
        {
            try
            {
                var clients = await _clientService.GetAllAsync(pageNumber, pageSize, searchTerm);
                return Ok(clients);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting clients");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Obtiene todos los clientes incluyendo eliminados (solo administradores)
        /// </summary>
        [HttpGet("all-including-deleted")]
      [Authorize(Roles = "Administrator")]
      public async Task<IActionResult> GetAllIncludingDeleted(int pageNumber = 1, int pageSize = 10, string? searchTerm = null)
        {
    try
        {
      var clients = await _clientService.GetAllIncludingDeletedAsync(pageNumber, pageSize, searchTerm);
           return Ok(clients);
     }
     catch (ArgumentException ex)
 {
            return BadRequest(new { message = ex.Message });
       }
      catch (Exception ex)
            {
       _logger.LogError(ex, "Error getting all clients including deleted");
    return StatusCode(500, new { message = "Internal server error" });
          }
        }

 /// <summary>
 /// Obtiene solo los clientes eliminados (solo administradores)
        /// </summary>
     [HttpGet("deleted")]
  [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetDeleted()
        {
    try
        {
    var clients = await _clientService.GetDeletedClientsAsync();
      return Ok(clients);
     }
      catch (Exception ex)
      {
       _logger.LogError(ex, "Error getting deleted clients");
    return StatusCode(500, new { message = "Internal server error" });
       }
        }

        /// <summary>
 /// Crea un nuevo cliente con validaciones completas
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
     public async Task<IActionResult> Create([FromBody] ClientCreateDto clientDto)
        {
            try
   {
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

      await _clientService.AddAsync(clientDto);
   
    return CreatedAtAction(
      nameof(GetById), 
  new { id = 0 }, // Idealmente retornar el ID real
        new { message = "Client created successfully" }
     );
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
      _logger.LogError(ex, "Error creating client");
  return StatusCode(500, new { message = "Internal server error" });
 }
        }

  /// <summary>
        /// Actualiza un cliente con validaciones completas
  /// </summary>
      [HttpPut("{id}")]
     [Authorize(Roles = "Administrator")]
   public async Task<IActionResult> Update(int id, [FromBody] ClientUpdateDto clientDto)
 {
  try
 {
       if (id != clientDto.ClientId)
    {
  return BadRequest(new { 
message = "ID mismatch", 
         details = $"URL ID ({id}) does not match body ID ({clientDto.ClientId})" 
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

await _clientService.UpdateAsync(clientDto);
  
      return Ok(new { message = "Client updated successfully" });
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
      _logger.LogError(ex, "Error updating client with ID: {ClientId}", id);
      return StatusCode(500, new { message = "Internal server error" });
}
        }

        /// <summary>
      /// Realiza borrado lógico de un cliente (cambia IsActive a false)
        /// </summary>
        [HttpDelete("{id}")]
[Authorize(Roles = "Administrator")]
  public async Task<IActionResult> Delete(int id)
   {
            try
     {
 await _clientService.DeleteAsync(id);
       return Ok(new { message = "Client deleted successfully (soft delete)" });
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
      _logger.LogError(ex, "Error deleting client with ID: {ClientId}", id);
      return StatusCode(500, new { message = "Internal server error" });
    }
        }

        /// <summary>
   /// Restaura un cliente eliminado lógicamente (solo administradores)
  /// </summary>
        [HttpPost("{id}/restore")]
     [Authorize(Roles = "Administrator")]
  public async Task<IActionResult> Restore(int id)
{
    try
 {
      await _clientService.RestoreAsync(id);
       return Ok(new { message = "Client restored successfully" });
            }
         catch (ArgumentException ex)
     {
          return BadRequest(new { message = ex.Message });
   }
    catch (Exception ex)
          {
      _logger.LogError(ex, "Error restoring client with ID: {ClientId}", id);
      return StatusCode(500, new { message = "Internal server error" });
    }
        }

/// <summary>
      /// Obtiene estadísticas de clientes (solo administradores)
 /// </summary>
        [HttpGet("statistics")]
     [Authorize(Roles = "Administrator")]
  public async Task<IActionResult> GetStatistics()
        {
    try
     {
      var activeCount = await _clientService.CountAsync();
      var totalCount = await _clientService.CountAllAsync();
       var deletedCount = totalCount - activeCount;

       return Ok(new { 
        activeClients = activeCount,
     deletedClients = deletedCount,
    totalClients = totalCount,
            deletionRate = totalCount > 0 ? Math.Round((double)deletedCount / totalCount * 100, 2) : 0
       });
     }
      catch (Exception ex)
            {
      _logger.LogError(ex, "Error getting client statistics");
      return StatusCode(500, new { message = "Internal server error" });
  }
     }

  /// <summary>
        /// Busca un cliente por número de identificación
/// </summary>
        [HttpGet("by-identification/{identification}")]
   [Authorize(Roles = "Administrator,user")]
     public async Task<IActionResult> GetByIdentification(string identification)
  {
         try
      {
    var client = await _clientService.GetByIdentificationAsync(identification);
    if (client == null) 
       return NotFound(new { message = "Client not found" });
     
   return Ok(client);
        }
      catch (ArgumentException ex)
         {
    return BadRequest(new { message = ex.Message });
}
   catch (Exception ex)
{
    _logger.LogError(ex, "Error getting client by identification: {Identification}", identification);
      return StatusCode(500, new { message = "Internal server error" });
  }
        }
    }
}

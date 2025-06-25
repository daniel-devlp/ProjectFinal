using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Application.Dtos;
using Project.Application.Services;
using Project.Domain.Entities;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientController : ControllerBase
    {
        private readonly IClientServices _clientService;

        public ClientController(IClientServices clientService)
        {
            _clientService = clientService;
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Administrator,user")]
        public async Task<IActionResult> GetById(int id)
        {
            var client = await _clientService.GetByIdAsync(id);
            if (client == null) return NotFound();
            return Ok(client);
        }

        [HttpGet]
        [Authorize(Roles = "Administrator,user")]

        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 10, string? searchTerm = null)
        {
            var clients = await _clientService.GetAllAsync(pageNumber, pageSize, searchTerm);
            return Ok(clients);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator,user")]
        public async Task<IActionResult> Create([FromBody] ClientCreateDto clientDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            await _clientService.AddAsync(clientDto);
            // You might want to return the new resource location or the DTO
            return Ok();
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator,user")]
        public async Task<IActionResult> Update(int id, [FromBody] ClientUpdateDto clientDto)
        {
            if (id != clientDto.ClientId) return BadRequest();
            if (!ModelState.IsValid) return BadRequest(ModelState);
            await _clientService.UpdateAsync(clientDto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int id)
        {
            await _clientService.DeleteAsync(id);
            return NoContent();
        }
    }
}

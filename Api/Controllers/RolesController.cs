using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Project.Application.Dtos;
using Project.Infrastructure.Frameworks.Identity;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<ApplicationRole> _roleManager;

        public RolesController(RoleManager<ApplicationRole> roleManager)
        {
            _roleManager = roleManager;
        }

        // Admin: List all roles
        [HttpGet]
      //  [Authorize(Roles = "Administrator")]
        public ActionResult<IEnumerable<RolDto>> GetRoles()
        {
            var roles = _roleManager.Roles.Select(r => new RolDto
            {
                Id = r.Id,
                Name = r.Name
            }).ToList();

            return Ok(roles);
        }

        // Admin: Get role by id
        [HttpGet("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<RolDto>> GetRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            var roleDto = new RolDto
            {
                Id = role.Id,
                Name = role.Name
            };

            return Ok(roleDto);
        }

        // Admin: Create role
        [HttpPost]
        //[Authorize(Roles = "Administrator")]
        public async Task<ActionResult> CreateRole([FromBody] RoleCreateDto dto)
        {
            var role = new ApplicationRole
            {
                Name = dto.Name,
                // Puedes inicializar otras propiedades si es necesario, por ejemplo:
                Description = dto.Description,
                IsActive = true
            };
            var result = await _roleManager.CreateAsync(role);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return CreatedAtAction(nameof(GetRole), new { id = role.Id }, null);
        }

        // Admin: Update role
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> UpdateRole(string id, [FromBody] RoleUpdateDto dto)
        {
            if (id != dto.Id) return BadRequest();

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            role.Name = dto.Name;
            var result = await _roleManager.UpdateAsync(role);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return NoContent();
        }

        // Admin: Delete role
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> DeleteRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            var result = await _roleManager.DeleteAsync(role);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return NoContent();
        }
    }
}

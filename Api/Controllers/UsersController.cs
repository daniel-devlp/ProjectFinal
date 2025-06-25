using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Project.Application.Dtos;
using Project.Infrastructure.Frameworks.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Project.Application.Dtos.UserDto;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer")] // Ensure JWT Bearer is used
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Admin: List all users
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public ActionResult<IEnumerable<UserDto>> GetUsers()
        {
            var users = _userManager.Users.ToList();
            var userDtos = users.Select(u => new UserDto
            {
                Id = u.Id,
                UserName = u.UserName,
                IdentificationNumber=u.Identification,
                Email = u.Email,
                EmailConfirmed = u.EmailConfirmed,
                Roles = _userManager.GetRolesAsync(u).Result,
                IsLocked = u.LockoutEnd.HasValue && u.LockoutEnd.Value > System.DateTimeOffset.UtcNow
            }).ToList();

            return Ok(userDtos);
        }

        // Admin: Get user by id
        [HttpGet("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<UserDto>> GetUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            var userDto = new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,
                Roles = await _userManager.GetRolesAsync(user),
                IsLocked = user.LockoutEnd.HasValue && user.LockoutEnd.Value > System.DateTimeOffset.UtcNow
            };
            return Ok(userDto);
        }

        // Admin: Create user (with role)
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> CreateUser([FromBody] UserCreateDto dto)
        {
            var user = new ApplicationUser
            {
                Identification = dto.IdentificationNumber,
                UserName = dto.UserName,
                Email = dto.Email,
                EmailConfirmed = false
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            if (dto.Roles != null && dto.Roles.Any())
            {
                foreach (var role in dto.Roles)
                {
                    if (await _roleManager.RoleExistsAsync(role))
                        await _userManager.AddToRoleAsync(user, role);
                }
            }

            var response = new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.Identification,
                Roles = dto.Roles
            };

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, response);
        }

        // Admin: Update user
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> UpdateUser(string id, [FromBody] UserUpdateDto dto)
        {
            if (id != dto.Id) return BadRequest();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.UserName = dto.UserName;
            user.Email = dto.Email;
            user.EmailConfirmed = dto.EmailConfirmed;
            user.LockoutEnd = dto.IsLocked ? System.DateTimeOffset.MaxValue : (System.DateTimeOffset?)null;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            var userRoles = await _userManager.GetRolesAsync(user);
            var rolesToAdd = dto.Roles.Except(userRoles).ToList();
            var rolesToRemove = userRoles.Except(dto.Roles).ToList();

            if (rolesToAdd.Any())
                await _userManager.AddToRolesAsync(user, rolesToAdd);
            if (rolesToRemove.Any())
                await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

            return NoContent();
        }

        // Admin: Delete user
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return NoContent();
        }

        // Admin: Unlock user
        [HttpPost("{id}/unlock")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> UnlockUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.LockoutEnd = null;
            await _userManager.UpdateAsync(user);
            await _userManager.ResetAccessFailedCountAsync(user);
            return NoContent();
        }

        // Normal user: Get own profile
        [HttpGet("me")]
        [Authorize(Roles = "Administrator,user")]
        public async Task<ActionResult<UserDto>> GetMyProfile()
        {
            string userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var userDto = new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                IdentificationNumber = user.Identification,
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,
                Roles = await _userManager.GetRolesAsync(user),
                IsLocked = user.LockoutEnd.HasValue && user.LockoutEnd.Value > System.DateTimeOffset.UtcNow
            };
            return Ok(userDto);
        }
    }
}
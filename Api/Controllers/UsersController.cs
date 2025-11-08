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
    //[Authorize(AuthenticationSchemes = "Bearer")] // Ensure JWT Bearer is used
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly PasswordHistoryService _passwordHistoryService;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            PasswordHistoryService passwordHistoryService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _passwordHistoryService = passwordHistoryService;
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
                IdentificationNumber = u.Identification,
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
       // [Authorize(Roles = "Administrator")]
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

            // Agregar la contraseña inicial al historial
            await _passwordHistoryService.AddPasswordToHistoryAsync(user.Id, user.PasswordHash);

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

        // Update user (Admin or own profile)
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator,user")]
        public async Task<ActionResult> UpdateUser(string id, [FromBody] UserUpdateWithPasswordDto dto)
        {
            if (id != dto.Id) return BadRequest("ID mismatch");

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound("User not found");

            // Get current user info
            string currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            bool isAdmin = User.IsInRole("Administrator");
            bool isOwner = currentUserId == id;

            // Authorization check: Admin can update any user, regular users can only update themselves
            if (!isAdmin && !isOwner)
            {
                return Forbid("No tienes permisos para actualizar este usuario.");
            }

            // Update basic user properties (Admin or own profile)
            if (isAdmin || isOwner)
            {
                user.UserName = dto.UserName;
                user.Email = dto.Email;

                // Only admin can change email confirmation status
                if (isAdmin)
                {
                    user.EmailConfirmed = dto.EmailConfirmed;
                }
            }

            // Handle password change
            if (!string.IsNullOrEmpty(dto.NewPassword))
            {
                // For regular users, current password is required
                if (!isAdmin && string.IsNullOrEmpty(dto.CurrentPassword))
                {
                    return BadRequest(new PasswordValidationErrorDto
                    {
                        ErrorCode = "CURRENT_PASSWORD_REQUIRED",
                        Message = "La contraseña actual es requerida.",
                        Errors = new List<string> { "Debes proporcionar tu contraseña actual para cambiarla." }
                    });
                }

                // Verify current password for non-admin users
                if (!isAdmin)
                {
                    var passwordValid = await _userManager.CheckPasswordAsync(user, dto.CurrentPassword);
                    if (!passwordValid)
                    {
                        return BadRequest(new PasswordValidationErrorDto
                        {
                            ErrorCode = "INVALID_CURRENT_PASSWORD",
                            Message = "La contraseña actual es incorrecta.",
                            Errors = new List<string> { "La contraseña actual proporcionada no es válida." }
                        });
                    }
                }

                // Check if new password was previously used
                var isPasswordReused = await _passwordHistoryService.IsPasswordReusedAsync(user.Id, dto.NewPassword);
                if (isPasswordReused)
                {
                    return BadRequest(new PasswordValidationErrorDto
                    {
                        ErrorCode = "PASSWORD_PREVIOUSLY_USED",
                        Message = "No puedes reutilizar una de tus contraseñas anteriores.",
                        Errors = new List<string> { "La contraseña ingresada ya ha sido utilizada anteriormente." }
                    });
                }

                // Change password
                IdentityResult passwordResult;
                if (isAdmin)
                {
                    // Admin can reset password without current password
                    var removePasswordResult = await _userManager.RemovePasswordAsync(user);
                    if (!removePasswordResult.Succeeded)
                        return BadRequest(removePasswordResult.Errors);

                    passwordResult = await _userManager.AddPasswordAsync(user, dto.NewPassword);
                }
                else
                {
                    // Regular user must provide current password
                    passwordResult = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
                }

                if (!passwordResult.Succeeded)
                {
                    return BadRequest(new PasswordValidationErrorDto
                    {
                        ErrorCode = "PASSWORD_CHANGE_FAILED",
                        Message = "Error al cambiar la contraseña.",
                        Errors = passwordResult.Errors.Select(e => e.Description).ToList()
                    });
                }

                // Add new password to history
                await _passwordHistoryService.AddPasswordToHistoryAsync(user.Id, user.PasswordHash);
            }

            // Admin-only operations
            if (isAdmin)
            {
                // Update lockout status
                user.LockoutEnd = dto.IsLocked ? System.DateTimeOffset.MaxValue : (System.DateTimeOffset?)null;

                // Update roles
                if (dto.Roles != null)
                {
                    var userRoles = await _userManager.GetRolesAsync(user);
                    var rolesToAdd = dto.Roles.Except(userRoles).ToList();
                    var rolesToRemove = userRoles.Except(dto.Roles).ToList();

                    if (rolesToAdd.Any())
                    {
                        var addRolesResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                        if (!addRolesResult.Succeeded)
                            return BadRequest(addRolesResult.Errors);
                    }

                    if (rolesToRemove.Any())
                    {
                        var removeRolesResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                        if (!removeRolesResult.Succeeded)
                            return BadRequest(removeRolesResult.Errors);
                    }
                }
            }

            // Update user
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "Usuario actualizado exitosamente." });
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
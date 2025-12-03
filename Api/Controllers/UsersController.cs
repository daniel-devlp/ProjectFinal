using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Project.Application.Dtos;
using Project.Application.Services;
using Project.Infrastructure.Frameworks.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Project.Application.Dtos.UserDto;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly PasswordHistoryService _passwordHistoryService;
        private readonly IRoleValidationService _roleValidationService;
        private readonly IImageService _imageService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            PasswordHistoryService passwordHistoryService,
            IRoleValidationService roleValidationService,
            IImageService imageService,
            ILogger<UsersController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _passwordHistoryService = passwordHistoryService;
            _roleValidationService = roleValidationService;
            _imageService = imageService;
            _logger = logger;
        }

        /// <summary>
        /// Lista todos los usuarios (Solo administradores)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            try
            {
                var users = _userManager.Users.ToList();
                var userDtos = new List<UserDto>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    userDtos.Add(new UserDto
                    {
                        Id = user.Id,
                        UserName = user.UserName ?? string.Empty,
                        IdentificationNumber = user.Identification, // ✅ Solo número de identificación
                        Email = user.Email ?? string.Empty,
                        EmailConfirmed = user.EmailConfirmed,
                        ProfileImageUri = user.ProfileImageUri,
                        Roles = roles,
                        IsLocked = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow,
                        CreatedAt = user.CreatedAt,
                        UpdatedAt = user.UpdatedAt
                    });
                }

                return Ok(userDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Obtiene un usuario por ID (Solo administradores)
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<UserDto>> GetUser(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null) return NotFound(new { message = "User not found" });

                var roles = await _userManager.GetRolesAsync(user);
                var userDto = new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    IdentificationNumber = user.Identification, // ✅ Solo número de identificación
                    Email = user.Email ?? string.Empty,
                    EmailConfirmed = user.EmailConfirmed,
                    ProfileImageUri = user.ProfileImageUri,
                    Roles = roles,
                    IsLocked = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt
                };

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user with ID: {UserId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Crea un nuevo usuario (Solo administradores)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> CreateUser([FromForm] UserCreateDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        message = "Validation failed",
                        errors = ModelState
                    });
                }
            
                // Validar que los roles existen
                if (dto.Roles != null && dto.Roles.Any())
                {
                    var invalidRoles = await _roleValidationService.GetInvalidRolesAsync(dto.Roles);
                    if (invalidRoles.Any())
                    {
                        return BadRequest(new
                        {
                            message = "Invalid roles provided",
                            invalidRoles = invalidRoles
                        });
                    }
                }

                // Verificar si el email ya existe
                var existingUserByEmail = await _userManager.FindByEmailAsync(dto.Email);
                if (existingUserByEmail != null)
                {
                    return BadRequest(new { message = "Email already exists" });
                }

                // Verificar si el nombre de usuario ya existe
                var existingUserByUsername = await _userManager.FindByNameAsync(dto.UserName);
                if (existingUserByUsername != null)
                {
                    return BadRequest(new { message = "Username already exists" });
                }

                // Subir imagen de perfil si se proporciona
                string? profileImageUrl = null;
                string? publicId = null;

                if (dto.ProfileImage != null)
                {
                    var imageResult = await _imageService.UploadImageAsync(dto.ProfileImage, "profiles");

                    if (!imageResult.Success)
                    {
                        return BadRequest(new
                        {
                            message = "Failed to upload profile image",
                            error = imageResult.ErrorMessage
                        });
                    }

                    profileImageUrl = imageResult.SecureUrl;
                    publicId = imageResult.PublicId;
                }

                // Crear usuario
                var user = new ApplicationUser
                {
                    Identification = dto.IdentificationNumber, // ✅ Solo número de identificación
                    UserName = dto.UserName,
                    Email = dto.Email,
                    EmailConfirmed = false,
                    ProfileImageUri = profileImageUrl ?? string.Empty,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, dto.Password);
                if (!result.Succeeded)
                {
                    // Si falla la creación y hay imagen, eliminarla
                    if (!string.IsNullOrEmpty(publicId))
                    {
                        try
                        {
                            await _imageService.DeleteImageAsync(publicId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to cleanup profile image after user creation failure");
                        }
                    }

                    return BadRequest(new
                    {
                        message = "User creation failed",
                        errors = result.Errors
                    });
                }

                // Agregar contraseña al historial
                await _passwordHistoryService.AddPasswordToHistoryAsync(user.Id, user.PasswordHash!);

                // Asignar rol por defecto "user" automáticamente
                var defaultRoles = new List<string> { "user" };
              
                // Si se proporcionan roles adicionales, validarlos y agregarlos
                if (dto.Roles != null && dto.Roles.Any())
                {
                    var invalidRoles = await _roleValidationService.GetInvalidRolesAsync(dto.Roles);
                    if (invalidRoles.Any())
                    {
                        return BadRequest(new
                        {
                            message = "Invalid roles provided",
                            invalidRoles = invalidRoles
                        });
                    }

                    // Combinar rol por defecto con roles adicionales (evitando duplicados)
                    defaultRoles = defaultRoles.Union(dto.Roles).ToList();
                }

                // Asignar todos los roles
                var addRolesResult = await _userManager.AddToRolesAsync(user, defaultRoles);
                if (!addRolesResult.Succeeded)
                {
                    _logger.LogWarning("Failed to add roles to user {UserId}: {Errors}",
                        user.Id, string.Join(", ", addRolesResult.Errors.Select(e => e.Description)));
                }

                var response = new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.Identification, 
                    user.ProfileImageUri,
                    Roles = defaultRoles
                };

                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Actualiza un usuario existente
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator,user")]
        public async Task<ActionResult> UpdateUser(string id, [FromForm] UserUpdateDto dto)
        {
            try
            {
                if (id != dto.Id)
                {
                    return BadRequest(new { message = "ID mismatch" });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        message = "Validation failed",
                        errors = ModelState
                    });
                }

                var user = await _userManager.FindByIdAsync(id);
                if (user == null) return NotFound(new { message = "User not found" });

                // Verificar permisos
                string? currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                bool isAdmin = User.IsInRole("Administrator");
                bool isOwner = currentUserId == id;

                if (!isAdmin && !isOwner)
                {
                    return Forbid("No tienes permisos para actualizar este usuario.");
                }

                // Validar roles si se proporcionan (solo admin)
                if (isAdmin && dto.Roles != null && dto.Roles.Any())
                {
                    var invalidRoles = await _roleValidationService.GetInvalidRolesAsync(dto.Roles);
                    if (invalidRoles.Any())
                    {
                        return BadRequest(new
                        {
                            message = "Invalid roles provided",
                            invalidRoles = invalidRoles
                        });
                    }
                }

                // Verificar email único
                var existingUserByEmail = await _userManager.FindByEmailAsync(dto.Email);
                if (existingUserByEmail != null && existingUserByEmail.Id != id)
                {
                    return BadRequest(new { message = "Email already exists" });
                }

                // Verificar username único
                var existingUserByUsername = await _userManager.FindByNameAsync(dto.UserName);
                if (existingUserByUsername != null && existingUserByUsername.Id != id)
                {
                    return BadRequest(new { message = "Username already exists" });
                }

                // Manejar imagen de perfil
                string? newImageUrl = user.ProfileImageUri;
                string? publicId = null;

                if (dto.ProfileImage != null)
                {
                    var imageResult = await _imageService.UploadImageAsync(dto.ProfileImage, "profiles");

                    if (!imageResult.Success)
                    {
                        return BadRequest(new
                        {
                            message = "Failed to upload profile image",
                            error = imageResult.ErrorMessage
                        });
                    }

                    newImageUrl = imageResult.SecureUrl;
                    publicId = imageResult.PublicId;

                    // Eliminar imagen anterior si existe
                    if (!string.IsNullOrWhiteSpace(user.ProfileImageUri))
                    {
                        try
                        {
                            await _imageService.DeleteImageByUrlAsync(user.ProfileImageUri);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to delete old profile image");
                        }
                    }
                }

                // Actualizar propiedades básicas
                if (isAdmin || isOwner)
                {
                    user.Identification = dto.IdentificationNumber; // ✅ Solo número de identificación
                    user.UserName = dto.UserName;
                    user.Email = dto.Email;
                    user.ProfileImageUri = newImageUrl ?? string.Empty;
                    user.UpdatedAt = DateTime.UtcNow;

                    if (isAdmin)
                    {
                        user.EmailConfirmed = dto.EmailConfirmed;
                        user.LockoutEnd = dto.IsLocked ? DateTimeOffset.MaxValue : null;
                    }
                }

                // Actualizar roles (solo admin)
                if (isAdmin && dto.Roles != null)
                {
                    var userRoles = await _userManager.GetRolesAsync(user);
                    var rolesToAdd = dto.Roles.Except(userRoles).ToList();
                    var rolesToRemove = userRoles.Except(dto.Roles).ToList();

                    if (rolesToAdd.Any())
                    {
                        await _userManager.AddToRolesAsync(user, rolesToAdd);
                    }

                    if (rolesToRemove.Any())
                    {
                        await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                    }
                }

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    return BadRequest(new
                    {
                        message = "User update failed",
                        errors = result.Errors
                    });
                }

                return Ok(new { message = "Usuario actualizado exitosamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user with ID: {UserId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Elimina un usuario (Solo administradores)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> DeleteUser(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null) return NotFound(new { message = "User not found" });

                // Eliminar imagen de perfil si existe
                if (!string.IsNullOrWhiteSpace(user.ProfileImageUri))
                {
                    try
                    {
                        await _imageService.DeleteImageByUrlAsync(user.ProfileImageUri);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete user profile image");
                    }
                }

                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    return BadRequest(new
                    {
                        message = "User deletion failed",
                        errors = result.Errors
                    });
                }

                return Ok(new { message = "User deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user with ID: {UserId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Desbloquea un usuario (Solo administradores)
        /// </summary>
        [HttpPost("{id}/unlock")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> UnlockUser(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null) return NotFound(new { message = "User not found" });

                user.LockoutEnd = null;
                user.IsBlocked = false;
                user.FailedLoginAttempts = 0;
                user.UpdatedAt = DateTime.UtcNow;

                await _userManager.UpdateAsync(user);
                await _userManager.ResetAccessFailedCountAsync(user);

                return Ok(new { message = "User unlocked successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unlocking user with ID: {UserId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Obtiene el perfil del usuario autenticado
        /// </summary>
        [HttpGet("me")]
        [Authorize(Roles = "Administrator,user")]
        public async Task<ActionResult<UserDto>> GetMyProfile()
        {
            try
            {
                string? userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null) return NotFound(new { message = "User not found" });

                var roles = await _userManager.GetRolesAsync(user);
                var userDto = new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    IdentificationNumber = user.Identification, // ✅ Solo número de identificación
                    Email = user.Email ?? string.Empty,
                    EmailConfirmed = user.EmailConfirmed,
                    ProfileImageUri = user.ProfileImageUri,
                    Roles = roles,
                    IsLocked = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt
                };

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Elimina solo la foto de perfil de un usuario
        /// </summary>
        [HttpDelete("{id}/profile-image")]
        [Authorize(Roles = "Administrator,user")]
        public async Task<ActionResult> DeleteUserProfileImage(string id)
        {
            try
            {
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                bool isAdmin = User.IsInRole("Administrator");
                bool isOwner = currentUserId == id;

                if (!isAdmin && !isOwner)
                {
                    return Forbid("No tienes permisos para eliminar esta imagen.");
                }

                var user = await _userManager.FindByIdAsync(id);
                if (user == null) return NotFound(new { message = "User not found" });

                if (string.IsNullOrWhiteSpace(user.ProfileImageUri))
                {
                    return BadRequest(new { message = "User has no profile image to delete" });
                }

                var result = await _imageService.DeleteImageByUrlAsync(user.ProfileImageUri);
                if (result)
                {
                    user.ProfileImageUri = string.Empty;
                    user.UpdatedAt = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);

                    return Ok(new { message = "Profile image deleted successfully" });
                }
                else
                {
                    return NotFound(new { message = "Image not found or could not be deleted" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting profile image for user ID: {UserId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
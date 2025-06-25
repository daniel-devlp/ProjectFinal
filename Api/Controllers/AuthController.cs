using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Project.Application.Dtos;
using Project.Infrastructure.Frameworks.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Api.Controllers
{

        [ApiController]
        [Route("api/[controller]")]
        public class AuthController : ControllerBase
        {
            private readonly SignInManager<ApplicationUser> _signInManager;
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly IConfiguration _configuration;

            public AuthController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IConfiguration configuration)
            {
                _signInManager = signInManager;
                _userManager = userManager;
                _configuration = configuration;
            }

            [HttpPost("login")]
            public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
            {
                var user = await _userManager.FindByEmailAsync(loginDto.Email);
                if (user == null)
                    return Unauthorized(new
                    {
                        errorCode = "USER_NOT_FOUND",
                        message = "Invalid credentials."
                    });

                var result = await _signInManager.PasswordSignInAsync(
                    user.UserName, loginDto.Password, false, lockoutOnFailure: true
                );
            if (result.IsLockedOut)
            {
                user.IsBlocked = true;
                await _userManager.UpdateAsync(user);

                return Unauthorized(new
                {
                    errorCode = "ACCOUNT_LOCKED",
                    message = "La cuenta está bloqueada por demasiados intentos fallidos. Intente más tarde."
                });
            }


            if (!result.Succeeded)
                    return Unauthorized(new
                    {
                        errorCode = "INVALID_CREDENTIALS",
                        message = "Invalid credentials."
                    });

                // Get user roles
                var roles = await _userManager.GetRolesAsync(user);

                // Build claims
                var claims = new[]
                {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
            }
                .Union(roles.Select(role => new Claim(ClaimTypes.Role, role)));

                var jwtSettings = _configuration.GetSection("Jwt");
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: jwtSettings["Issuer"],
                    audience: jwtSettings["Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpireMinutes"])),
                    signingCredentials: creds
                );

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo,
                    user = new
                    {
                        email = user.Email,
                        username = user.UserName,
                        roles = roles
                    }
                });
            }
        }
}

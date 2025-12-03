using Microsoft.AspNetCore.Identity;
using Project.Application.Services;
using Project.Infrastructure.Frameworks.Identity;

namespace Project.Infrastructure.Services
{
    public class RoleValidationService : IRoleValidationService
    {
   private readonly RoleManager<ApplicationRole> _roleManager;

        public RoleValidationService(RoleManager<ApplicationRole> roleManager)
 {
  _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        }

public async Task<bool> ValidateRolesExistAsync(IList<string> roles)
      {
        if (roles == null || !roles.Any()) return true;

        foreach (var role in roles)
    {
  if (!await _roleManager.RoleExistsAsync(role))
   return false;
      }
    return true;
        }

public async Task<List<string>> GetInvalidRolesAsync(IList<string> roles)
{
      var invalidRoles = new List<string>();
    
      if (roles == null || !roles.Any()) return invalidRoles;

            foreach (var role in roles)
        {
  if (!await _roleManager.RoleExistsAsync(role))
      invalidRoles.Add(role);
        }
        
 return invalidRoles;
      }
    }
}
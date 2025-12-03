using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.Application.Services
{
    public interface IRoleValidationService
    {
 Task<bool> ValidateRolesExistAsync(IList<string> roles);
   Task<List<string>> GetInvalidRolesAsync(IList<string> roles);
    }
}
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Project.Infrastructure.Frameworks.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.Infrastructure.Frameworks.Identity
{
    public class CustomPasswordValidator : IPasswordValidator<ApplicationUser>
    {
        private readonly ApplicationDBContext _context;
        private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
        private readonly int _passwordHistoryLimit;

        public CustomPasswordValidator(
            ApplicationDBContext context, 
            IPasswordHasher<ApplicationUser> passwordHasher,
            int passwordHistoryLimit = 5) // Número de contraseñas anteriores a recordar
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _passwordHistoryLimit = passwordHistoryLimit;
        }

        public async Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user, string password)
        {
            var errors = new List<IdentityError>();

            // Verificar si la contraseña ya fue utilizada anteriormente
            var passwordHistories = await _context.UserPasswordHistories
                .Where(ph => ph.UserId == user.Id)
                .OrderByDescending(ph => ph.CreatedAt)
                .Take(_passwordHistoryLimit)
                .ToListAsync();

            foreach (var history in passwordHistories)
            {
                var verificationResult = _passwordHasher.VerifyHashedPassword(user, history.PasswordHash, password);
                if (verificationResult == PasswordVerificationResult.Success)
                {
                    errors.Add(new IdentityError
                    {
                        Code = "PASSWORD_PREVIOUSLY_USED",
                        Description = $"No puedes reutilizar una de tus últimas {_passwordHistoryLimit} contraseñas."
                    });
                    break;
                }
            }

            return errors.Count == 0 ? IdentityResult.Success : IdentityResult.Failed(errors.ToArray());
        }
    }
}
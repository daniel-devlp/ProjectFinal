using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Project.Infrastructure.Frameworks.EntityFramework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Project.Infrastructure.Frameworks.Identity
{
    public class PasswordHistoryService
    {
        private readonly ApplicationDBContext _context;
        private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
        private readonly int _passwordHistoryLimit;

        public PasswordHistoryService(
            ApplicationDBContext context,
            IPasswordHasher<ApplicationUser> passwordHasher,
            int passwordHistoryLimit = 5)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _passwordHistoryLimit = passwordHistoryLimit;
        }

        public async Task<bool> IsPasswordReusedAsync(string userId, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            var passwordHistories = await _context.UserPasswordHistories
                .Where(ph => ph.UserId == userId)
                .OrderByDescending(ph => ph.CreatedAt)
                .Take(_passwordHistoryLimit)
                .ToListAsync();

            foreach (var history in passwordHistories)
            {
                var verificationResult = _passwordHasher.VerifyHashedPassword(user, history.PasswordHash, newPassword);
                if (verificationResult == PasswordVerificationResult.Success)
                {
                    return true;
                }
            }

            return false;
        }

        public async Task AddPasswordToHistoryAsync(string userId, string passwordHash)
        {
            // Agregar nueva contraseña al historial
            var passwordHistory = new UserPasswordHistory
            {
                UserId = userId,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow
            };

            _context.UserPasswordHistories.Add(passwordHistory);

            // Mantener solo las últimas N contraseñas
            var oldPasswords = await _context.UserPasswordHistories
                .Where(ph => ph.UserId == userId)
                .OrderByDescending(ph => ph.CreatedAt)
                .Skip(_passwordHistoryLimit)
                .ToListAsync();

            if (oldPasswords.Any())
            {
                _context.UserPasswordHistories.RemoveRange(oldPasswords);
            }

            await _context.SaveChangesAsync();
        }
    }
}
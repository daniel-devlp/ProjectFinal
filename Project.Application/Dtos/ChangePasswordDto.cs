using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Project.Application.Dtos
{
    public class ChangePasswordDto
    {
        [Required]
        public string CurrentPassword { get; set; }
        
        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; }
        
        [Required]
        [Compare("NewPassword")]
        public string ConfirmNewPassword { get; set; }
    }

    public class UserUpdateWithPasswordDto
    {
        [Required]
        public string Id { get; set; }
        
        public string IdentificationNumber { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        public string UserName { get; set; }
        
        // Password fields (optional - only if changing password)
        public string CurrentPassword { get; set; }
        
        [MinLength(6)]
        public string NewPassword { get; set; }
        
        [Compare("NewPassword", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmNewPassword { get; set; }
        
        // Admin-only fields
        public bool EmailConfirmed { get; set; }
        public IList<string> Roles { get; set; }
        public bool IsLocked { get; set; }
    }
}
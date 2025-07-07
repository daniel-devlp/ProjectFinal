using System;
using System.ComponentModel.DataAnnotations;

namespace Project.Infrastructure.Frameworks.Identity
{
    public class UserPasswordHistory
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; }
        
        [Required]
        public string PasswordHash { get; set; }
        
        [Required]
        public DateTime CreatedAt { get; set; }
        
        // Navegación
        public ApplicationUser User { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Project.Application.Validators;

namespace Project.Application.Dtos
{
    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string IdentificationNumber { get; set; } = string.Empty; // ✅ Solo número de identificación
        public string Email { get; set; } = string.Empty;
        public bool EmailConfirmed { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string ProfileImageUri { get; set; } = string.Empty;
        public IList<string> Roles { get; set; } = new List<string>();
        public bool IsLocked { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class UserCreateDto
    {
        [Required(ErrorMessage = "El número de identificación es requerido")]
        [IdentificationValidation] // ✅ Validación unificada para cédula/pasaporte
        public string IdentificationNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [StringLength(100, ErrorMessage = "El email no puede exceder 100 caracteres")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        [StringLength(50, ErrorMessage = "El nombre de usuario no puede exceder 50 caracteres")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [StrongPassword]
        public string Password { get; set; } = string.Empty;

        public IList<string> Roles { get; set; } = new List<string>();
        
        // Archivo de imagen opcional
        public IFormFile? ProfileImage { get; set; }
    }

    public class UserUpdateDto
    {
        [Required(ErrorMessage = "El ID es requerido")]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "El número de identificación es requerido")]
        [IdentificationValidation] // ✅ Validación unificada para cédula/pasaporte
        public string IdentificationNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [StringLength(100, ErrorMessage = "El email no puede exceder 100 caracteres")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        [StringLength(50, ErrorMessage = "El nombre de usuario no puede exceder 50 caracteres")]
        public string UserName { get; set; } = string.Empty;

        public bool EmailConfirmed { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
        public bool IsLocked { get; set; }
        
        // Archivo de imagen opcional para actualización
        public IFormFile? ProfileImage { get; set; }
        public string? ProfileImageUri { get; set; } // Para mantener la imagen actual
    }
}

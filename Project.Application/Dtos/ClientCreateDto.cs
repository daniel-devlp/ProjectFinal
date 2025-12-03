using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project.Application.Validators;

namespace Project.Application.Dtos
{
public class ClientCreateDto
  {
        [Required(ErrorMessage = "El tipo de identificación es requerido")]
        [ClientIdentificationTypeValidation]
  public string IdentificationType { get; set; } = string.Empty;

 [Required(ErrorMessage = "El número de identificación es requerido")]
      [ClientIdentificationValidation]
   public string IdentificationNumber { get; set; } = string.Empty;

     [Required(ErrorMessage = "El nombre es requerido")]
     [StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 50 caracteres")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El nombre solo puede contener letras y espacios")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es requerido")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 50 caracteres")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El apellido solo puede contener letras y espacios")]
        public string LastName { get; set; } = string.Empty;

 [Required(ErrorMessage = "El teléfono es requerido")]
   [EcuadorianPhone]
     public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es requerido")]
   [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [StringLength(100, ErrorMessage = "El email no puede exceder 100 caracteres")]
        public string Email { get; set; } = string.Empty;

  [Required(ErrorMessage = "La dirección es requerida")]
    [StringLength(200, ErrorMessage = "La dirección no puede exceder 200 caracteres")]
      public string Address { get; set; } = string.Empty;
    }
}

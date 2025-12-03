using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Project.Application.Validators
{
    public class IdentificationValidationAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null) return false;
            
            var identification = value.ToString();
            if (string.IsNullOrWhiteSpace(identification)) return false;
   
            return ValidateCedula(identification) || ValidatePassport(identification);
 }
    
        public override string FormatErrorMessage(string name)
        {
  return "La identificación debe ser una cédula ecuatoriana válida (10 dígitos) o un pasaporte válido (6-12 caracteres alfanuméricos).";
        }
    
        private static bool ValidateCedula(string cedula)
   {
     if (cedula.Length != 10 || !cedula.All(char.IsDigit))
    return false;
      
         // ? Algoritmo de validación de cédula ecuatoriana CORREGIDO
      var digits = cedula.Select(c => int.Parse(c.ToString())).ToArray();
 var provincia = int.Parse(cedula.Substring(0, 2));
       
     // Validar código de provincia (01-24, también 30 para extranjeros)
        if (provincia < 1 || (provincia > 24 && provincia != 30)) return false;
    
 // Validar tercer dígito (debe ser 0-5 para personas naturales)
         if (digits[2] > 5) return false;
            
      // ? Algoritmo de verificación CORREGIDO según estándar ecuatoriano
   int suma = 0;
        
            // Procesar los primeros 9 dígitos
   for (int i = 0; i < 9; i++)
  {
      int digit = digits[i];
      
      // ? Posiciones impares (índice 0,2,4,6,8 - base 0) se multiplican por 2
           if (i % 2 == 0) // Posiciones 1,3,5,7,9 en base 1
     {
 digit *= 2;
   if (digit > 9) digit -= 9; // Si es mayor que 9, restar 9
    }
          suma += digit;
        }
    
            // ? Calcular dígito verificador
    int verificador = (10 - (suma % 10)) % 10;
        return verificador == digits[9];
  }
        
        private static bool ValidatePassport(string passport)
     {
   // ? Validación mejorada de pasaporte ecuatoriano
     if (passport.Length < 6 || passport.Length > 12)
     return false;
      
  // Solo caracteres alfanuméricos (A-Z, 0-9), sin espacios
       return Regex.IsMatch(passport, @"^[A-Z0-9]+$", RegexOptions.IgnoreCase);
        }
    }
    
    public class StrongPasswordAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
  {
  if (value == null) return false;
            
     var password = value.ToString();
  if (string.IsNullOrWhiteSpace(password)) return false;
       
// Al menos 8 caracteres, una mayúscula, un número y un carácter especial
     return password.Length >= 8 &&
  Regex.IsMatch(password, @"[A-Z]") && // Al menos una mayúscula
    Regex.IsMatch(password, @"[0-9]") && // Al menos un número
      Regex.IsMatch(password, @"[!@#$%^&*(),.?""':;¨*´+[{}|<>_-]"); // Al menos un carácter especial
        }
        
      public override string FormatErrorMessage(string name)
  {
     return "La contraseña debe tener al menos 8 caracteres, una mayúscula, un número y un carácter especial.";
        }
    }
}
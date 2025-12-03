using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Project.Application.Validators
{
    public class ClientIdentificationValidationAttribute : ValidationAttribute
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
            return "La identificación debe ser una cédula válida (10 dígitos) o un pasaporte válido (6-20 caracteres alfanuméricos).";
        }
    
        private static bool ValidateCedula(string cedula)
        {
   if (cedula.Length != 10 || !cedula.All(char.IsDigit))
   return false;
    
      // Algoritmo de validación de cédula ecuatoriana
            var digits = cedula.Select(c => int.Parse(c.ToString())).ToArray();
      var provincia = int.Parse(cedula.Substring(0, 2));
        
    // Validar código de provincia (01-24)
    if (provincia < 1 || provincia > 24) return false;
            
 // Validar tercer dígito (debe ser menor a 6 para personas naturales)
      if (digits[2] >= 6) return false;
       
     // Algoritmo de verificación
  int suma = 0;
    for (int i = 0; i < 9; i++)
    {
    int digit = digits[i];
 if (i % 2 == 0) // Posiciones impares (0, 2, 4, 6, 8)
      {
    digit *= 2;
if (digit > 9) digit -= 9;
       }
     suma += digit;
       }
    
            int verificador = (10 - (suma % 10)) % 10;
            return verificador == digits[9];
 }
        
        private static bool ValidatePassport(string passport)
      {
      // Validar formato de pasaporte: 6-20 caracteres alfanuméricos
  if (passport.Length < 6 || passport.Length > 20)
       return false;
       
            return Regex.IsMatch(passport, @"^[A-Za-z0-9]+$");
        }
    }
    
    public class ClientIdentificationTypeValidationAttribute : ValidationAttribute
    {
        private readonly string[] _validTypes = { "CEDULA", "DNI", "PASAPORTE" };
        
        public override bool IsValid(object? value)
        {
    if (value == null) return false;
       
     var type = value.ToString()?.ToUpper();
            return _validTypes.Contains(type);
        }
        
        public override string FormatErrorMessage(string name)
  {
            return "El tipo de identificación debe ser: CEDULA, DNI o PASAPORTE.";
        }
    }
    
    public class EcuadorianPhoneAttribute : ValidationAttribute
    {
  public override bool IsValid(object? value)
        {
      if (value == null) return false;
      
            var phone = value.ToString();
            if (string.IsNullOrWhiteSpace(phone)) return false;
     
        // Validar teléfono ecuatoriano: 10 dígitos que empiecen con 09 (móvil) o códigos de área válidos
       if (phone.Length != 10 || !phone.All(char.IsDigit))
 return false;
             
     // Móviles: 09xxxxxxxx
       if (phone.StartsWith("09"))
    return true;
         
  // Teléfonos fijos: códigos de área válidos
            var validAreaCodes = new[] { "02", "03", "04", "05", "06", "07" };
            return validAreaCodes.Any(code => phone.StartsWith(code));
     }
     
   public override string FormatErrorMessage(string name)
        {
            return "El teléfono debe ser un número ecuatoriano válido (10 dígitos, móvil 09xxxxxxxx o fijo con código de área válido).";
        }
    }
}
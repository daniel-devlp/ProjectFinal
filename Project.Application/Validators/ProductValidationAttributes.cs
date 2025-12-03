using System.ComponentModel.DataAnnotations;

namespace Project.Application.Validators
{
    public class UniqueProductCodeAttribute : ValidationAttribute
    {
        public override string FormatErrorMessage(string name)
        {
       return "El código del producto debe ser único.";
      }
    }
  
    public class PositiveDecimalAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null) return false;
         
       if (value is decimal decimalValue)
          {
   return decimalValue > 0;
}
            
            return false;
        }
  
        public override string FormatErrorMessage(string name)
    {
      return "El precio debe ser un número positivo mayor a 0.";
        }
    }
    
    public class NonNegativeIntegerAttribute : ValidationAttribute
    {
  public override bool IsValid(object? value)
        {
            if (value == null) return false;
   
            if (value is int intValue)
            {
 return intValue >= 0;
    }
         
    return false;
        }
        
        public override string FormatErrorMessage(string name)
   {
            return "El stock debe ser un número entero mayor o igual a 0.";
        }
    }
    
    public class ProductCodeFormatAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
   {
            if (value == null) return false;
       
     var code = value.ToString();
            if (string.IsNullOrWhiteSpace(code)) return false;
    
            // El código debe tener entre 3 y 20 caracteres, solo letras, números y guiones
 if (code.Length < 3 || code.Length > 20) return false;
   
            return System.Text.RegularExpressions.Regex.IsMatch(code, @"^[A-Za-z0-9\-]+$");
      }
     
        public override string FormatErrorMessage(string name)
  {
         return "El código debe tener entre 3 y 20 caracteres, solo letras, números y guiones.";
        }
    }
}
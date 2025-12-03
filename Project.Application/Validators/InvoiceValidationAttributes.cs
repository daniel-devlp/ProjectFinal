using System.ComponentModel.DataAnnotations;

namespace Project.Application.Validators
{
    public class InvoiceValidationAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
        if (value == null) return false;
            
         // Validaciones específicas para facturas se pueden agregar aquí
            return true;
        }
    }

    public class PositiveQuantityAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
  {
 if (value == null) return false;
  
            if (value is int intValue)
   {
     return intValue > 0;
    }
            
     return false;
        }
        
   public override string FormatErrorMessage(string name)
        {
  return "La cantidad debe ser un número entero mayor a 0.";
 }
    }

    public class PositiveUnitPriceAttribute : ValidationAttribute
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
    return "El precio unitario debe ser un número positivo mayor a 0.";
    }
    }

    public class ValidInvoiceDetailsAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
if (value is not IEnumerable<object> details) return false;

      return details.Any(); // Debe tener al menos un detalle
}
        
        public override string FormatErrorMessage(string name)
        {
     return "La factura debe contener al menos un detalle.";
        }
    }
}
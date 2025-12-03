using Project.Domain.Interfaces;

namespace Project.Application.Services
{
    public interface IInvoiceNumberService
    {
   Task<string> GenerateInvoiceNumberAsync();
   Task<bool> IsInvoiceNumberUniqueAsync(string invoiceNumber);
    }

    public class InvoiceNumberService : IInvoiceNumberService
    {
   private readonly IUnitOfWork _unitOfWork;
        
        public InvoiceNumberService(IUnitOfWork unitOfWork)
        {
  _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

    public async Task<string> GenerateInvoiceNumberAsync()
        {
      string invoiceNumber;
      int attempts = 0;
 const int maxAttempts = 10;

     do
            {
     // ? Formato: INV-YYYYMMDD-HHMMSS-XXX (donde XXX es un número aleatorio)
        var timestamp = DateTime.UtcNow;
          var datePart = timestamp.ToString("yyyyMMdd");
      var timePart = timestamp.ToString("HHmmss");
        var randomPart = new Random().Next(100, 999);

   invoiceNumber = $"INV-{datePart}-{timePart}-{randomPart}";
    
    attempts++;
            } 
  while (!await IsInvoiceNumberUniqueAsync(invoiceNumber) && attempts < maxAttempts);

 if (attempts >= maxAttempts)
  {
       throw new InvalidOperationException("Could not generate a unique invoice number after multiple attempts");
  }

  return invoiceNumber;
        }

 public async Task<bool> IsInvoiceNumberUniqueAsync(string invoiceNumber)
    {
     if (string.IsNullOrWhiteSpace(invoiceNumber)) return false;

       // Verificar unicidad incluyendo facturas eliminadas
      return !await _unitOfWork.Invoices.ExistsByNumberAsync(invoiceNumber);
 }
    }
}
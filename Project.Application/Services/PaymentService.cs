// SERVICIO DE PAGOS - PREPARADO PARA IMPLEMENTACIÓN FUTURA
// Descomentar cuando sea necesario implementar sistema de pagos

/*
using Project.Application.Dtos;
using Project.Domain.Entities;
using Project.Domain.Interfaces;

namespace Project.Application.Services
{
   public class PaymentService : IPaymentService
    {
     private readonly IUnitOfWork _unitOfWork;

      public PaymentService(IUnitOfWork unitOfWork)
      {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
   }

      public async Task<Payment> ProcessPaymentAsync(int invoiceId, string paymentMethodId, decimal amount)
        {
      if (invoiceId <= 0)
      throw new ArgumentException("Invoice ID must be greater than zero", nameof(invoiceId));
        
  if (string.IsNullOrWhiteSpace(paymentMethodId))
     throw new ArgumentException("Payment method ID is required", nameof(paymentMethodId));
     
    if (amount <= 0)
        throw new ArgumentException("Amount must be greater than zero", nameof(amount));

  // Verificar que la factura existe
      var invoice = await _unitOfWork.Invoices.GetByIdAsync(invoiceId);
    if (invoice == null)
    throw new InvalidOperationException("Invoice not found");

            // Verificar que el método de pago existe y está activo
      if (!await _unitOfWork.PaymentMethods.IsPaymentMethodActiveAsync(paymentMethodId))
  throw new InvalidOperationException("Payment method not available");

      // Verificar que el monto coincide con el total de la factura
         if (amount != invoice.Total)
           throw new InvalidOperationException("Payment amount does not match invoice total");

  await _unitOfWork.BeginTransactionAsync();
        try
        {
          // Generar transaction ID único
      var transactionId = $"TXN-{DateTime.UtcNow:yyyyMMddHHmmss}-{invoiceId}";

          // Crear el pago
     var payment = new Payment(invoiceId, paymentMethodId, amount, transactionId);

    await _unitOfWork.Payments.AddAsync(payment);
       await _unitOfWork.SaveChangesAsync();

    // Simular procesamiento del pago
         // En una implementación real, aquí se haría la integración con el procesador de pagos
       bool paymentSuccess = await SimulatePaymentProcessingAsync(payment);

       if (paymentSuccess)
  {
         payment.MarkAsCompleted("Payment processed successfully");
       }
  else
        {
          payment.MarkAsFailed("Payment processing failed - insufficient funds");
       }

     _unitOfWork.Payments.Update(payment);
 await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

       return payment;
        }
   catch
     {
 await _unitOfWork.RollbackTransactionAsync();
       throw;
        }
    }

        public async Task<Payment> RefundPaymentAsync(int paymentId, decimal refundAmount)
  {
      if (paymentId <= 0)
         throw new ArgumentException("Payment ID must be greater than zero", nameof(paymentId));
        
  if (refundAmount <= 0)
        throw new ArgumentException("Refund amount must be greater than zero", nameof(refundAmount));

    var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId);
       if (payment == null)
         throw new InvalidOperationException("Payment not found");

    if (payment.Status != PaymentStatus.Completed)
       throw new InvalidOperationException("Only completed payments can be refunded");

  if (refundAmount > payment.Amount)
     throw new InvalidOperationException("Refund amount cannot exceed original payment amount");

       await _unitOfWork.BeginTransactionAsync();
        try
 {
    // En una implementación real, aquí se procesaría el reembolso con el procesador de pagos
  payment.MarkAsRefunded();
      _unitOfWork.Payments.Update(payment);
        await _unitOfWork.SaveChangesAsync();
    await _unitOfWork.CommitTransactionAsync();

       return payment;
        }
catch
        {
   await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

 public async Task<IEnumerable<PaymentMethod>> GetAvailablePaymentMethodsAsync()
        {
     var paymentMethods = await _unitOfWork.PaymentMethods.GetActivePaymentMethodsAsync();
    return paymentMethods;
  }

   public async Task<bool> ValidatePaymentAsync(Payment payment)
     {
    if (payment == null) return false;

      // Validar que la factura existe
    var invoice = await _unitOfWork.Invoices.GetByIdAsync(payment.InvoiceId);
       if (invoice == null) return false;

      // Validar que el método de pago está activo
     var isMethodActive = await _unitOfWork.PaymentMethods.IsPaymentMethodActiveAsync(payment.PaymentMethodId);
  if (!isMethodActive) return false;

        // Validar que el monto es correcto
      if (payment.Amount != invoice.Total) return false;

   return true;
        }

public async Task<Payment> GetPaymentStatusAsync(string transactionId)
        {
      if (string.IsNullOrWhiteSpace(transactionId))
     throw new ArgumentException("Transaction ID is required", nameof(transactionId));

   return await _unitOfWork.Payments.GetByTransactionIdAsync(transactionId);
 }

  public async Task<IEnumerable<Payment>> GetUserPaymentHistoryAsync(string userId)
     {
     if (string.IsNullOrWhiteSpace(userId))
     throw new ArgumentException("User ID is required", nameof(userId));

     return await _unitOfWork.Payments.GetPaymentsByUserAsync(userId);
        }

    // Método privado para simular el procesamiento del pago
        private async Task<bool> SimulatePaymentProcessingAsync(Payment payment)
        {
        await Task.Delay(1000); // Simular tiempo de procesamiento

            // Simular éxito/fallo basado en alguna lógica
            // En este caso, 90% de éxito
      var random = new Random();
   return random.Next(1, 101) <= 90;
        }
    }
}
*/
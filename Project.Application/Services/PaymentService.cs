using Project.Application.Dtos;
using Project.Domain.Entities;
using Project.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Project.Application.Services
{
public class PaymentService : IPaymentService
    {
     private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PaymentService> _logger;

      public PaymentService(IUnitOfWork unitOfWork, ILogger<PaymentService> logger)
   {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
  _logger = logger ?? throw new ArgumentNullException(nameof(logger));
   }

 public async Task<Payment> ProcessPaymentAsync(int invoiceId, string paymentMethodId, decimal amount, string? additionalInfo = null)
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

        // Verificar que no hay un pago exitoso previo
    if (await _unitOfWork.Payments.HasSuccessfulPaymentAsync(invoiceId))
         throw new InvalidOperationException("Invoice has already been paid");

  await _unitOfWork.BeginTransactionAsync();
        try
        {
 // Generar transaction ID único
      var transactionId = $"TXN-{DateTime.UtcNow:yyyyMMddHHmmss}-{invoiceId}-{Guid.NewGuid().ToString()[..8]}";

 // Crear el pago
     var payment = new Payment(invoiceId, paymentMethodId, amount, transactionId);

    await _unitOfWork.Payments.AddAsync(payment);
       await _unitOfWork.SaveChangesAsync();

     _logger.LogInformation("Payment created with transaction ID: {TransactionId}", transactionId);

    // Simular procesamiento del pago para móvil (95% éxito)
         bool paymentSuccess = await SimulatePaymentProcessingAsync(payment, additionalInfo);

       if (paymentSuccess)
  {
      payment.MarkAsCompleted($"Payment processed successfully via {paymentMethodId}");
         _logger.LogInformation("Payment completed successfully: {TransactionId}", transactionId);
       }
  else
        {
       payment.MarkAsFailed("Payment processing failed - insufficient funds or network error");
      _logger.LogWarning("Payment failed: {TransactionId}", transactionId);
 }

     _unitOfWork.Payments.Update(payment);
 await _unitOfWork.SaveChangesAsync();
     await _unitOfWork.CommitTransactionAsync();

       return payment;
        }
   catch (Exception ex)
        {
   _logger.LogError(ex, "Error processing payment for invoice {InvoiceId}", invoiceId);
 await _unitOfWork.RollbackTransactionAsync();
  throw;
  }
    }

       public async Task<Payment> ProcessMobilePaymentAsync(int invoiceId, string paymentMethodId, decimal amount)
        {
  _logger.LogInformation("Processing mobile payment for invoice {InvoiceId} with method {PaymentMethodId}", 
          invoiceId, paymentMethodId);
        
      return await ProcessPaymentAsync(invoiceId, paymentMethodId, amount, "Mobile Payment");
      }

 public async Task<Payment> RefundPaymentAsync(int paymentId, decimal refundAmount, string? reason = null)
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
            _logger.LogInformation("Processing refund for payment {PaymentId}, amount: {RefundAmount}", 
      paymentId, refundAmount);
        
    // En una implementación real, aquí se procesaría el reembolso con el procesador de pagos
  payment.MarkAsRefunded();
      _unitOfWork.Payments.Update(payment);
        await _unitOfWork.SaveChangesAsync();
    await _unitOfWork.CommitTransactionAsync();

    _logger.LogInformation("Refund completed for payment {PaymentId}", paymentId);
 return payment;
        }
catch (Exception ex)
        {
      _logger.LogError(ex, "Error processing refund for payment {PaymentId}", paymentId);
   await _unitOfWork.RollbackTransactionAsync();
 throw;
      }
    }

  public async Task<Payment> CancelPaymentAsync(int paymentId, string reason)
   {
        var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId);
   if (payment == null)
  throw new InvalidOperationException("Payment not found");

            if (payment.Status != PaymentStatus.Pending && payment.Status != PaymentStatus.Processing)
   throw new InvalidOperationException("Only pending or processing payments can be cancelled");

    payment.Status = PaymentStatus.Cancelled;
        payment.FailureReason = reason;
     payment.ProcessedAt = DateTime.UtcNow;

 _unitOfWork.Payments.Update(payment);
  await _unitOfWork.SaveChangesAsync();

    _logger.LogInformation("Payment cancelled: {PaymentId}, Reason: {Reason}", paymentId, reason);
    return payment;
    }

 public async Task<IEnumerable<PaymentMethod>> GetAvailablePaymentMethodsAsync()
        {
  var paymentMethods = await _unitOfWork.PaymentMethods.GetOrderedPaymentMethodsAsync();
        _logger.LogInformation("Retrieved {Count} active payment methods", paymentMethods.Count());
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

     // Validar que no hay pagos exitosos previos
        if (await _unitOfWork.Payments.HasSuccessfulPaymentAsync(payment.InvoiceId)) return false;

   return true;
        }

        public async Task<bool> ValidatePaymentAmountAsync(int invoiceId, decimal amount)
  {
        var invoice = await _unitOfWork.Invoices.GetByIdAsync(invoiceId);
   if (invoice == null) return false;
        
        return Math.Abs(invoice.Total - amount) < 0.01m; // Permitir diferencias mínimas por redondeo
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

    // Método privado para simular el procesamiento del pago (ideal para móvil)
    private async Task<bool> SimulatePaymentProcessingAsync(Payment payment, string? additionalInfo)
        {
    await Task.Delay(Random.Shared.Next(1000, 3000)); // Simular tiempo de procesamiento

     // Obtener método de pago para lógica específica
 var paymentMethod = await _unitOfWork.PaymentMethods.GetByIdAsync(payment.PaymentMethodId);
            
 // Lógica de simulación basada en el tipo de pago
       var successRate = paymentMethod?.Type switch
         {
  PaymentType.Cash => 0.99,           // 99% éxito para efectivo
    PaymentType.CreditCard => 0.95,     // 95% éxito para tarjetas
      PaymentType.DebitCard => 0.96,      // 96% éxito para débito
     PaymentType.BankTransfer => 0.92,   // 92% éxito para transferencias
        PaymentType.MobileMoney => 0.94,    // 94% éxito para dinero móvil
       _ => 0.90         // 90% por defecto
            };

      return Random.Shared.NextDouble() <= successRate;
   }
    }
}
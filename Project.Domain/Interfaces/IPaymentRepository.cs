// INTERFACES PARA MÓDULO DE PAGOS - PREPARADO PARA IMPLEMENTACIÓN FUTURA
// Descomentar cuando sea necesario implementar sistema de pagos

using Project.Domain.Entities;

namespace Project.Domain.Interfaces
{
    public interface IPaymentRepository : IRepository<Payment>
    {
     Task<IEnumerable<Payment>> GetPaymentsByInvoiceAsync(int invoiceId);
        Task<IEnumerable<Payment>> GetPaymentsByUserAsync(string userId);
  Task<IEnumerable<Payment>> GetPaymentsByStatusAsync(PaymentStatus status);
       Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<Payment> GetByTransactionIdAsync(string transactionId);
        Task<decimal> GetTotalPaymentsAsync(DateTime startDate, DateTime endDate);
   Task<IEnumerable<Payment>> GetFailedPaymentsAsync();
        Task<IEnumerable<Payment>> GetPendingPaymentsAsync();
        Task<bool> HasSuccessfulPaymentAsync(int invoiceId);
    }

    public interface IPaymentMethodRepository : IRepository<PaymentMethod>
    {
   Task<IEnumerable<PaymentMethod>> GetActivePaymentMethodsAsync();
     Task<PaymentMethod> GetByIdAsync(string paymentMethodId);
    Task<IEnumerable<PaymentMethod>> GetByTypeAsync(PaymentType type);
        Task<bool> IsPaymentMethodActiveAsync(string paymentMethodId);
  Task<IEnumerable<PaymentMethod>> GetOrderedPaymentMethodsAsync();
    }

    public interface IPaymentService
    {
     Task<Payment> ProcessPaymentAsync(int invoiceId, string paymentMethodId, decimal amount, string? additionalInfo = null);
        Task<Payment> RefundPaymentAsync(int paymentId, decimal refundAmount, string? reason = null);
     Task<IEnumerable<PaymentMethod>> GetAvailablePaymentMethodsAsync();
    Task<bool> ValidatePaymentAsync(Payment payment);
    Task<Payment> GetPaymentStatusAsync(string transactionId);
        Task<IEnumerable<Payment>> GetUserPaymentHistoryAsync(string userId);
        Task<Payment> CancelPaymentAsync(int paymentId, string reason);
        
    // Métodos específicos para móvil
        Task<Payment> ProcessMobilePaymentAsync(int invoiceId, string paymentMethodId, decimal amount);
        Task<bool> ValidatePaymentAmountAsync(int invoiceId, decimal amount);
    }
}
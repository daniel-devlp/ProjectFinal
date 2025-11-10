using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Application.Dtos;
using Project.Application.Services;
using Project.Domain.Entities;
using Project.Domain.Interfaces;
using System.Security.Claims;

namespace Api.Controllers
{
    [ApiController]
   [Route("api/[controller]")]
    [Authorize]
 public class PaymentController : ControllerBase
   {
        private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentController> _logger;

      public PaymentController(IPaymentService paymentService, ILogger<PaymentController> logger)
 {
  _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

/// <summary>
 /// Procesa un pago (ideal para móvil con simulación)
 /// </summary>
 [HttpPost("process")]
  public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentDto paymentDto)
 {
     try
  {
           var userId = GetCurrentUserId();
     _logger.LogInformation("Processing payment for user {UserId}, invoice {InvoiceId}", 
       userId, paymentDto.InvoiceId);

    var payment = await _paymentService.ProcessPaymentAsync(
paymentDto.InvoiceId, 
     paymentDto.PaymentMethodId, 
  paymentDto.Amount,
    paymentDto.AdditionalInfo
  );
       
            var result = new PaymentResultDto
     {
      Success = payment.Status == PaymentStatus.Completed,
   TransactionId = payment.TransactionId,
   Status = payment.Status.ToString(),
       Message = payment.Status == PaymentStatus.Completed 
 ? "Payment processed successfully" 
      : payment.FailureReason ?? "Payment processing failed",
Payment = MapToPaymentDto(payment)
      };

      if (result.Success)
      {
     return Ok(result);
      }
            else
   {
    return BadRequest(result);
         }
    }
       catch (Exception ex)
   {
    _logger.LogError(ex, "Error processing payment");
      return BadRequest(new PaymentResultDto 
  { 
     Success = false, 
       Message = ex.Message,
        ErrorCode = "PAYMENT_ERROR"
        });
 }
       }

        /// <summary>
 /// Procesa un pago móvil específico
      /// </summary>
 [HttpPost("mobile")]
      public async Task<IActionResult> ProcessMobilePayment([FromBody] MobilePaymentDto mobilePaymentDto)
        {
     try
    {
   var userId = GetCurrentUserId();
     _logger.LogInformation("Processing mobile payment for user {UserId}, device {DeviceId}", 
      userId, mobilePaymentDto.DeviceId);

            var payment = await _paymentService.ProcessMobilePaymentAsync(
    mobilePaymentDto.InvoiceId,
    mobilePaymentDto.PaymentMethodId,
   mobilePaymentDto.Amount
 );
       
    var result = new PaymentResultDto
      {
      Success = payment.Status == PaymentStatus.Completed,
   TransactionId = payment.TransactionId,
   Status = payment.Status.ToString(),
 Message = payment.Status == PaymentStatus.Completed 
 ? "Mobile payment processed successfully" 
      : payment.FailureReason ?? "Mobile payment processing failed",
      Payment = MapToPaymentDto(payment)
      };

   return Ok(result);
   }
   catch (Exception ex)
   {
    _logger.LogError(ex, "Error processing mobile payment");
      return BadRequest(new PaymentResultDto 
   { 
        Success = false, 
       Message = ex.Message,
        ErrorCode = "MOBILE_PAYMENT_ERROR"
 });
     }
       }

   /// <summary>
/// Obtiene el estado de un pago por transaction ID
/// </summary>
 [HttpGet("status/{transactionId}")]
   public async Task<IActionResult> GetPaymentStatus(string transactionId)
   {
     try
    {
   var payment = await _paymentService.GetPaymentStatusAsync(transactionId);
   if (payment == null)
   return NotFound(new { message = "Payment not found" });
         
    return Ok(new PaymentResultDto
        {
  Success = payment.Status == PaymentStatus.Completed,
    TransactionId = payment.TransactionId,
      Status = payment.Status.ToString(),
          Payment = MapToPaymentDto(payment)
 });
      }
    catch (Exception ex)
   {
      _logger.LogError(ex, "Error getting payment status for transaction {TransactionId}", transactionId);
    return BadRequest(new { message = ex.Message });
     }
  }

 /// <summary>
/// Obtiene métodos de pago disponibles (ideal para móvil)
/// </summary>
     [HttpGet("methods")]
public async Task<IActionResult> GetPaymentMethods()
    {
 try
     {
   var methods = await _paymentService.GetAvailablePaymentMethodsAsync();
      var methodDtos = methods.Select(MapToPaymentMethodDto).ToList();
        
    return Ok(new { 
       message = "Payment methods retrieved successfully",
 methods = methodDtos 
   });
     }
   catch (Exception ex)
   {
      _logger.LogError(ex, "Error getting payment methods");
    return BadRequest(new { message = ex.Message });
   }
  }

 /// <summary>
    /// Obtiene historial de pagos del usuario
  /// </summary>
    [HttpGet("history")]
 public async Task<IActionResult> GetPaymentHistory()
   {
  try
    {
   var userId = GetCurrentUserId();
      var payments = await _paymentService.GetUserPaymentHistoryAsync(userId);
        var paymentDtos = payments.Select(MapToPaymentDto).ToList();
        
       var historyDto = new PaymentHistoryDto
          {
      Payments = paymentDtos,
   TotalPaid = paymentDtos.Where(p => p.Status == "Completed").Sum(p => p.Amount),
       TotalTransactions = paymentDtos.Count,
    LastPaymentDate = paymentDtos.OrderByDescending(p => p.PaymentDate).FirstOrDefault()?.PaymentDate
        };
        
 return Ok(historyDto);
 }
  catch (Exception ex)
   {
  _logger.LogError(ex, "Error getting payment history for user");
 return BadRequest(new { message = ex.Message });
  }
     }

      /// <summary>
 /// Cancela un pago pendiente
  /// </summary>
      [HttpPost("{paymentId}/cancel")]
 public async Task<IActionResult> CancelPayment(int paymentId, [FromBody] CancelPaymentRequest request)
     {
   try
 {
        var payment = await _paymentService.CancelPaymentAsync(paymentId, request.Reason ?? "Cancelled by user");
     return Ok(new { 
 message = "Payment cancelled successfully",
     payment = MapToPaymentDto(payment)
   });
   }
catch (Exception ex)
  {
      _logger.LogError(ex, "Error cancelling payment {PaymentId}", paymentId);
     return BadRequest(new { message = ex.Message });
 }
     }

  /// <summary>
 /// Procesa un reembolso (solo administradores)
   /// </summary>
      [HttpPost("refund")]
      [Authorize(Roles = "Administrator")]
     public async Task<IActionResult> RefundPayment([FromBody] RefundPaymentDto refundDto)
  {
 try
  {
    var payment = await _paymentService.RefundPaymentAsync(
refundDto.PaymentId, 
     refundDto.RefundAmount,
     refundDto.Reason
    );
  
     return Ok(new { 
        message = "Refund processed successfully",
    payment = MapToPaymentDto(payment)
      });
   }
      catch (Exception ex)
  {
      _logger.LogError(ex, "Error processing refund");
 return BadRequest(new { message = ex.Message });
 }
     }

   /// <summary>
     /// Valida el monto de un pago
  /// </summary>
     [HttpPost("validate-amount")]
     public async Task<IActionResult> ValidatePaymentAmount([FromBody] ValidateAmountRequest request)
        {
 try
      {
        var isValid = await _paymentService.ValidatePaymentAmountAsync(request.InvoiceId, request.Amount);
    return Ok(new { isValid = isValid });
    }
     catch (Exception ex)
      {
 _logger.LogError(ex, "Error validating payment amount");
  return BadRequest(new { message = ex.Message });
     }
       }

 private string GetCurrentUserId()
   {
       var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
     if (string.IsNullOrEmpty(userId))
    throw new UnauthorizedAccessException("User not authenticated");
return userId;
      }

 private static PaymentDto MapToPaymentDto(Payment payment)
 {
  return new PaymentDto
      {
    PaymentId = payment.PaymentId,
      InvoiceId = payment.InvoiceId,
 PaymentMethodId = payment.PaymentMethodId,
  Amount = payment.Amount,
     TransactionId = payment.TransactionId,
 Status = payment.Status.ToString(),
   PaymentDate = payment.PaymentDate,
   ProcessedAt = payment.ProcessedAt,
      ProcessorResponse = payment.ProcessorResponse,
       FailureReason = payment.FailureReason
   };
 }

  private static PaymentMethodDto MapToPaymentMethodDto(PaymentMethod method)
        {
   return new PaymentMethodDto
    {
 PaymentMethodId = method.PaymentMethodId,
        Name = method.Name,
    Description = method.Description,
    IsActive = method.IsActive,
  Type = method.Type.ToString(),
     MinAmount = method.MinAmount,
  MaxAmount = method.MaxAmount,
      ProcessingFee = method.ProcessingFee,
        IconUrl = method.IconUrl,
       DisplayOrder = method.DisplayOrder
      };
        }
    }

    // Request DTOs
    public class CancelPaymentRequest
    {
   public string? Reason { get; set; }
    }

 public class ValidateAmountRequest
    {
    public int InvoiceId { get; set; }
public decimal Amount { get; set; }
    }
}
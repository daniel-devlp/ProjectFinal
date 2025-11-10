// CONTROLADOR DE PAGOS - PREPARADO PARA IMPLEMENTACIÓN FUTURA
// Descomentar cuando sea necesario implementar sistema de pagos

/*
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Application.Dtos;
using Project.Application.Services;
using System.Security.Claims;

namespace Api.Controllers
{
    [ApiController]
   [Route("api/[controller]")]
    [Authorize]
   public class PaymentController : ControllerBase
   {
        private readonly IPaymentService _paymentService;

      public PaymentController(IPaymentService paymentService)
        {
  _paymentService = paymentService;
        }

/// <summary>
  /// Procesa un pago
   /// </summary>
 [HttpPost("process")]
      public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentDto paymentDto)
 {
     try
  {
       var result = await _paymentService.ProcessPaymentAsync(
paymentDto.InvoiceId, 
     paymentDto.PaymentMethodId, 
  paymentDto.Amount
  );
       
    return Ok(result);
 }
       catch (Exception ex)
 {
return BadRequest(new { message = ex.Message });
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
   return NotFound();
         
    return Ok(payment);
      }
    catch (Exception ex)
   {
      return BadRequest(new { message = ex.Message });
  }
  }

 /// <summary>
/// Obtiene métodos de pago disponibles
/// </summary>
     [HttpGet("methods")]
public async Task<IActionResult> GetPaymentMethods()
    {
 try
     {
   var methods = await _paymentService.GetAvailablePaymentMethodsAsync();
    return Ok(methods);
 }
   catch (Exception ex)
   {
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
       return Ok(payments);
 }
  catch (Exception ex)
   {
  return BadRequest(new { message = ex.Message });
  }
      }

      /// <summary>
 /// Procesa un reembolso
   /// </summary>
      [HttpPost("refund")]
      [Authorize(Roles = "Administrator")]
     public async Task<IActionResult> RefundPayment([FromBody] RefundPaymentDto refundDto)
  {
 try
  {
    var payment = await _paymentService.RefundPaymentAsync(
refundDto.PaymentId, 
     refundDto.RefundAmount
    );
  
   return Ok(payment);
   }
      catch (Exception ex)
  {
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
 }

// DTOs adicionales para pagos
   public class RefundPaymentDto
   {
  public int PaymentId { get; set; }
     public decimal RefundAmount { get; set; }
      public string? Reason { get; set; }
   }
}
*/
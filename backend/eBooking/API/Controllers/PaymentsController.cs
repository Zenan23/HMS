using Contracts.DTOs;
using Contracts.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Persistence.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentsController : BaseController<PaymentDto, CreatePaymentDto, UpdatePaymentDto>
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(
            IPaymentService paymentService,
            ILogger<PaymentsController> logger)
            : base(paymentService, logger)
        {
            _paymentService = paymentService;
        }

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="createPaymentDto">Payment details</param>
        /// <returns>Processed payment</returns>
        [HttpPost("process")]
        public async Task<ActionResult<ApiResponse<PaymentDto>>> ProcessPayment([FromBody] CreatePaymentDto createPaymentDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                  .Select(e => e.ErrorMessage);
                    return BadRequest(ApiResponse<PaymentDto>.ErrorResult("Validation failed.", errors));
                }

                // Get user agent and IP address from request
                var userAgent = Request.Headers["User-Agent"].ToString();
                var ipAddress = GetClientIpAddress();

                var payment = await _paymentService.ProcessPaymentAsync(createPaymentDto, userAgent, ipAddress);
                return Ok(ApiResponse<PaymentDto>.SuccessResult(payment, "Payment processed successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment");
                return StatusCode(500, ApiResponse<PaymentDto>.ErrorResult("An error occurred while processing the payment."));
            }
        }

        /// <summary>
        /// Refund a payment
        /// </summary>
        /// <param name="id">Payment ID</param>
        /// <param name="request">Refund request</param>
        /// <returns>Refund result</returns>
        [HttpPost("{id}/refund")]
        public async Task<ActionResult<ApiResponse<bool>>> RefundPayment([FromRoute] int id, [FromBody] RefundRequest request)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(ApiResponse<bool>.ErrorResult("Invalid payment ID."));
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                  .Select(e => e.ErrorMessage);
                    return BadRequest(ApiResponse<bool>.ErrorResult("Validation failed.", errors));
                }

                var result = await _paymentService.RefundPaymentAsync(id, request.Amount, request.Reason, request.InitiatedByUserId);

                if (!result)
                {
                    return BadRequest(ApiResponse<bool>.ErrorResult("Payment cannot be refunded. Check payment status and refund amount."));
                }

                return Ok(ApiResponse<bool>.SuccessResult(result, "Payment refunded successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refunding payment with ID: {PaymentId}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResult("An error occurred while processing the refund."));
            }
        }

        /// <summary>
        /// Cancel a payment
        /// </summary>
        /// <param name="id">Payment ID</param>
        /// <param name="request">Cancellation request</param>
        /// <returns>Cancellation result</returns>
        [HttpPost("{id}/cancel")]
        public async Task<ActionResult<ApiResponse<bool>>> CancelPayment([FromRoute] int id, [FromBody] CancelPaymentRequest request)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(ApiResponse<bool>.ErrorResult("Invalid payment ID."));
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                  .Select(e => e.ErrorMessage);
                    return BadRequest(ApiResponse<bool>.ErrorResult("Validation failed.", errors));
                }

                var result = await _paymentService.CancelPaymentAsync(id, request.Reason, request.InitiatedByUserId);

                if (!result)
                {
                    return BadRequest(ApiResponse<bool>.ErrorResult("Payment cannot be cancelled. Only pending or processing payments can be cancelled."));
                }

                return Ok(ApiResponse<bool>.SuccessResult(result, "Payment cancelled successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling payment with ID: {PaymentId}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResult("An error occurred while cancelling the payment."));
            }
        }

        /// <summary>
        /// Get payments by user ID
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of payments</returns>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<PaymentDto>>>> GetByUserId([FromRoute] int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest(ApiResponse<IEnumerable<PaymentDto>>.ErrorResult("Invalid user ID."));
                }

                var payments = await _paymentService.GetByUserIdAsync(userId);
                return Ok(ApiResponse<IEnumerable<PaymentDto>>.SuccessResult(payments, "Payments retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payments for user ID: {UserId}", userId);
                return StatusCode(500, ApiResponse<IEnumerable<PaymentDto>>.ErrorResult("An error occurred while retrieving payments."));
            }
        }

        /// <summary>
        /// Get payments by booking ID
        /// </summary>
        /// <param name="bookingId">Booking ID</param>
        /// <returns>List of payments</returns>
        [HttpGet("booking/{bookingId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<PaymentDto>>>> GetByBookingId([FromRoute] int bookingId)
        {
            try
            {
                if (bookingId <= 0)
                {
                    return BadRequest(ApiResponse<IEnumerable<PaymentDto>>.ErrorResult("Invalid booking ID."));
                }

                var payments = await _paymentService.GetByBookingIdAsync(bookingId);
                return Ok(ApiResponse<IEnumerable<PaymentDto>>.SuccessResult(payments, "Payments retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payments for booking ID: {BookingId}", bookingId);
                return StatusCode(500, ApiResponse<IEnumerable<PaymentDto>>.ErrorResult("An error occurred while retrieving payments."));
            }
        }

        /// <summary>
        /// Get payments by status
        /// </summary>
        /// <param name="status">Payment status</param>
        /// <returns>List of payments</returns>
        [HttpGet("status/{status}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<PaymentDto>>>> GetByStatus([FromRoute] PaymentStatus status)
        {
            try
            {
                var payments = await _paymentService.GetByStatusAsync(status);
                return Ok(ApiResponse<IEnumerable<PaymentDto>>.SuccessResult(payments, "Payments retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payments with status: {Status}", status);
                return StatusCode(500, ApiResponse<IEnumerable<PaymentDto>>.ErrorResult("An error occurred while retrieving payments."));
            }
        }

        /// <summary>
        /// Get payments by payment method
        /// </summary>
        /// <param name="method">Payment method</param>
        /// <returns>List of payments</returns>
        [HttpGet("method/{method}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<PaymentDto>>>> GetByPaymentMethod([FromRoute] PaymentMethod method)
        {
            try
            {
                var payments = await _paymentService.GetByPaymentMethodAsync(method);
                return Ok(ApiResponse<IEnumerable<PaymentDto>>.SuccessResult(payments, "Payments retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payments with method: {Method}", method);
                return StatusCode(500, ApiResponse<IEnumerable<PaymentDto>>.ErrorResult("An error occurred while retrieving payments."));
            }
        }

        /// <summary>
        /// Get payment audit logs
        /// </summary>
        /// <param name="id">Payment ID</param>
        /// <returns>List of audit logs</returns>
        [HttpGet("{id}/audit-logs")]
        public async Task<ActionResult<ApiResponse<IEnumerable<PaymentAuditLogDto>>>> GetPaymentAuditLogs([FromRoute] int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(ApiResponse<IEnumerable<PaymentAuditLogDto>>.ErrorResult("Invalid payment ID."));
                }

                var auditLogs = await _paymentService.GetPaymentAuditLogsAsync(id);
                return Ok(ApiResponse<IEnumerable<PaymentAuditLogDto>>.SuccessResult(auditLogs, "Audit logs retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit logs for payment ID: {PaymentId}", id);
                return StatusCode(500, ApiResponse<IEnumerable<PaymentAuditLogDto>>.ErrorResult("An error occurred while retrieving audit logs."));
            }
        }

        /// <summary>
        /// Get payment statistics
        /// </summary>
        /// <param name="fromDate">From date (optional)</param>
        /// <param name="toDate">To date (optional)</param>
        /// <returns>Payment statistics</returns>
        [HttpGet("statistics")]
        public async Task<ActionResult<ApiResponse<Contracts.DTOs.PaymentStatistics>>> GetPaymentStatistics(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var statistics = await _paymentService.GetPaymentStatisticsAsync(fromDate, toDate);
                return Ok(ApiResponse<Contracts.DTOs.PaymentStatistics>.SuccessResult(statistics, "Payment statistics retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment statistics");
                return StatusCode(500, ApiResponse<Contracts.DTOs.PaymentStatistics>.ErrorResult("An error occurred while retrieving payment statistics."));
            }
        }

        private string GetClientIpAddress()
        {
            var ipAddress = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = Request.Headers["X-Real-IP"].FirstOrDefault();
            }
            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString();
            }
            return ipAddress ?? "Unknown";
        }
    }

    public class RefundRequest
    {
        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Reason is required")]
        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
        public string Reason { get; set; } = string.Empty;

        public int? InitiatedByUserId { get; set; }
    }

    public class CancelPaymentRequest
    {
        [Required(ErrorMessage = "Reason is required")]
        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
        public string Reason { get; set; } = string.Empty;

        public int? InitiatedByUserId { get; set; }
    }


}

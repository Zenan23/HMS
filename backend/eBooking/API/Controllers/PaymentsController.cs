using API.Attributes;
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
        private readonly IBookingService _bookingService;

        public PaymentsController(
            IPaymentService paymentService,
            IBookingService bookingService,
            ILogger<PaymentsController> logger)
            : base(paymentService, logger)
        {
            _paymentService = paymentService;
            _bookingService = bookingService;
        }

        /// <summary>
        /// Provjerava da li pozivalac smije pokrenuti plaćanje za dto.BookingId — samo vlasnik
        /// rezervacije ili Employee/Admin. UserId u DTO-u se PRESKRIPUJE stvarnim vlasnikom
        /// rezervacije (klijent ne smije platiti "u ime" tuđe rezervacije niti lažirati UserId).
        /// Vraća null ako je sve u redu, inače ActionResult sa greškom koju treba direktno vratiti.
        /// </summary>
        private async Task<ActionResult?> AuthorizeAndPrepareCheckoutAsync(CreateHostedCheckoutDto dto)
        {
            var booking = await _bookingService.GetByIdAsync(dto.BookingId);
            if (booking == null)
            {
                return NotFound(ApiResponse<object>.ErrorResult("Rezervacija nije pronađena."));
            }

            if (!IsSelfOrElevated(booking.UserId))
            {
                return Forbid();
            }

            dto.UserId = booking.UserId;
            return null;
        }

        // Lista SVIH plaćanja (bez filtera po korisniku) — samo za osoblje, finansijski podaci.
        [HttpGet]
        [AuthorizeRole(UserRole.Employee)]
        public override Task<ActionResult<ApiResponse<PaginatedResult<PaymentDto>>>> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
            => base.GetAll(pageNumber, pageSize);

        // Pojedinačno plaćanje po ID-u — spriječi da korisnik pogodi/enumeriše tuđi paymentId.
        [HttpGet("{id}")]
        public override async Task<ActionResult<ApiResponse<PaymentDto>>> GetById([FromRoute] int id)
        {
            var result = await base.GetById(id);
            if (result.Result is OkObjectResult ok && ok.Value is ApiResponse<PaymentDto> resp && resp.Data != null)
            {
                if (!IsSelfOrElevated(resp.Data.UserId))
                {
                    return Forbid();
                }
            }
            return result;
        }

        /// <summary>
        /// Konfiguracija plaćanja za mobilnu aplikaciju (publishable key, feature flags).
        /// </summary>
        [HttpGet("config")]
        public ActionResult<ApiResponse<PaymentConfigDto>> GetConfig()
        {
            var config = _paymentService.GetPaymentConfig();
            return Ok(ApiResponse<PaymentConfigDto>.SuccessResult(config, "Konfiguracija plaćanja."));
        }

        /// <summary>
        /// Stripe PaymentIntent za in-app Payment Sheet.
        /// </summary>
        [HttpPost("stripe/intent")]
        public async Task<ActionResult<ApiResponse<StripeIntentResponseDto>>> StripeIntent([FromBody] CreateHostedCheckoutDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return BadRequest(ApiResponse<StripeIntentResponseDto>.ErrorResult("Validation failed.", errors));
                }

                var authError = await AuthorizeAndPrepareCheckoutAsync(dto);
                if (authError != null) return authError;

                var userAgent = Request.Headers["User-Agent"].ToString();
                var ipAddress = GetClientIpAddress();
                var result = await _paymentService.StartStripeIntentAsync(dto, userAgent, ipAddress);
                return Ok(ApiResponse<StripeIntentResponseDto>.SuccessResult(result, "PaymentIntent kreiran."));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<StripeIntentResponseDto>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Stripe intent greška");
                return StatusCode(500, ApiResponse<StripeIntentResponseDto>.ErrorResult("Greška pri kreiranju PaymentIntent-a."));
            }
        }

        /// <summary>
        /// Potvrda Stripe PaymentIntent-a nakon Payment Sheet-a (ako webhook nije stigao).
        /// </summary>
        [HttpPost("stripe/confirm")]
        public async Task<ActionResult<ApiResponse<bool>>> StripeConfirm([FromQuery] string payment_intent_id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(payment_intent_id))
                    return BadRequest(ApiResponse<bool>.ErrorResult("payment_intent_id je obavezan."));
                var ok = await _paymentService.TryConfirmStripePaymentIntentAsync(payment_intent_id);
                return Ok(ApiResponse<bool>.SuccessResult(ok, ok ? "Plaćanje potvrđeno." : "Plaćanje još nije završeno ili je već obrađeno."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Stripe confirm greška");
                return StatusCode(500, ApiResponse<bool>.ErrorResult("Greška."));
            }
        }

        /// <summary>
        /// PayPal narudžba za in-app WebView (approve URL).
        /// </summary>
        [HttpPost("paypal/order")]
        public async Task<ActionResult<ApiResponse<PayPalNativeOrderResponseDto>>> PayPalOrder([FromBody] CreateHostedCheckoutDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return BadRequest(ApiResponse<PayPalNativeOrderResponseDto>.ErrorResult("Validation failed.", errors));
                }

                var authError = await AuthorizeAndPrepareCheckoutAsync(dto);
                if (authError != null) return authError;

                var userAgent = Request.Headers["User-Agent"].ToString();
                var ipAddress = GetClientIpAddress();
                var result = await _paymentService.StartPayPalNativeOrderAsync(dto, userAgent, ipAddress);
                return Ok(ApiResponse<PayPalNativeOrderResponseDto>.SuccessResult(result, "PayPal narudžba kreirana."));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<PayPalNativeOrderResponseDto>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PayPal order greška");
                return StatusCode(500, ApiResponse<PayPalNativeOrderResponseDto>.ErrorResult("Greška pri kreiranju PayPal narudžbe."));
            }
        }

        /// <summary>
        /// Započinje hosted checkout (Stripe ili PayPal). Vraća URL za redirect u preglednik.
        /// </summary>
        [HttpPost("hosted-checkout")]
        public async Task<ActionResult<ApiResponse<HostedCheckoutResponseDto>>> HostedCheckout([FromBody] CreateHostedCheckoutDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return BadRequest(ApiResponse<HostedCheckoutResponseDto>.ErrorResult("Validation failed.", errors));
                }

                var authError = await AuthorizeAndPrepareCheckoutAsync(dto);
                if (authError != null) return authError;

                var userAgent = Request.Headers["User-Agent"].ToString();
                var ipAddress = GetClientIpAddress();
                var result = await _paymentService.StartHostedCheckoutAsync(dto, userAgent, ipAddress);
                return Ok(ApiResponse<HostedCheckoutResponseDto>.SuccessResult(result, "Otvorite redirectUrl u pregledniku."));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<HostedCheckoutResponseDto>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hosted checkout greška");
                return StatusCode(500, ApiResponse<HostedCheckoutResponseDto>.ErrorResult("Greška pri kreiranju plaćanja."));
            }
        }

        /// <summary>
        /// Nakon povratka sa PayPal-a: capture narudžbe (token = order id iz query stringa).
        /// </summary>
        [HttpPost("paypal/capture")]
        public async Task<ActionResult<ApiResponse<bool>>> PayPalCapture([FromBody] PayPalCaptureRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return BadRequest(ApiResponse<bool>.ErrorResult("Validation failed.", errors));
                }

                int? userId = null;
                var uidClaim = User.FindFirst("userId") ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (uidClaim != null && int.TryParse(uidClaim.Value, out var uid))
                    userId = uid;

                var ok = await _paymentService.CapturePayPalAfterReturnAsync(request.Token, userId);
                if (!ok)
                    return BadRequest(ApiResponse<bool>.ErrorResult("PayPal capture nije uspio."));
                return Ok(ApiResponse<bool>.SuccessResult(true, "Plaćanje potvrđeno."));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PayPal capture greška");
                return StatusCode(500, ApiResponse<bool>.ErrorResult("Greška pri PayPal capture-u."));
            }
        }

        /// <summary>
        /// Polling: potvrdi Stripe sesiju ako webhook još nije stigao (session_id sa Stripe redirecta).
        /// </summary>
        [HttpPost("stripe/finalize")]
        public async Task<ActionResult<ApiResponse<bool>>> StripeFinalize([FromQuery] string session_id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(session_id))
                    return BadRequest(ApiResponse<bool>.ErrorResult("session_id je obavezan."));
                var ok = await _paymentService.TryFinalizeStripeFromSessionIdAsync(session_id);
                return Ok(ApiResponse<bool>.SuccessResult(ok, ok ? "Sesija obrađena." : "Sesija još nije plaćena ili je već obrađena."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Stripe finalize greška");
                return StatusCode(500, ApiResponse<bool>.ErrorResult("Greška."));
            }
        }

        /// <summary>
        /// Legacy endpoint – koristite hosted-checkout.
        /// </summary>
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

                var userAgent = Request.Headers["User-Agent"].ToString();
                var ipAddress = GetClientIpAddress();

                var payment = await _paymentService.ProcessPaymentAsync(createPaymentDto, userAgent, ipAddress);
                return Ok(ApiResponse<PaymentDto>.SuccessResult(payment, "Payment processed successfully."));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<PaymentDto>.ErrorResult(ex.Message));
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

                var payment = await _paymentService.GetByIdAsync(id);
                if (payment == null)
                {
                    return NotFound(ApiResponse<bool>.ErrorResult("Payment not found."));
                }

                // Samo vlasnik plaćanja (svoja rezervacija) ili Employee/Admin smiju zatražiti povrat —
                // bez ovoga bilo koji prijavljeni korisnik mogao je refundirati tuđe plaćanje pogađanjem ID-a.
                if (!IsSelfOrElevated(payment.UserId))
                {
                    return Forbid();
                }

                // InitiatedByUserId se ne uzima iz tijela zahtjeva (klijent ga može lažirati) — uvijek
                // se upisuje stvarni pozivalac iz JWT-a radi tačnog audit traga.
                var result = await _paymentService.RefundPaymentAsync(id, request.Amount, request.Reason, GetCurrentUserId());

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

                var payment = await _paymentService.GetByIdAsync(id);
                if (payment == null)
                {
                    return NotFound(ApiResponse<bool>.ErrorResult("Payment not found."));
                }

                if (!IsSelfOrElevated(payment.UserId))
                {
                    return Forbid();
                }

                var result = await _paymentService.CancelPaymentAsync(id, request.Reason, GetCurrentUserId());

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

                if (!IsSelfOrElevated(userId))
                {
                    return Forbid();
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

                var booking = await _bookingService.GetByIdAsync(bookingId);
                if (booking == null)
                {
                    return NotFound(ApiResponse<IEnumerable<PaymentDto>>.ErrorResult("Booking not found."));
                }

                // Samo vlasnik rezervacije ili Employee/Admin smiju vidjeti plaćanja vezana za nju.
                if (!IsSelfOrElevated(booking.UserId))
                {
                    return Forbid();
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

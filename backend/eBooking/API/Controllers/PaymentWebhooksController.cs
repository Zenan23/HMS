using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Persistence.Interfaces;

namespace API.Controllers
{
    [Route("api/webhooks")]
    [ApiController]
    [AllowAnonymous]
    public class PaymentWebhooksController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentWebhooksController> _logger;

        public PaymentWebhooksController(IPaymentService paymentService, ILogger<PaymentWebhooksController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        [HttpPost("stripe")]
        public async Task<IActionResult> StripeWebhook()
        {
            Request.Body.Position = 0;
            using var reader = new StreamReader(Request.Body, leaveOpen: true);
            var json = await reader.ReadToEndAsync();
            Request.Body.Position = 0;

            var signature = Request.Headers["Stripe-Signature"].ToString();
            if (string.IsNullOrEmpty(signature))
                return BadRequest();

            try
            {
                var ok = await _paymentService.ProcessStripeWebhookAsync(json, signature);
                if (!ok)
                    return BadRequest();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Stripe webhook obrada");
                return StatusCode(500);
            }
        }

        [HttpPost("paypal")]
        public async Task<IActionResult> PayPalWebhook()
        {
            Request.Body.Position = 0;
            using var reader = new StreamReader(Request.Body, leaveOpen: true);
            var json = await reader.ReadToEndAsync();
            Request.Body.Position = 0;

            var transmissionId = Request.Headers["PAYPAL-TRANSMISSION-ID"].ToString();
            var transmissionTime = Request.Headers["PAYPAL-TRANSMISSION-TIME"].ToString();
            var certUrl = Request.Headers["PAYPAL-CERT-URL"].ToString();
            var authAlgo = Request.Headers["PAYPAL-AUTH-ALGO"].ToString();
            var transmissionSig = Request.Headers["PAYPAL-TRANSMISSION-SIG"].ToString();

            if (string.IsNullOrEmpty(transmissionId))
                return BadRequest();

            try
            {
                var ok = await _paymentService.ProcessPayPalWebhookAsync(
                    json, transmissionId, transmissionTime, certUrl, authAlgo, transmissionSig);
                return ok ? Ok() : BadRequest();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PayPal webhook obrada");
                return StatusCode(500);
            }
        }
    }
}

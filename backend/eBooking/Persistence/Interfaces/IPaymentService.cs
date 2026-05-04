using Contracts.DTOs;
using Contracts.Enums;

namespace Persistence.Interfaces
{
    public interface IPaymentService : IBaseService<PaymentDto, CreatePaymentDto, UpdatePaymentDto>
    {
        /// <summary>Hosted checkout (Stripe ili PayPal). Vraća URL za redirect.</summary>
        Task<HostedCheckoutResponseDto> StartHostedCheckoutAsync(CreateHostedCheckoutDto dto, string? userAgent = null, string? ipAddress = null);

        /// <summary>Ponovno učitavanje Stripe sesije nakon povratka korisnika (polling).</summary>
        Task<bool> TryFinalizeStripeFromSessionIdAsync(string checkoutSessionId);

        /// <summary>Stripe webhook (raw JSON + Stripe-Signature header).</summary>
        Task<bool> ProcessStripeWebhookAsync(string json, string stripeSignatureHeader);

        /// <summary>PayPal webhook (headers + raw body).</summary>
        Task<bool> ProcessPayPalWebhookAsync(string rawBody, string transmissionId, string transmissionTime, string certUrl, string authAlgo, string transmissionSig);

        /// <summary>Capture PayPal narudžbe nakon što korisnik potvrdi (token = order id).</summary>
        Task<bool> CapturePayPalAfterReturnAsync(string orderId, int? userId = null);

        Task<PaymentDto> ProcessPaymentAsync(CreatePaymentDto createPaymentDto, string? userAgent = null, string? ipAddress = null);
        Task<bool> RefundPaymentAsync(int paymentId, decimal amount, string reason, int? initiatedByUserId = null);
        Task<bool> CancelPaymentAsync(int paymentId, string reason, int? initiatedByUserId = null);
        Task<IEnumerable<PaymentDto>> GetByUserIdAsync(int userId);
        Task<IEnumerable<PaymentDto>> GetByBookingIdAsync(int bookingId);
        Task<IEnumerable<PaymentDto>> GetByStatusAsync(PaymentStatus status);
        Task<IEnumerable<PaymentDto>> GetByPaymentMethodAsync(PaymentMethod paymentMethod);
        Task<IEnumerable<PaymentAuditLogDto>> GetPaymentAuditLogsAsync(int paymentId);
        Task<decimal> GetTotalPaymentsAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<decimal> GetTotalRefundsAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<PaymentStatistics> GetPaymentStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
    }

}

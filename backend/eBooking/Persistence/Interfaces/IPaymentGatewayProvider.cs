using Contracts.Enums;
using Persistence.Models;

namespace Persistence.Interfaces
{
    public interface IPaymentGatewayProvider
    {
        PaymentMethod SupportedMethod { get; }

        Task<HostedCheckoutSessionResult> CreateHostedCheckoutAsync(
            Payment pendingPayment,
            HostedCheckoutUrls urls,
            CancellationToken cancellationToken = default);

        Task<RefundResult> ProcessRefundAsync(
            Payment payment,
            decimal amount,
            string reason,
            CancellationToken cancellationToken = default);
    }

    public class HostedCheckoutUrls
    {
        public string SuccessUrl { get; set; } = string.Empty;
        public string CancelUrl { get; set; } = string.Empty;
    }

    public class HostedCheckoutSessionResult
    {
        public bool IsSuccess { get; set; }
        public string? RedirectUrl { get; set; }
        public string? ProviderCheckoutId { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class RefundResult
    {
        public bool IsSuccess { get; set; }
        public string? RefundTransactionId { get; set; }
        public string? ErrorMessage { get; set; }
        public decimal RefundedAmount { get; set; }
        public DateTime ProcessedAt { get; set; }
    }

    public class PaymentIntentSessionResult
    {
        public bool IsSuccess { get; set; }
        public string? ClientSecret { get; set; }
        public string? PaymentIntentId { get; set; }
        public string? ErrorMessage { get; set; }
    }
}

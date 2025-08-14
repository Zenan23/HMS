using API.DTOs;
using API.Enums;

namespace API.Interfaces
{
    public interface IPaymentProvider
    {
        PaymentMethod SupportedMethod { get; }
        Task<PaymentResult> ProcessPaymentAsync(CreatePaymentDto paymentDto);
        Task<RefundResult> ProcessRefundAsync(int paymentId, decimal amount, string reason);
    }

    public class PaymentResult
    {
        public bool IsSuccess { get; set; }
        public string? TransactionId { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ProviderResponse { get; set; }
        public DateTime ProcessedAt { get; set; }
    }

    public class RefundResult
    {
        public bool IsSuccess { get; set; }
        public string? RefundTransactionId { get; set; }
        public string? ErrorMessage { get; set; }
        public decimal RefundedAmount { get; set; }
        public DateTime ProcessedAt { get; set; }
    }

}

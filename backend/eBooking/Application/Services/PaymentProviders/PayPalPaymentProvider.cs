using Contracts.DTOs;
using Contracts.Enums;
using Microsoft.Extensions.Logging;
using Persistence.Interfaces;

namespace Application.Services.PaymentProviders
{
    public class PayPalPaymentProvider : IPaymentProvider
    {
        private readonly ILogger<PayPalPaymentProvider> _logger;

        public PayPalPaymentProvider(ILogger<PayPalPaymentProvider> logger)
        {
            _logger = logger;
        }

        public PaymentMethod SupportedMethod => PaymentMethod.PayPal;

        public async Task<PaymentResult> ProcessPaymentAsync(CreatePaymentDto paymentDto)
        {
            try
            {
                _logger.LogInformation("Processing PayPal payment for amount: {Amount}", paymentDto.Amount);

                // Simulate processing delay
                await Task.Delay(3000);

                if (paymentDto.PayPalData == null)
                {
                    return new PaymentResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "PayPal data is required for PayPal payments",
                        ProcessedAt = DateTime.UtcNow
                    };
                }

                // Mock validation - simulate some emails failing
                var isValidPayPal = ValidatePayPalAccount(paymentDto.PayPalData);
                if (!isValidPayPal)
                {
                    return new PaymentResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "PayPal account not found or insufficient funds",
                        ProcessedAt = DateTime.UtcNow
                    };
                }

                // Simulate successful payment
                var transactionId = GeneratePayPalTransactionId();

                _logger.LogInformation("PayPal payment processed successfully. Transaction ID: {TransactionId}", transactionId);

                return new PaymentResult
                {
                    IsSuccess = true,
                    TransactionId = transactionId,
                    ProviderResponse = $"PayPal payment processed successfully for {paymentDto.PayPalData.PayPalEmail}",
                    ProcessedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing PayPal payment");
                return new PaymentResult
                {
                    IsSuccess = false,
                    ErrorMessage = "PayPal payment processing failed due to technical error",
                    ProcessedAt = DateTime.UtcNow
                };
            }
        }

        public async Task<RefundResult> ProcessRefundAsync(int paymentId, decimal amount, string reason)
        {
            try
            {
                _logger.LogInformation("Processing PayPal refund for payment {PaymentId}, amount: {Amount}", paymentId, amount);

                // Simulate processing delay
                await Task.Delay(2000);

                // Simulate successful refund (95% success rate for PayPal)
                var random = new Random();
                var isSuccessful = random.Next(1, 21) <= 19;

                if (!isSuccessful)
                {
                    return new RefundResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "PayPal refund failed - transaction cannot be refunded",
                        ProcessedAt = DateTime.UtcNow
                    };
                }

                var refundTransactionId = GeneratePayPalTransactionId("REF");

                _logger.LogInformation("PayPal refund processed successfully. Refund Transaction ID: {RefundTransactionId}", refundTransactionId);

                return new RefundResult
                {
                    IsSuccess = true,
                    RefundTransactionId = refundTransactionId,
                    RefundedAmount = amount,
                    ProcessedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing PayPal refund for payment {PaymentId}", paymentId);
                return new RefundResult
                {
                    IsSuccess = false,
                    ErrorMessage = "PayPal refund processing failed due to technical error",
                    ProcessedAt = DateTime.UtcNow
                };
            }
        }

        private bool ValidatePayPalAccount(PayPalPaymentData payPalData)
        {
            // Mock validation logic
            // Simulate failure for certain email patterns
            var failingEmails = new[] { "blocked@example.com", "suspended@paypal.com", "invalid@test.com" };

            if (failingEmails.Contains(payPalData.PayPalEmail.ToLower()))
            {
                return false;
            }

            // Simulate temporary account issues (5% failure rate)
            var random = new Random();
            return random.Next(1, 21) > 1;
        }

        private string GeneratePayPalTransactionId(string prefix = "PP")
        {
            return $"{prefix}_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid().ToString("N")[..12].ToUpper()}";
        }
    }
}
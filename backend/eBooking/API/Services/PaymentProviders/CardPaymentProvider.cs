using API.DTOs;
using API.Enums;
using API.Interfaces;

namespace API.Services.PaymentProviders
{
    public class CardPaymentProvider : IPaymentProvider
    {
        private readonly ILogger<CardPaymentProvider> _logger;

        public CardPaymentProvider(ILogger<CardPaymentProvider> logger)
        {
            _logger = logger;
        }

        public PaymentMethod SupportedMethod => PaymentMethod.Card;

        public async Task<PaymentResult> ProcessPaymentAsync(CreatePaymentDto paymentDto)
        {
            try
            {
                _logger.LogInformation("Processing card payment for amount: {Amount}", paymentDto.Amount);

                // Simulate processing delay
                await Task.Delay(2000);

                if (paymentDto.CardData == null)
                {
                    return new PaymentResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Card data is required for card payments",
                        ProcessedAt = DateTime.UtcNow
                    };
                }

                // Mock validation - simulate some cards failing
                var isValidCard = ValidateCard(paymentDto.CardData);
                if (!isValidCard)
                {
                    return new PaymentResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Invalid card details or insufficient funds",
                        ProcessedAt = DateTime.UtcNow
                    };
                }

                // Simulate successful payment
                var transactionId = GenerateTransactionId();

                _logger.LogInformation("Card payment processed successfully. Transaction ID: {TransactionId}", transactionId);

                return new PaymentResult
                {
                    IsSuccess = true,
                    TransactionId = transactionId,
                    ProviderResponse = $"Card payment processed successfully for {paymentDto.CardData.CardholderName}",
                    ProcessedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing card payment");
                return new PaymentResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Payment processing failed due to technical error",
                    ProcessedAt = DateTime.UtcNow
                };
            }
        }

        public async Task<RefundResult> ProcessRefundAsync(int paymentId, decimal amount, string reason)
        {
            try
            {
                _logger.LogInformation("Processing card refund for payment {PaymentId}, amount: {Amount}", paymentId, amount);

                // Simulate processing delay
                await Task.Delay(1500);

                // Simulate successful refund (90% success rate)
                var random = new Random();
                var isSuccessful = random.Next(1, 11) <= 9;

                if (!isSuccessful)
                {
                    return new RefundResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Refund failed - original transaction not found or already refunded",
                        ProcessedAt = DateTime.UtcNow
                    };
                }

                var refundTransactionId = GenerateTransactionId("REF");

                _logger.LogInformation("Card refund processed successfully. Refund Transaction ID: {RefundTransactionId}", refundTransactionId);

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
                _logger.LogError(ex, "Error processing card refund for payment {PaymentId}", paymentId);
                return new RefundResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Refund processing failed due to technical error",
                    ProcessedAt = DateTime.UtcNow
                };
            }
        }

        private bool ValidateCard(CardPaymentData cardData)
        {
            // Mock validation logic
            // Simulate failure for certain card numbers
            var failingCards = new[] { "4000000000000002", "4000000000000010", "4000000000000028" };

            if (failingCards.Contains(cardData.CardNumber))
            {
                return false;
            }

            // Check expiry date
            var currentDate = DateTime.Now;
            if (cardData.ExpiryYear < currentDate.Year ||
                (cardData.ExpiryYear == currentDate.Year && cardData.ExpiryMonth < currentDate.Month))
            {
                return false;
            }

            return true;
        }

        private string GenerateTransactionId(string prefix = "TXN")
        {
            return $"{prefix}_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        }
    }

}

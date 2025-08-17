
using Contracts.DTOs;
using Contracts.Enums;
using Microsoft.Extensions.Logging;
using Persistence.Interfaces;

namespace Application.Services.PaymentProviders
{
    public class BankTransferPaymentProvider : IPaymentProvider
    {
        private readonly ILogger<BankTransferPaymentProvider> _logger;

        public BankTransferPaymentProvider(ILogger<BankTransferPaymentProvider> logger)
        {
            _logger = logger;
        }

        public PaymentMethod SupportedMethod => PaymentMethod.BankTransfer;

        public async Task<PaymentResult> ProcessPaymentAsync(CreatePaymentDto paymentDto)
        {
            try
            {
                _logger.LogInformation("Processing bank transfer payment for amount: {Amount}", paymentDto.Amount);

                // Simulate processing delay (bank transfers take longer)
                await Task.Delay(5000);

                if (paymentDto.BankTransferData == null)
                {
                    return new PaymentResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Bank transfer data is required for bank transfer payments",
                        ProcessedAt = DateTime.UtcNow
                    };
                }

                // Mock validation - simulate some accounts failing
                var isValidBankAccount = ValidateBankAccount(paymentDto.BankTransferData);
                if (!isValidBankAccount)
                {
                    return new PaymentResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Invalid bank account details or insufficient funds",
                        ProcessedAt = DateTime.UtcNow
                    };
                }

                // Simulate successful payment
                var transactionId = GenerateBankTransactionId();

                _logger.LogInformation("Bank transfer payment processed successfully. Transaction ID: {TransactionId}", transactionId);

                return new PaymentResult
                {
                    IsSuccess = true,
                    TransactionId = transactionId,
                    ProviderResponse = $"Bank transfer processed successfully for {paymentDto.BankTransferData.AccountHolderName}",
                    ProcessedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing bank transfer payment");
                return new PaymentResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Bank transfer processing failed due to technical error",
                    ProcessedAt = DateTime.UtcNow
                };
            }
        }

        public async Task<RefundResult> ProcessRefundAsync(int paymentId, decimal amount, string reason)
        {
            try
            {
                _logger.LogInformation("Processing bank transfer refund for payment {PaymentId}, amount: {Amount}", paymentId, amount);

                // Simulate processing delay (bank refunds take even longer)
                await Task.Delay(7000);

                // Simulate successful refund (85% success rate for bank transfers)
                var random = new Random();
                var isSuccessful = random.Next(1, 21) <= 17;

                if (!isSuccessful)
                {
                    return new RefundResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Bank transfer refund failed - account may be closed or invalid",
                        ProcessedAt = DateTime.UtcNow
                    };
                }

                var refundTransactionId = GenerateBankTransactionId("BT_REF");

                _logger.LogInformation("Bank transfer refund processed successfully. Refund Transaction ID: {RefundTransactionId}", refundTransactionId);

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
                _logger.LogError(ex, "Error processing bank transfer refund for payment {PaymentId}", paymentId);
                return new RefundResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Bank transfer refund processing failed due to technical error",
                    ProcessedAt = DateTime.UtcNow
                };
            }
        }

        private bool ValidateBankAccount(BankTransferPaymentData bankData)
        {
            // Mock validation logic
            // Simulate failure for certain account patterns
            var failingAccounts = new[] { "0000000000", "9999999999", "1111111111" };

            if (failingAccounts.Contains(bankData.BankAccountNumber))
            {
                return false;
            }

            // Check routing number format (simple check)
            if (bankData.BankRoutingNumber.Length != 9 || !bankData.BankRoutingNumber.All(char.IsDigit))
            {
                return false;
            }

            // Simulate account validation issues (15% failure rate)
            var random = new Random();
            return random.Next(1, 21) > 3;
        }

        private string GenerateBankTransactionId(string prefix = "BT")
        {
            return $"{prefix}_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid().ToString("N")[..10].ToUpper()}";
        }
    }

}

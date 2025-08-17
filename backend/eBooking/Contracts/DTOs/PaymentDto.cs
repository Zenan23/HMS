using Contracts.Enums;
using System.ComponentModel.DataAnnotations;

namespace Contracts.DTOs
{
    public class PaymentDto : BaseEntityDto
    {
        public int UserId { get; set; }
        public int BookingId { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus Status { get; set; }
        public string? TransactionId { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? FailureReason { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? RefundedAt { get; set; }
        public decimal? RefundAmount { get; set; }

        // Additional info
        public string UserName { get; set; } = string.Empty;
        public string BookingReference { get; set; } = string.Empty;
    }

    public class CreatePaymentDto : CreateBaseEntityDto
    {
        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Booking ID is required")]
        public int BookingId { get; set; }

        [Required(ErrorMessage = "Amount is required")]
      //  [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Payment method is required")]
        public PaymentMethod PaymentMethod { get; set; }

        [StringLength(3, ErrorMessage = "Currency code must be 3 characters")]
        public string Currency { get; set; } = "USD";

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        // Payment method specific data
        public CardPaymentData? CardData { get; set; }
        public PayPalPaymentData? PayPalData { get; set; }
        public BankTransferPaymentData? BankTransferData { get; set; }
    }

    public class UpdatePaymentDto : UpdateBaseEntityDto
    {
        public PaymentStatus Status { get; set; }
        public string? TransactionId { get; set; }
        public string? FailureReason { get; set; }
        public string? Description { get; set; }
    }

    // Payment method specific data classes
    public class CardPaymentData
    {
        [Required(ErrorMessage = "Card number is required")]
        [StringLength(19, ErrorMessage = "Card number is invalid")]
        public string CardNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Expiry month is required")]
        [Range(1, 12, ErrorMessage = "Expiry month must be between 1 and 12")]
        public int ExpiryMonth { get; set; }

        [Required(ErrorMessage = "Expiry year is required")]
        [Range(2024, 2040, ErrorMessage = "Expiry year is invalid")]
        public int ExpiryYear { get; set; }

        [Required(ErrorMessage = "CVV is required")]
        [StringLength(4, MinimumLength = 3, ErrorMessage = "CVV must be 3 or 4 digits")]
        public string CVV { get; set; } = string.Empty;

        [Required(ErrorMessage = "Cardholder name is required")]
        [StringLength(100, ErrorMessage = "Cardholder name cannot exceed 100 characters")]
        public string CardholderName { get; set; } = string.Empty;
    }

    public class PayPalPaymentData
    {
        [Required(ErrorMessage = "PayPal email is required")]
        [EmailAddress(ErrorMessage = "Invalid PayPal email format")]
        public string PayPalEmail { get; set; } = string.Empty;
    }

    public class BankTransferPaymentData
    {
        [Required(ErrorMessage = "Bank account number is required")]
        public string BankAccountNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Bank routing number is required")]
        public string BankRoutingNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Account holder name is required")]
        public string AccountHolderName { get; set; } = string.Empty;

        public string? BankName { get; set; }
    }

}

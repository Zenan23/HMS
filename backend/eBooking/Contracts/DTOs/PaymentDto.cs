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

        /// <summary>Stripe session id or PayPal order id while checkout is in progress.</summary>
        public string? CheckoutId { get; set; }

        // Additional info
        public string UserName { get; set; } = string.Empty;
        public string BookingReference { get; set; } = string.Empty;
    }

    /// <summary>Starts hosted checkout (Stripe Checkout or PayPal order).</summary>
    public class CreateHostedCheckoutDto
    {
        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Booking ID is required")]
        public int BookingId { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Payment method is required")]
        public PaymentMethod PaymentMethod { get; set; }

        [StringLength(3, ErrorMessage = "Currency code must be 3 characters")]
        public string Currency { get; set; } = "USD";

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }
    }

    public class HostedCheckoutResponseDto
    {
        public int PaymentId { get; set; }
        public string RedirectUrl { get; set; } = string.Empty;
        public PaymentMethod PaymentMethod { get; set; }
    }

    /// <summary>Legacy DTO kept for API compatibility; prefer <see cref="CreateHostedCheckoutDto"/>.</summary>
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
    }

    public class UpdatePaymentDto : UpdateBaseEntityDto
    {
        public PaymentStatus Status { get; set; }
        public string? TransactionId { get; set; }
        public string? FailureReason { get; set; }
        public string? Description { get; set; }
        public string? CheckoutId { get; set; }
    }

}

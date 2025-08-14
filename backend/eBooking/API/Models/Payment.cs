using API.Enums;

namespace API.Models
{
    public class Payment : BaseEntity
    {
        public int UserId { get; set; }
        public int BookingId { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus Status { get; set; }
        public string? TransactionId { get; set; } 
        public string? PaymentProviderResponse { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? FailureReason { get; set; }
        public string Currency { get; set; } = "USD";
        public string? Description { get; set; }
        public DateTime? RefundedAt { get; set; }
        public decimal? RefundAmount { get; set; }
        public User User { get; set; } = null!;
        public Booking Booking { get; set; } = null!;
        public ICollection<PaymentAuditLog> AuditLogs { get; set; } = new List<PaymentAuditLog>();
    }
}

using Contracts.Enums;

namespace Persistence.Models
{
    public class PaymentAuditLog : BaseEntity
    {
        public int PaymentId { get; set; }
        public PaymentStatus FromStatus { get; set; }
        public PaymentStatus ToStatus { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? Details { get; set; }
        public string? ErrorMessage { get; set; }
        public string? UserAgent { get; set; } 
        public string? IpAddress { get; set; }
        public int? InitiatedByUserId { get; set; }
        public DateTime AttemptedAt { get; set; }
        public Payment Payment { get; set; } = null!;
        public User? InitiatedByUser { get; set; }
    }
}

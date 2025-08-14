using API.Enums;
using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class PaymentAuditLogDto : BaseEntityDto
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

        // Additional info
        public string? InitiatedByUserName { get; set; }
    }

    public class CreatePaymentAuditLogDto : CreateBaseEntityDto
    {
        [Required(ErrorMessage = "Payment ID is required")]
        public int PaymentId { get; set; }

        [Required(ErrorMessage = "From status is required")]
        public PaymentStatus FromStatus { get; set; }

        [Required(ErrorMessage = "To status is required")]
        public PaymentStatus ToStatus { get; set; }

        [Required(ErrorMessage = "Action is required")]
        [StringLength(100, ErrorMessage = "Action cannot exceed 100 characters")]
        public string Action { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Details cannot exceed 1000 characters")]
        public string? Details { get; set; }

        [StringLength(500, ErrorMessage = "Error message cannot exceed 500 characters")]
        public string? ErrorMessage { get; set; }

        [StringLength(500, ErrorMessage = "User agent cannot exceed 500 characters")]
        public string? UserAgent { get; set; }

        [StringLength(45, ErrorMessage = "IP address cannot exceed 45 characters")]
        public string? IpAddress { get; set; }

        public int? InitiatedByUserId { get; set; }
    }

    public class UpdatePaymentAuditLogDto : UpdateBaseEntityDto
    {
        [StringLength(1000, ErrorMessage = "Details cannot exceed 1000 characters")]
        public string? Details { get; set; }

        [StringLength(500, ErrorMessage = "Error message cannot exceed 500 characters")]
        public string? ErrorMessage { get; set; }
    }

}

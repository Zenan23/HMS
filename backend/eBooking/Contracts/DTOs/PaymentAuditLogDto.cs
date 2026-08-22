using Contracts.Enums;
using System.ComponentModel.DataAnnotations;

namespace Contracts.DTOs
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
        [Required(ErrorMessage = "ID plaćanja je obavezan.")]
        public int PaymentId { get; set; }

        [Required(ErrorMessage = "Prethodni status je obavezan.")]
        public PaymentStatus FromStatus { get; set; }

        [Required(ErrorMessage = "Novi status je obavezan.")]
        public PaymentStatus ToStatus { get; set; }

        [Required(ErrorMessage = "Akcija je obavezna.")]
        [StringLength(100, ErrorMessage = "Akcija ne smije imati više od 100 karaktera.")]
        public string Action { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Detalji ne smiju imati više od 1000 karaktera.")]
        public string? Details { get; set; }

        [StringLength(500, ErrorMessage = "Poruka o grešci ne smije imati više od 500 karaktera.")]
        public string? ErrorMessage { get; set; }

        [StringLength(500, ErrorMessage = "User agent ne smije imati više od 500 karaktera.")]
        public string? UserAgent { get; set; }

        [StringLength(45, ErrorMessage = "IP adresa ne smije imati više od 45 karaktera.")]
        public string? IpAddress { get; set; }

        public int? InitiatedByUserId { get; set; }
    }

    public class UpdatePaymentAuditLogDto : UpdateBaseEntityDto
    {
        [StringLength(1000, ErrorMessage = "Detalji ne smiju imati više od 1000 karaktera.")]
        public string? Details { get; set; }

        [StringLength(500, ErrorMessage = "Poruka o grešci ne smije imati više od 500 karaktera.")]
        public string? ErrorMessage { get; set; }
    }

}

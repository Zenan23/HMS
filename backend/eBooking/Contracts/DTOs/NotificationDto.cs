using System.ComponentModel.DataAnnotations;

namespace Contracts.DTOs
{
    public class NotificationDto : BaseEntityDto
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime SentDate { get; set; }
        public DateTime? ReadDate { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string? ActionUrl { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int? BookingId { get; set; }
    }

    public class CreateNotificationDto : CreateBaseEntityDto
    {
        [Required(ErrorMessage = "Naslov je obavezan.")]
        [StringLength(100, ErrorMessage = "Naslov ne smije imati više od 100 karaktera.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Poruka je obavezna.")]
        [StringLength(1000, ErrorMessage = "Poruka ne smije imati više od 1000 karaktera.")]
        public string Message { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tip je obavezan.")]
        [StringLength(50, ErrorMessage = "Tip ne smije imati više od 50 karaktera.")]
        public string Type { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Prioritet ne smije imati više od 20 karaktera.")]
        public string Priority { get; set; } = "Normal";

        [Url(ErrorMessage = "URL za akciju mora biti validan URL.")]
        public string? ActionUrl { get; set; }

        [Required(ErrorMessage = "ID korisnika je obavezan.")]
        public int UserId { get; set; }

        public int? BookingId { get; set; }
    }

    public class UpdateNotificationDto : UpdateBaseEntityDto
    {
        [Required(ErrorMessage = "Naslov je obavezan.")]
        [StringLength(100, ErrorMessage = "Naslov ne smije imati više od 100 karaktera.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Poruka je obavezna.")]
        [StringLength(1000, ErrorMessage = "Poruka ne smije imati više od 1000 karaktera.")]
        public string Message { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tip je obavezan.")]
        [StringLength(50, ErrorMessage = "Tip ne smije imati više od 50 karaktera.")]
        public string Type { get; set; } = string.Empty;

        public bool IsRead { get; set; }

        [StringLength(20, ErrorMessage = "Prioritet ne smije imati više od 20 karaktera.")]
        public string Priority { get; set; } = "Normal";

        [Url(ErrorMessage = "URL za akciju mora biti validan URL.")]
        public string? ActionUrl { get; set; }
    }

}

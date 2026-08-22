using Contracts.Enums;
using System.ComponentModel.DataAnnotations;

namespace Contracts.DTOs
{
    public class SupportTicketDto : BaseEntityDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string MessageBody { get; set; } = string.Empty;
        public SupportTicketStatus Status { get; set; }
        public SupportTicketPriority Priority { get; set; }
        public string? AdminResponse { get; set; }
        public DateTime? RespondedAt { get; set; }
        public string? RespondedByUserName { get; set; }
    }

    public class CreateSupportTicketDto : CreateBaseEntityDto
    {
        [Required(ErrorMessage = "ID korisnika je obavezan.")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Naslov je obavezan.")]
        [StringLength(200, ErrorMessage = "Naslov ne smije imati više od 200 karaktera.")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Sadržaj poruke je obavezan.")]
        [StringLength(5000, ErrorMessage = "Sadržaj poruke ne smije imati više od 5000 karaktera.")]
        public string MessageBody { get; set; } = string.Empty;

        public SupportTicketStatus Status { get; set; } = SupportTicketStatus.Open;
        public SupportTicketPriority Priority { get; set; } = SupportTicketPriority.Medium;
    }

    public class UpdateSupportTicketDto : UpdateBaseEntityDto
    {
        [Required(ErrorMessage = "ID korisnika je obavezan.")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Naslov je obavezan.")]
        [StringLength(200, ErrorMessage = "Naslov ne smije imati više od 200 karaktera.")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Sadržaj poruke je obavezan.")]
        [StringLength(5000, ErrorMessage = "Sadržaj poruke ne smije imati više od 5000 karaktera.")]
        public string MessageBody { get; set; } = string.Empty;

        [Required(ErrorMessage = "Status je obavezan.")]
        public SupportTicketStatus Status { get; set; }

        [Required(ErrorMessage = "Prioritet je obavezan.")]
        public SupportTicketPriority Priority { get; set; }

        // Odgovor osoblja — opciono, samo Employee/Admin smiju ga stvarno postaviti
        // (provjera i stamp RespondedAt/RespondedByUserId rade se u SupportTicketsController.Update).
        [StringLength(5000, ErrorMessage = "Odgovor ne smije imati više od 5000 karaktera.")]
        public string? AdminResponse { get; set; }
    }
}

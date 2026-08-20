using Contracts.Enums;

namespace Persistence.Models
{
    public class SupportTicket : BaseEntity
    {
        public int UserId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string MessageBody { get; set; } = string.Empty;
        public SupportTicketStatus Status { get; set; } = SupportTicketStatus.Open;
        public SupportTicketPriority Priority { get; set; } = SupportTicketPriority.Medium;
        public User User { get; set; } = null!;

        // Odgovor osoblja na tiket. RespondedAt/RespondedByUserId se postavljaju isključivo
        // server-side (SupportTicketsController.Update), klijent ne može sam sebi "potpisati" odgovor.
        public string? AdminResponse { get; set; }
        public DateTime? RespondedAt { get; set; }
        public int? RespondedByUserId { get; set; }
        public User? RespondedByUser { get; set; }
    }
}

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
    }
}

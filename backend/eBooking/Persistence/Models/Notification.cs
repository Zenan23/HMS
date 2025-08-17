namespace Persistence.Models
{
    public class Notification : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public DateTime SentDate { get; set; }
        public DateTime? ReadDate { get; set; }
        public string Priority { get; set; } = "Normal"; 
        public string? ActionUrl { get; set; } 
        public int UserId { get; set; }
        public int? BookingId { get; set; }
        public User User { get; set; } = null!;
        public Booking? Booking { get; set; }
    }
}

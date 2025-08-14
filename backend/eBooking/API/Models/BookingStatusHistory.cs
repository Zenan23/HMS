using API.Enums;

namespace API.Models
{
    public class BookingStatusHistory : BaseEntity
    {
        public BookingStatus FromStatus { get; set; }
        public BookingStatus ToStatus { get; set; }
        public DateTime ChangeDate { get; set; }
        public string? Reason { get; set; }
        public string? Notes { get; set; } 
        public int BookingId { get; set; }
        public int? ChangedByUserId { get; set; }
        public Booking Booking { get; set; } = null!;
        public User? ChangedByUser { get; set; }
    }
}

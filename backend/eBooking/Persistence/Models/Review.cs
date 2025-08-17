namespace Persistence.Models
{
    public class Review : BaseEntity
    {
        public int Rating { get; set; } 
        public string Title { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public DateTime ReviewDate { get; set; }
        public bool IsVerified { get; set; } = false;
        public bool IsApproved { get; set; } = true; 
        public int HotelId { get; set; }
        public int? UserId { get; set; } 
        public int? BookingId { get; set; }
        public Hotel Hotel { get; set; } = null!;
        public User? User { get; set; }
        public Booking? Booking { get; set; }
    }

}

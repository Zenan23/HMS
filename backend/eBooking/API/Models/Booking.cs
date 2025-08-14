using API.Enums;

namespace API.Models
{
    public class Booking : BaseEntity
    {
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int NumberOfGuests { get; set; }
        public decimal TotalPrice { get; set; }
        public BookingStatus Status { get; set; }
        public string SpecialRequests { get; set; } = string.Empty;
        public int RoomId { get; set; }
        public int? UserId { get; set; }
        public Room Room { get; set; } = null!;
        public User? User { get; set; }
        public ICollection<BookingService> BookingServices { get; set; } = new List<BookingService>();
    }
}

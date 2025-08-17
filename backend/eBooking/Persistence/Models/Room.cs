using Contracts.Enums;

namespace Persistence.Models
{
    public class Room : BaseEntity
    {
        public string RoomNumber { get; set; } = string.Empty;
        public RoomType RoomType { get; set; }
        public decimal PricePerNight { get; set; }
        public int MaxOccupancy { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsAvailable { get; set; } = true;
        public int HotelId { get; set; }
        public Hotel Hotel { get; set; } = null!;
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }

}

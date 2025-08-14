namespace API.Models
{
    public class BookingService : BaseEntity
    {
        public int BookingId { get; set; }
        public int ServiceId { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; } = 1;

        public Booking Booking { get; set; } = null!;
        public Service Service { get; set; } = null!;
    }
}




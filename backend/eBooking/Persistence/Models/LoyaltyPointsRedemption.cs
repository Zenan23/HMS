namespace Persistence.Models
{
    public class LoyaltyPointsRedemption : BaseEntity
    {
        public int UserId { get; set; }
        public int BookingId { get; set; }
        public int PointsUsed { get; set; }
        public DateTime RedeemedAt { get; set; }
        public decimal EquivalentValueAmount { get; set; }
        public User User { get; set; } = null!;
        public Booking Booking { get; set; } = null!;
    }
}

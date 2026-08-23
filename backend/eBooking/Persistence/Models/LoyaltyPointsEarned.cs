namespace Persistence.Models
{
    /// <summary>
    /// Log zarađenih loyalty bodova. Redovi se dodaju automatski (PaymentService, kad
    /// Payment.Status -> Completed) ili ručno od strane osoblja (korekcije preko generičkog CRUD-a).
    /// </summary>
    public class LoyaltyPointsEarned : BaseEntity
    {
        public int UserId { get; set; }
        public int? BookingId { get; set; }
        public int? PaymentId { get; set; }
        public int PointsEarned { get; set; }
        public DateTime EarnedAt { get; set; }
        public string Reason { get; set; } = string.Empty;
        public User User { get; set; } = null!;
        public Booking? Booking { get; set; }
        public Payment? Payment { get; set; }
    }
}

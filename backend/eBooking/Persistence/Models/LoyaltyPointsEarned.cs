namespace Persistence.Models
{
    /// <summary>
    /// Log zarađenih loyalty bodova — simetričan LoyaltyPointsRedemption (koji bilježi POTROŠENE
    /// bodove). Namjerno NEMA mutable "balans" kolonu na User-u; trenutni balans korisnika se
    /// računa on-the-fly kao SUM(LoyaltyPointsEarned) - SUM(LoyaltyPointsRedemption) da se izbjegnu
    /// concurrency bugovi (dvije istovremene transakcije koje bi obje pročitale/upisale isti
    /// mutable brojač). Redovi se dodaju automatski (PaymentService, kad Payment.Status -> Completed)
    /// ili ručno od strane osoblja (korekcije preko generičkog CRUD-a).
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

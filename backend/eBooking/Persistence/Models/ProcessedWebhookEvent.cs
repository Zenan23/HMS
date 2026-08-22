namespace Persistence.Models
{
    /// <summary>Idempotent webhook processing (Stripe event id).</summary>
    public class ProcessedWebhookEvent
    {
        public int Id { get; set; }
        public string Provider { get; set; } = string.Empty;
        public string EventId { get; set; } = string.Empty;
        public DateTime ReceivedAt { get; set; }

        /// <summary>
        /// Popunjava se naknadno (best-effort), kada se webhook uspješno poveže sa konkretnim Payment
        /// zapisom. Dedup provjera (TryMarkProcessedAsync) se i dalje radi PRIJE nego što ovo znamo,
        /// pa je ovo polje uvijek nullable — nikad ne blokira dedup logiku.
        /// </summary>
        public int? PaymentId { get; set; }
        public Payment? Payment { get; set; }
    }
}

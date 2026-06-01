namespace Persistence.Models
{
    /// <summary>Idempotent webhook processing (Stripe event id / PayPal transmission id).</summary>
    public class ProcessedWebhookEvent
    {
        public int Id { get; set; }
        public string Provider { get; set; } = string.Empty;
        public string EventId { get; set; } = string.Empty;
        public DateTime ReceivedAt { get; set; }
    }
}

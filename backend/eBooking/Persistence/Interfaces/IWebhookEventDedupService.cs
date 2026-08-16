namespace Persistence.Interfaces
{
    public interface IWebhookEventDedupService
    {
        /// <summary>Returns false if this provider/event id was already processed.</summary>
        Task<bool> TryMarkProcessedAsync(string provider, string eventId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Best-effort: poveži već zabilježen webhook event sa Payment zapisom, kada se to sazna
        /// (nakon dedup provjere). Ne baca izuzetak ako event ne postoji — samo tiho ne uradi ništa.
        /// </summary>
        Task LinkPaymentAsync(string provider, string eventId, int paymentId, CancellationToken cancellationToken = default);
    }
}

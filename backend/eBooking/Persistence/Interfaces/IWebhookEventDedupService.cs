namespace Persistence.Interfaces
{
    public interface IWebhookEventDedupService
    {
        /// <summary>Returns false if this provider/event id was already processed.</summary>
        Task<bool> TryMarkProcessedAsync(string provider, string eventId, CancellationToken cancellationToken = default);
    }
}

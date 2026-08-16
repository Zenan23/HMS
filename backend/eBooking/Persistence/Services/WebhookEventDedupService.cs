using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using Persistence.Interfaces;
using Persistence.Models;

namespace Persistence.Services
{
    public class WebhookEventDedupService : IWebhookEventDedupService
    {
        private readonly ApplicationDbContext _db;

        public WebhookEventDedupService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<bool> TryMarkProcessedAsync(string provider, string eventId, CancellationToken cancellationToken = default)
        {
            if (await _db.ProcessedWebhookEvents.AsNoTracking()
                    .AnyAsync(e => e.Provider == provider && e.EventId == eventId, cancellationToken))
                return false;

            _db.ProcessedWebhookEvents.Add(new ProcessedWebhookEvent
            {
                Provider = provider,
                EventId = eventId,
                ReceivedAt = DateTime.UtcNow
            });

            try
            {
                await _db.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (DbUpdateException)
            {
                return false;
            }
        }

        public async Task LinkPaymentAsync(string provider, string eventId, int paymentId, CancellationToken cancellationToken = default)
        {
            var evt = await _db.ProcessedWebhookEvents
                .FirstOrDefaultAsync(e => e.Provider == provider && e.EventId == eventId, cancellationToken);

            if (evt == null || evt.PaymentId == paymentId)
                return;

            evt.PaymentId = paymentId;
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}

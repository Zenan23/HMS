using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using Persistence.Services;
using Xunit;

namespace eBooking.Tests;

public class WebhookEventDedupServiceTests
{
    [Fact]
    public async Task TryMarkProcessedAsync_duplicate_returns_false()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("WebhookDedup_" + Guid.NewGuid())
            .Options;

        await using (var ctx = new ApplicationDbContext(options))
        {
            var svc = new WebhookEventDedupService(ctx);
            Assert.True(await svc.TryMarkProcessedAsync("Stripe", "evt_1"));
            Assert.False(await svc.TryMarkProcessedAsync("Stripe", "evt_1"));
        }
    }

    [Fact]
    public async Task TryMarkProcessedAsync_different_ids_both_true()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("WebhookDedup_" + Guid.NewGuid())
            .Options;

        await using (var ctx = new ApplicationDbContext(options))
        {
            var svc = new WebhookEventDedupService(ctx);
            Assert.True(await svc.TryMarkProcessedAsync("PayPal", "t1"));
            Assert.True(await svc.TryMarkProcessedAsync("PayPal", "t2"));
        }
    }
}

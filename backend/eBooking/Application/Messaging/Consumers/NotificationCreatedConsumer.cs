using Application.Hubs;
using Contracts.Messages;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Application.Messaging.Consumers
{
    // Ostaje u API procesu (registrovan preko AddApiMessaging): treba IHubContext<NotificationsHub>
    // da bi mogao realtime pushati notifikacije klijentima koji su već WebSocket-om povezani na API.
    // Worker (odvojen proces) nema pristup tim konekcijama, zato ovaj konzument NIJE premješten.
    public class NotificationCreatedConsumer : IConsumer<NotificationCreated>
    {
        private readonly ILogger<NotificationCreatedConsumer> _logger;
        private readonly IHubContext<NotificationsHub> _hubContext;

        public NotificationCreatedConsumer(ILogger<NotificationCreatedConsumer> logger, IHubContext<NotificationsHub> hubContext)
        {
            _logger = logger;
            _hubContext = hubContext;
        }

        public async Task Consume(ConsumeContext<NotificationCreated> context)
        {
            var msg = context.Message;
            _logger.LogInformation("Sending realtime notification to user {UserId}", msg.UserId);
            await _hubContext.Clients.Group($"user:{msg.UserId}").SendAsync("notificationCreated", new
            {
                notificationId = msg.NotificationId
            });
        }
    }
}



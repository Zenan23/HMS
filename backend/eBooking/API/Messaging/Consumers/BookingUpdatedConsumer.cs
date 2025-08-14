using API.Contracts.Messages;
using API.Interfaces;
using MassTransit;

namespace API.Messaging.Consumers
{
    public class BookingUpdatedConsumer : IConsumer<BookingUpdated>
    {
        private readonly ILogger<BookingUpdatedConsumer> _logger;
        private readonly INotificationService _notificationService;

        public BookingUpdatedConsumer(ILogger<BookingUpdatedConsumer> logger, INotificationService notificationService)
        {
            _logger = logger;
            _notificationService = notificationService;
        }

        public async Task Consume(ConsumeContext<BookingUpdated> context)
        {
            var msg = context.Message;
            _logger.LogInformation("Received BookingUpdated: {BookingId} -> {Status}", msg.BookingId, msg.Status);

            if (msg.UserId.HasValue)
            {
                await _notificationService.CreateAsync(new DTOs.CreateNotificationDto
                {
                    Title = "Ažuriran status rezervacije",
                    Message = $"Vaša rezervacija #{msg.BookingId} je ažurirana na status: {msg.Status}.",
                    Type = "Booking",
                    Priority = "Normal",
                    UserId = msg.UserId.Value,
                    BookingId = msg.BookingId
                });
            }
        }
    }
}



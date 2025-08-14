using API.Contracts.Messages;
using API.Interfaces;
using MassTransit;

namespace API.Messaging.Consumers
{
    public class BookingCreatedConsumer : IConsumer<BookingCreated>
    {
        private readonly ILogger<BookingCreatedConsumer> _logger;
        private readonly INotificationService _notificationService;

        public BookingCreatedConsumer(ILogger<BookingCreatedConsumer> logger, INotificationService notificationService)
        {
            _logger = logger;
            _notificationService = notificationService;
        }

        public async Task Consume(ConsumeContext<BookingCreated> context)
        {
            var msg = context.Message;
            _logger.LogInformation("Received BookingCreated: {BookingId}", msg.BookingId);

            if (msg.UserId.HasValue)
            {
                await _notificationService.CreateAsync(new DTOs.CreateNotificationDto
                {
                    Title = "Rezervacija kreirana",
                    Message = $"Vaša rezervacija #{msg.BookingId} je uspješno kreirana. Check-in: {msg.CheckIn:d}.",
                    Type = "Booking",
                    Priority = "Normal",
                    UserId = msg.UserId.Value,
                    BookingId = msg.BookingId
                });
            }
        }
    }
}



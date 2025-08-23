using Contracts.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;
using Persistence.Interfaces;

namespace API.Messaging.Consumers
{
    public class BookingConfirmedConsumer : IConsumer<BookingConfirmed>
    {
        private readonly ILogger<BookingConfirmedConsumer> _logger;
        private readonly INotificationService _notificationService;

        public BookingConfirmedConsumer(
            ILogger<BookingConfirmedConsumer> logger,
            INotificationService notificationService)
        {
            _logger = logger;
            _notificationService = notificationService;
        }

        public async Task Consume(ConsumeContext<BookingConfirmed> context)
        {
            var msg = context.Message;
            _logger.LogInformation("Received BookingConfirmed: {BookingId} for User {UserId}", msg.BookingId, msg.UserId);

            try
            {
                // Send confirmation notification to user
                if (msg.UserId.HasValue)
                {
                    await _notificationService.CreateAsync(new Contracts.DTOs.CreateNotificationDto
                    {
                        Title = "Rezervacija potvrđena",
                        Message = $"Vaša rezervacija je uspešno potvrđena. Možete se prijaviti u hotel na planirani datum.",
                        Type = "Booking",
                        Priority = "Medium",
                        UserId = msg.UserId.Value,
                        BookingId = msg.BookingId
                    });
                }

                // TODO: Send notification to hotel staff about new confirmed booking
                // This could be implemented by creating a separate notification for hotel staff
                // or by adding a specific notification type for staff notifications

                _logger.LogInformation("Booking confirmation processed successfully for booking {BookingId}", msg.BookingId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing BookingConfirmed event for booking {BookingId}", msg.BookingId);
                throw;
            }
        }
    }
}

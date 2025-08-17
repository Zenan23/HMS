using Contracts.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;
using Persistence.Interfaces;

namespace API.Messaging.Consumers
{
    public class UpcomingCheckInReminderConsumer : IConsumer<UpcomingCheckInReminder>
    {
        private readonly ILogger<UpcomingCheckInReminderConsumer> _logger;
        private readonly INotificationService _notificationService;

        public UpcomingCheckInReminderConsumer(ILogger<UpcomingCheckInReminderConsumer> logger, INotificationService notificationService)
        {
            _logger = logger;
            _notificationService = notificationService;
        }

        public async Task Consume(ConsumeContext<UpcomingCheckInReminder> context)
        {
            var msg = context.Message;
            _logger.LogInformation("Received UpcomingCheckInReminder for Booking {BookingId}", msg.BookingId);

            if (msg.UserId.HasValue)
            {
                await _notificationService.CreateAsync(new Contracts.DTOs.CreateNotificationDto
                {
                    Title = "Podsjetnik za check-in",
                    Message = $"Sutra imate check-in ({msg.CheckIn:d}). Vidimo se!",
                    Type = "Reminder",
                    Priority = "Normal",
                    UserId = msg.UserId.Value,
                    BookingId = msg.BookingId
                });
            }
        }
    }
}



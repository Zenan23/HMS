using Contracts.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;
using Persistence.Interfaces;

namespace API.Messaging.Consumers
{
    public class PaymentCompletedConsumer : IConsumer<PaymentCompleted>
    {
        private readonly ILogger<PaymentCompletedConsumer> _logger;
        private readonly INotificationService _notificationService;

        public PaymentCompletedConsumer(ILogger<PaymentCompletedConsumer> logger, INotificationService notificationService)
        {
            _logger = logger;
            _notificationService = notificationService;
        }

        public async Task Consume(ConsumeContext<PaymentCompleted> context)
        {
            var msg = context.Message;
            _logger.LogInformation("Received PaymentCompleted: {PaymentId} for Booking {BookingId}", msg.PaymentId, msg.BookingId);

            if (msg.UserId.HasValue)
            {
                await _notificationService.CreateAsync(new Contracts.DTOs.CreateNotificationDto
                {
                    Title = "Uspješno plaćanje",
                    Message = $"Uplata {msg.Amount:C} je evidentirana. Transakcija: {msg.TransactionId}",
                    Type = "Payment",
                    Priority = "High",
                    UserId = msg.UserId.Value,
                    BookingId = msg.BookingId
                });
            }
        }
    }
}



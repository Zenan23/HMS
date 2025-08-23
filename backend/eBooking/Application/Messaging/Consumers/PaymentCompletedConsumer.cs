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
        private readonly IBookingService _bookingService;

        public PaymentCompletedConsumer(
            ILogger<PaymentCompletedConsumer> logger, 
            INotificationService notificationService,
            IBookingService bookingService)
        {
            _logger = logger;
            _notificationService = notificationService;
            _bookingService = bookingService;
        }

        public async Task Consume(ConsumeContext<PaymentCompleted> context)
        {
            var msg = context.Message;
            _logger.LogInformation("Received PaymentCompleted: {PaymentId} for Booking {BookingId}", msg.PaymentId, msg.BookingId);

            try
            {
                // Confirm the booking after successful payment
                var bookingConfirmed = await _bookingService.ConfirmBookingAfterPaymentAsync(msg.BookingId, msg.PaymentId);
                
                if (bookingConfirmed)
                {
                    _logger.LogInformation("Booking {BookingId} successfully confirmed after payment {PaymentId}", msg.BookingId, msg.PaymentId);
                }
                else
                {
                    _logger.LogWarning("Failed to confirm booking {BookingId} after payment {PaymentId}", msg.BookingId, msg.PaymentId);
                }

                // Send notification to user
                if (msg.UserId.HasValue)
                {
                    await _notificationService.CreateAsync(new Contracts.DTOs.CreateNotificationDto
                    {
                        Title = "Uspješno plaćanje i potvrda rezervacije",
                        Message = $"Uplata {msg.Amount:C} je evidentirana i vaša rezervacija je potvrđena. Transakcija: {msg.TransactionId}",
                        Type = "Payment",
                        Priority = "High",
                        UserId = msg.UserId.Value,
                        BookingId = msg.BookingId
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing PaymentCompleted event for payment {PaymentId} and booking {BookingId}", msg.PaymentId, msg.BookingId);
                throw;
            }
        }
    }
}



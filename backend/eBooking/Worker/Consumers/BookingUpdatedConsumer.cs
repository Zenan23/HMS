using Contracts.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;
using Persistence.Interfaces;

namespace Worker.Consumers
{
    public class BookingUpdatedConsumer : IConsumer<BookingUpdated>
    {
        private readonly ILogger<BookingUpdatedConsumer> _logger;
        private readonly INotificationService _notificationService;

        // msg.Status je BookingStatus.ToString() (engleski naziv enum člana) — prevodi se ovdje, u
        // trenutku slanja notifikacije korisniku, umjesto da se engleski tekst šalje direktno.
        // Mora se poklapati sa Reservation.statusLabel na mobile strani (reservation.dart).
        private static readonly Dictionary<string, string> StatusLabelsBs = new()
        {
            ["Pending"] = "Na čekanju",
            ["Confirmed"] = "Potvrđena",
            ["CheckedIn"] = "Check-in",
            ["CheckedOut"] = "Check-out",
            ["Cancelled"] = "Otkazana",
            ["NoShow"] = "No-show",
        };

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
                var statusBs = StatusLabelsBs.GetValueOrDefault(msg.Status, msg.Status);
                await _notificationService.CreateAsync(new Contracts.DTOs.CreateNotificationDto
                {
                    Title = "Ažuriran status rezervacije",
                    Message = $"Vaša rezervacija #{msg.BookingId} je ažurirana na status: {statusBs}.",
                    Type = "Booking",
                    Priority = "Normal",
                    UserId = msg.UserId.Value,
                    BookingId = msg.BookingId
                });
            }
        }
    }
}

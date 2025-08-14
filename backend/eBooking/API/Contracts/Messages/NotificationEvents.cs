namespace API.Contracts.Messages
{
    public record NotificationCreated(int NotificationId, int UserId);
    public record UpcomingCheckInReminder(int BookingId, int? UserId, DateTime CheckIn);
}



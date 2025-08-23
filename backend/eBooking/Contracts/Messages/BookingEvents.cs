namespace Contracts.Messages
{
    public record BookingUpdated(int BookingId, string Status, int? UserId, int RoomId);
    public record BookingConfirmed(int BookingId, int? UserId, int PaymentId);
}



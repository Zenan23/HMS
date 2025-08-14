namespace API.Contracts.Messages
{
    public record BookingCreated(int BookingId, int? UserId, int RoomId, int HotelId, DateTime CheckIn, DateTime CheckOut);
    public record BookingUpdated(int BookingId, string Status, int? UserId, int RoomId);
}



using API.Models;

namespace API.Interfaces
{
    public interface IBookingRepository : IRepository<Booking>
    {
        Task<IEnumerable<Booking>> GetBookingsByGuestAsync(int guestId);
        Task<IEnumerable<Booking>> GetBookingsByRoomAsync(int roomId);
        Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkIn, DateTime checkOut);
    }
}

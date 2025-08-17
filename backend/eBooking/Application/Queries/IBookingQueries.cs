using Persistence.Models;

namespace Application.Queries
{
    public interface IBookingQueries
    {
        Task<IEnumerable<int>> GetBookingIdsByRoomAsync(int roomId);
        Task<IEnumerable<Booking>> GetOverlappingActiveBookingsAsync(int roomId, DateTime checkIn, DateTime checkOut, int? excludeBookingId = null);
    }
}



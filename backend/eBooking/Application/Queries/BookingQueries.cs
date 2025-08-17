using Contracts.Enums;
using Microsoft.Extensions.Logging;
using Persistence.Interfaces;
using Persistence.Models;

namespace Application.Queries
{
    public class BookingQueries : IBookingQueries
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly ILogger<BookingQueries> _logger;

        public BookingQueries(IBookingRepository bookingRepository, ILogger<BookingQueries> logger)
        {
            _bookingRepository = bookingRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<int>> GetBookingIdsByRoomAsync(int roomId)
        {
            var bookings = await _bookingRepository.GetAllAsync();
            return bookings.Where(b => b.RoomId == roomId).Select(b => b.Id).ToList();
        }

        public async Task<IEnumerable<Booking>> GetOverlappingActiveBookingsAsync(int roomId, DateTime checkIn, DateTime checkOut, int? excludeBookingId = null)
        {
            var bookings = await _bookingRepository.GetAllAsync();
            var overlapping = bookings.Where(b =>
                !b.IsDeleted &&
                b.RoomId == roomId &&
                b.Status != BookingStatus.Cancelled &&
                b.Status != BookingStatus.CheckedOut &&
                (excludeBookingId == null || b.Id != excludeBookingId) &&
                b.CheckInDate < checkOut &&
                b.CheckOutDate > checkIn);
            return overlapping.ToList();
        }
    }
}



using API.DTOs;
using API.Enums;

namespace API.Interfaces
{
    public interface IBookingService : IBaseService<BookingDto, CreateBookingDto, UpdateBookingDto>
    {
        Task<IEnumerable<BookingDto>> GetByGuestIdAsync(int guestId);
        Task<IEnumerable<BookingDto>> GetByUserIdAsync(int userId);
        Task<IEnumerable<BookingDto>> GetByRoomIdAsync(int roomId);
        Task<IEnumerable<BookingDto>> GetByStatusAsync(BookingStatus status);
        Task<bool> CancelBookingAsync(int id, int? cancelledByUserId = null);
        Task<bool> CheckInAsync(int id, int? checkedInByUserId = null);
        Task<bool> CheckOutAsync(int id, int? checkedOutByUserId = null);
        Task<IEnumerable<BookingDto>> GetBookingsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkIn, DateTime checkOut, int? excludeBookingId = null);
        Task<IEnumerable<BookingDto>> GetPaidBookingsByUserIdAsync(int userId);
        Task<IEnumerable<BookingDto>> GetNoPaidBookingsByUserIdAsync(int userId);
    }

}

using API.DTOs;

namespace API.Interfaces
{
    public interface IBookingStatusHistoryService : IBaseService<BookingStatusHistoryDto, CreateBookingStatusHistoryDto, UpdateBookingStatusHistoryDto>
    {
        Task<IEnumerable<BookingStatusHistoryDto>> GetByBookingIdAsync(int bookingId);
        Task<IEnumerable<BookingStatusHistoryDto>> GetByUserIdAsync(int userId);
    }
}

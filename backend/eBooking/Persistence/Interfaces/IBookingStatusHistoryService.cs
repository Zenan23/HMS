using Contracts.DTOs;

namespace Persistence.Interfaces
{
    public interface IBookingStatusHistoryService : IBaseService<BookingStatusHistoryDto, CreateBookingStatusHistoryDto, UpdateBookingStatusHistoryDto>
    {
        Task<IEnumerable<BookingStatusHistoryDto>> GetByBookingIdAsync(int bookingId);
        Task<IEnumerable<BookingStatusHistoryDto>> GetByUserIdAsync(int userId);
    }
}

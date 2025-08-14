using API.DTOs;
using API.Interfaces;
using API.Models;
using AutoMapper;

namespace API.Services
{
    public class BookingStatusHistoryService : BaseDtoService<BookingStatusHistory, BookingStatusHistoryDto, CreateBookingStatusHistoryDto, UpdateBookingStatusHistoryDto>, IBookingStatusHistoryService
    {
        public BookingStatusHistoryService(
            IRepository<BookingStatusHistory> repository,
            IMapper mapper,
            ILogger<BookingStatusHistoryService> logger)
            : base(repository, mapper, logger)
        {
        }

        public async Task<IEnumerable<BookingStatusHistoryDto>> GetByBookingIdAsync(int bookingId)
        {
            try
            {
                _logger.LogInformation("Getting BookingStatusHistory entries for booking ID: {BookingId}", bookingId);
                var entities = await _repository.GetAllAsync();
                var filteredEntities = entities.Where(h => h.BookingId == bookingId && !h.IsDeleted);
                return _mapper.Map<IEnumerable<BookingStatusHistoryDto>>(filteredEntities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting BookingStatusHistory entries for booking ID: {BookingId}", bookingId);
                throw;
            }
        }

        public async Task<IEnumerable<BookingStatusHistoryDto>> GetByUserIdAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Getting BookingStatusHistory entries for user ID: {UserId}", userId);
                var entities = await _repository.GetAllAsync();
                var filteredEntities = entities.Where(h => h.ChangedByUserId == userId && !h.IsDeleted);
                return _mapper.Map<IEnumerable<BookingStatusHistoryDto>>(filteredEntities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting BookingStatusHistory entries for user ID: {UserId}", userId);
                throw;
            }
        }
    }
}

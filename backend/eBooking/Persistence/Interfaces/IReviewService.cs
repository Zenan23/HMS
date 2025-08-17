using Contracts.DTOs;

namespace Persistence.Interfaces
{
    public interface IReviewService : IBaseService<ReviewDto, CreateReviewDto, UpdateReviewDto>
    {
        Task<IEnumerable<ReviewDto>> GetByHotelIdAsync(int hotelId);
        Task<IEnumerable<ReviewDto>> GetByUserIdAsync(int userId);
        Task<IEnumerable<ReviewDto>> GetByBookingIdAsync(int bookingId);
        Task<IEnumerable<ReviewDto>> GetPendingReviewsAsync();
        Task<bool> ApproveReviewAsync(int reviewId, int? approvedByUserId = null);
        Task<bool> RejectReviewAsync(int reviewId, int? rejectedByUserId = null);
        Task<double> GetAverageRatingAsync(int hotelId);
        Task<IEnumerable<ReviewDto>> GetByRatingAsync(int rating);
        Task<ReviewStatistics> GetReviewStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
    }
}

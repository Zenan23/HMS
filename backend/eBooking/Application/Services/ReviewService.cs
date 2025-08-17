using AutoMapper;
using Contracts.DTOs;
using Microsoft.Extensions.Logging;
using Persistence.Interfaces;
using Persistence.Models;

namespace Application.Services
{
    public class ReviewService : BaseDtoService<Review, ReviewDto, CreateReviewDto, UpdateReviewDto>, IReviewService
    {
        public ReviewService(
            IRepository<Review> repository,
            IMapper mapper,
            ILogger<ReviewService> logger)
            : base(repository, mapper, logger)
        {
        }

        public async Task<IEnumerable<ReviewDto>> GetByHotelIdAsync(int hotelId)
        {
            try
            {
                _logger.LogInformation("Getting reviews for hotel ID: {HotelId}", hotelId);
                var entities = await _repository.GetAllAsync();
                var filteredEntities = entities.Where(r => r.HotelId == hotelId && !r.IsDeleted && r.IsApproved)
                                             .OrderByDescending(r => r.ReviewDate);
                return _mapper.Map<IEnumerable<ReviewDto>>(filteredEntities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reviews for hotel ID: {HotelId}", hotelId);
                throw;
            }
        }

        public async Task<IEnumerable<ReviewDto>> GetByUserIdAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Getting reviews for user ID: {UserId}", userId);
                var entities = await _repository.GetAllAsync();
                var filteredEntities = entities.Where(r => r.UserId == userId && !r.IsDeleted)
                                             .OrderByDescending(r => r.ReviewDate);
                return _mapper.Map<IEnumerable<ReviewDto>>(filteredEntities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reviews for user ID: {UserId}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<ReviewDto>> GetByBookingIdAsync(int bookingId)
        {
            try
            {
                _logger.LogInformation("Getting reviews for booking ID: {BookingId}", bookingId);
                var entities = await _repository.GetAllAsync();
                var filteredEntities = entities.Where(r => r.BookingId == bookingId && !r.IsDeleted)
                                             .OrderByDescending(r => r.ReviewDate);
                return _mapper.Map<IEnumerable<ReviewDto>>(filteredEntities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reviews for booking ID: {BookingId}", bookingId);
                throw;
            }
        }

        public async Task<IEnumerable<ReviewDto>> GetPendingReviewsAsync()
        {
            try
            {
                _logger.LogInformation("Getting pending reviews");
                var entities = await _repository.GetAllAsync();
                var filteredEntities = entities.Where(r => !r.IsApproved && !r.IsDeleted)
                                             .OrderBy(r => r.ReviewDate);
                return _mapper.Map<IEnumerable<ReviewDto>>(filteredEntities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending reviews");
                throw;
            }
        }

        public async Task<bool> ApproveReviewAsync(int reviewId, int? approvedByUserId = null)
        {
            try
            {
                _logger.LogInformation("Approving review {ReviewId}", reviewId);

                var review = await _repository.GetByIdAsync(reviewId);
                if (review == null || review.IsDeleted)
                {
                    _logger.LogWarning("Review {ReviewId} not found for approval", reviewId);
                    return false;
                }

                review.IsApproved = true;
                review.UpdatedAt = DateTime.UtcNow;

                await _repository.UpdateAsync(review);

                _logger.LogInformation("Review {ReviewId} approved successfully", reviewId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving review {ReviewId}", reviewId);
                throw;
            }
        }

        public async Task<bool> RejectReviewAsync(int reviewId, int? rejectedByUserId = null)
        {
            try
            {
                _logger.LogInformation("Rejecting review {ReviewId}", reviewId);

                var review = await _repository.GetByIdAsync(reviewId);
                if (review == null || review.IsDeleted)
                {
                    _logger.LogWarning("Review {ReviewId} not found for rejection", reviewId);
                    return false;
                }

                review.IsApproved = false;
                review.UpdatedAt = DateTime.UtcNow;

                await _repository.UpdateAsync(review);

                _logger.LogInformation("Review {ReviewId} rejected successfully", reviewId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting review {ReviewId}", reviewId);
                throw;
            }
        }

        public async Task<double> GetAverageRatingAsync(int hotelId)
        {
            try
            {
                _logger.LogInformation("Getting average rating for hotel ID: {HotelId}", hotelId);
                var entities = await _repository.GetAllAsync();
                var approvedReviews = entities.Where(r => r.HotelId == hotelId && !r.IsDeleted && r.IsApproved);

                if (!approvedReviews.Any())
                {
                    return 0;
                }

                var average = approvedReviews.Average(r => r.Rating);
                _logger.LogInformation("Average rating for hotel {HotelId}: {Average}", hotelId, average);

                return Math.Round(average, 2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating average rating for hotel ID: {HotelId}", hotelId);
                throw;
            }
        }

        public async Task<IEnumerable<ReviewDto>> GetByRatingAsync(int rating)
        {
            try
            {
                _logger.LogInformation("Getting reviews with rating: {Rating}", rating);
                var entities = await _repository.GetAllAsync();
                var filteredEntities = entities.Where(r => r.Rating == rating && !r.IsDeleted && r.IsApproved)
                                             .OrderByDescending(r => r.ReviewDate);
                return _mapper.Map<IEnumerable<ReviewDto>>(filteredEntities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reviews with rating: {Rating}", rating);
                throw;
            }
        }

        public override async Task<ReviewDto> CreateAsync(CreateReviewDto createDto)
        {
            try
            {
                // Set review date to current time
                var review = _mapper.Map<Review>(createDto);
                review.ReviewDate = DateTime.UtcNow;
                review.CreatedAt = DateTime.UtcNow;
                review.UpdatedAt = DateTime.UtcNow;

                var createdReview = await _repository.AddAsync(review);

                _logger.LogInformation("Review created successfully with ID: {ReviewId}", createdReview.Id);

                return _mapper.Map<ReviewDto>(createdReview);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating review");
                throw;
            }
        }

        public async Task<ReviewStatistics> GetReviewStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var entities = await _repository.GetAllAsync();
                var query = entities.Where(r => !r.IsDeleted);

                if (fromDate.HasValue)
                    query = query.Where(r => r.ReviewDate >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(r => r.ReviewDate <= toDate.Value);

                var totalReviews = query.Count();
                var approvedReviews = query.Where(r => r.IsApproved).ToList();
                var averageRating = approvedReviews.Count > 0 ? approvedReviews.Average(r => (double)r.Rating) : 0;

                var fiveStarReviews = approvedReviews.Count(r => r.Rating == 5);
                var fourStarReviews = approvedReviews.Count(r => r.Rating == 4);
                var threeStarReviews = approvedReviews.Count(r => r.Rating == 3);
                var twoStarReviews = approvedReviews.Count(r => r.Rating == 2);
                var oneStarReviews = approvedReviews.Count(r => r.Rating == 1);

                // Monthly data for last 12 months
                var monthlyData = new List<MonthlyReviewData>();
                for (int i = 11; i >= 0; i--)
                {
                    var monthStart = DateTime.UtcNow.AddMonths(-i).Date.AddDays(1 - DateTime.UtcNow.AddMonths(-i).Day);
                    var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                    
                    var monthQuery = query.Where(r => r.ReviewDate >= monthStart && r.ReviewDate <= monthEnd);
                    var monthReviews = monthQuery.Where(r => r.IsApproved).ToList();
                    var monthCount = monthReviews.Count;
                    var monthAverage = monthReviews.Count > 0 ? monthReviews.Average(r => (double)r.Rating) : 0;

                    monthlyData.Add(new MonthlyReviewData
                    {
                        Month = monthStart.ToString("MMM yyyy"),
                        ReviewCount = monthCount,
                        AverageRating = Math.Round(monthAverage, 2)
                    });
                }

                return new ReviewStatistics
                {
                    TotalReviews = totalReviews,
                    AverageRating = Math.Round(averageRating, 2),
                    FiveStarReviews = fiveStarReviews,
                    FourStarReviews = fourStarReviews,
                    ThreeStarReviews = threeStarReviews,
                    TwoStarReviews = twoStarReviews,
                    OneStarReviews = oneStarReviews,
                    FromDate = fromDate,
                    ToDate = toDate,
                    MonthlyData = monthlyData
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating review statistics");
                throw;
            }
        }
    }
}

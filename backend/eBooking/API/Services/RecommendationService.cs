using API.DTOs;
using API.Interfaces;
using API.Models;
using AutoMapper;

namespace API.Services
{
    public class RecommendationService : BaseDtoService<Recommendation, RecommendationDto, CreateRecommendationDto, UpdateRecommendationDto>, IRecommendationService
    {
        private readonly IHotelService _hotelService;
        private readonly IReviewService _reviewService;

        public RecommendationService(
            IRepository<Recommendation> repository,
            IReviewService reviewService,
            IHotelService hotelService,
            IMapper mapper,
            ILogger<RecommendationService> logger)
            : base(repository, mapper, logger)
        {
            _hotelService = hotelService;
            _reviewService = reviewService;
        }

        public async Task<IEnumerable<RecommendationDto>> GetByUserIdAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Getting recommendations for user ID: {UserId}", userId);
                var entities = await _repository.GetAllAsync();
                var filteredEntities = entities.Where(r => r.UserId == userId && !r.IsDeleted && r.IsActive)
                                             .OrderByDescending(r => r.Priority)
                                             .ThenBy(r => r.ValidFrom);
                return _mapper.Map<IEnumerable<RecommendationDto>>(filteredEntities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recommendations for user ID: {UserId}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<RecommendationDto>> GetByHotelIdAsync(int hotelId)
        {
            try
            {
                _logger.LogInformation("Getting recommendations for hotel ID: {HotelId}", hotelId);
                var entities = await _repository.GetAllAsync();
                var filteredEntities = entities.Where(r => r.HotelId == hotelId && !r.IsDeleted && r.IsActive)
                                             .OrderByDescending(r => r.Priority)
                                             .ThenBy(r => r.ValidFrom);
                return _mapper.Map<IEnumerable<RecommendationDto>>(filteredEntities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recommendations for hotel ID: {HotelId}", hotelId);
                throw;
            }
        }

        public async Task<IEnumerable<RecommendationDto>> GetByTypeAsync(string type)
        {
            try
            {
                _logger.LogInformation("Getting recommendations for type: {Type}", type);
                var entities = await _repository.GetAllAsync();
                var filteredEntities = entities.Where(r => r.Type.Equals(type, StringComparison.OrdinalIgnoreCase) && !r.IsDeleted && r.IsActive)
                                             .OrderByDescending(r => r.Priority)
                                             .ThenBy(r => r.ValidFrom);
                return _mapper.Map<IEnumerable<RecommendationDto>>(filteredEntities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recommendations for type: {Type}", type);
                throw;
            }
        }

        /// <summary>
        /// User-based collaborative filtering hotel recommendations
        /// </summary>
        public async Task<IEnumerable<HotelDto>> GetUserBasedHotelRecommendationsAsync(int userId)
        {
            var allReviews = await _reviewService.GetAllAsync();
            var allHotels = await _hotelService.GetAllAsync();

            // Reviews of current user
            var userReviews = allReviews.Where(r => r.UserId == userId && r.IsApproved && !r.IsDeleted).ToList();
            var userHotelIds = userReviews.Select(r => r.HotelId).Distinct().ToList();

            // Find similar users (who rated same hotels)
            var similarUsers = allReviews.Where(r => userHotelIds.Contains(r.HotelId) && r.UserId != userId)
                                        .Select(r => r.UserId)
                                        .Distinct()
                                        .ToList();

            // For each similar user, get their reviews
            var similarUserReviews = allReviews.Where(r => similarUsers.Contains(r.UserId ?? 0) && r.IsApproved && !r.IsDeleted).ToList();

            // Calculate average rating per hotel by similar users
            var recommendedHotels = similarUserReviews
                .Where(r => !userHotelIds.Contains(r.HotelId) && r.Rating >= 4) // Only hotels not rated by current user, and with high rating
                .GroupBy(r => r.HotelId)
                .Select(g => new { HotelId = g.Key, AvgRating = g.Average(r => r.Rating), Count = g.Count() })
                .OrderByDescending(h => h.AvgRating)
                .ThenByDescending(h => h.Count)
                .Take(3) // Top 3
                .ToList();

            // Get hotel entities
            var hotels = allHotels.Where(h => recommendedHotels.Select(r => r.HotelId).Contains(h.Id)).ToList();

            // Fallback: ako nema sličnih korisnika ili preporuka, vrati top hotele po prosječnom ratingu
            if (hotels.Count == 0)
            {
                var topByRating = allReviews
                    .Where(r => r.IsApproved && !r.IsDeleted)
                    .GroupBy(r => r.HotelId)
                    .Select(g => new { HotelId = g.Key, Avg = g.Average(x => (double)x.Rating), Cnt = g.Count() })
                    .Where(x => x.Avg >= 4.0)
                    .OrderByDescending(x => x.Avg)
                    .ThenByDescending(x => x.Cnt)
                    .Take(3)
                    .Select(x => x.HotelId)
                    .ToHashSet();

                hotels = allHotels.Where(h => topByRating.Contains(h.Id)).ToList();
            }

            return _mapper.Map<IEnumerable<HotelDto>>(hotels);
        }

        public async Task<IEnumerable<RecommendationDto>> GetActiveRecommendationsAsync()
        {
            try
            {
                _logger.LogInformation("Getting active recommendations");
                var now = DateTime.UtcNow;
                var entities = await _repository.GetAllAsync();
                var filteredEntities = entities.Where(r => r.IsActive && !r.IsDeleted &&
                                                          r.ValidFrom <= now &&
                                                          (r.ValidTo == null || r.ValidTo >= now))
                                             .OrderByDescending(r => r.Priority)
                                             .ThenBy(r => r.ValidFrom);
                return _mapper.Map<IEnumerable<RecommendationDto>>(filteredEntities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active recommendations");
                throw;
            }
        }
    }
}

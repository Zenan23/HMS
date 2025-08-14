using API.DTOs;

namespace API.Interfaces
{
    public interface IRecommendationService : IBaseService<RecommendationDto, CreateRecommendationDto, UpdateRecommendationDto>
    {
        Task<IEnumerable<RecommendationDto>> GetByUserIdAsync(int userId);
        Task<IEnumerable<RecommendationDto>> GetByHotelIdAsync(int hotelId);
        Task<IEnumerable<RecommendationDto>> GetByTypeAsync(string type);
        Task<IEnumerable<RecommendationDto>> GetActiveRecommendationsAsync();
    /// <summary>
    /// Get recommended hotels for a user using user-based collaborative filtering
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>List of recommended hotels</returns>
    Task<IEnumerable<HotelDto>> GetUserBasedHotelRecommendationsAsync(int userId);
    }
}

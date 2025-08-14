using API.DTOs;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RecommendationsController : BaseController<RecommendationDto, CreateRecommendationDto, UpdateRecommendationDto>
    {
        private readonly IRecommendationService _recommendationService;

        public RecommendationsController(
            IRecommendationService recommendationService,
            ILogger<RecommendationsController> logger)
            : base(recommendationService, logger)
        {
            _recommendationService = recommendationService;
        }


        /// <summary>
        /// Get user-based hotel recommendations (collaborative filtering)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of recommended hotels</returns>
        [HttpGet("user/{userId}/hotels")]
        public async Task<ActionResult<ApiResponse<IEnumerable<HotelDto>>>> GetUserBasedHotelRecommendations([FromRoute] int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest(ApiResponse<IEnumerable<HotelDto>>.ErrorResult("Invalid user ID."));
                }

                var hotels = await _recommendationService.GetUserBasedHotelRecommendationsAsync(userId);
                return Ok(ApiResponse<IEnumerable<HotelDto>>.SuccessResult(hotels, "Recommended hotels retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recommended hotels for user ID: {UserId}", userId);
                return StatusCode(500, ApiResponse<IEnumerable<HotelDto>>.ErrorResult("An error occurred while retrieving recommended hotels."));
            }
        }

        /// <summary>
        /// Get recommendations by user ID
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of recommendations</returns>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<RecommendationDto>>>> GetByUserId([FromRoute] int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest(ApiResponse<IEnumerable<RecommendationDto>>.ErrorResult("Invalid user ID."));
                }

                var recommendations = await _recommendationService.GetByUserIdAsync(userId);
                return Ok(ApiResponse<IEnumerable<RecommendationDto>>.SuccessResult(recommendations, "Recommendations retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recommendations for user ID: {UserId}", userId);
                return StatusCode(500, ApiResponse<IEnumerable<RecommendationDto>>.ErrorResult("An error occurred while retrieving recommendations."));
            }
        }

        /// <summary>
        /// Get recommendations by hotel ID
        /// </summary>
        /// <param name="hotelId">Hotel ID</param>
        /// <returns>List of recommendations</returns>
        [HttpGet("hotel/{hotelId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<RecommendationDto>>>> GetByHotelId([FromRoute] int hotelId)
        {
            try
            {
                if (hotelId <= 0)
                {
                    return BadRequest(ApiResponse<IEnumerable<RecommendationDto>>.ErrorResult("Invalid hotel ID."));
                }

                var recommendations = await _recommendationService.GetByHotelIdAsync(hotelId);
                return Ok(ApiResponse<IEnumerable<RecommendationDto>>.SuccessResult(recommendations, "Recommendations retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recommendations for hotel ID: {HotelId}", hotelId);
                return StatusCode(500, ApiResponse<IEnumerable<RecommendationDto>>.ErrorResult("An error occurred while retrieving recommendations."));
            }
        }

        /// <summary>
        /// Get recommendations by type
        /// </summary>
        /// <param name="type">Recommendation type</param>
        /// <returns>List of recommendations</returns>
        [HttpGet("type/{type}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<RecommendationDto>>>> GetByType([FromRoute] string type)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(type))
                {
                    return BadRequest(ApiResponse<IEnumerable<RecommendationDto>>.ErrorResult("Recommendation type is required."));
                }

                var recommendations = await _recommendationService.GetByTypeAsync(type);
                return Ok(ApiResponse<IEnumerable<RecommendationDto>>.SuccessResult(recommendations, "Recommendations retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recommendations for type: {Type}", type);
                return StatusCode(500, ApiResponse<IEnumerable<RecommendationDto>>.ErrorResult("An error occurred while retrieving recommendations."));
            }
        }

        /// <summary>
        /// Get active recommendations
        /// </summary>
        /// <returns>List of active recommendations</returns>
        [HttpGet("active")]
        public async Task<ActionResult<ApiResponse<IEnumerable<RecommendationDto>>>> GetActiveRecommendations()
        {
            try
            {
                var recommendations = await _recommendationService.GetActiveRecommendationsAsync();
                return Ok(ApiResponse<IEnumerable<RecommendationDto>>.SuccessResult(recommendations, "Active recommendations retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active recommendations");
                return StatusCode(500, ApiResponse<IEnumerable<RecommendationDto>>.ErrorResult("An error occurred while retrieving active recommendations."));
            }
        }
    }
}

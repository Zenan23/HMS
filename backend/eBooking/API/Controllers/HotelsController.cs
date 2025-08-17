using API.Attributes;
using AutoMapper;
using Contracts.DTOs;
using Contracts.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Persistence.Interfaces;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class HotelsController : BaseController<HotelDto, CreateHotelDto, UpdateHotelDto>
    {
        private readonly IHotelService _hotelService;
        private readonly IMapper _mapper;

        public HotelsController(
            IHotelService hotelService,
            IMapper mapper,
            ILogger<HotelsController> logger)
            : base(hotelService, logger)
        {
            _hotelService = hotelService;
            _mapper = mapper;
        }

        /// <summary>
        /// Get hotels by city
        /// </summary>
        /// <param name="city">City name</param>
        /// <returns>Hotels in the specified city</returns>
        [HttpGet("by-city/{city}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<HotelDto>>>> GetHotelsByCity([FromRoute] string city)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(city))
                {
                    return BadRequest(ApiResponse<IEnumerable<HotelDto>>.ErrorResult("City parameter cannot be empty."));
                }

                var hotels = await _hotelService.GetHotelsByCityAsync(city);
                var hotelDtos = _mapper.Map<IEnumerable<HotelDto>>(hotels);

                return Ok(ApiResponse<IEnumerable<HotelDto>>.SuccessResult(
                    hotelDtos,
                    $"Hotels in {city} retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving hotels for city: {City}", city);
                return StatusCode(500, ApiResponse<IEnumerable<HotelDto>>.ErrorResult("An error occurred while retrieving hotels."));
            }
        }

        /// <summary>
        /// Get hotels by city
        /// </summary>
        /// <param name="city">City name</param>
        /// <returns>Hotels in the specified city</returns>
        [HttpGet("by-name/{name}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<HotelDto>>>> GetHotelsByName([FromRoute] string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return BadRequest(ApiResponse<IEnumerable<HotelDto>>.ErrorResult("City parameter cannot be empty."));
                }

                var hotels = await _hotelService.GetHotelsByNameAsync(name);
                var hotelDtos = _mapper.Map<IEnumerable<HotelDto>>(hotels);

                return Ok(ApiResponse<IEnumerable<HotelDto>>.SuccessResult(
                    hotelDtos,
                    $"Hotels in {name} retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving hotels for name: {name}", name);
                return StatusCode(500, ApiResponse<IEnumerable<HotelDto>>.ErrorResult("An error occurred while retrieving hotels."));
            }
        }

        /// <summary>
        /// Get hotels by star rating
        /// </summary>
        /// <param name="starRating">Star rating (1-5)</param>
        /// <returns>Hotels with the specified star rating</returns>
        [HttpGet("by-rating/{starRating}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<HotelDto>>>> GetHotelsByRating([FromRoute] int starRating)
        {
            try
            {
                if (starRating < 1 || starRating > 5)
                {
                    return BadRequest(ApiResponse<IEnumerable<HotelDto>>.ErrorResult("Star rating must be between 1 and 5."));
                }

                // Get all hotels and filter by star rating
                var allHotels = await _hotelService.GetAllHotelsAsync();
                var filteredHotels = allHotels.Where(h => h.AverageRating >= starRating && h.AverageRating < starRating + 1);

                return Ok(ApiResponse<IEnumerable<HotelDto>>.SuccessResult(
                    filteredHotels,
                    $"Hotels with {starRating} star rating retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving hotels with star rating: {StarRating}", starRating);
                return StatusCode(500, ApiResponse<IEnumerable<HotelDto>>.ErrorResult("An error occurred while retrieving hotels."));
            }
        }

        /// <summary>
        /// Create a new hotel (Admin/Manager only)
        /// </summary>
        /// <param name="createDto">Hotel creation data</param>
        /// <returns>Created hotel</returns>
        [HttpPost]
        [AuthorizeRole(UserRole.Admin)]
        public override async Task<ActionResult<ApiResponse<HotelDto>>> Create([FromBody] CreateHotelDto createDto)
        {
            return await base.Create(createDto);
        }

        /// <summary>
        /// Update an existing hotel (Admin/Manager only)
        /// </summary>
        /// <param name="id">Hotel ID</param>
        /// <param name="updateDto">Hotel update data</param>
        /// <returns>Updated hotel</returns>
        [HttpPut("{id}")]
        [AuthorizeRole(UserRole.Admin)]
        public override async Task<ActionResult<ApiResponse<HotelDto>>> Update([FromRoute] int id, [FromBody] UpdateHotelDto updateDto)
        {
            return await base.Update(id, updateDto);
        }

        /// <summary>
        /// Delete a hotel (Admin only)
        /// </summary>
        /// <param name="id">Hotel ID</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("{id}")]
        [AuthorizeRole(UserRole.Admin)]
        public override async Task<ActionResult<ApiResponse<bool>>> Delete([FromRoute] int id)
        {
            return await base.Delete(id);
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

                var hotels = await _hotelService.GetUserBasedHotelRecommendationsAsync(userId);
                return Ok(ApiResponse<IEnumerable<HotelDto>>.SuccessResult(hotels, "Recommended hotels retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recommended hotels for user ID: {UserId}", userId);
                return StatusCode(500, ApiResponse<IEnumerable<HotelDto>>.ErrorResult("An error occurred while retrieving recommended hotels."));
            }
        }
    }
}

using Contracts.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Persistence.Interfaces;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ServicesController : BaseController<ServiceDto, CreateServiceDto, UpdateServiceDto>
    {
        private readonly IServiceService _serviceService;

        public ServicesController(
            IServiceService serviceService,
            ILogger<ServicesController> logger)
            : base(serviceService, logger)
        {
            _serviceService = serviceService;
        }

        /// <summary>
        /// Get services by hotel ID
        /// </summary>
        /// <param name="hotelId">Hotel ID</param>
        /// <returns>List of services</returns>
        [HttpGet("hotel/{hotelId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ServiceDto>>>> GetByHotelId([FromRoute] int hotelId)
        {
            try
            {
                if (hotelId <= 0)
                {
                    return BadRequest(ApiResponse<IEnumerable<ServiceDto>>.ErrorResult("Invalid hotel ID."));
                }

                var services = await _serviceService.GetByHotelIdAsync(hotelId);
                return Ok(ApiResponse<IEnumerable<ServiceDto>>.SuccessResult(services, "Services retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving services for hotel ID: {HotelId}", hotelId);
                return StatusCode(500, ApiResponse<IEnumerable<ServiceDto>>.ErrorResult("An error occurred while retrieving services."));
            }
        }

        /// <summary>
        /// Get services by category
        /// </summary>
        /// <param name="category">Service category</param>
        /// <returns>List of services</returns>
        [HttpGet("category/{category}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ServiceDto>>>> GetByCategory([FromRoute] string category)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(category))
                {
                    return BadRequest(ApiResponse<IEnumerable<ServiceDto>>.ErrorResult("Service category is required."));
                }

                var services = await _serviceService.GetByCategoryAsync(category);
                return Ok(ApiResponse<IEnumerable<ServiceDto>>.SuccessResult(services, "Services retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving services for category: {Category}", category);
                return StatusCode(500, ApiResponse<IEnumerable<ServiceDto>>.ErrorResult("An error occurred while retrieving services."));
            }
        }

        /// <summary>
        /// Get available services
        /// </summary>
        /// <returns>List of available services</returns>
        [HttpGet("available")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ServiceDto>>>> GetAvailableServices()
        {
            try
            {
                var services = await _serviceService.GetAvailableServicesAsync();
                return Ok(ApiResponse<IEnumerable<ServiceDto>>.SuccessResult(services, "Available services retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available services");
                return StatusCode(500, ApiResponse<IEnumerable<ServiceDto>>.ErrorResult("An error occurred while retrieving available services."));
            }
        }
    }
}

using API.DTOs;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BookingStatusHistoryController : BaseController<BookingStatusHistoryDto, CreateBookingStatusHistoryDto, UpdateBookingStatusHistoryDto>
    {
        private readonly IBookingStatusHistoryService _bookingStatusHistoryService;

        public BookingStatusHistoryController(
            IBookingStatusHistoryService bookingStatusHistoryService,
            ILogger<BookingStatusHistoryController> logger)
            : base(bookingStatusHistoryService, logger)
        {
            _bookingStatusHistoryService = bookingStatusHistoryService;
        }

        /// <summary>
        /// Get booking status history by booking ID
        /// </summary>
        /// <param name="bookingId">Booking ID</param>
        /// <returns>List of booking status history entries</returns>
        [HttpGet("booking/{bookingId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<BookingStatusHistoryDto>>>> GetByBookingId([FromRoute] int bookingId)
        {
            try
            {
                if (bookingId <= 0)
                {
                    return BadRequest(ApiResponse<IEnumerable<BookingStatusHistoryDto>>.ErrorResult("Invalid booking ID."));
                }

                var history = await _bookingStatusHistoryService.GetByBookingIdAsync(bookingId);
                return Ok(ApiResponse<IEnumerable<BookingStatusHistoryDto>>.SuccessResult(history, "Booking status history retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving booking status history for booking ID: {BookingId}", bookingId);
                return StatusCode(500, ApiResponse<IEnumerable<BookingStatusHistoryDto>>.ErrorResult("An error occurred while retrieving booking status history."));
            }
        }

        /// <summary>
        /// Get booking status history by user ID
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of booking status history entries</returns>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<BookingStatusHistoryDto>>>> GetByUserId([FromRoute] int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest(ApiResponse<IEnumerable<BookingStatusHistoryDto>>.ErrorResult("Invalid user ID."));
                }

                var history = await _bookingStatusHistoryService.GetByUserIdAsync(userId);
                return Ok(ApiResponse<IEnumerable<BookingStatusHistoryDto>>.SuccessResult(history, "Booking status history retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving booking status history for user ID: {UserId}", userId);
                return StatusCode(500, ApiResponse<IEnumerable<BookingStatusHistoryDto>>.ErrorResult("An error occurred while retrieving booking status history."));
            }
        }
    }
}

using API.Attributes;
using Contracts.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Persistence.Interfaces;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IBookingService _bookingService;
        private readonly IHotelService _hotelService;
        private readonly IUserService _userService;
        private readonly IReviewService _reviewService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IPaymentService paymentService,
            IBookingService bookingService,
            IHotelService hotelService,
            IUserService userService,
            IReviewService reviewService,
            ILogger<DashboardController> logger)
        {
            _paymentService = paymentService;
            _bookingService = bookingService;
            _hotelService = hotelService;
            _userService = userService;
            _reviewService = reviewService;
            _logger = logger;
        }

        /// <summary>
        /// Get comprehensive dashboard statistics
        /// </summary>
        /// <param name="fromDate">From date (optional)</param>
        /// <param name="toDate">To date (optional)</param>
        /// <returns>Dashboard statistics</returns>
        [HttpGet("statistics")]
        [AuthorizeRole(Contracts.Enums.UserRole.Admin)]
        public async Task<ActionResult<ApiResponse<DashboardStatistics>>> GetDashboardStatistics(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var paymentStats = await _paymentService.GetPaymentStatisticsAsync(fromDate, toDate);
                var bookingStats = await _bookingService.GetBookingStatisticsAsync(fromDate, toDate);
                var hotelStats = await _hotelService.GetHotelStatisticsAsync();
                var userStats = await _userService.GetUserStatisticsAsync();
                var reviewStats = await _reviewService.GetReviewStatisticsAsync(fromDate, toDate);

                var dashboardStats = new DashboardStatistics
                {
                    PaymentStats = paymentStats,
                    BookingStats = bookingStats,
                    HotelStats = hotelStats,
                    UserStats = userStats,
                    ReviewStats = reviewStats
                };

                return Ok(ApiResponse<DashboardStatistics>.SuccessResult(
                    dashboardStats,
                    "Dashboard statistics retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard statistics");
                return StatusCode(500, ApiResponse<DashboardStatistics>.ErrorResult("An error occurred while retrieving dashboard statistics."));
            }
        }

        /// <summary>
        /// Get payment statistics
        /// </summary>
        /// <param name="fromDate">From date (optional)</param>
        /// <param name="toDate">To date (optional)</param>
        /// <returns>Payment statistics</returns>
        [HttpGet("payments/statistics")]
        [AuthorizeRole(Contracts.Enums.UserRole.Admin)]
        public async Task<ActionResult<ApiResponse<Contracts.DTOs.PaymentStatistics>>> GetPaymentStatistics(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var statistics = await _paymentService.GetPaymentStatisticsAsync(fromDate, toDate);
                return Ok(ApiResponse<Contracts.DTOs.PaymentStatistics>.SuccessResult(
                    statistics,
                    "Payment statistics retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment statistics");
                return StatusCode(500, ApiResponse<Contracts.DTOs.PaymentStatistics>.ErrorResult("An error occurred while retrieving payment statistics."));
            }
        }

        /// <summary>
        /// Get booking statistics
        /// </summary>
        /// <param name="fromDate">From date (optional)</param>
        /// <param name="toDate">To date (optional)</param>
        /// <returns>Booking statistics</returns>
        [HttpGet("bookings/statistics")]
        [AuthorizeRole(Contracts.Enums.UserRole.Admin)]
        public async Task<ActionResult<ApiResponse<BookingStatistics>>> GetBookingStatistics(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var statistics = await _bookingService.GetBookingStatisticsAsync(fromDate, toDate);
                return Ok(ApiResponse<BookingStatistics>.SuccessResult(
                    statistics,
                    "Booking statistics retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving booking statistics");
                return StatusCode(500, ApiResponse<BookingStatistics>.ErrorResult("An error occurred while retrieving booking statistics."));
            }
        }

        /// <summary>
        /// Get hotel statistics
        /// </summary>
        /// <returns>Hotel statistics</returns>
        [HttpGet("hotels/statistics")]
        [AuthorizeRole(Contracts.Enums.UserRole.Admin)]
        public async Task<ActionResult<ApiResponse<HotelStatistics>>> GetHotelStatistics()
        {
            try
            {
                var statistics = await _hotelService.GetHotelStatisticsAsync();
                return Ok(ApiResponse<HotelStatistics>.SuccessResult(
                    statistics,
                    "Hotel statistics retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving hotel statistics");
                return StatusCode(500, ApiResponse<HotelStatistics>.ErrorResult("An error occurred while retrieving hotel statistics."));
            }
        }

        /// <summary>
        /// Get user statistics
        /// </summary>
        /// <returns>User statistics</returns>
        [HttpGet("users/statistics")]
        [AuthorizeRole(Contracts.Enums.UserRole.Admin)]
        public async Task<ActionResult<ApiResponse<UserStatistics>>> GetUserStatistics()
        {
            try
            {
                var statistics = await _userService.GetUserStatisticsAsync();
                return Ok(ApiResponse<UserStatistics>.SuccessResult(
                    statistics,
                    "User statistics retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user statistics");
                return StatusCode(500, ApiResponse<UserStatistics>.ErrorResult("An error occurred while retrieving user statistics."));
            }
        }

        /// <summary>
        /// Get review statistics
        /// </summary>
        /// <param name="fromDate">From date (optional)</param>
        /// <param name="toDate">To date (optional)</param>
        /// <returns>Review statistics</returns>
        [HttpGet("reviews/statistics")]
        [AuthorizeRole(Contracts.Enums.UserRole.Admin)]
        public async Task<ActionResult<ApiResponse<ReviewStatistics>>> GetReviewStatistics(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var statistics = await _reviewService.GetReviewStatisticsAsync(fromDate, toDate);
                return Ok(ApiResponse<ReviewStatistics>.SuccessResult(
                    statistics,
                    "Review statistics retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving review statistics");
                return StatusCode(500, ApiResponse<ReviewStatistics>.ErrorResult("An error occurred while retrieving review statistics."));
            }
        }
    }
}

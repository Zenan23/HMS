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
                    "Statistika za pregled je uspješno učitana."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard statistics");
                return StatusCode(500, ApiResponse<DashboardStatistics>.ErrorResult("Došlo je do greške pri učitavanju statistike za pregled."));
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
                    "Statistika plaćanja je uspješno učitana."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment statistics");
                return StatusCode(500, ApiResponse<Contracts.DTOs.PaymentStatistics>.ErrorResult("Došlo je do greške pri učitavanju statistike plaćanja."));
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
                    "Statistika rezervacija je uspješno učitana."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving booking statistics");
                return StatusCode(500, ApiResponse<BookingStatistics>.ErrorResult("Došlo je do greške pri učitavanju statistike rezervacija."));
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
                    "Statistika hotela je uspješno učitana."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving hotel statistics");
                return StatusCode(500, ApiResponse<HotelStatistics>.ErrorResult("Došlo je do greške pri učitavanju statistike hotela."));
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
                    "Statistika korisnika je uspješno učitana."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user statistics");
                return StatusCode(500, ApiResponse<UserStatistics>.ErrorResult("Došlo je do greške pri učitavanju statistike korisnika."));
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
                    "Statistika recenzija je uspješno učitana."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving review statistics");
                return StatusCode(500, ApiResponse<ReviewStatistics>.ErrorResult("Došlo je do greške pri učitavanju statistike recenzija."));
            }
        }
    }
}

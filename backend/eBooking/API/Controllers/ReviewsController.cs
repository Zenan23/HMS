using API.DTOs;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReviewsController : BaseController<ReviewDto, CreateReviewDto, UpdateReviewDto>
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(
            IReviewService reviewService,
            ILogger<ReviewsController> logger)
            : base(reviewService, logger)
        {
            _reviewService = reviewService;
        }

        /// <summary>
        /// Get reviews by hotel ID
        /// </summary>
        /// <param name="hotelId">Hotel ID</param>
        /// <returns>List of reviews</returns>
        [HttpGet("hotel/{hotelId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ReviewDto>>>> GetByHotelId([FromRoute] int hotelId)
        {
            try
            {
                if (hotelId <= 0)
                {
                    return BadRequest(ApiResponse<IEnumerable<ReviewDto>>.ErrorResult("Invalid hotel ID."));
                }

                var reviews = await _reviewService.GetByHotelIdAsync(hotelId);
                return Ok(ApiResponse<IEnumerable<ReviewDto>>.SuccessResult(reviews, "Reviews retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reviews for hotel ID: {HotelId}", hotelId);
                return StatusCode(500, ApiResponse<IEnumerable<ReviewDto>>.ErrorResult("An error occurred while retrieving reviews."));
            }
        }

        /// <summary>
        /// Get reviews by user ID
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of reviews</returns>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ReviewDto>>>> GetByUserId([FromRoute] int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest(ApiResponse<IEnumerable<ReviewDto>>.ErrorResult("Invalid user ID."));
                }

                var reviews = await _reviewService.GetByUserIdAsync(userId);
                return Ok(ApiResponse<IEnumerable<ReviewDto>>.SuccessResult(reviews, "Reviews retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reviews for user ID: {UserId}", userId);
                return StatusCode(500, ApiResponse<IEnumerable<ReviewDto>>.ErrorResult("An error occurred while retrieving reviews."));
            }
        }

        /// <summary>
        /// Get reviews by booking ID
        /// </summary>
        /// <param name="bookingId">Booking ID</param>
        /// <returns>List of reviews</returns>
        [HttpGet("booking/{bookingId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ReviewDto>>>> GetByBookingId([FromRoute] int bookingId)
        {
            try
            {
                if (bookingId <= 0)
                {
                    return BadRequest(ApiResponse<IEnumerable<ReviewDto>>.ErrorResult("Invalid booking ID."));
                }

                var reviews = await _reviewService.GetByBookingIdAsync(bookingId);
                return Ok(ApiResponse<IEnumerable<ReviewDto>>.SuccessResult(reviews, "Reviews retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reviews for booking ID: {BookingId}", bookingId);
                return StatusCode(500, ApiResponse<IEnumerable<ReviewDto>>.ErrorResult("An error occurred while retrieving reviews."));
            }
        }

        /// <summary>
        /// Get reviews by rating
        /// </summary>
        /// <param name="rating">Rating (1-5)</param>
        /// <returns>List of reviews</returns>
        [HttpGet("rating/{rating}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ReviewDto>>>> GetByRating([FromRoute] int rating)
        {
            try
            {
                if (rating < 1 || rating > 5)
                {
                    return BadRequest(ApiResponse<IEnumerable<ReviewDto>>.ErrorResult("Rating must be between 1 and 5."));
                }

                var reviews = await _reviewService.GetByRatingAsync(rating);
                return Ok(ApiResponse<IEnumerable<ReviewDto>>.SuccessResult(reviews, "Reviews retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reviews with rating: {Rating}", rating);
                return StatusCode(500, ApiResponse<IEnumerable<ReviewDto>>.ErrorResult("An error occurred while retrieving reviews."));
            }
        }

        /// <summary>
        /// Get pending reviews (for moderation)
        /// </summary>
        /// <returns>List of pending reviews</returns>
        [HttpGet("pending")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ReviewDto>>>> GetPendingReviews()
        {
            try
            {
                var reviews = await _reviewService.GetPendingReviewsAsync();
                return Ok(ApiResponse<IEnumerable<ReviewDto>>.SuccessResult(reviews, "Pending reviews retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending reviews");
                return StatusCode(500, ApiResponse<IEnumerable<ReviewDto>>.ErrorResult("An error occurred while retrieving pending reviews."));
            }
        }

        /// <summary>
        /// Get average rating for a hotel
        /// </summary>
        /// <param name="hotelId">Hotel ID</param>
        /// <returns>Average rating</returns>
        [HttpGet("hotel/{hotelId}/average-rating")]
        public async Task<ActionResult<ApiResponse<double>>> GetAverageRating([FromRoute] int hotelId)
        {
            try
            {
                if (hotelId <= 0)
                {
                    return BadRequest(ApiResponse<double>.ErrorResult("Invalid hotel ID."));
                }

                var averageRating = await _reviewService.GetAverageRatingAsync(hotelId);
                return Ok(ApiResponse<double>.SuccessResult(averageRating, "Average rating retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving average rating for hotel ID: {HotelId}", hotelId);
                return StatusCode(500, ApiResponse<double>.ErrorResult("An error occurred while retrieving average rating."));
            }
        }

        /// <summary>
        /// Approve a review
        /// </summary>
        /// <param name="id">Review ID</param>
        /// <param name="request">Approval request</param>
        /// <returns>Approval result</returns>
        [HttpPost("{id}/approve")]
        public async Task<ActionResult<ApiResponse<bool>>> ApproveReview([FromRoute] int id, [FromBody] ApproveReviewRequest request)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(ApiResponse<bool>.ErrorResult("Invalid review ID."));
                }

                var result = await _reviewService.ApproveReviewAsync(id, request.ApprovedByUserId);

                if (!result)
                {
                    return BadRequest(ApiResponse<bool>.ErrorResult("Review not found or cannot be approved."));
                }

                return Ok(ApiResponse<bool>.SuccessResult(result, "Review approved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving review with ID: {ReviewId}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResult("An error occurred while approving the review."));
            }
        }

        /// <summary>
        /// Reject a review
        /// </summary>
        /// <param name="id">Review ID</param>
        /// <param name="request">Rejection request</param>
        /// <returns>Rejection result</returns>
        [HttpPost("{id}/reject")]
        public async Task<ActionResult<ApiResponse<bool>>> RejectReview([FromRoute] int id, [FromBody] RejectReviewRequest request)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(ApiResponse<bool>.ErrorResult("Invalid review ID."));
                }

                var result = await _reviewService.RejectReviewAsync(id, request.RejectedByUserId);

                if (!result)
                {
                    return BadRequest(ApiResponse<bool>.ErrorResult("Review not found or cannot be rejected."));
                }

                return Ok(ApiResponse<bool>.SuccessResult(result, "Review rejected successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting review with ID: {ReviewId}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResult("An error occurred while rejecting the review."));
            }
        }
    }

    public class ApproveReviewRequest
    {
        public int? ApprovedByUserId { get; set; }
    }

    public class RejectReviewRequest
    {
        public int? RejectedByUserId { get; set; }
    }

}

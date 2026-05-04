using Contracts.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Persistence.Interfaces;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LoyaltyPointsRedemptionsController : BaseController<LoyaltyPointsRedemptionDto, CreateLoyaltyPointsRedemptionDto, UpdateLoyaltyPointsRedemptionDto>
    {
        private readonly ILoyaltyPointsRedemptionService _loyaltyPointsRedemptionService;

        public LoyaltyPointsRedemptionsController(
            ILoyaltyPointsRedemptionService loyaltyPointsRedemptionService,
            ILogger<LoyaltyPointsRedemptionsController> logger)
            : base(loyaltyPointsRedemptionService, logger)
        {
            _loyaltyPointsRedemptionService = loyaltyPointsRedemptionService;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<LoyaltyPointsRedemptionDto>>>> GetByUserId([FromRoute] int userId)
        {
            if (userId <= 0)
            {
                return BadRequest(ApiResponse<IEnumerable<LoyaltyPointsRedemptionDto>>.ErrorResult("Invalid user ID."));
            }

            var redemptions = await _loyaltyPointsRedemptionService.GetByUserIdAsync(userId);
            return Ok(ApiResponse<IEnumerable<LoyaltyPointsRedemptionDto>>.SuccessResult(redemptions, "Loyalty redemptions retrieved successfully."));
        }

        [HttpGet("booking/{bookingId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<LoyaltyPointsRedemptionDto>>>> GetByBookingId([FromRoute] int bookingId)
        {
            if (bookingId <= 0)
            {
                return BadRequest(ApiResponse<IEnumerable<LoyaltyPointsRedemptionDto>>.ErrorResult("Invalid booking ID."));
            }

            var redemptions = await _loyaltyPointsRedemptionService.GetByBookingIdAsync(bookingId);
            return Ok(ApiResponse<IEnumerable<LoyaltyPointsRedemptionDto>>.SuccessResult(redemptions, "Loyalty redemptions retrieved successfully."));
        }
    }
}

using API.Attributes;
using Contracts.DTOs;
using Contracts.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Persistence.Interfaces;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LoyaltyPointsRedemptionsController : BaseController<LoyaltyPointsRedemptionDto, CreateLoyaltyPointsRedemptionDto, UpdateLoyaltyPointsRedemptionDto>
    {
        private readonly ILoyaltyPointsRedemptionService _loyaltyPointsRedemptionService;
        private readonly ILoyaltyPointsEarnedService _loyaltyPointsEarnedService;
        private readonly IBookingService _bookingService;

        // 100 bodova = 5 EUR/USD (fiksna stopa) — server-side, klijentu se NE vjeruje za
        // EquivalentValueAmount kad gost sam kreira redemption.
        private const decimal PointsToCurrencyRate = 0.05m;

        public LoyaltyPointsRedemptionsController(
            ILoyaltyPointsRedemptionService loyaltyPointsRedemptionService,
            ILoyaltyPointsEarnedService loyaltyPointsEarnedService,
            IBookingService bookingService,
            ILogger<LoyaltyPointsRedemptionsController> logger)
            : base(loyaltyPointsRedemptionService, logger)
        {
            _loyaltyPointsRedemptionService = loyaltyPointsRedemptionService;
            _loyaltyPointsEarnedService = loyaltyPointsEarnedService;
            _bookingService = bookingService;
        }

        /// <summary>
        /// Trenutni balans bodova korisnika = SUM(zarađeno) - SUM(potrošeno). Računa se on-the-fly,
        /// nema mutable kolonu za balans (izbjegava concurrency bugove).
        /// </summary>
        [HttpGet("balance/{userId}")]
        public async Task<ActionResult<ApiResponse<int>>> GetBalance([FromRoute] int userId)
        {
            if (userId <= 0)
            {
                return BadRequest(ApiResponse<int>.ErrorResult("Nevažeći ID korisnika."));
            }

            if (!IsSelfOrElevated(userId))
            {
                return Forbid();
            }

            var earned = await _loyaltyPointsEarnedService.GetTotalPointsForUserAsync(userId);
            var used = await _loyaltyPointsRedemptionService.GetTotalPointsUsedForUserAsync(userId);
            var balance = earned - used;

            return Ok(ApiResponse<int>.SuccessResult(balance, "Loyalty balans je uspješno učitan."));
        }

        /// <summary>
        /// Employee/Admin mogu kreirati redemption za bilo kog korisnika (ručna korekcija, npr.
        /// korisnička podrška). Gost smije kreirati SAMO za sebe, SAMO za svoju rezervaciju, i
        /// SAMO u granicama trenutnog balansa — EquivalentValueAmount se uvijek računa server-side
        /// po fiksnoj stopi, nikad se ne uzima iz zahtjeva.
        /// </summary>
        [HttpPost]
        public override async Task<ActionResult<ApiResponse<LoyaltyPointsRedemptionDto>>> Create([FromBody] CreateLoyaltyPointsRedemptionDto createDto)
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role);
            var isStaff = roleClaim != null &&
                (roleClaim.Value == UserRole.Employee.ToString() || roleClaim.Value == UserRole.Admin.ToString());

            if (!isStaff)
            {
                if (!IsSelfOrElevated(createDto.UserId))
                {
                    return Forbid();
                }

                var booking = await _bookingService.GetByIdAsync(createDto.BookingId);
                if (booking == null || booking.UserId != createDto.UserId)
                {
                    return BadRequest(ApiResponse<LoyaltyPointsRedemptionDto>.ErrorResult("Rezervacija ne postoji ili ne pripada vama."));
                }

                var earned = await _loyaltyPointsEarnedService.GetTotalPointsForUserAsync(createDto.UserId);
                var used = await _loyaltyPointsRedemptionService.GetTotalPointsUsedForUserAsync(createDto.UserId);
                var balance = earned - used;

                if (createDto.PointsUsed <= 0 || createDto.PointsUsed > balance)
                {
                    return BadRequest(ApiResponse<LoyaltyPointsRedemptionDto>.ErrorResult(
                        $"Nemate dovoljno bodova za ovu akciju. Trenutni balans: {balance}."));
                }

                createDto.EquivalentValueAmount = createDto.PointsUsed * PointsToCurrencyRate;
                createDto.RedeemedAt = DateTime.UtcNow;
            }

            return await base.Create(createDto);
        }

        [HttpPut("{id}")]
        [AuthorizeRole(UserRole.Employee)]
        public override Task<ActionResult<ApiResponse<LoyaltyPointsRedemptionDto>>> Update([FromRoute] int id, [FromBody] UpdateLoyaltyPointsRedemptionDto updateDto)
            => base.Update(id, updateDto);

        [HttpDelete("{id}")]
        [AuthorizeRole(UserRole.Employee)]
        public override Task<ActionResult<ApiResponse<bool>>> Delete([FromRoute] int id)
            => base.Delete(id);

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<LoyaltyPointsRedemptionDto>>>> GetByUserId([FromRoute] int userId)
        {
            if (userId <= 0)
            {
                return BadRequest(ApiResponse<IEnumerable<LoyaltyPointsRedemptionDto>>.ErrorResult("Nevažeći ID korisnika."));
            }

            if (!IsSelfOrElevated(userId))
            {
                return Forbid();
            }

            var redemptions = await _loyaltyPointsRedemptionService.GetByUserIdAsync(userId);
            return Ok(ApiResponse<IEnumerable<LoyaltyPointsRedemptionDto>>.SuccessResult(redemptions, "Iskorišteni loyalty bodovi su uspješno učitani."));
        }

        [HttpGet("booking/{bookingId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<LoyaltyPointsRedemptionDto>>>> GetByBookingId([FromRoute] int bookingId)
        {
            if (bookingId <= 0)
            {
                return BadRequest(ApiResponse<IEnumerable<LoyaltyPointsRedemptionDto>>.ErrorResult("Nevažeći ID rezervacije."));
            }

            var redemptions = await _loyaltyPointsRedemptionService.GetByBookingIdAsync(bookingId);
            return Ok(ApiResponse<IEnumerable<LoyaltyPointsRedemptionDto>>.SuccessResult(redemptions, "Iskorišteni loyalty bodovi su uspješno učitani."));
        }
    }
}

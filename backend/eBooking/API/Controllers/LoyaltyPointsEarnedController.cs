using API.Attributes;
using Contracts.DTOs;
using Contracts.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Persistence.Interfaces;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LoyaltyPointsEarnedController : BaseController<LoyaltyPointsEarnedDto, CreateLoyaltyPointsEarnedDto, UpdateLoyaltyPointsEarnedDto>
    {
        private readonly ILoyaltyPointsEarnedService _loyaltyPointsEarnedService;

        public LoyaltyPointsEarnedController(
            ILoyaltyPointsEarnedService loyaltyPointsEarnedService,
            ILogger<LoyaltyPointsEarnedController> logger)
            : base(loyaltyPointsEarnedService, logger)
        {
            _loyaltyPointsEarnedService = loyaltyPointsEarnedService;
        }

        // Automatsko zarađivanje ide direktno kroz PaymentService (DbContext), ovaj CRUD je
        // za ručne korekcije osoblja (npr. bonus bodovi) — isto ograničenje kao LoyaltyPointsRedemptions.
        [HttpPost]
        [AuthorizeRole(UserRole.Employee, UserRole.Admin)]
        public override Task<ActionResult<ApiResponse<LoyaltyPointsEarnedDto>>> Create([FromBody] CreateLoyaltyPointsEarnedDto createDto)
            => base.Create(createDto);

        [HttpPut("{id}")]
        [AuthorizeRole(UserRole.Employee, UserRole.Admin)]
        public override Task<ActionResult<ApiResponse<LoyaltyPointsEarnedDto>>> Update([FromRoute] int id, [FromBody] UpdateLoyaltyPointsEarnedDto updateDto)
            => base.Update(id, updateDto);

        [HttpDelete("{id}")]
        [AuthorizeRole(UserRole.Employee, UserRole.Admin)]
        public override Task<ActionResult<ApiResponse<bool>>> Delete([FromRoute] int id)
            => base.Delete(id);

        // Lista svih zapisa — samo osoblje/admin (nije korisno gostu, gost koristi balance endpoint).
        [HttpGet]
        [AuthorizeRole(UserRole.Employee, UserRole.Admin)]
        public override Task<ActionResult<ApiResponse<PaginatedResult<LoyaltyPointsEarnedDto>>>> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
            => base.GetAll(pageNumber, pageSize);

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<LoyaltyPointsEarnedDto>>>> GetByUserId([FromRoute] int userId)
        {
            if (userId <= 0)
            {
                return BadRequest(ApiResponse<IEnumerable<LoyaltyPointsEarnedDto>>.ErrorResult("Nevažeći ID korisnika."));
            }

            if (!IsSelfOrElevated(userId))
            {
                return Forbid();
            }

            var earned = await _loyaltyPointsEarnedService.GetByUserIdAsync(userId);
            return Ok(ApiResponse<IEnumerable<LoyaltyPointsEarnedDto>>.SuccessResult(earned, "Zarađeni loyalty bodovi su uspješno učitani."));
        }
    }
}

using System.Security.Claims;
using Contracts.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Persistence.Interfaces;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PriceAdjustmentsController : BaseController<PriceAdjustmentDto, CreatePriceAdjustmentDto, UpdatePriceAdjustmentDto>
    {
        private readonly IPriceAdjustmentService _priceAdjustmentService;

        public PriceAdjustmentsController(
            IPriceAdjustmentService priceAdjustmentService,
            ILogger<PriceAdjustmentsController> logger)
            : base(priceAdjustmentService, logger)
        {
            _priceAdjustmentService = priceAdjustmentService;
        }

        [HttpGet("active")]
        public async Task<ActionResult<ApiResponse<IEnumerable<PriceAdjustmentDto>>>> GetActive(
            [FromQuery] DateTime? atDate, [FromQuery] int? hotelId)
        {
            var activeAdjustments = await _priceAdjustmentService.GetActiveAdjustmentsAsync(atDate ?? DateTime.UtcNow, hotelId);
            return Ok(ApiResponse<IEnumerable<PriceAdjustmentDto>>.SuccessResult(activeAdjustments, "Active price adjustments retrieved successfully."));
        }

        // Server-side postavljanje CreatedByUserId iz JWT-a — klijent ga ne šalje/ne može falsifikovati.
        [HttpPost]
        public override async Task<ActionResult<ApiResponse<PriceAdjustmentDto>>> Create([FromBody] CreatePriceAdjustmentDto createDto)
        {
            var uidClaim = User.FindFirst("userId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
            if (uidClaim != null && int.TryParse(uidClaim.Value, out var uid))
            {
                createDto.CreatedByUserId = uid;
            }

            return await base.Create(createDto);
        }
    }
}

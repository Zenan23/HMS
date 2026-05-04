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
        public async Task<ActionResult<ApiResponse<IEnumerable<PriceAdjustmentDto>>>> GetActive([FromQuery] DateTime? atDate)
        {
            var activeAdjustments = await _priceAdjustmentService.GetActiveAdjustmentsAsync(atDate ?? DateTime.UtcNow);
            return Ok(ApiResponse<IEnumerable<PriceAdjustmentDto>>.SuccessResult(activeAdjustments, "Active price adjustments retrieved successfully."));
        }
    }
}

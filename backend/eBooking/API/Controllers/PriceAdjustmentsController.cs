using System.Security.Claims;
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
            return Ok(ApiResponse<IEnumerable<PriceAdjustmentDto>>.SuccessResult(activeAdjustments, "Aktivne korekcije cijena su uspješno učitane."));
        }

        // Server-side postavljanje CreatedByUserId iz JWT-a — klijent ga ne šalje/ne može falsifikovati.
        [HttpPost]
        [AuthorizeRole(UserRole.Employee, UserRole.Admin)]
        public override async Task<ActionResult<ApiResponse<PriceAdjustmentDto>>> Create([FromBody] CreatePriceAdjustmentDto createDto)
        {
            var uidClaim = User.FindFirst("userId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
            if (uidClaim != null && int.TryParse(uidClaim.Value, out var uid))
            {
                createDto.CreatedByUserId = uid;
            }

            return await base.Create(createDto);
        }

        [HttpPut("{id}")]
        [AuthorizeRole(UserRole.Employee, UserRole.Admin)]
        public override Task<ActionResult<ApiResponse<PriceAdjustmentDto>>> Update([FromRoute] int id, [FromBody] UpdatePriceAdjustmentDto updateDto)
            => base.Update(id, updateDto);

        [HttpDelete("{id}")]
        [AuthorizeRole(UserRole.Employee, UserRole.Admin)]
        public override Task<ActionResult<ApiResponse<bool>>> Delete([FromRoute] int id)
            => base.Delete(id);
    }
}

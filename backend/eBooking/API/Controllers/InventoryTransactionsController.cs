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
    public class InventoryTransactionsController : BaseController<InventoryTransactionDto, CreateInventoryTransactionDto, UpdateInventoryTransactionDto>
    {
        private readonly IInventoryTransactionService _inventoryTransactionService;

        public InventoryTransactionsController(
            IInventoryTransactionService inventoryTransactionService,
            ILogger<InventoryTransactionsController> logger)
            : base(inventoryTransactionService, logger)
        {
            _inventoryTransactionService = inventoryTransactionService;
        }

        [HttpPost]
        [AuthorizeRole(UserRole.Employee)]
        public override Task<ActionResult<ApiResponse<InventoryTransactionDto>>> Create([FromBody] CreateInventoryTransactionDto createDto)
            => base.Create(createDto);

        [HttpPut("{id}")]
        [AuthorizeRole(UserRole.Employee)]
        public override Task<ActionResult<ApiResponse<InventoryTransactionDto>>> Update([FromRoute] int id, [FromBody] UpdateInventoryTransactionDto updateDto)
            => base.Update(id, updateDto);

        [HttpDelete("{id}")]
        [AuthorizeRole(UserRole.Employee)]
        public override Task<ActionResult<ApiResponse<bool>>> Delete([FromRoute] int id)
            => base.Delete(id);

        [HttpGet("item/{inventoryItemId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<InventoryTransactionDto>>>> GetByInventoryItemId([FromRoute] int inventoryItemId)
        {
            if (inventoryItemId <= 0)
            {
                return BadRequest(ApiResponse<IEnumerable<InventoryTransactionDto>>.ErrorResult("Invalid inventory item ID."));
            }

            var transactions = await _inventoryTransactionService.GetByInventoryItemIdAsync(inventoryItemId);
            return Ok(ApiResponse<IEnumerable<InventoryTransactionDto>>.SuccessResult(transactions, "Inventory transactions retrieved successfully."));
        }

        [HttpGet("staff/{staffUserId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<InventoryTransactionDto>>>> GetByStaffUserId([FromRoute] int staffUserId)
        {
            if (staffUserId <= 0)
            {
                return BadRequest(ApiResponse<IEnumerable<InventoryTransactionDto>>.ErrorResult("Invalid staff user ID."));
            }

            var transactions = await _inventoryTransactionService.GetByStaffUserIdAsync(staffUserId);
            return Ok(ApiResponse<IEnumerable<InventoryTransactionDto>>.SuccessResult(transactions, "Inventory transactions retrieved successfully."));
        }
    }
}

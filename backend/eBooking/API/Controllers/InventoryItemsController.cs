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
    public class InventoryItemsController : BaseController<InventoryItemDto, CreateInventoryItemDto, UpdateInventoryItemDto>
    {
        public InventoryItemsController(
            IInventoryItemService inventoryItemService,
            ILogger<InventoryItemsController> logger)
            : base(inventoryItemService, logger)
        {
        }

        [HttpPost]
        [AuthorizeRole(UserRole.Employee)]
        public override Task<ActionResult<ApiResponse<InventoryItemDto>>> Create([FromBody] CreateInventoryItemDto createDto)
            => base.Create(createDto);

        [HttpPut("{id}")]
        [AuthorizeRole(UserRole.Employee)]
        public override Task<ActionResult<ApiResponse<InventoryItemDto>>> Update([FromRoute] int id, [FromBody] UpdateInventoryItemDto updateDto)
            => base.Update(id, updateDto);

        [HttpDelete("{id}")]
        [AuthorizeRole(UserRole.Employee)]
        public override Task<ActionResult<ApiResponse<bool>>> Delete([FromRoute] int id)
            => base.Delete(id);
    }
}

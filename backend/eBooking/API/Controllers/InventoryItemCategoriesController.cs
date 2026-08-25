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
    public class InventoryItemCategoriesController : BaseController<InventoryItemCategoryDto, CreateInventoryItemCategoryDto, UpdateInventoryItemCategoryDto>
    {
        public InventoryItemCategoriesController(
            IInventoryItemCategoryService inventoryItemCategoryService,
            ILogger<InventoryItemCategoriesController> logger)
            : base(inventoryItemCategoryService, logger)
        {
        }

        [HttpPost]
        [AuthorizeRole(UserRole.Employee)]
        public override Task<ActionResult<ApiResponse<InventoryItemCategoryDto>>> Create([FromBody] CreateInventoryItemCategoryDto createDto)
            => base.Create(createDto);

        [HttpPut("{id}")]
        [AuthorizeRole(UserRole.Employee)]
        public override Task<ActionResult<ApiResponse<InventoryItemCategoryDto>>> Update([FromRoute] int id, [FromBody] UpdateInventoryItemCategoryDto updateDto)
            => base.Update(id, updateDto);

        [HttpDelete("{id}")]
        [AuthorizeRole(UserRole.Employee)]
        public override Task<ActionResult<ApiResponse<bool>>> Delete([FromRoute] int id)
            => base.Delete(id);
    }
}

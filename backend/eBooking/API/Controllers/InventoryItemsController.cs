using Contracts.DTOs;
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
    }
}

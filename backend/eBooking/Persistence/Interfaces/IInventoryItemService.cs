using Contracts.DTOs;

namespace Persistence.Interfaces
{
    public interface IInventoryItemService : IBaseService<InventoryItemDto, CreateInventoryItemDto, UpdateInventoryItemDto>
    {
    }
}

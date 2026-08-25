using Contracts.DTOs;

namespace Persistence.Interfaces
{
    public interface IInventoryItemCategoryService : IBaseService<InventoryItemCategoryDto, CreateInventoryItemCategoryDto, UpdateInventoryItemCategoryDto>
    {
    }
}

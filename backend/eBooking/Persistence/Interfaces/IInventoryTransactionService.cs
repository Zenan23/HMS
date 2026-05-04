using Contracts.DTOs;

namespace Persistence.Interfaces
{
    public interface IInventoryTransactionService : IBaseService<InventoryTransactionDto, CreateInventoryTransactionDto, UpdateInventoryTransactionDto>
    {
        Task<IEnumerable<InventoryTransactionDto>> GetByInventoryItemIdAsync(int inventoryItemId);
        Task<IEnumerable<InventoryTransactionDto>> GetByStaffUserIdAsync(int staffUserId);
    }
}

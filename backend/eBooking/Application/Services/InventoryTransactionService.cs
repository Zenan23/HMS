using AutoMapper;
using Contracts.DTOs;
using Microsoft.Extensions.Logging;
using Persistence.Interfaces;
using Persistence.Models;

namespace Application.Services
{
    public class InventoryTransactionService : BaseDtoService<InventoryTransaction, InventoryTransactionDto, CreateInventoryTransactionDto, UpdateInventoryTransactionDto>, IInventoryTransactionService
    {
        public InventoryTransactionService(
            IRepository<InventoryTransaction> repository,
            IMapper mapper,
            ILogger<InventoryTransactionService> logger)
            : base(repository, mapper, logger)
        {
        }

        public async Task<IEnumerable<InventoryTransactionDto>> GetByInventoryItemIdAsync(int inventoryItemId)
        {
            var entities = await _repository.GetAllAsync();
            var filtered = entities
                .Where(x => x.InventoryItemId == inventoryItemId && !x.IsDeleted)
                .OrderByDescending(x => x.TransactionDate);
            return _mapper.Map<IEnumerable<InventoryTransactionDto>>(filtered);
        }

        public async Task<IEnumerable<InventoryTransactionDto>> GetByStaffUserIdAsync(int staffUserId)
        {
            var entities = await _repository.GetAllAsync();
            var filtered = entities
                .Where(x => x.StaffUserId == staffUserId && !x.IsDeleted)
                .OrderByDescending(x => x.TransactionDate);
            return _mapper.Map<IEnumerable<InventoryTransactionDto>>(filtered);
        }
    }
}

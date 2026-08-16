using AutoMapper;
using Contracts.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Data;
using Persistence.Interfaces;
using Persistence.Models;

namespace Application.Services
{
    public class InventoryTransactionService : BaseDtoService<InventoryTransaction, InventoryTransactionDto, CreateInventoryTransactionDto, UpdateInventoryTransactionDto>, IInventoryTransactionService
    {
        private readonly ApplicationDbContext _context;

        public InventoryTransactionService(
            IRepository<InventoryTransaction> repository,
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<InventoryTransactionService> logger)
            : base(repository, mapper, logger)
        {
            _context = context;
        }

        // Generički Repository<T>.GetAllAsync() ne radi .Include(), pa bi InventoryItem/StaffUser
        // navigacije (i time InventoryItemName/StaffUserName u DTO-u) ostale prazne. Zato ovdje
        // eksplicitno queryamo preko DbContext-a sa Include, samo za ovaj servis.
        private IQueryable<InventoryTransaction> QueryWithIncludes() =>
            _context.InventoryTransactions
                .Include(x => x.InventoryItem)
                .Include(x => x.StaffUser);

        public override async Task<InventoryTransactionDto?> GetByIdAsync(int id)
        {
            var entity = await QueryWithIncludes().FirstOrDefaultAsync(x => x.Id == id);
            return entity == null ? null : _mapper.Map<InventoryTransactionDto>(entity);
        }

        public override async Task<IEnumerable<InventoryTransactionDto>> GetAllAsync()
        {
            var entities = await QueryWithIncludes()
                .OrderByDescending(x => x.TransactionDate)
                .ToListAsync();
            return _mapper.Map<IEnumerable<InventoryTransactionDto>>(entities);
        }

        public override async Task<IEnumerable<InventoryTransactionDto>> GetAllAsync(int pageNumber, int pageSize)
        {
            var skip = (pageNumber - 1) * pageSize;
            var entities = await QueryWithIncludes()
                .OrderByDescending(x => x.TransactionDate)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();
            return _mapper.Map<IEnumerable<InventoryTransactionDto>>(entities);
        }

        public async Task<IEnumerable<InventoryTransactionDto>> GetByInventoryItemIdAsync(int inventoryItemId)
        {
            var entities = await QueryWithIncludes()
                .Where(x => x.InventoryItemId == inventoryItemId)
                .OrderByDescending(x => x.TransactionDate)
                .ToListAsync();
            return _mapper.Map<IEnumerable<InventoryTransactionDto>>(entities);
        }

        public async Task<IEnumerable<InventoryTransactionDto>> GetByStaffUserIdAsync(int staffUserId)
        {
            var entities = await QueryWithIncludes()
                .Where(x => x.StaffUserId == staffUserId)
                .OrderByDescending(x => x.TransactionDate)
                .ToListAsync();
            return _mapper.Map<IEnumerable<InventoryTransactionDto>>(entities);
        }
    }
}

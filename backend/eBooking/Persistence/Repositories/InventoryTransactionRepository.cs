using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using Persistence.Models;

namespace Persistence.Repositories
{
    public class InventoryTransactionRepository : Repository<InventoryTransaction>
    {
        public InventoryTransactionRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<InventoryTransaction>> GetAllAsync()
        {
            return await _dbSet
                .Include(t => t.StaffUser)
                .ToListAsync();
        }

        public override async Task<InventoryTransaction?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(t => t.StaffUser)
                .FirstOrDefaultAsync(t => t.Id == id);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using Persistence.Models;

namespace Persistence.Repositories
{
    public class RoomMaintenanceLogRepository : Repository<RoomMaintenanceLog>
    {
        public RoomMaintenanceLogRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<RoomMaintenanceLog>> GetAllAsync()
        {
            return await _dbSet
                .Include(l => l.Room)
                .ToListAsync();
        }

        public override async Task<RoomMaintenanceLog?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(l => l.Room)
                .FirstOrDefaultAsync(l => l.Id == id);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using Persistence.Models;

namespace Persistence.Repositories
{
    public class RoomRepository : Repository<Room>
    {
        public RoomRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Room>> GetAllAsync()
        {
            return await _dbSet
                .Include(r => r.Hotel)
                .ToListAsync();
        }

        public override async Task<Room?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(r => r.Hotel)
                .FirstOrDefaultAsync(r => r.Id == id);
        }
    }
}

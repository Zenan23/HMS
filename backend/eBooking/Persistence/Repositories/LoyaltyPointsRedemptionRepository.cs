using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using Persistence.Models;

namespace Persistence.Repositories
{
    public class LoyaltyPointsRedemptionRepository : Repository<LoyaltyPointsRedemption>
    {
        public LoyaltyPointsRedemptionRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<LoyaltyPointsRedemption>> GetAllAsync()
        {
            return await _dbSet
                .Include(r => r.User)
                .Include(r => r.Booking)
                    .ThenInclude(b => b.Room)
                .ToListAsync();
        }

        public override async Task<LoyaltyPointsRedemption?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(r => r.User)
                .Include(r => r.Booking)
                    .ThenInclude(b => b.Room)
                .FirstOrDefaultAsync(r => r.Id == id);
        }
    }
}

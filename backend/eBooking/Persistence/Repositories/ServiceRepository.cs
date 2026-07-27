using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using Persistence.Models;

namespace Persistence.Repositories
{
    public class ServiceRepository : Repository<Service>
    {
        public ServiceRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Service>> GetAllAsync()
        {
            return await _dbSet
                .Include(s => s.Hotel)
                .ToListAsync();
        }

        public override async Task<Service?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(s => s.Hotel)
                .FirstOrDefaultAsync(s => s.Id == id);
        }
    }
}

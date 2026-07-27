using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using Persistence.Models;

namespace Persistence.Repositories
{
    public class SupportTicketRepository : Repository<SupportTicket>
    {
        public SupportTicketRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<SupportTicket>> GetAllAsync()
        {
            return await _dbSet
                .Include(t => t.User)
                .ToListAsync();
        }

        public override async Task<SupportTicket?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id);
        }
    }
}

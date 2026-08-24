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

        // GetAllAsync() (bez parametara) je već imao .Include(r => r.Hotel) iznad, ali paginirani
        // GET /api/Rooms (desktop app) zapravo poziva GetPagedAsync, koja je nasljeđivala običnu
        // Repository<T> implementaciju bez Include-a — zato je HotelName dolazio prazan samo u
        // ovoj konkretnoj (paginiranoj) listi soba.
        public override async Task<IEnumerable<Room>> GetPagedAsync(int skip, int take)
        {
            if (skip < 0) skip = 0;
            if (take <= 0) take = 10;
            return await _dbSet
                .Include(r => r.Hotel)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }
    }
}

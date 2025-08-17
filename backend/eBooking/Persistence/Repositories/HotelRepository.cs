using Persistence.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using Persistence.Models;

namespace Persistence.Repositories
{
    public class HotelRepository : Repository<Hotel>, IHotelRepository
    {
        public HotelRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Hotel>> GetHotelsByCityAsync(string city)
        {
            return await _dbSet
                .Where(h => h.City.ToLower() == city.ToLower())
                .ToListAsync();
        }

        public async Task<Hotel?> GetHotelWithRoomsAsync(int hotelId)
        {
            return await _dbSet
                .Include(h => h.Rooms)
                .FirstOrDefaultAsync(h => h.Id == hotelId);
        }
    }
}

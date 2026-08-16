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

        // Generički Repository<T>.GetAllAsync/GetByIdAsync ne radi .Include(), pa bez ovoga
        // City/Country navigacija ostaje null i CityName/CountryName u DTO-u bi bili prazni.
        public override async Task<Hotel?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(h => h.City!)
                    .ThenInclude(c => c.Country)
                .FirstOrDefaultAsync(h => h.Id == id);
        }

        public override async Task<IEnumerable<Hotel>> GetAllAsync()
        {
            return await _dbSet
                .Include(h => h.City!)
                    .ThenInclude(c => c.Country)
                .ToListAsync();
        }

        public async Task<IEnumerable<Hotel>> GetHotelsByCityAsync(string city)
        {
            return await _dbSet
                .Include(h => h.City!)
                    .ThenInclude(c => c.Country)
                .Where(h => h.City != null && h.City.Name.ToLower() == city.ToLower())
                .ToListAsync();
        }

        public async Task<IEnumerable<Hotel>> GetHotelsByNameAsync(string name)
        {
            return await _dbSet
                .Include(h => h.City!)
                    .ThenInclude(c => c.Country)
                .Where(h => h.Name.ToLower() == name.ToLower())
                .ToListAsync();
        }

        public async Task<Hotel?> GetHotelWithRoomsAsync(int hotelId)
        {
            return await _dbSet
                .Include(h => h.Rooms)
                .Include(h => h.City!)
                    .ThenInclude(c => c.Country)
                .FirstOrDefaultAsync(h => h.Id == hotelId);
        }
    }
}

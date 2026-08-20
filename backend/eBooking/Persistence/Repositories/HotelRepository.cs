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

        // Prava DB-level paginacija za listu hotela (koristi je HotelService.GetAllAsync(pageNumber,
        // pageSize)) — .Include() se mora ponoviti ovdje jer generički Repository<T>.GetPagedAsync
        // ne zna za City/Country navigaciju.
        public override async Task<IEnumerable<Hotel>> GetPagedAsync(int skip, int take)
        {
            if (skip < 0) skip = 0;
            if (take <= 0) take = 10;
            return await _dbSet
                .Include(h => h.City!)
                    .ThenInclude(c => c.Country)
                .OrderBy(h => h.Id)
                .Skip(skip).Take(take)
                .ToListAsync();
        }

        // Gornja granica broja rezultata za "pretraži po X" endpointe koji nemaju eksplicitnu
        // paginaciju u ugovoru API-ja — sprječava da endpoint vrati neograničen broj zapisa
        // (uputa: "Endpointi tipa RetrieveAll bez limita smatraju se greškom za neprihvatanje").
        private const int MaxUnboundedResults = 200;

        public async Task<IEnumerable<Hotel>> GetHotelsByCityAsync(string city)
        {
            return await _dbSet
                .Include(h => h.City!)
                    .ThenInclude(c => c.Country)
                .Where(h => h.City != null && h.City.Name.ToLower() == city.ToLower())
                .OrderBy(h => h.Id)
                .Take(MaxUnboundedResults)
                .ToListAsync();
        }

        public async Task<IEnumerable<Hotel>> GetHotelsByNameAsync(string name)
        {
            return await _dbSet
                .Include(h => h.City!)
                    .ThenInclude(c => c.Country)
                .Where(h => h.Name.ToLower() == name.ToLower())
                .OrderBy(h => h.Id)
                .Take(MaxUnboundedResults)
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

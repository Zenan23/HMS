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

        // Generički Repository<T>.GetAllAsync/GetByIdAsync ne radi .Include(), pa bez ovoga
        // ServiceCategory navigacija ostaje null i Category (naziv) u DTO-u bi bio prazan
        // (isti obrazac kao HotelRepository za City/Country).
        public override async Task<IEnumerable<Service>> GetAllAsync()
        {
            return await _dbSet
                .Include(s => s.Hotel)
                .Include(s => s.ServiceCategory)
                .ToListAsync();
        }

        public override async Task<Service?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(s => s.Hotel)
                .Include(s => s.ServiceCategory)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        // Paginacija (GET /api/Services) prolazi kroz ovu metodu — bez Include-a bi i HotelName
        // i Category (novo, prije je Category bio goli string bez potrebe za Include) ostali
        // prazni u paginiranoj listi, dok bi GetAllAsync/GetByIdAsync radili ispravno.
        public override async Task<IEnumerable<Service>> GetPagedAsync(int skip, int take)
        {
            if (skip < 0) skip = 0;
            if (take <= 0) take = 10;
            return await _dbSet
                .Include(s => s.Hotel)
                .Include(s => s.ServiceCategory)
                .OrderBy(s => s.Id)
                .Skip(skip).Take(take)
                .ToListAsync();
        }
    }
}

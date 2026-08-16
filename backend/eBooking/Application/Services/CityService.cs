using AutoMapper;
using Contracts.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Data;
using Persistence.Interfaces;
using Persistence.Models;

namespace Application.Services
{
    public class CityService : BaseDtoService<City, CityDto, CreateCityDto, UpdateCityDto>, ICityService
    {
        private readonly ApplicationDbContext _context;

        public CityService(
            IRepository<City> repository,
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<CityService> logger)
            : base(repository, mapper, logger)
        {
            _context = context;
        }

        // Generički Repository<T>.GetAllAsync() ne radi .Include(), pa bi Country navigacija
        // (i time CountryName u DTO-u) ostala prazna. Zato ovdje eksplicitno queryamo preko
        // DbContext-a sa Include, samo za ovaj servis.
        private IQueryable<City> QueryWithIncludes() => _context.Cities.Include(c => c.Country);

        public override async Task<CityDto?> GetByIdAsync(int id)
        {
            var entity = await QueryWithIncludes().FirstOrDefaultAsync(c => c.Id == id);
            return entity == null ? null : _mapper.Map<CityDto>(entity);
        }

        public override async Task<IEnumerable<CityDto>> GetAllAsync()
        {
            var entities = await QueryWithIncludes().OrderBy(c => c.Name).ToListAsync();
            return _mapper.Map<IEnumerable<CityDto>>(entities);
        }

        public override async Task<IEnumerable<CityDto>> GetAllAsync(int pageNumber, int pageSize)
        {
            var skip = (pageNumber - 1) * pageSize;
            var entities = await QueryWithIncludes()
                .OrderBy(c => c.Name)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();
            return _mapper.Map<IEnumerable<CityDto>>(entities);
        }

        public async Task<IEnumerable<CityDto>> GetByCountryIdAsync(int countryId)
        {
            var entities = await QueryWithIncludes()
                .Where(c => c.CountryId == countryId)
                .OrderBy(c => c.Name)
                .ToListAsync();
            return _mapper.Map<IEnumerable<CityDto>>(entities);
        }
    }
}

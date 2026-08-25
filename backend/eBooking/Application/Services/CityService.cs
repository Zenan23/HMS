using AutoMapper;
using Contracts.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Persistence.Data;
using Persistence.Interfaces;
using Persistence.Models;

namespace Application.Services
{
    public class CityService : BaseDtoService<City, CityDto, CreateCityDto, UpdateCityDto>, ICityService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;

        // Gradovi/države se mijenjaju izrazito rijetko, a čitaju se na skoro svakom
        // dropdown-u (rezervacije, hoteli, registracija) — zato se puna lista kešira.
        private const string AllCitiesCacheKey = "cities_all";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

        public CityService(
            IRepository<City> repository,
            ApplicationDbContext context,
            IMapper mapper,
            IMemoryCache cache,
            ILogger<CityService> logger)
            : base(repository, mapper, logger)
        {
            _context = context;
            _cache = cache;
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
            var cached = await _cache.GetOrCreateAsync(AllCitiesCacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;
                var entities = await QueryWithIncludes().OrderBy(c => c.Name).ToListAsync();
                return _mapper.Map<IEnumerable<CityDto>>(entities).ToList();
            });
            return cached ?? new List<CityDto>();
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

        public override async Task<CityDto> CreateAsync(CreateCityDto createDto)
        {
            var result = await base.CreateAsync(createDto);
            _cache.Remove(AllCitiesCacheKey);
            return result;
        }

        public override async Task<bool> UpdateAsync(int id, UpdateCityDto updateDto)
        {
            var result = await base.UpdateAsync(id, updateDto);
            _cache.Remove(AllCitiesCacheKey);
            return result;
        }

        public override async Task<bool> DeleteAsync(int id)
        {
            var result = await base.DeleteAsync(id);
            _cache.Remove(AllCitiesCacheKey);
            return result;
        }
    }
}

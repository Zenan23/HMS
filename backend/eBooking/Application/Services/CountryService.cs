using AutoMapper;
using Contracts.DTOs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Persistence.Interfaces;
using Persistence.Models;

namespace Application.Services
{
    public class CountryService : BaseDtoService<Country, CountryDto, CreateCountryDto, UpdateCountryDto>, ICountryService
    {
        private readonly IMemoryCache _cache;

        // Države se mijenjaju izrazito rijetko, a čitaju se na skoro svakom dropdown-u —
        // zato se puna lista kešira (isti princip kao CityService/ServiceCategoryService).
        private const string AllCountriesCacheKey = "countries_all";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

        public CountryService(
            IRepository<Country> repository,
            IMapper mapper,
            IMemoryCache cache,
            ILogger<CountryService> logger)
            : base(repository, mapper, logger)
        {
            _cache = cache;
        }

        public override async Task<IEnumerable<CountryDto>> GetAllAsync()
        {
            var cached = await _cache.GetOrCreateAsync(AllCountriesCacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;
                var result = await base.GetAllAsync();
                return result.ToList();
            });
            return cached ?? new List<CountryDto>();
        }

        public override async Task<CountryDto> CreateAsync(CreateCountryDto createDto)
        {
            var result = await base.CreateAsync(createDto);
            _cache.Remove(AllCountriesCacheKey);
            return result;
        }

        public override async Task<bool> UpdateAsync(int id, UpdateCountryDto updateDto)
        {
            var result = await base.UpdateAsync(id, updateDto);
            _cache.Remove(AllCountriesCacheKey);
            return result;
        }

        public override async Task<bool> DeleteAsync(int id)
        {
            var result = await base.DeleteAsync(id);
            _cache.Remove(AllCountriesCacheKey);
            return result;
        }
    }
}

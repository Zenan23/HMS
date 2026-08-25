using AutoMapper;
using Contracts.DTOs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Persistence.Interfaces;
using Persistence.Models;

namespace Application.Services
{
    public class ServiceCategoryService : BaseDtoService<ServiceCategory, ServiceCategoryDto, CreateServiceCategoryDto, UpdateServiceCategoryDto>, IServiceCategoryService
    {
        private readonly IMemoryCache _cache;

        // Kategorije usluga se mijenjaju izrazito rijetko, a čitaju se na svakom
        // service-form dropdown pozivu — zato se puna lista kešira.
        private const string AllServiceCategoriesCacheKey = "service_categories_all";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

        public ServiceCategoryService(
            IRepository<ServiceCategory> repository,
            IMapper mapper,
            IMemoryCache cache,
            ILogger<ServiceCategoryService> logger)
            : base(repository, mapper, logger)
        {
            _cache = cache;
        }

        public override async Task<IEnumerable<ServiceCategoryDto>> GetAllAsync()
        {
            var cached = await _cache.GetOrCreateAsync(AllServiceCategoriesCacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;
                var result = await base.GetAllAsync();
                return result.ToList();
            });
            return cached ?? new List<ServiceCategoryDto>();
        }

        public override async Task<ServiceCategoryDto> CreateAsync(CreateServiceCategoryDto createDto)
        {
            var result = await base.CreateAsync(createDto);
            _cache.Remove(AllServiceCategoriesCacheKey);
            return result;
        }

        public override async Task<bool> UpdateAsync(int id, UpdateServiceCategoryDto updateDto)
        {
            var result = await base.UpdateAsync(id, updateDto);
            _cache.Remove(AllServiceCategoriesCacheKey);
            return result;
        }

        public override async Task<bool> DeleteAsync(int id)
        {
            var result = await base.DeleteAsync(id);
            _cache.Remove(AllServiceCategoriesCacheKey);
            return result;
        }
    }
}

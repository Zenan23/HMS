using AutoMapper;
using Contracts.DTOs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Persistence.Interfaces;
using Persistence.Models;

namespace Application.Services
{
    public class InventoryItemCategoryService : BaseDtoService<InventoryItemCategory, InventoryItemCategoryDto, CreateInventoryItemCategoryDto, UpdateInventoryItemCategoryDto>, IInventoryItemCategoryService
    {
        private readonly IMemoryCache _cache;

        // Kategorije artikala skladišta se mijenjaju izrazito rijetko, a čitaju se na svakom
        // inventory-item-form dropdown pozivu — zato se puna lista kešira (isti princip kao
        // ServiceCategoryService).
        private const string AllInventoryItemCategoriesCacheKey = "inventory_item_categories_all";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

        public InventoryItemCategoryService(
            IRepository<InventoryItemCategory> repository,
            IMapper mapper,
            IMemoryCache cache,
            ILogger<InventoryItemCategoryService> logger)
            : base(repository, mapper, logger)
        {
            _cache = cache;
        }

        public override async Task<IEnumerable<InventoryItemCategoryDto>> GetAllAsync()
        {
            var cached = await _cache.GetOrCreateAsync(AllInventoryItemCategoriesCacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;
                var result = await base.GetAllAsync();
                return result.ToList();
            });
            return cached ?? new List<InventoryItemCategoryDto>();
        }

        public override async Task<InventoryItemCategoryDto> CreateAsync(CreateInventoryItemCategoryDto createDto)
        {
            var result = await base.CreateAsync(createDto);
            _cache.Remove(AllInventoryItemCategoriesCacheKey);
            return result;
        }

        public override async Task<bool> UpdateAsync(int id, UpdateInventoryItemCategoryDto updateDto)
        {
            var result = await base.UpdateAsync(id, updateDto);
            _cache.Remove(AllInventoryItemCategoriesCacheKey);
            return result;
        }

        public override async Task<bool> DeleteAsync(int id)
        {
            var result = await base.DeleteAsync(id);
            _cache.Remove(AllInventoryItemCategoriesCacheKey);
            return result;
        }
    }
}

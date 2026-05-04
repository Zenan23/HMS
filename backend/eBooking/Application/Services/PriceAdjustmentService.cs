using AutoMapper;
using Contracts.DTOs;
using Microsoft.Extensions.Logging;
using Persistence.Interfaces;
using Persistence.Models;

namespace Application.Services
{
    public class PriceAdjustmentService : BaseDtoService<PriceAdjustment, PriceAdjustmentDto, CreatePriceAdjustmentDto, UpdatePriceAdjustmentDto>, IPriceAdjustmentService
    {
        public PriceAdjustmentService(
            IRepository<PriceAdjustment> repository,
            IMapper mapper,
            ILogger<PriceAdjustmentService> logger)
            : base(repository, mapper, logger)
        {
        }

        public async Task<IEnumerable<PriceAdjustmentDto>> GetActiveAdjustmentsAsync(DateTime atDate)
        {
            var entities = await _repository.GetAllAsync();
            var filtered = entities.Where(x =>
                x.StartDate <= atDate &&
                x.EndDate >= atDate &&
                !x.IsDeleted);
            return _mapper.Map<IEnumerable<PriceAdjustmentDto>>(filtered);
        }
    }
}

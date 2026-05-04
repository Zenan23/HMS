using Contracts.DTOs;

namespace Persistence.Interfaces
{
    public interface IPriceAdjustmentService : IBaseService<PriceAdjustmentDto, CreatePriceAdjustmentDto, UpdatePriceAdjustmentDto>
    {
        Task<IEnumerable<PriceAdjustmentDto>> GetActiveAdjustmentsAsync(DateTime atDate);
    }
}

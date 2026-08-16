using Contracts.DTOs;

namespace Persistence.Interfaces
{
    public interface IPriceAdjustmentService : IBaseService<PriceAdjustmentDto, CreatePriceAdjustmentDto, UpdatePriceAdjustmentDto>
    {
        Task<IEnumerable<PriceAdjustmentDto>> GetActiveAdjustmentsAsync(DateTime atDate, int? hotelId = null);
        Task<decimal> ApplyActiveAdjustmentsAsync(decimal basePrice, DateTime atDate, int? hotelId = null);
    }
}

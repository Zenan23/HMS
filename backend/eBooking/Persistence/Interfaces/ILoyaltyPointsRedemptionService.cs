using Contracts.DTOs;

namespace Persistence.Interfaces
{
    public interface ILoyaltyPointsRedemptionService : IBaseService<LoyaltyPointsRedemptionDto, CreateLoyaltyPointsRedemptionDto, UpdateLoyaltyPointsRedemptionDto>
    {
        Task<IEnumerable<LoyaltyPointsRedemptionDto>> GetByUserIdAsync(int userId);
        Task<IEnumerable<LoyaltyPointsRedemptionDto>> GetByBookingIdAsync(int bookingId);
    }
}

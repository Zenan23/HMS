using Contracts.DTOs;

namespace Persistence.Interfaces
{
    public interface ILoyaltyPointsRedemptionService : IBaseService<LoyaltyPointsRedemptionDto, CreateLoyaltyPointsRedemptionDto, UpdateLoyaltyPointsRedemptionDto>
    {
        Task<IEnumerable<LoyaltyPointsRedemptionDto>> GetByUserIdAsync(int userId);
        Task<IEnumerable<LoyaltyPointsRedemptionDto>> GetByBookingIdAsync(int bookingId);

        /// <summary>Zbir svih bodova koje je korisnik ikad potrošio — koristi se za balans.</summary>
        Task<int> GetTotalPointsUsedForUserAsync(int userId);
    }
}

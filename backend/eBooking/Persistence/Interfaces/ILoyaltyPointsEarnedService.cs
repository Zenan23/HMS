using Contracts.DTOs;

namespace Persistence.Interfaces
{
    public interface ILoyaltyPointsEarnedService : IBaseService<LoyaltyPointsEarnedDto, CreateLoyaltyPointsEarnedDto, UpdateLoyaltyPointsEarnedDto>
    {
        Task<IEnumerable<LoyaltyPointsEarnedDto>> GetByUserIdAsync(int userId);

        /// <summary>Zbir svih bodova koje je korisnik ikad zaradio (ne umanjen za potrošene) — koristi se za balans.</summary>
        Task<int> GetTotalPointsForUserAsync(int userId);
    }
}

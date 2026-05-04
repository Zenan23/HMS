using AutoMapper;
using Contracts.DTOs;
using Microsoft.Extensions.Logging;
using Persistence.Interfaces;
using Persistence.Models;

namespace Application.Services
{
    public class LoyaltyPointsRedemptionService : BaseDtoService<LoyaltyPointsRedemption, LoyaltyPointsRedemptionDto, CreateLoyaltyPointsRedemptionDto, UpdateLoyaltyPointsRedemptionDto>, ILoyaltyPointsRedemptionService
    {
        public LoyaltyPointsRedemptionService(
            IRepository<LoyaltyPointsRedemption> repository,
            IMapper mapper,
            ILogger<LoyaltyPointsRedemptionService> logger)
            : base(repository, mapper, logger)
        {
        }

        public async Task<IEnumerable<LoyaltyPointsRedemptionDto>> GetByUserIdAsync(int userId)
        {
            var entities = await _repository.GetAllAsync();
            var filtered = entities
                .Where(x => x.UserId == userId && !x.IsDeleted)
                .OrderByDescending(x => x.RedeemedAt);
            return _mapper.Map<IEnumerable<LoyaltyPointsRedemptionDto>>(filtered);
        }

        public async Task<IEnumerable<LoyaltyPointsRedemptionDto>> GetByBookingIdAsync(int bookingId)
        {
            var entities = await _repository.GetAllAsync();
            var filtered = entities
                .Where(x => x.BookingId == bookingId && !x.IsDeleted)
                .OrderByDescending(x => x.RedeemedAt);
            return _mapper.Map<IEnumerable<LoyaltyPointsRedemptionDto>>(filtered);
        }
    }
}

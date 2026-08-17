using AutoMapper;
using Contracts.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Data;
using Persistence.Interfaces;
using Persistence.Models;

namespace Application.Services
{
    public class LoyaltyPointsEarnedService : BaseDtoService<LoyaltyPointsEarned, LoyaltyPointsEarnedDto, CreateLoyaltyPointsEarnedDto, UpdateLoyaltyPointsEarnedDto>, ILoyaltyPointsEarnedService
    {
        private readonly ApplicationDbContext _context;

        public LoyaltyPointsEarnedService(
            IRepository<LoyaltyPointsEarned> repository,
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<LoyaltyPointsEarnedService> logger)
            : base(repository, mapper, logger)
        {
            _context = context;
        }

        // Generički Repository<T>.GetAllAsync() ne radi .Include(), pa bi User/Booking navigacija
        // (i time UserName/BookingLabel u DTO-u) ostala prazna. Isti obrazac kao CityService.
        private IQueryable<LoyaltyPointsEarned> QueryWithIncludes() => _context.LoyaltyPointsEarned
            .Include(lpe => lpe.User)
            .Include(lpe => lpe.Booking)
                .ThenInclude(b => b!.Room);

        public override async Task<LoyaltyPointsEarnedDto?> GetByIdAsync(int id)
        {
            var entity = await QueryWithIncludes().FirstOrDefaultAsync(lpe => lpe.Id == id);
            return entity == null ? null : _mapper.Map<LoyaltyPointsEarnedDto>(entity);
        }

        public override async Task<IEnumerable<LoyaltyPointsEarnedDto>> GetAllAsync()
        {
            var entities = await QueryWithIncludes().OrderByDescending(lpe => lpe.EarnedAt).ToListAsync();
            return _mapper.Map<IEnumerable<LoyaltyPointsEarnedDto>>(entities);
        }

        public override async Task<IEnumerable<LoyaltyPointsEarnedDto>> GetAllAsync(int pageNumber, int pageSize)
        {
            var skip = (pageNumber - 1) * pageSize;
            var entities = await QueryWithIncludes()
                .OrderByDescending(lpe => lpe.EarnedAt)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();
            return _mapper.Map<IEnumerable<LoyaltyPointsEarnedDto>>(entities);
        }

        public async Task<IEnumerable<LoyaltyPointsEarnedDto>> GetByUserIdAsync(int userId)
        {
            var entities = await QueryWithIncludes()
                .Where(x => x.UserId == userId && !x.IsDeleted)
                .OrderByDescending(x => x.EarnedAt)
                .ToListAsync();
            return _mapper.Map<IEnumerable<LoyaltyPointsEarnedDto>>(entities);
        }

        public async Task<int> GetTotalPointsForUserAsync(int userId)
        {
            return await _context.LoyaltyPointsEarned
                .Where(x => x.UserId == userId && !x.IsDeleted)
                .SumAsync(x => (int?)x.PointsEarned) ?? 0;
        }
    }
}

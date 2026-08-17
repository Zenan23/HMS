using AutoMapper;
using Contracts.DTOs;
using Contracts.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Data;
using Persistence.Interfaces;
using Persistence.Models;

namespace Application.Services
{
    public class SupportTicketService : BaseDtoService<SupportTicket, SupportTicketDto, CreateSupportTicketDto, UpdateSupportTicketDto>, ISupportTicketService
    {
        private readonly ApplicationDbContext _context;

        public SupportTicketService(
            IRepository<SupportTicket> repository,
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<SupportTicketService> logger)
            : base(repository, mapper, logger)
        {
            _context = context;
        }

        // Generički Repository<T>.GetAllAsync() ne radi .Include(), pa bi User/RespondedByUser
        // navigacija (i time UserName/RespondedByUserName u DTO-u) ostala prazna. Isti obrazac
        // kao CityService/PriceAdjustmentService.
        private IQueryable<SupportTicket> QueryWithIncludes() => _context.SupportTickets
            .Include(st => st.User)
            .Include(st => st.RespondedByUser);

        public override async Task<SupportTicketDto?> GetByIdAsync(int id)
        {
            var entity = await QueryWithIncludes().FirstOrDefaultAsync(st => st.Id == id);
            return entity == null ? null : _mapper.Map<SupportTicketDto>(entity);
        }

        public override async Task<IEnumerable<SupportTicketDto>> GetAllAsync()
        {
            var entities = await QueryWithIncludes().OrderByDescending(st => st.CreatedAt).ToListAsync();
            return _mapper.Map<IEnumerable<SupportTicketDto>>(entities);
        }

        public override async Task<IEnumerable<SupportTicketDto>> GetAllAsync(int pageNumber, int pageSize)
        {
            var skip = (pageNumber - 1) * pageSize;
            var entities = await QueryWithIncludes()
                .OrderByDescending(st => st.CreatedAt)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();
            return _mapper.Map<IEnumerable<SupportTicketDto>>(entities);
        }

        public async Task<IEnumerable<SupportTicketDto>> GetByUserIdAsync(int userId)
        {
            var entities = await QueryWithIncludes()
                .Where(x => x.UserId == userId && !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
            return _mapper.Map<IEnumerable<SupportTicketDto>>(entities);
        }

        public async Task<IEnumerable<SupportTicketDto>> GetByStatusAsync(SupportTicketStatus status)
        {
            var entities = await QueryWithIncludes()
                .Where(x => x.Status == status && !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
            return _mapper.Map<IEnumerable<SupportTicketDto>>(entities);
        }

        public async Task<bool> SetResponseMetadataAsync(int id, int respondedByUserId)
        {
            var entity = await _context.SupportTickets.FirstOrDefaultAsync(st => st.Id == id);
            if (entity == null)
            {
                return false;
            }

            entity.RespondedAt = DateTime.UtcNow;
            entity.RespondedByUserId = respondedByUserId;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

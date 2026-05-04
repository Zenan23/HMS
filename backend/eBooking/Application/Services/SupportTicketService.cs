using AutoMapper;
using Contracts.DTOs;
using Contracts.Enums;
using Microsoft.Extensions.Logging;
using Persistence.Interfaces;
using Persistence.Models;

namespace Application.Services
{
    public class SupportTicketService : BaseDtoService<SupportTicket, SupportTicketDto, CreateSupportTicketDto, UpdateSupportTicketDto>, ISupportTicketService
    {
        public SupportTicketService(
            IRepository<SupportTicket> repository,
            IMapper mapper,
            ILogger<SupportTicketService> logger)
            : base(repository, mapper, logger)
        {
        }

        public async Task<IEnumerable<SupportTicketDto>> GetByUserIdAsync(int userId)
        {
            var entities = await _repository.GetAllAsync();
            var filtered = entities
                .Where(x => x.UserId == userId && !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt);
            return _mapper.Map<IEnumerable<SupportTicketDto>>(filtered);
        }

        public async Task<IEnumerable<SupportTicketDto>> GetByStatusAsync(SupportTicketStatus status)
        {
            var entities = await _repository.GetAllAsync();
            var filtered = entities
                .Where(x => x.Status == status && !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt);
            return _mapper.Map<IEnumerable<SupportTicketDto>>(filtered);
        }
    }
}

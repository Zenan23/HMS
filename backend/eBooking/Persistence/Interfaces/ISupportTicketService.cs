using Contracts.DTOs;
using Contracts.Enums;

namespace Persistence.Interfaces
{
    public interface ISupportTicketService : IBaseService<SupportTicketDto, CreateSupportTicketDto, UpdateSupportTicketDto>
    {
        Task<IEnumerable<SupportTicketDto>> GetByUserIdAsync(int userId);
        Task<IEnumerable<SupportTicketDto>> GetByStatusAsync(SupportTicketStatus status);
    }
}

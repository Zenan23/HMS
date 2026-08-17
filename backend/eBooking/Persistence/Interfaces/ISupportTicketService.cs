using Contracts.DTOs;
using Contracts.Enums;

namespace Persistence.Interfaces
{
    public interface ISupportTicketService : IBaseService<SupportTicketDto, CreateSupportTicketDto, UpdateSupportTicketDto>
    {
        Task<IEnumerable<SupportTicketDto>> GetByUserIdAsync(int userId);
        Task<IEnumerable<SupportTicketDto>> GetByStatusAsync(SupportTicketStatus status);

        /// <summary>
        /// Postavlja RespondedAt/RespondedByUserId server-side, nakon što je AdminResponse
        /// uspješno sačuvan preko generičkog UpdateAsync-a. Odvojeno od UpdateAsync jer
        /// UpdateSupportTicketDto namjerno ne nosi ova dva polja (klijent ih ne smije slati).
        /// </summary>
        Task<bool> SetResponseMetadataAsync(int id, int respondedByUserId);
    }
}

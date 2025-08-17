using Contracts.DTOs;

namespace Persistence.Interfaces
{
    public interface IPaymentAuditLogService : IBaseService<PaymentAuditLogDto, CreatePaymentAuditLogDto, UpdatePaymentAuditLogDto>
    {
        Task<IEnumerable<PaymentAuditLogDto>> GetByPaymentIdAsync(int paymentId);
        Task<IEnumerable<PaymentAuditLogDto>> GetByUserIdAsync(int userId);
        Task<PaymentAuditLogDto> LogPaymentActionAsync(CreatePaymentAuditLogDto auditLogDto);
    }
}

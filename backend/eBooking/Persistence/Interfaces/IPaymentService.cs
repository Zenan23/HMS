using Contracts.DTOs;
using Contracts.Enums;

namespace Persistence.Interfaces
{
    public interface IPaymentService : IBaseService<PaymentDto, CreatePaymentDto, UpdatePaymentDto>
    {
        Task<PaymentDto> ProcessPaymentAsync(CreatePaymentDto createPaymentDto, string? userAgent = null, string? ipAddress = null);
        Task<bool> RefundPaymentAsync(int paymentId, decimal amount, string reason, int? initiatedByUserId = null);
        Task<bool> CancelPaymentAsync(int paymentId, string reason, int? initiatedByUserId = null);
        Task<IEnumerable<PaymentDto>> GetByUserIdAsync(int userId);
        Task<IEnumerable<PaymentDto>> GetByBookingIdAsync(int bookingId);
        Task<IEnumerable<PaymentDto>> GetByStatusAsync(PaymentStatus status);
        Task<IEnumerable<PaymentDto>> GetByPaymentMethodAsync(PaymentMethod paymentMethod);
        Task<IEnumerable<PaymentAuditLogDto>> GetPaymentAuditLogsAsync(int paymentId);
        Task<decimal> GetTotalPaymentsAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<decimal> GetTotalRefundsAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<PaymentStatistics> GetPaymentStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
    }

}

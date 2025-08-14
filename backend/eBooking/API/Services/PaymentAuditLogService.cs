using API.DTOs;
using API.Interfaces;
using API.Models;
using AutoMapper;

namespace API.Services
{
    public class PaymentAuditLogService : BaseDtoService<PaymentAuditLog, PaymentAuditLogDto, CreatePaymentAuditLogDto, UpdatePaymentAuditLogDto>, IPaymentAuditLogService
    {
        public PaymentAuditLogService(
            IRepository<PaymentAuditLog> repository,
            IMapper mapper,
            ILogger<PaymentAuditLogService> logger)
            : base(repository, mapper, logger)
        {
        }

        public async Task<IEnumerable<PaymentAuditLogDto>> GetByPaymentIdAsync(int paymentId)
        {
            try
            {
                _logger.LogInformation("Getting payment audit logs for payment ID: {PaymentId}", paymentId);
                var entities = await _repository.GetAllAsync();
                var filteredEntities = entities.Where(log => log.PaymentId == paymentId && !log.IsDeleted)
                                             .OrderByDescending(log => log.AttemptedAt);
                return _mapper.Map<IEnumerable<PaymentAuditLogDto>>(filteredEntities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment audit logs for payment ID: {PaymentId}", paymentId);
                throw;
            }
        }

        public async Task<IEnumerable<PaymentAuditLogDto>> GetByUserIdAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Getting payment audit logs for user ID: {UserId}", userId);
                var entities = await _repository.GetAllAsync();
                var filteredEntities = entities.Where(log => log.InitiatedByUserId == userId && !log.IsDeleted)
                                             .OrderByDescending(log => log.AttemptedAt);
                return _mapper.Map<IEnumerable<PaymentAuditLogDto>>(filteredEntities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment audit logs for user ID: {UserId}", userId);
                throw;
            }
        }

        public async Task<PaymentAuditLogDto> LogPaymentActionAsync(CreatePaymentAuditLogDto auditLogDto)
        {
            try
            {
                _logger.LogInformation("Logging payment action: {Action} for payment {PaymentId}", auditLogDto.Action, auditLogDto.PaymentId);
                return await CreateAsync(auditLogDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging payment action: {Action} for payment {PaymentId}", auditLogDto.Action, auditLogDto.PaymentId);
                throw;
            }
        }
    }
}

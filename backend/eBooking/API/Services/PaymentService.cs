using API.DTOs;
using API.Enums;
using API.Interfaces;
using API.Models;
using MassTransit;
using API.Contracts.Messages;
using AutoMapper;
using API.Queries;

namespace API.Services
{
    public class PaymentService : BaseDtoService<Payment, PaymentDto, CreatePaymentDto, UpdatePaymentDto>, IPaymentService
    {
        private readonly IEnumerable<IPaymentProvider> _paymentProviders;
        private readonly IPaymentAuditLogService _auditLogService;
        private readonly IBookingQueries _bookingQueries;

        private readonly IPublishEndpoint _publishEndpoint;

        public PaymentService(
            IRepository<Payment> repository,
            IMapper mapper,
            ILogger<PaymentService> logger,
            IEnumerable<IPaymentProvider> paymentProviders,
            IPaymentAuditLogService auditLogService,
            IBookingQueries bookingQueries,
            IPublishEndpoint publishEndpoint)
            : base(repository, mapper, logger)
        {
            _paymentProviders = paymentProviders;
            _auditLogService = auditLogService;
            _bookingQueries = bookingQueries;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<PaymentDto> ProcessPaymentAsync(CreatePaymentDto createPaymentDto, string? userAgent = null, string? ipAddress = null)
        {
            var payment = await CreateAsync(createPaymentDto);

            try
            {
                _logger.LogInformation("Processing payment {PaymentId} with method {PaymentMethod}", payment.Id, createPaymentDto.PaymentMethod);

                // Log payment attempt
                await LogPaymentActionAsync(payment.Id, PaymentStatus.Pending, PaymentStatus.Processing,
                    "ProcessPayment", $"Starting payment processing for amount {createPaymentDto.Amount:C}",
                    null, userAgent, ipAddress, createPaymentDto.UserId);

                // Find the appropriate payment provider
                var provider = _paymentProviders.FirstOrDefault(p => p.SupportedMethod == createPaymentDto.PaymentMethod);
                if (provider == null)
                {
                    await UpdatePaymentStatus(payment.Id, PaymentStatus.Failed, "Payment method not supported", userAgent, ipAddress, createPaymentDto.UserId);
                    throw new InvalidOperationException($"Payment method {createPaymentDto.PaymentMethod} is not supported");
                }

                // Process payment with provider
                var result = await provider.ProcessPaymentAsync(createPaymentDto);

                if (result.IsSuccess)
                {
                    // Update payment as completed
                    var updateDto = new UpdatePaymentDto
                    {
                        Id = payment.Id,
                        Status = PaymentStatus.Completed,
                        TransactionId = result.TransactionId,
                        Description = payment.Description
                    };

                    await UpdateAsync(payment.Id, updateDto);

                    // Update the payment entity directly for processed date
                    var paymentEntity = await _repository.GetByIdAsync(payment.Id);
                    if (paymentEntity != null)
                    {
                        paymentEntity.ProcessedAt = result.ProcessedAt;
                        paymentEntity.PaymentProviderResponse = result.ProviderResponse;
                        await _repository.UpdateAsync(paymentEntity);
                    }

                    // Ne diramo booking iz PaymentService; event će obraditi consumer

                    // Log successful payment
                    await LogPaymentActionAsync(payment.Id, PaymentStatus.Processing, PaymentStatus.Completed,
                        "ProcessPayment", $"Payment completed successfully. Transaction ID: {result.TransactionId}",
                        null, userAgent, ipAddress, createPaymentDto.UserId);

                    _logger.LogInformation("Payment {PaymentId} processed successfully", payment.Id);

                    // Publish PaymentCompleted event
                    await _publishEndpoint.Publish(new PaymentCompleted(
                        payment.Id,
                        createPaymentDto.BookingId,
                        createPaymentDto.UserId,
                        createPaymentDto.Amount,
                        result.TransactionId
                    ));
                }
                else
                {
                    // Update payment as failed
                    await UpdatePaymentStatus(payment.Id, PaymentStatus.Failed, result.ErrorMessage, userAgent, ipAddress, createPaymentDto.UserId);
                    _logger.LogWarning("Payment {PaymentId} failed: {ErrorMessage}", payment.Id, result.ErrorMessage);
                }

                // Return updated payment
                return await GetByIdAsync(payment.Id) ?? payment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment {PaymentId}", payment.Id);
                await UpdatePaymentStatus(payment.Id, PaymentStatus.Failed, "Payment processing failed due to technical error", userAgent, ipAddress, createPaymentDto.UserId);
                throw;
            }
        }

        public async Task<bool> RefundPaymentAsync(int paymentId, decimal amount, string reason, int? initiatedByUserId = null)
        {
            try
            {
                _logger.LogInformation("Processing refund for payment {PaymentId}, amount: {Amount}", paymentId, amount);

                var payment = await _repository.GetByIdAsync(paymentId);
                if (payment == null)
                {
                    _logger.LogWarning("Payment {PaymentId} not found for refund", paymentId);
                    return false;
                }

                if (payment.Status != PaymentStatus.Completed)
                {
                    _logger.LogWarning("Payment {PaymentId} cannot be refunded - status is {Status}", paymentId, payment.Status);
                    return false;
                }

                if (amount > payment.Amount)
                {
                    _logger.LogWarning("Refund amount {RefundAmount} exceeds payment amount {PaymentAmount}", amount, payment.Amount);
                    return false;
                }

                // Log refund attempt
                await LogPaymentActionAsync(paymentId, payment.Status, payment.Status,
                    "RefundPayment", $"Starting refund processing for amount {amount:C}. Reason: {reason}",
                    null, null, null, initiatedByUserId);

                // Find the appropriate payment provider
                var provider = _paymentProviders.FirstOrDefault(p => p.SupportedMethod == payment.PaymentMethod);
                if (provider == null)
                {
                    await LogPaymentActionAsync(paymentId, payment.Status, payment.Status,
                        "RefundPayment", null, "Payment provider not found", null, null, initiatedByUserId);
                    return false;
                }

                // Process refund with provider
                var result = await provider.ProcessRefundAsync(paymentId, amount, reason);

                if (result.IsSuccess)
                {
                    // Update payment with refund information
                    var currentRefundAmount = payment.RefundAmount ?? 0;
                    var newRefundAmount = currentRefundAmount + result.RefundedAmount;

                    payment.RefundAmount = newRefundAmount;
                    payment.RefundedAt = result.ProcessedAt;
                    payment.UpdatedAt = DateTime.UtcNow;

                    // Update status based on refund amount
                    if (newRefundAmount >= payment.Amount)
                    {
                        payment.Status = PaymentStatus.Refunded;
                    }
                    else
                    {
                        payment.Status = PaymentStatus.PartiallyRefunded;
                    }

                    await _repository.UpdateAsync(payment);

                    // Log successful refund
                    await LogPaymentActionAsync(paymentId, PaymentStatus.Completed, payment.Status,
                        "RefundPayment", $"Refund completed successfully. Amount: {result.RefundedAmount:C}. Refund Transaction ID: {result.RefundTransactionId}",
                        null, null, null, initiatedByUserId);

                    _logger.LogInformation("Refund processed successfully for payment {PaymentId}", paymentId);
                    return true;
                }
                else
                {
                    // Log failed refund
                    await LogPaymentActionAsync(paymentId, payment.Status, payment.Status,
                        "RefundPayment", null, result.ErrorMessage, null, null, initiatedByUserId);

                    _logger.LogWarning("Refund failed for payment {PaymentId}: {ErrorMessage}", paymentId, result.ErrorMessage);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing refund for payment {PaymentId}", paymentId);
                await LogPaymentActionAsync(paymentId, PaymentStatus.Completed, PaymentStatus.Completed,
                    "RefundPayment", null, "Refund processing failed due to technical error", null, null, initiatedByUserId);
                throw;
            }
        }

        public async Task<bool> CancelPaymentAsync(int paymentId, string reason, int? initiatedByUserId = null)
        {
            try
            {
                _logger.LogInformation("Cancelling payment {PaymentId}. Reason: {Reason}", paymentId, reason);

                var payment = await _repository.GetByIdAsync(paymentId);
                if (payment == null)
                {
                    _logger.LogWarning("Payment {PaymentId} not found for cancellation", paymentId);
                    return false;
                }

                if (payment.Status != PaymentStatus.Pending && payment.Status != PaymentStatus.Processing)
                {
                    _logger.LogWarning("Payment {PaymentId} cannot be cancelled - status is {Status}", paymentId, payment.Status);
                    return false;
                }

                // Update payment status
                var fromStatus = payment.Status;
                payment.Status = PaymentStatus.Cancelled;
                payment.FailureReason = reason;
                payment.UpdatedAt = DateTime.UtcNow;

                await _repository.UpdateAsync(payment);

                // Log cancellation
                await LogPaymentActionAsync(paymentId, fromStatus, PaymentStatus.Cancelled,
                    "CancelPayment", $"Payment cancelled. Reason: {reason}",
                    null, null, null, initiatedByUserId);

                _logger.LogInformation("Payment {PaymentId} cancelled successfully", paymentId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling payment {PaymentId}", paymentId);
                throw;
            }
        }

        public async Task<IEnumerable<PaymentDto>> GetByUserIdAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Getting payments for user ID: {UserId}", userId);
                var entities = await _repository.GetAllAsync();
                var filteredEntities = entities.Where(p => p.UserId == userId && !p.IsDeleted)
                                             .OrderByDescending(p => p.CreatedAt);
                return _mapper.Map<IEnumerable<PaymentDto>>(filteredEntities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payments for user ID: {UserId}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<PaymentDto>> GetByRoomIdAsync(int roomId)
        {
            try
            {
                _logger.LogInformation("Getting payments for room ID: {RoomId}", roomId);
                var entities = await _repository.GetAllAsync();
                var bookingIds = (await _bookingQueries.GetBookingIdsByRoomAsync(roomId)).ToHashSet();
                var filteredEntities = entities.Where(p => p.BookingId != 0 && bookingIds.Contains(p.BookingId))
                                             .OrderByDescending(p => p.CreatedAt);
                return _mapper.Map<IEnumerable<PaymentDto>>(filteredEntities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payments for room ID: {RoomId}", roomId);
                throw;
            }
        }

        public async Task<IEnumerable<PaymentDto>> GetByBookingIdAsync(int bookingId)
        {
            try
            {
                _logger.LogInformation("Getting payments for booking ID: {BookingId}", bookingId);
                var entities = await _repository.GetAllAsync();
                var filteredEntities = entities.Where(p => p.BookingId == bookingId && !p.IsDeleted)
                                             .OrderByDescending(p => p.CreatedAt);
                return _mapper.Map<IEnumerable<PaymentDto>>(filteredEntities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payments for booking ID: {BookingId}", bookingId);
                throw;
            }
        }

        public async Task<IEnumerable<PaymentDto>> GetByStatusAsync(PaymentStatus status)
        {
            try
            {
                _logger.LogInformation("Getting payments with status: {Status}", status);
                var entities = await _repository.GetAllAsync();
                var filteredEntities = entities.Where(p => p.Status == status && !p.IsDeleted)
                                             .OrderByDescending(p => p.CreatedAt);
                return _mapper.Map<IEnumerable<PaymentDto>>(filteredEntities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payments with status: {Status}", status);
                throw;
            }
        }

        public async Task<IEnumerable<PaymentDto>> GetByPaymentMethodAsync(PaymentMethod paymentMethod)
        {
            try
            {
                _logger.LogInformation("Getting payments with method: {PaymentMethod}", paymentMethod);
                var entities = await _repository.GetAllAsync();
                var filteredEntities = entities.Where(p => p.PaymentMethod == paymentMethod && !p.IsDeleted)
                                             .OrderByDescending(p => p.CreatedAt);
                return _mapper.Map<IEnumerable<PaymentDto>>(filteredEntities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payments with method: {PaymentMethod}", paymentMethod);
                throw;
            }
        }

        public async Task<IEnumerable<PaymentAuditLogDto>> GetPaymentAuditLogsAsync(int paymentId)
        {
            return await _auditLogService.GetByPaymentIdAsync(paymentId);
        }

        public async Task<decimal> GetTotalPaymentsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var entities = await _repository.GetAllAsync();
                var query = entities.Where(p => p.Status == PaymentStatus.Completed && !p.IsDeleted);

                if (fromDate.HasValue)
                    query = query.Where(p => p.ProcessedAt >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(p => p.ProcessedAt <= toDate.Value);

                return query.Sum(p => p.Amount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating total payments");
                throw;
            }
        }

        public async Task<decimal> GetTotalRefundsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var entities = await _repository.GetAllAsync();
                var query = entities.Where(p => (p.Status == PaymentStatus.Refunded || p.Status == PaymentStatus.PartiallyRefunded) && !p.IsDeleted);

                if (fromDate.HasValue)
                    query = query.Where(p => p.RefundedAt >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(p => p.RefundedAt <= toDate.Value);

                return query.Sum(p => p.RefundAmount ?? 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating total refunds");
                throw;
            }
        }

        private async Task UpdatePaymentStatus(int paymentId, PaymentStatus status, string? failureReason, string? userAgent, string? ipAddress, int? userId)
        {
            var payment = await _repository.GetByIdAsync(paymentId);
            if (payment != null)
            {
                var fromStatus = payment.Status;
                payment.Status = status;
                payment.FailureReason = failureReason;
                payment.UpdatedAt = DateTime.UtcNow;
                await _repository.UpdateAsync(payment);

                // Log status change
                await LogPaymentActionAsync(paymentId, fromStatus, status,
                    "UpdateStatus", failureReason, failureReason, userAgent, ipAddress, userId);
            }
        }

        private async Task LogPaymentActionAsync(int paymentId, PaymentStatus fromStatus, PaymentStatus toStatus,
            string action, string? details, string? errorMessage, string? userAgent, string? ipAddress, int? userId)
        {
            try
            {
                var auditLog = new CreatePaymentAuditLogDto
                {
                    PaymentId = paymentId,
                    FromStatus = fromStatus,
                    ToStatus = toStatus,
                    Action = action,
                    Details = details,
                    ErrorMessage = errorMessage,
                    UserAgent = userAgent,
                    IpAddress = ipAddress,
                    InitiatedByUserId = userId
                };

                await _auditLogService.LogPaymentActionAsync(auditLog);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging payment action for payment {PaymentId}", paymentId);
                // Don't throw - audit logging shouldn't break payment processing
            }
        }
    }
}

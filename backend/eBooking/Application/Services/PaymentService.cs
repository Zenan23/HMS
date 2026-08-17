using MassTransit;
using AutoMapper;
using Persistence.Models;
using Contracts.DTOs;
using Persistence.Interfaces;
using Application.Queries;
using Application.Configuration;
using Application.Services.PaymentProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Contracts.Enums;
using Contracts.Messages;
using Stripe;
using Stripe.Checkout;
using DomainPaymentMethod = Contracts.Enums.PaymentMethod;

namespace Application.Services
{
    public class PaymentService : BaseDtoService<Payment, PaymentDto, CreatePaymentDto, UpdatePaymentDto>, IPaymentService
    {
        private readonly IEnumerable<IPaymentGatewayProvider> _paymentProviders;
        private readonly StripePaymentProvider _stripePaymentProvider;
        private readonly PayPalPaymentProvider _payPalPaymentProvider;
        private readonly IPaymentAuditLogService _auditLogService;
        private readonly IBookingQueries _bookingQueries;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly PaymentOptions _paymentOptions;
        private readonly IWebhookEventDedupService _webhookDedup;
        private readonly IRepository<LoyaltyPointsEarned> _loyaltyPointsEarnedRepository;

        // 1 loyalty bod na svakih 10 (EUR/USD, ovisno o Payment.Currency) uplaćenih — ista logika
        // za sve provajdere jer svi prolaze kroz MarkPaymentCompletedCoreAsync.
        private const decimal LoyaltyPointsPerCurrencyUnit = 0.1m;

        public PaymentService(
            IRepository<Payment> repository,
            IMapper mapper,
            ILogger<PaymentService> logger,
            IEnumerable<IPaymentGatewayProvider> paymentProviders,
            StripePaymentProvider stripePaymentProvider,
            PayPalPaymentProvider payPalPaymentProvider,
            IPaymentAuditLogService auditLogService,
            IBookingQueries bookingQueries,
            IPublishEndpoint publishEndpoint,
            IOptions<PaymentOptions> paymentOptions,
            IWebhookEventDedupService webhookDedup,
            IRepository<LoyaltyPointsEarned> loyaltyPointsEarnedRepository)
            : base(repository, mapper, logger)
        {
            _paymentProviders = paymentProviders;
            _stripePaymentProvider = stripePaymentProvider;
            _payPalPaymentProvider = payPalPaymentProvider;
            _auditLogService = auditLogService;
            _bookingQueries = bookingQueries;
            _publishEndpoint = publishEndpoint;
            _paymentOptions = paymentOptions.Value;
            _webhookDedup = webhookDedup;
            _loyaltyPointsEarnedRepository = loyaltyPointsEarnedRepository;
        }

        public async Task<HostedCheckoutResponseDto> StartHostedCheckoutAsync(CreateHostedCheckoutDto dto, string? userAgent = null, string? ipAddress = null)
        {
            if (!_paymentOptions.UseHostedCheckout)
                throw new InvalidOperationException("Hosted checkout je onemogućen (Payments:UseHostedCheckout).");

            if (dto.PaymentMethod is not DomainPaymentMethod.Stripe and not DomainPaymentMethod.PayPal)
            {
                throw new InvalidOperationException("Podržane su samo metode Stripe i PayPal.");
            }

            var paymentEntity = _mapper.Map<Payment>(dto);
            paymentEntity = await _repository.AddAsync(paymentEntity);

            await LogPaymentActionAsync(paymentEntity.Id, PaymentStatus.Pending, PaymentStatus.Processing,
                "HostedCheckout", $"Kreiranje hosted checkout-a za {dto.Amount:C}",
                null, userAgent, ipAddress, dto.UserId);

            var provider = _paymentProviders.FirstOrDefault(p => p.SupportedMethod == dto.PaymentMethod);
            if (provider == null)
            {
                await UpdatePaymentStatus(paymentEntity.Id, PaymentStatus.Failed, "Provajder nije pronađen", userAgent, ipAddress, dto.UserId);
                throw new InvalidOperationException($"Nema provajdera za {dto.PaymentMethod}.");
            }

            HostedCheckoutUrls urls;
            if (dto.PaymentMethod == DomainPaymentMethod.Stripe)
            {
                urls = new HostedCheckoutUrls
                {
                    SuccessUrl = $"{_paymentOptions.Stripe.SuccessUrl.TrimEnd('/')}?paymentId={paymentEntity.Id}",
                    CancelUrl = $"{_paymentOptions.Stripe.CancelUrl.TrimEnd('/')}?paymentId={paymentEntity.Id}",
                };
            }
            else
            {
                urls = new HostedCheckoutUrls
                {
                    SuccessUrl = $"{_paymentOptions.PayPal.ReturnUrl.TrimEnd('/')}?paymentId={paymentEntity.Id}",
                    CancelUrl = $"{_paymentOptions.PayPal.CancelUrl.TrimEnd('/')}?paymentId={paymentEntity.Id}",
                };
            }

            var sessionResult = await provider.CreateHostedCheckoutAsync(paymentEntity, urls);
            if (!sessionResult.IsSuccess || string.IsNullOrEmpty(sessionResult.RedirectUrl))
            {
                await UpdatePaymentStatus(paymentEntity.Id, PaymentStatus.Failed, sessionResult.ErrorMessage, userAgent, ipAddress, dto.UserId);
                throw new InvalidOperationException(sessionResult.ErrorMessage ?? "Checkout nije kreiran.");
            }

            paymentEntity.Status = PaymentStatus.Processing;
            paymentEntity.CheckoutId = sessionResult.ProviderCheckoutId;
            paymentEntity.PaymentProviderResponse = "checkout_created";
            paymentEntity.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(paymentEntity);

            await LogPaymentActionAsync(paymentEntity.Id, PaymentStatus.Processing, PaymentStatus.Processing,
                "HostedCheckout", $"Redirect URL kreiran. CheckoutId={sessionResult.ProviderCheckoutId}",
                null, userAgent, ipAddress, dto.UserId);

            return new HostedCheckoutResponseDto
            {
                PaymentId = paymentEntity.Id,
                RedirectUrl = sessionResult.RedirectUrl,
                PaymentMethod = dto.PaymentMethod,
            };
        }

        public PaymentConfigDto GetPaymentConfig()
        {
            var stripeOk = !string.IsNullOrWhiteSpace(_paymentOptions.Stripe.SecretKey)
                && !string.IsNullOrWhiteSpace(_paymentOptions.Stripe.PublishableKey);
            var payPalOk = !string.IsNullOrWhiteSpace(_paymentOptions.PayPal.ClientId)
                && !string.IsNullOrWhiteSpace(_paymentOptions.PayPal.ClientSecret);

            return new PaymentConfigDto
            {
                EnableNativeCheckout = _paymentOptions.EnableNativeCheckout,
                UseHostedCheckout = _paymentOptions.UseHostedCheckout,
                StripePublishableKey = stripeOk ? _paymentOptions.Stripe.PublishableKey : null,
                StripeConfigured = stripeOk,
                PayPalConfigured = payPalOk,
            };
        }

        public async Task<StripeIntentResponseDto> StartStripeIntentAsync(CreateHostedCheckoutDto dto, string? userAgent = null, string? ipAddress = null)
        {
            if (!_paymentOptions.EnableNativeCheckout)
                throw new InvalidOperationException("In-app checkout je onemogućen (Payments:EnableNativeCheckout).");

            if (dto.PaymentMethod != DomainPaymentMethod.Stripe)
                throw new InvalidOperationException("Ovaj endpoint podržava samo Stripe.");

            if (string.IsNullOrWhiteSpace(_paymentOptions.Stripe.SecretKey) || string.IsNullOrWhiteSpace(_paymentOptions.Stripe.PublishableKey))
                throw new InvalidOperationException("Stripe SecretKey ili PublishableKey nisu konfigurisani.");

            var paymentEntity = _mapper.Map<Payment>(dto);
            paymentEntity = await _repository.AddAsync(paymentEntity);

            await LogPaymentActionAsync(paymentEntity.Id, PaymentStatus.Pending, PaymentStatus.Processing,
                "StripeIntent", $"Kreiranje PaymentIntent-a za {dto.Amount:C}",
                null, userAgent, ipAddress, dto.UserId);

            var intentResult = await _stripePaymentProvider.CreatePaymentIntentAsync(paymentEntity);
            if (!intentResult.IsSuccess || string.IsNullOrEmpty(intentResult.ClientSecret) || string.IsNullOrEmpty(intentResult.PaymentIntentId))
            {
                await UpdatePaymentStatus(paymentEntity.Id, PaymentStatus.Failed, intentResult.ErrorMessage, userAgent, ipAddress, dto.UserId);
                throw new InvalidOperationException(intentResult.ErrorMessage ?? "PaymentIntent nije kreiran.");
            }

            paymentEntity.Status = PaymentStatus.Processing;
            paymentEntity.CheckoutId = intentResult.PaymentIntentId;
            paymentEntity.PaymentProviderResponse = "payment_intent_created";
            paymentEntity.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(paymentEntity);

            await LogPaymentActionAsync(paymentEntity.Id, PaymentStatus.Processing, PaymentStatus.Processing,
                "StripeIntent", $"PaymentIntent kreiran. Id={intentResult.PaymentIntentId}",
                null, userAgent, ipAddress, dto.UserId);

            return new StripeIntentResponseDto
            {
                PaymentId = paymentEntity.Id,
                ClientSecret = intentResult.ClientSecret,
                PaymentIntentId = intentResult.PaymentIntentId,
                Currency = dto.Currency,
            };
        }

        public async Task<PayPalNativeOrderResponseDto> StartPayPalNativeOrderAsync(CreateHostedCheckoutDto dto, string? userAgent = null, string? ipAddress = null)
        {
            if (!_paymentOptions.EnableNativeCheckout)
                throw new InvalidOperationException("In-app checkout je onemogućen (Payments:EnableNativeCheckout).");

            if (dto.PaymentMethod != DomainPaymentMethod.PayPal)
                throw new InvalidOperationException("Ovaj endpoint podržava samo PayPal.");

            var paymentEntity = _mapper.Map<Payment>(dto);
            paymentEntity = await _repository.AddAsync(paymentEntity);

            await LogPaymentActionAsync(paymentEntity.Id, PaymentStatus.Pending, PaymentStatus.Processing,
                "PayPalNativeOrder", $"Kreiranje PayPal narudžbe za {dto.Amount:C}",
                null, userAgent, ipAddress, dto.UserId);

            var orderResult = await _payPalPaymentProvider.CreateNativeOrderAsync(
                paymentEntity,
                paymentEntity.Id,
                dto.ReturnUrl,
                dto.CancelUrl);
            if (!orderResult.IsSuccess || string.IsNullOrEmpty(orderResult.OrderId) || string.IsNullOrEmpty(orderResult.ApproveUrl))
            {
                await UpdatePaymentStatus(paymentEntity.Id, PaymentStatus.Failed, orderResult.ErrorMessage, userAgent, ipAddress, dto.UserId);
                throw new InvalidOperationException(orderResult.ErrorMessage ?? "PayPal narudžba nije kreirana.");
            }

            paymentEntity.Status = PaymentStatus.Processing;
            paymentEntity.CheckoutId = orderResult.OrderId;
            paymentEntity.PaymentProviderResponse = "paypal_order_created";
            paymentEntity.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(paymentEntity);

            await LogPaymentActionAsync(paymentEntity.Id, PaymentStatus.Processing, PaymentStatus.Processing,
                "PayPalNativeOrder", $"Order kreiran. Id={orderResult.OrderId}",
                null, userAgent, ipAddress, dto.UserId);

            return new PayPalNativeOrderResponseDto
            {
                PaymentId = paymentEntity.Id,
                OrderId = orderResult.OrderId,
                ApproveUrl = orderResult.ApproveUrl,
            };
        }

        public async Task<bool> TryConfirmStripePaymentIntentAsync(string paymentIntentId)
        {
            if (string.IsNullOrWhiteSpace(_paymentOptions.Stripe.SecretKey) || string.IsNullOrWhiteSpace(paymentIntentId))
                return false;

            try
            {
                var client = new StripeClient(_paymentOptions.Stripe.SecretKey);
                var service = new PaymentIntentService(client);
                var intent = await service.GetAsync(paymentIntentId);
                return await FinalizeStripePaymentIntentInternalAsync(intent);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "TryConfirmStripePaymentIntent nije uspio za {PaymentIntentId}", paymentIntentId);
                return false;
            }
        }

        public async Task<bool> TryFinalizeStripeFromSessionIdAsync(string checkoutSessionId)
        {
            if (string.IsNullOrWhiteSpace(_paymentOptions.Stripe.SecretKey) || string.IsNullOrWhiteSpace(checkoutSessionId))
                return false;

            try
            {
                var client = new StripeClient(_paymentOptions.Stripe.SecretKey);
                var service = new SessionService(client);
                var session = await service.GetAsync(checkoutSessionId);
                return await FinalizeStripeSessionInternalAsync(session);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "TryFinalizeStripeFromSessionId nije uspio za {SessionId}", checkoutSessionId);
                return false;
            }
        }

        public async Task<bool> ProcessStripeWebhookAsync(string json, string stripeSignatureHeader)
        {
            if (string.IsNullOrWhiteSpace(_paymentOptions.Stripe.WebhookSecret))
            {
                _logger.LogWarning("Stripe WebhookSecret nije postavljen.");
                return false;
            }

            Stripe.Event stripeEvent;
            try
            {
                stripeEvent = EventUtility.ConstructEvent(json, stripeSignatureHeader, _paymentOptions.Stripe.WebhookSecret, throwOnApiVersionMismatch: false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Stripe potpis webhooka nije validan.");
                return false;
            }

            if (!await _webhookDedup.TryMarkProcessedAsync("Stripe", stripeEvent.Id))
                return true;

            if (stripeEvent.Type == Stripe.Events.CheckoutSessionCompleted)
            {
                if (stripeEvent.Data.Object is Session session)
                {
                    await TryLinkWebhookPaymentAsync("Stripe", stripeEvent.Id, session.Metadata);
                    return await FinalizeStripeSessionInternalAsync(session);
                }
            }

            if (stripeEvent.Type == Stripe.Events.PaymentIntentSucceeded)
            {
                if (stripeEvent.Data.Object is PaymentIntent intent)
                {
                    await TryLinkWebhookPaymentAsync("Stripe", stripeEvent.Id, intent.Metadata);
                    return await FinalizeStripePaymentIntentInternalAsync(intent);
                }
            }

            return true;
        }

        /// <summary>
        /// Best-effort: poveži ProcessedWebhookEvent sa Payment zapisom (audit/debug), ako je payment_id
        /// dostupan u metapodacima. Nikad ne baca — greška ovdje ne smije uticati na obradu webhooka.
        /// </summary>
        private async Task TryLinkWebhookPaymentAsync(string provider, string eventId, IDictionary<string, string>? metadata)
        {
            try
            {
                if (metadata != null && metadata.TryGetValue("payment_id", out var paymentIdStr) && int.TryParse(paymentIdStr, out var paymentId))
                {
                    await _webhookDedup.LinkPaymentAsync(provider, eventId, paymentId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Nije moguće povezati webhook event {Provider}/{EventId} sa Payment zapisom (nekritično).", provider, eventId);
            }
        }

        private async Task TryLinkWebhookPaymentAsync(string provider, string eventId, int paymentId)
        {
            try
            {
                await _webhookDedup.LinkPaymentAsync(provider, eventId, paymentId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Nije moguće povezati webhook event {Provider}/{EventId} sa Payment zapisom (nekritično).", provider, eventId);
            }
        }

        public async Task<bool> ProcessPayPalWebhookAsync(string rawBody, string transmissionId, string transmissionTime, string certUrl, string authAlgo, string transmissionSig)
        {
            if (string.IsNullOrWhiteSpace(transmissionId))
                return false;

            if (!await _payPalPaymentProvider.VerifyWebhookAsync(transmissionId, transmissionTime, certUrl, authAlgo, transmissionSig, rawBody))
            {
                _logger.LogWarning("PayPal webhook verifikacija nije uspjela.");
                return false;
            }

            if (!await _webhookDedup.TryMarkProcessedAsync("PayPal", transmissionId))
                return true;

            using var doc = System.Text.Json.JsonDocument.Parse(rawBody);
            var root = doc.RootElement;
            if (!root.TryGetProperty("event_type", out var et))
                return true;

            var eventType = et.GetString();
            if (eventType != "PAYMENT.CAPTURE.COMPLETED")
                return true;

            if (!root.TryGetProperty("resource", out var resource))
                return true;

            var captureId = resource.TryGetProperty("id", out var idEl) ? idEl.GetString() : null;
            string? customId = null;
            if (resource.TryGetProperty("custom_id", out var cid))
                customId = cid.GetString();

            var paymentId = 0;
            if (!int.TryParse(customId, out paymentId))
            {
                string? orderId = null;
                if (resource.TryGetProperty("supplementary_data", out var sup) &&
                    sup.TryGetProperty("related_ids", out var rel) &&
                    rel.TryGetProperty("order_id", out var oid))
                {
                    orderId = oid.GetString();
                }

                if (!string.IsNullOrEmpty(orderId))
                {
                    var matches = await _repository.FindAsync(p => p.CheckoutId == orderId && !p.IsDeleted);
                    var pay = matches.OrderByDescending(p => p.Id).FirstOrDefault();
                    if (pay != null)
                        paymentId = pay.Id;
                }
            }

            if (paymentId == 0)
            {
                _logger.LogWarning("PayPal webhook: nije moguće mapirati payment id.");
                return true;
            }

            await TryLinkWebhookPaymentAsync("PayPal", transmissionId, paymentId);

            return await MarkPayPalPaymentCompletedAsync(paymentId, captureId, rawBody);
        }

        public async Task<bool> CapturePayPalAfterReturnAsync(string orderId, int? userId = null)
        {
            var result = await _payPalPaymentProvider.CaptureOrderAsync(orderId);
            if (!result.IsSuccess || string.IsNullOrEmpty(result.CaptureId))
            {
                _logger.LogWarning("PayPal capture nije uspio: {Msg}", result.ErrorMessage);
                throw new InvalidOperationException(result.ErrorMessage ?? "PayPal capture nije uspio.");
            }

            var payments = await _repository.FindAsync(p => p.CheckoutId == orderId && !p.IsDeleted);
            var paymentEntity = payments.OrderByDescending(p => p.Id).FirstOrDefault();
            if (paymentEntity == null)
            {
                _logger.LogWarning("Nije pronađen Payment za PayPal order {OrderId}", orderId);
                throw new InvalidOperationException("Nije pronađeno plaćanje za PayPal narudžbu.");
            }

            return await MarkPayPalPaymentCompletedAsync(paymentEntity.Id, result.CaptureId, $"capture_status:{result.OrderStatus}");
        }

        public Task<PaymentDto> ProcessPaymentAsync(CreatePaymentDto createPaymentDto, string? userAgent = null, string? ipAddress = null)
        {
            return Task.FromException<PaymentDto>(new InvalidOperationException("Koristite POST api/Payments/hosted-checkout (hosted Stripe/PayPal checkout)."));
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
                var result = await provider.ProcessRefundAsync(payment, amount, reason);

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

        public async Task<IEnumerable<PaymentDto>> GetByPaymentMethodAsync(DomainPaymentMethod paymentMethod)
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

        public async Task<PaymentStatistics> GetPaymentStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var entities = await _repository.GetAllAsync();
                var query = entities.Where(p => !p.IsDeleted);

                if (fromDate.HasValue)
                    query = query.Where(p => p.ProcessedAt >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(p => p.ProcessedAt <= toDate.Value);

                var totalPayments = query.Where(p => p.Status == PaymentStatus.Completed).Sum(p => p.Amount);
                var totalRefunds = query.Where(p => p.Status == PaymentStatus.Refunded || p.Status == PaymentStatus.PartiallyRefunded)
                    .Sum(p => p.RefundAmount ?? 0);
                var totalTransactions = query.Count();
                var successfulTransactions = query.Count(p => p.Status == PaymentStatus.Completed);
                var failedTransactions = query.Count(p => p.Status == PaymentStatus.Failed);

                // Monthly data for last 12 months
                var monthlyData = new List<MonthlyPaymentData>();
                for (int i = 11; i >= 0; i--)
                {
                    var monthStart = DateTime.UtcNow.AddMonths(-i).Date.AddDays(1 - DateTime.UtcNow.AddMonths(-i).Day);
                    var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                    
                    var monthQuery = query.Where(p => p.ProcessedAt >= monthStart && p.ProcessedAt <= monthEnd);
                    var monthAmount = monthQuery.Where(p => p.Status == PaymentStatus.Completed).Sum(p => p.Amount);
                    var monthCount = monthQuery.Count();

                    monthlyData.Add(new MonthlyPaymentData
                    {
                        Month = monthStart.ToString("MMM yyyy"),
                        TotalAmount = monthAmount,
                        TransactionCount = monthCount
                    });
                }

                return new PaymentStatistics
                {
                    TotalPayments = totalPayments,
                    TotalRefunds = totalRefunds,
                    NetPayments = totalPayments - totalRefunds,
                    TotalTransactions = totalTransactions,
                    SuccessfulTransactions = successfulTransactions,
                    FailedTransactions = failedTransactions,
                    FromDate = fromDate,
                    ToDate = toDate,
                    MonthlyData = monthlyData
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating payment statistics");
                throw;
            }
        }

        private async Task<bool> FinalizeStripeSessionInternalAsync(Session session)
        {
            if (session.PaymentStatus != "paid")
            {
                _logger.LogInformation("Stripe sesija {SessionId} status: {PayStatus}", session.Id, session.PaymentStatus);
                return false;
            }

            if (session.Metadata == null || !session.Metadata.ContainsKey("payment_id") || !int.TryParse(session.Metadata["payment_id"], out var paymentId))
            {
                _logger.LogWarning("Stripe sesija bez payment_id metapodatka.");
                return false;
            }

            var payment = await _repository.GetByIdAsync(paymentId);
            if (payment == null || payment.PaymentMethod != DomainPaymentMethod.Stripe)
                return false;

            if (payment.Status == PaymentStatus.Completed)
                return true;

            var intentId = session.PaymentIntentId;
            if (string.IsNullOrEmpty(intentId))
            {
                _logger.LogWarning("Stripe sesija bez PaymentIntentId.");
                return false;
            }

            return await MarkPaymentCompletedCoreAsync(payment, intentId, $"stripe_session:{session.Id}");
        }

        private async Task<bool> FinalizeStripePaymentIntentInternalAsync(PaymentIntent intent)
        {
            if (!string.Equals(intent.Status, "succeeded", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Stripe PaymentIntent {IntentId} status: {Status}", intent.Id, intent.Status);
                return false;
            }

            if (intent.Metadata == null || !intent.Metadata.ContainsKey("payment_id") || !int.TryParse(intent.Metadata["payment_id"], out var paymentId))
            {
                var payments = await _repository.FindAsync(p => p.CheckoutId == intent.Id && !p.IsDeleted);
                var byCheckout = payments.OrderByDescending(p => p.Id).FirstOrDefault();
                if (byCheckout == null)
                {
                    _logger.LogWarning("Stripe PaymentIntent bez payment_id metapodatka.");
                    return false;
                }
                paymentId = byCheckout.Id;
            }

            var payment = await _repository.GetByIdAsync(paymentId);
            if (payment == null || payment.PaymentMethod != DomainPaymentMethod.Stripe)
                return false;

            if (payment.Status == PaymentStatus.Completed)
                return true;

            return await MarkPaymentCompletedCoreAsync(payment, intent.Id, $"stripe_payment_intent:{intent.Id}");
        }

        private async Task<bool> MarkPayPalPaymentCompletedAsync(int paymentId, string? captureId, string? providerResponse)
        {
            var payment = await _repository.GetByIdAsync(paymentId);
            if (payment == null || payment.PaymentMethod != DomainPaymentMethod.PayPal)
                return false;

            if (payment.Status == PaymentStatus.Completed)
                return true;

            if (string.IsNullOrEmpty(captureId))
                return false;

            return await MarkPaymentCompletedCoreAsync(payment, captureId, providerResponse);
        }

        private async Task<bool> MarkPaymentCompletedCoreAsync(Payment payment, string transactionId, string? providerResponse)
        {
            if (payment.Status == PaymentStatus.Completed)
                return true;

            payment.Status = PaymentStatus.Completed;
            payment.TransactionId = transactionId;
            payment.ProcessedAt = DateTime.UtcNow;
            payment.PaymentProviderResponse = providerResponse;
            payment.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(payment);

            await LogPaymentActionAsync(payment.Id, PaymentStatus.Processing, PaymentStatus.Completed,
                "PaymentComplete", $"Transakcija {transactionId}", null, null, null, payment.UserId);

            await _publishEndpoint.Publish(new PaymentCompleted(
                payment.Id,
                payment.BookingId,
                payment.UserId,
                payment.Amount,
                transactionId));

            await AwardLoyaltyPointsAsync(payment);

            return true;
        }

        /// <summary>
        /// Best-effort dodjela loyalty bodova kad plaćanje pređe u Completed. Umotano u try/catch —
        /// greška ovdje NIKAD ne smije srušiti uspješno završeno plaćanje (isti princip kao
        /// webhook dedup PaymentId linkanje). Balans korisnika se ne čuva nigdje kao kolona,
        /// računa se on-the-fly kao SUM(LoyaltyPointsEarned) - SUM(LoyaltyPointsRedemption).
        /// </summary>
        private async Task AwardLoyaltyPointsAsync(Payment payment)
        {
            try
            {
                var points = (int)Math.Floor(payment.Amount * LoyaltyPointsPerCurrencyUnit);
                if (points <= 0)
                {
                    return;
                }

                await _loyaltyPointsEarnedRepository.AddAsync(new LoyaltyPointsEarned
                {
                    UserId = payment.UserId,
                    BookingId = payment.BookingId,
                    PaymentId = payment.Id,
                    PointsEarned = points,
                    EarnedAt = DateTime.UtcNow,
                    Reason = $"Uplata #{payment.Id} ({payment.Amount:0.##} {payment.Currency})"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to award loyalty points for payment {PaymentId}", payment.Id);
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

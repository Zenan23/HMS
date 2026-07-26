using Application.Configuration;
using Contracts.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Persistence.Interfaces;
using Persistence.Models;
using Stripe;
using Stripe.Checkout;
using DomainPaymentMethod = Contracts.Enums.PaymentMethod;

namespace Application.Services.PaymentProviders
{
    public class StripePaymentProvider : IPaymentGatewayProvider
    {
        private readonly StripePaymentOptions _stripe;
        private readonly ILogger<StripePaymentProvider> _logger;

        public StripePaymentProvider(IOptions<PaymentOptions> paymentOptions, ILogger<StripePaymentProvider> logger)
        {
            _logger = logger;
            _stripe = paymentOptions.Value.Stripe;
        }

        public DomainPaymentMethod SupportedMethod => DomainPaymentMethod.Stripe;

        public async Task<PaymentIntentSessionResult> CreatePaymentIntentAsync(
            Payment pendingPayment,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_stripe.SecretKey))
                {
                    return new PaymentIntentSessionResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Stripe SecretKey nije konfigurisan."
                    };
                }

                var unitAmount = (long)Math.Round(pendingPayment.Amount * 100m, MidpointRounding.AwayFromZero);
                if (unitAmount <= 0)
                {
                    return new PaymentIntentSessionResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Iznos mora biti veći od 0."
                    };
                }

                var client = new StripeClient(_stripe.SecretKey);
                var service = new PaymentIntentService(client);
                var options = new PaymentIntentCreateOptions
                {
                    Amount = unitAmount,
                    Currency = pendingPayment.Currency.ToLowerInvariant(),
                    // Eksplicitno kartica — pouzdanije za Payment Sheet nego automatic_payment_methods.
                    PaymentMethodTypes = new List<string> { "card" },
                    Metadata = new Dictionary<string, string>
                    {
                        ["payment_id"] = pendingPayment.Id.ToString(),
                        ["booking_id"] = pendingPayment.BookingId.ToString(),
                        ["user_id"] = pendingPayment.UserId.ToString(),
                    },
                    Description = string.IsNullOrWhiteSpace(pendingPayment.Description)
                        ? $"Rezervacija #{pendingPayment.BookingId}"
                        : pendingPayment.Description,
                };

                var intent = await service.CreateAsync(options, requestOptions: null, cancellationToken);
                if (string.IsNullOrEmpty(intent.ClientSecret))
                {
                    return new PaymentIntentSessionResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Stripe nije vratio client secret."
                    };
                }

                return new PaymentIntentSessionResult
                {
                    IsSuccess = true,
                    ClientSecret = intent.ClientSecret,
                    PaymentIntentId = intent.Id,
                };
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe PaymentIntent greška");
                return new PaymentIntentSessionResult { IsSuccess = false, ErrorMessage = ex.Message };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Neočekivana greška pri Stripe PaymentIntent-u");
                return new PaymentIntentSessionResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Greška pri kreiranju Stripe PaymentIntent-a."
                };
            }
        }

        public async Task<HostedCheckoutSessionResult> CreateHostedCheckoutAsync(
            Payment pendingPayment,
            HostedCheckoutUrls urls,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_stripe.SecretKey))
                {
                    return new HostedCheckoutSessionResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Stripe SecretKey nije konfigurisan."
                    };
                }

                var client = new StripeClient(_stripe.SecretKey);
                var service = new SessionService(client);

                var unitAmount = (long)Math.Round(pendingPayment.Amount * 100m, MidpointRounding.AwayFromZero);
                if (unitAmount <= 0)
                {
                    return new HostedCheckoutSessionResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Iznos mora biti veći od 0."
                    };
                }

                var successUrl = urls.SuccessUrl.Contains("{CHECKOUT_SESSION_ID}", StringComparison.Ordinal)
                    ? urls.SuccessUrl
                    : $"{urls.SuccessUrl.TrimEnd('/')}{(urls.SuccessUrl.Contains('?', StringComparison.Ordinal) ? "&" : "?")}session_id={{CHECKOUT_SESSION_ID}}";

                var options = new SessionCreateOptions
                {
                    Mode = "payment",
                    SuccessUrl = successUrl,
                    CancelUrl = urls.CancelUrl,
                    ClientReferenceId = pendingPayment.Id.ToString(),
                    Metadata = new Dictionary<string, string>
                    {
                        ["payment_id"] = pendingPayment.Id.ToString(),
                        ["booking_id"] = pendingPayment.BookingId.ToString(),
                        ["user_id"] = pendingPayment.UserId.ToString(),
                    },
                    LineItems = new List<SessionLineItemOptions>
                    {
                        new()
                        {
                            Quantity = 1,
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                Currency = pendingPayment.Currency.ToLowerInvariant(),
                                UnitAmount = unitAmount,
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = $"Rezervacija #{pendingPayment.BookingId}",
                                    Description = string.IsNullOrWhiteSpace(pendingPayment.Description)
                                        ? null
                                        : pendingPayment.Description,
                                },
                            },
                        },
                    },
                };

                var session = await service.CreateAsync(options, requestOptions: null, cancellationToken);

                return new HostedCheckoutSessionResult
                {
                    IsSuccess = true,
                    RedirectUrl = session.Url,
                    ProviderCheckoutId = session.Id,
                };
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe Checkout Session greška");
                return new HostedCheckoutSessionResult { IsSuccess = false, ErrorMessage = ex.Message };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Neočekivana greška pri Stripe checkout-u");
                return new HostedCheckoutSessionResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Greška pri kreiranju Stripe sesije."
                };
            }
        }

        public async Task<RefundResult> ProcessRefundAsync(
            Payment payment,
            decimal amount,
            string reason,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_stripe.SecretKey))
                {
                    return new RefundResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Stripe nije konfigurisan.",
                        ProcessedAt = DateTime.UtcNow
                    };
                }

                if (string.IsNullOrWhiteSpace(payment.TransactionId))
                {
                    return new RefundResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Nedostaje PaymentIntent (TransactionId).",
                        ProcessedAt = DateTime.UtcNow
                    };
                }

                var client = new StripeClient(_stripe.SecretKey);
                var service = new RefundService(client);
                var refundAmount = (long)Math.Round(amount * 100m, MidpointRounding.AwayFromZero);

                var options = new RefundCreateOptions
                {
                    PaymentIntent = payment.TransactionId,
                    Amount = refundAmount,
                    Reason = RefundReasons.RequestedByCustomer,
                    Metadata = new Dictionary<string, string> { ["reason"] = reason },
                };

                var refund = await service.CreateAsync(options, requestOptions: null, cancellationToken);

                return new RefundResult
                {
                    IsSuccess = true,
                    RefundTransactionId = refund.Id,
                    RefundedAmount = amount,
                    ProcessedAt = DateTime.UtcNow,
                };
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe refund greška za payment {PaymentId}", payment.Id);
                return new RefundResult
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message,
                    ProcessedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Refund greška za payment {PaymentId}", payment.Id);
                return new RefundResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Refund nije uspio.",
                    ProcessedAt = DateTime.UtcNow
                };
            }
        }
    }
}

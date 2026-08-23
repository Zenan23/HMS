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
                    // Eksplicitna lista (ne automatic_payment_methods) — pouzdanije za Payment Sheet i
                    // sprječava da se nenamjerno pojave metode koje nisu dio prijave (Google/Apple Pay,
                    // Klarna i sl.), iako su uključene u Stripe nalogu. Tri metode iz prijave (poglavlje
                    // 7): kartica, PayPal (procesiran kroz Stripe), bankovni transfer (SEPA Direct Debit —
                    // pravi bank-to-bank transfer mehanizam, najbliži doslovnom značenju "bankovni
                    // transfer" iz prijave; alternativa EPS je probana i radi, ali SEPA je zadržan).
                    // SEPA je "delayed notification" metoda — potvrda zna stići par minuta kasnije čak i
                    // u test modu (vidi docs.stripe.com/testing). To NIJE bug — app i UI (Reservations
                    // ekran) to eksplicitno prikazuju kao "plaćanje u obradi", ne kao grešku. Detalji:
                    // PAYMENT_INTEGRATION.md.
                    // Payment Sheet automatski prikazuje sve tri korisniku i vodi ga kroz odgovarajući
                    // flow (unos kartice / PayPal redirect / IBAN+mandat za SEPA) — nema dodatnog koda.
                    PaymentMethodTypes = new List<string> { "card", "paypal", "sepa_debit" },
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
                    // Ista eksplicitna lista kao native in-app flow (vidi CreatePaymentIntentAsync) —
                    // fallback hosted checkout ne smije iznenada prikazati druge Dashboard-enabled
                    // metode (Google/Apple Pay, Klarna i sl.) koje nisu dio prijave.
                    PaymentMethodTypes = new List<string> { "card", "paypal", "sepa_debit" },
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

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Configuration;
using Contracts.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Persistence.Interfaces;
using Persistence.Models;

namespace Application.Services.PaymentProviders
{
    public class PayPalPaymentProvider : IPaymentGatewayProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly PayPalPaymentOptions _paypal;
        private readonly PaymentOptions _paymentOptions;
        private readonly ILogger<PayPalPaymentProvider> _logger;
        private readonly SemaphoreSlim _tokenLock = new(1, 1);
        private string? _cachedAccessToken;
        private DateTime _tokenExpiresAtUtc;

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        public PayPalPaymentProvider(
            IHttpClientFactory httpClientFactory,
            IOptions<PaymentOptions> paymentOptions,
            ILogger<PayPalPaymentProvider> logger)
        {
            _httpClientFactory = httpClientFactory;
            _paymentOptions = paymentOptions.Value;
            _paypal = paymentOptions.Value.PayPal;
            _logger = logger;
        }

        public PaymentMethod SupportedMethod => PaymentMethod.PayPal;

        public async Task<HostedCheckoutSessionResult> CreateHostedCheckoutAsync(
            Payment pendingPayment,
            HostedCheckoutUrls urls,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_paypal.ClientId) || string.IsNullOrWhiteSpace(_paypal.ClientSecret))
                {
                    return new HostedCheckoutSessionResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "PayPal ClientId/ClientSecret nisu konfigurisani."
                    };
                }

                var token = await GetAccessTokenAsync(cancellationToken);
                if (token == null)
                {
                    return new HostedCheckoutSessionResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "PayPal OAuth token nije dostupan."
                    };
                }

                var client = CreateApiClient(token);
                var amountStr = pendingPayment.Amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
                var currency = pendingPayment.Currency.ToUpperInvariant();

                var body = new PayPalCreateOrderRequest
                {
                    Intent = "CAPTURE",
                    PurchaseUnits =
                    [
                        new PayPalPurchaseUnit
                        {
                            Amount = new PayPalMoney { CurrencyCode = currency, Value = amountStr },
                            CustomId = pendingPayment.Id.ToString(),
                            Description = pendingPayment.Description ?? $"Booking {pendingPayment.BookingId}",
                        }
                    ],
                    ApplicationContext = new PayPalApplicationContext
                    {
                        ReturnUrl = urls.SuccessUrl,
                        CancelUrl = urls.CancelUrl,
                        UserAction = "PAYNOW",
                    },
                };

                var json = JsonSerializer.Serialize(body, JsonOpts);
                using var request = new HttpRequestMessage(HttpMethod.Post, "v2/checkout/orders")
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json"),
                };

                using var response = await client.SendAsync(request, cancellationToken);
                var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("PayPal create order failed: {Status} {Body}", response.StatusCode, responseText);
                    return new HostedCheckoutSessionResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "PayPal kreiranje narudžbe nije uspjelo."
                    };
                }

                using var doc = JsonDocument.Parse(responseText);
                var root = doc.RootElement;
                var orderId = root.GetProperty("id").GetString();
                string? approveUrl = null;
                if (root.TryGetProperty("links", out var links))
                {
                    foreach (var link in links.EnumerateArray())
                    {
                        if (link.GetProperty("rel").GetString() == "approve")
                        {
                            approveUrl = link.GetProperty("href").GetString();
                            break;
                        }
                    }
                }

                if (string.IsNullOrEmpty(orderId) || string.IsNullOrEmpty(approveUrl))
                {
                    return new HostedCheckoutSessionResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "PayPal odgovor ne sadrži order id ili approve link."
                    };
                }

                return new HostedCheckoutSessionResult
                {
                    IsSuccess = true,
                    RedirectUrl = approveUrl,
                    ProviderCheckoutId = orderId,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PayPal checkout greška");
                return new HostedCheckoutSessionResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Greška pri PayPal checkout-u."
                };
            }
        }

        /// <summary>Capture after buyer returns (token = order id).</summary>
        public async Task<PayPalCaptureOperationResult> CaptureOrderAsync(string orderId, CancellationToken cancellationToken = default)
        {
            try
            {
                var token = await GetAccessTokenAsync(cancellationToken);
                if (token == null)
                {
                    return new PayPalCaptureOperationResult(false, null, null, "OAuth token nedostaje.");
                }

                var client = CreateApiClient(token);
                using var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    $"v2/checkout/orders/{Uri.EscapeDataString(orderId)}/capture");

                using var response = await client.SendAsync(request, cancellationToken);
                var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("PayPal capture failed: {Status} {Body}", response.StatusCode, responseText);
                    return new PayPalCaptureOperationResult(false, null, null, "Capture nije uspio.");
                }

                using var doc = JsonDocument.Parse(responseText);
                var root = doc.RootElement;
                var status = root.GetProperty("status").GetString();
                string? captureId = null;
                if (root.TryGetProperty("purchase_units", out var units) && units.GetArrayLength() > 0)
                {
                    var pu = units[0];
                    if (pu.TryGetProperty("payments", out var payments) &&
                        payments.TryGetProperty("captures", out var captures) &&
                        captures.GetArrayLength() > 0)
                    {
                        captureId = captures[0].GetProperty("id").GetString();
                    }
                }

                return new PayPalCaptureOperationResult(
                    string.Equals(status, "COMPLETED", StringComparison.OrdinalIgnoreCase),
                    captureId,
                    status,
                    null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PayPal capture greška za order {OrderId}", orderId);
                return new PayPalCaptureOperationResult(false, null, null, ex.Message);
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
                if (string.IsNullOrWhiteSpace(payment.TransactionId))
                {
                    return new RefundResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Nedostaje PayPal capture id (TransactionId).",
                        ProcessedAt = DateTime.UtcNow
                    };
                }

                var token = await GetAccessTokenAsync(cancellationToken);
                if (token == null)
                {
                    return new RefundResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "OAuth token nedostaje.",
                        ProcessedAt = DateTime.UtcNow
                    };
                }

                var client = CreateApiClient(token);
                var amountStr = amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
                var body = new { amount = new { value = amountStr, currency_code = payment.Currency.ToUpperInvariant() } };
                var json = JsonSerializer.Serialize(body, JsonOpts);
                using var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    $"v2/payments/captures/{Uri.EscapeDataString(payment.TransactionId)}/refund")
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json"),
                };

                using var response = await client.SendAsync(request, cancellationToken);
                var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("PayPal refund failed: {Status} {Body}", response.StatusCode, responseText);
                    return new RefundResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "PayPal refund nije uspio.",
                        ProcessedAt = DateTime.UtcNow
                    };
                }

                using var doc = JsonDocument.Parse(responseText);
                var refundId = doc.RootElement.GetProperty("id").GetString();

                return new RefundResult
                {
                    IsSuccess = true,
                    RefundTransactionId = refundId,
                    RefundedAmount = amount,
                    ProcessedAt = DateTime.UtcNow,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PayPal refund greška");
                return new RefundResult
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message,
                    ProcessedAt = DateTime.UtcNow
                };
            }
        }

        public async Task<bool> VerifyWebhookAsync(
            string transmissionId,
            string transmissionTime,
            string certUrl,
            string authAlgo,
            string transmissionSig,
            string webhookBody,
            CancellationToken cancellationToken = default)
        {
            if (_paymentOptions.SkipPayPalWebhookVerification)
            {
                _logger.LogWarning("PayPal webhook verifikacija je preskočena (SkipPayPalWebhookVerification).");
                return true;
            }

            if (string.IsNullOrWhiteSpace(_paypal.WebhookId))
                return false;

            try
            {
                var token = await GetAccessTokenAsync(cancellationToken);
                if (token == null)
                    return false;

                var client = CreateApiClient(token);
                var verifyPayload = new
                {
                    transmission_id = transmissionId,
                    transmission_time = transmissionTime,
                    cert_url = certUrl,
                    auth_algo = authAlgo,
                    transmission_sig = transmissionSig,
                    webhook_id = _paypal.WebhookId,
                    webhook_event = JsonSerializer.Deserialize<JsonElement>(webhookBody),
                };

                var json = JsonSerializer.Serialize(verifyPayload, JsonOpts);
                using var request = new HttpRequestMessage(HttpMethod.Post, "v1/notifications/verify-webhook-signature")
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json"),
                };

                using var response = await client.SendAsync(request, cancellationToken);
                var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
                if (!response.IsSuccessStatusCode)
                    return false;

                using var doc = JsonDocument.Parse(responseText);
                return doc.RootElement.TryGetProperty("verification_status", out var vs) &&
                       string.Equals(vs.GetString(), "SUCCESS", StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PayPal webhook verify greška");
                return false;
            }
        }

        private HttpClient CreateApiClient(string bearer)
        {
            var client = _httpClientFactory.CreateClient("PayPalApi");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearer);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }

        private async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken)
        {
            await _tokenLock.WaitAsync(cancellationToken);
            try
            {
                if (_cachedAccessToken != null && DateTime.UtcNow < _tokenExpiresAtUtc.AddMinutes(-2))
                    return _cachedAccessToken;

                var baseUrl = _paypal.BaseUrl.TrimEnd('/');
                using var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(baseUrl + "/");
                var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_paypal.ClientId}:{_paypal.ClientSecret}"));
                using var request = new HttpRequestMessage(HttpMethod.Post, "v1/oauth2/token");
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
                request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "client_credentials",
                });

                using var response = await client.SendAsync(request, cancellationToken);
                var text = await response.Content.ReadAsStringAsync(cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("PayPal OAuth failed: {Status} {Body}", response.StatusCode, text);
                    return null;
                }

                using var doc = JsonDocument.Parse(text);
                _cachedAccessToken = doc.RootElement.GetProperty("access_token").GetString();
                var expiresIn = doc.RootElement.GetProperty("expires_in").GetInt32();
                _tokenExpiresAtUtc = DateTime.UtcNow.AddSeconds(expiresIn);
                return _cachedAccessToken;
            }
            finally
            {
                _tokenLock.Release();
            }
        }

        #region PayPal JSON models

        private sealed class PayPalCreateOrderRequest
        {
            public string Intent { get; set; } = "CAPTURE";
            public List<PayPalPurchaseUnit> PurchaseUnits { get; set; } = new();
            public PayPalApplicationContext? ApplicationContext { get; set; }
        }

        private sealed class PayPalPurchaseUnit
        {
            public PayPalMoney Amount { get; set; } = new();
            public string? CustomId { get; set; }
            public string? Description { get; set; }
        }

        private sealed class PayPalMoney
        {
            public string CurrencyCode { get; set; } = "USD";
            public string Value { get; set; } = "0.00";
        }

        private sealed class PayPalApplicationContext
        {
            public string ReturnUrl { get; set; } = string.Empty;
            public string CancelUrl { get; set; } = string.Empty;
            public string UserAction { get; set; } = "PAYNOW";
        }

        #endregion
    }

    public sealed record PayPalCaptureOperationResult(
        bool IsSuccess,
        string? CaptureId,
        string? OrderStatus,
        string? ErrorMessage);
}

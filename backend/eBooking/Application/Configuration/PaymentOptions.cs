namespace Application.Configuration
{
    public class PaymentOptions
    {
        public const string SectionName = "Payments";

        /// <summary>When true, only hosted checkout + webhooks are supported.</summary>
        public bool UseHostedCheckout { get; set; } = true;

        public StripePaymentOptions Stripe { get; set; } = new();
        public PayPalPaymentOptions PayPal { get; set; } = new();

        /// <summary>Skip PayPal webhook signature verification (local dev only).</summary>
        public bool SkipPayPalWebhookVerification { get; set; }
    }

    public class StripePaymentOptions
    {
        public string SecretKey { get; set; } = string.Empty;
        public string WebhookSecret { get; set; } = string.Empty;
        public string SuccessUrl { get; set; } = string.Empty;
        public string CancelUrl { get; set; } = string.Empty;
    }

    public class PayPalPaymentOptions
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        /// <summary>e.g. https://api-m.sandbox.paypal.com</summary>
        public string BaseUrl { get; set; } = "https://api-m.sandbox.paypal.com";
        public string WebhookId { get; set; } = string.Empty;
        public string ReturnUrl { get; set; } = string.Empty;
        public string CancelUrl { get; set; } = string.Empty;
    }
}

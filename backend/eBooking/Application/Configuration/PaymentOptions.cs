namespace Application.Configuration
{
    public class PaymentOptions
    {
        public const string SectionName = "Payments";

        /// <summary>When true, hosted checkout redirect flow remains available (fallback).</summary>
        public bool UseHostedCheckout { get; set; } = true;

        /// <summary>When true, mobile clients use in-app Stripe Payment Sheet (kartica + Google Pay).</summary>
        public bool EnableNativeCheckout { get; set; } = true;

        public StripePaymentOptions Stripe { get; set; } = new();
    }

    public class StripePaymentOptions
    {
        public string SecretKey { get; set; } = string.Empty;
        /// <summary>Publishable key for mobile Payment Sheet (pk_test_… / pk_live_…).</summary>
        public string PublishableKey { get; set; } = string.Empty;
        public string WebhookSecret { get; set; } = string.Empty;
        public string SuccessUrl { get; set; } = string.Empty;
        public string CancelUrl { get; set; } = string.Empty;
    }
}

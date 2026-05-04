using System.ComponentModel.DataAnnotations;

namespace Contracts.DTOs
{
    public class PayPalCaptureRequestDto
    {
        /// <summary>PayPal order id (query param <c>token</c> on return URL).</summary>
        [Required]
        public string Token { get; set; } = string.Empty;
    }
}

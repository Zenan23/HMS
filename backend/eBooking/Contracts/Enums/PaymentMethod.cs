namespace Contracts.Enums
{
    public enum PaymentMethod
    {
        /// <summary>Legacy; do not use for new hosted checkout.</summary>
        Card = 1,
        PayPal = 2,
        /// <summary>Legacy; do not use for new hosted checkout.</summary>
        BankTransfer = 3,
        Stripe = 4,
    }
}

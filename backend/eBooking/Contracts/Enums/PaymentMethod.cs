namespace Contracts.Enums
{
    public enum PaymentMethod
    {
        /// <summary>Legacy; do not use for new hosted checkout.</summary>
        Card = 1,
        /// <summary>
        /// PayPal je uklonjen iz aplikacije (vidi TODO-payment-paypal-removal.md). Vrijednost
        /// ostaje rezervisana (ne brisati/renumerisati) da ne polomi historijske Payment redove
        /// koji su eventualno sačuvani sa ovom vrijednosti.
        /// </summary>
        [Obsolete("PayPal je uklonjen. Ne koristiti za novi checkout.")]
        PayPal = 2,
        /// <summary>Legacy; do not use for new hosted checkout.</summary>
        BankTransfer = 3,
        Stripe = 4,
    }
}

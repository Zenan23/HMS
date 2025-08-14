namespace API.Contracts.Messages
{
    public record PaymentCompleted(int PaymentId, int BookingId, int? UserId, decimal Amount, string? TransactionId);
}



namespace Contracts.Exceptions
{
    /// <summary>
    /// Traženi domenski resurs ne postoji (ili je soft-deleted). Middleware
    /// (<c>GlobalExceptionMiddleware</c>) mapira ovaj tip na HTTP 404.
    /// </summary>
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message)
        {
        }

        public NotFoundException(string entityName, object key)
            : base($"{entityName} sa ID {key} nije pronađen.")
        {
        }
    }
}

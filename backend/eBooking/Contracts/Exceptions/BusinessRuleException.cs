namespace Contracts.Exceptions
{
    /// <summary>
    /// Kršenje poslovnog pravila koje klijent može ispraviti (npr. soba nije dostupna za odabrani
    /// period, plaćanje već postoji za rezervaciju). Middleware (<c>GlobalExceptionMiddleware</c>)
    /// mapira ovaj tip na HTTP 409 sa porukom koja je bezbjedna za prikaz korisniku — poruka
    /// ove izuzetka NIKAD ne smije sadržavati interne detalje (stack trace, SQL, itd.), jer se
    /// šalje direktno u response.
    /// </summary>
    public class BusinessRuleException : Exception
    {
        public BusinessRuleException(string message) : base(message)
        {
        }

        public BusinessRuleException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}

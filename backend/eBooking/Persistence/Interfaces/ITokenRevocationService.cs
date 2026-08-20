namespace Persistence.Interfaces
{
    /// <summary>
    /// Server-side invalidacija JWT tokena (logout). Vidi Persistence.Models.RevokedToken za
    /// objašnjenje pristupa (JWT je stateless pa se poništeni tokeni moraju evidentirati na serveru).
    /// </summary>
    public interface ITokenRevocationService
    {
        /// <summary>Poništi token (logout) — jti se upisuje u listu poništenih dok ne istekne.</summary>
        Task RevokeAsync(string jti, int userId, DateTime expiresAt, CancellationToken cancellationToken = default);

        /// <summary>Da li je token sa datim jti poništen (koristi se u JWT bearer OnTokenValidated event-u).</summary>
        Task<bool> IsRevokedAsync(string jti, CancellationToken cancellationToken = default);
    }
}

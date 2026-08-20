using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using Persistence.Interfaces;
using Persistence.Models;

namespace Persistence.Services
{
    public class TokenRevocationService : ITokenRevocationService
    {
        private readonly ApplicationDbContext _db;

        public TokenRevocationService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task RevokeAsync(string jti, int userId, DateTime expiresAt, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(jti))
                return;

            // Idempotentno — ako je token (npr. duplim klikom na logout) već poništen, ne radi ništa.
            var alreadyRevoked = await _db.RevokedTokens.AsNoTracking()
                .AnyAsync(t => t.Jti == jti, cancellationToken);
            if (alreadyRevoked)
                return;

            _db.RevokedTokens.Add(new RevokedToken
            {
                Jti = jti,
                UserId = userId,
                ExpiresAt = expiresAt,
                RevokedAt = DateTime.UtcNow
            });

            try
            {
                await _db.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                // Race sa istim jti (unique index) — token je već poništen, ništa dodatno ne treba.
            }
        }

        public async Task<bool> IsRevokedAsync(string jti, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(jti))
                return false;

            return await _db.RevokedTokens.AsNoTracking()
                .AnyAsync(t => t.Jti == jti, cancellationToken);
        }
    }
}

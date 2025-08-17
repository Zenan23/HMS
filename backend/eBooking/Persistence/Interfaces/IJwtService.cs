using Persistence.Models;
using System.Security.Claims;

namespace Persistence.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        ClaimsPrincipal? ValidateToken(string token);
        DateTime GetTokenExpiration(string token);
    }

}

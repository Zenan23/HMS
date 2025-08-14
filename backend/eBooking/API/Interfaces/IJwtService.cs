using API.Models;
using System.Security.Claims;

namespace API.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        ClaimsPrincipal? ValidateToken(string token);
        DateTime GetTokenExpiration(string token);
    }

}

using API.Enums;
using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace API.Attributes
{
    public class AuthorizeRoleAttribute : AuthorizeAttribute
    {
        public AuthorizeRoleAttribute(params UserRole[] roles)
        {
            Roles = string.Join(",", roles.Select(r => r.ToString()));
        }
    }
}

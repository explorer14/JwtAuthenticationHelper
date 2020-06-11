using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace JwtGenerator.Types
{
    public sealed class TokenWithClaimsPrincipal
    {
        public string AccessToken { get; internal set; }

        public ClaimsPrincipal ClaimsPrincipal { get; internal set; }

        public AuthenticationProperties AuthProperties { get; internal set; }
    }
}
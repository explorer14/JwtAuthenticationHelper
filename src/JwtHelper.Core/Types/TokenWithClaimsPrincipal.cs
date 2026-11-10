using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace JwtHelper.Core.Types
{
    public sealed class TokenWithClaimsPrincipal
    {
        public string AccessToken { get; internal init; }

        public ClaimsPrincipal ClaimsPrincipal { get; internal init; }

        public AuthenticationProperties AuthProperties { get; internal init; }
    }
}
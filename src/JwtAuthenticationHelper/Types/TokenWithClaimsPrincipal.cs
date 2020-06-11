using Microsoft.AspNetCore.Authentication;
using System;
using System.Security.Claims;

namespace JwtAuthenticationHelper.Types
{
    [Obsolete("Please use the JwtGenerator in combination with Cookies or JwtBearer extension packages. This project will be removed")]
    public sealed class TokenWithClaimsPrincipal
    {
        public string AccessToken { get; internal set; }

        public ClaimsPrincipal ClaimsPrincipal { get; internal set; }

        public AuthenticationProperties AuthProperties { get; internal set; }
    }
}
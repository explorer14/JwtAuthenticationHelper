using JwtAuthenticationHelper.Types;
using System.Collections.Generic;
using System.Security.Claims;

namespace JwtAuthenticationHelper.Abstractions
{
    public interface IJwtTokenGenerator
    {
        /// <summary>
        /// Generate a JSON Web Token with a ClaimsPrincipal object both containing the claims passed
        /// in. Use this method for ASP.NET Core application with cookie authentication.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="userClaims">a list of claims about the authenticated user</param>
        /// <returns>
        /// <see cref="TokenWithClaimsPrincipal"/> containing the JWT and the <see
        /// cref="ClaimsPrincipal"/> instance
        /// </returns>
        TokenWithClaimsPrincipal GenerateAccessTokenWithClaimsPrincipal(string userName,
            IEnumerable<Claim> userClaims);

        /// <summary>
        /// Generate a string JSON Web Token containing the claims passed in. Use this method for
        /// ASP.NET Core Web API applications with cookieless authentication.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="userClaims"></param>
        /// <returns><see cref="string"/>a plain JWT</returns>
        string GenerateAccessToken(string userName, IEnumerable<Claim> userClaims);
    }
}
using System.Collections.Generic;
using System.Security.Claims;

namespace JwtAuthenticationHelper
{
    public sealed class ClaimsPrincipalFactory
    {
        /// <summary>
        /// Create a <see cref="ClaimsPrincipal"/> using the claims passed
        /// in. After this call returns the <see cref="ClaimsIdentity.IsAuthenticated"/> flag will be
        /// set to <see cref="true"/> indicating that the authentication has been successful.
        /// </summary>
        /// <param name="claims">The claims with which to initialise the <see cref="ClaimsPrincipal"/></param>
        /// <param name="authenticationType">This could be any string. Defaults to "Password"</param>
        /// <param name="roleType">This could be any string. Defaults to "Recipient"</param>
        /// <returns>An instance of <see cref="ClaimsPrincipal"/> with the claims embedded</returns>
        public static ClaimsPrincipal CreatePrincipal(IEnumerable<Claim> claims, string authenticationType = null, string roleType = null)
        {
            var claimsPrincipal = new ClaimsPrincipal();
            claimsPrincipal.AddIdentity(new ClaimsIdentity(claims,
                                                           string.IsNullOrWhiteSpace(authenticationType) ? "Password" : authenticationType,
                                                           ClaimTypes.Name,
                                                           string.IsNullOrWhiteSpace(roleType) ? "Recipient" : roleType));

            return claimsPrincipal;
        }
    }
}
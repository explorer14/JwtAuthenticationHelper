using JwtHelper.Core.Abstractions;
using JwtHelper.Core.Types;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace JwtHelper.Core
{
    /// <summary>
    ///   A generic Json Web Token generator for use with token based
    ///   authentication in web applications
    /// </summary>
    public sealed class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly TokenOptions tokenOptions;

        /// <summary>
        ///   Create a token generator instance
        /// </summary>
        /// <param name="tokenOptions"> </param>

        public JwtTokenGenerator(TokenOptions tokenOptions)
        {
            this.tokenOptions = tokenOptions ??
                throw new ArgumentNullException(
                    $"An instance of valid {nameof(TokenOptions)} must be passed in order to generate a JWT!"); ;
        }

        public string GenerateAccessToken(string userName, IEnumerable<Claim> userClaims)
        {
            var expiration = TimeSpan.FromMinutes(tokenOptions.TokenExpiryInMinutes);
            var jwt = new JwtSecurityToken(issuer: tokenOptions.Issuer,
                                           audience: tokenOptions.Audience,
                                           claims: MergeUserClaimsWithDefaultClaims(userName, userClaims),
                                           notBefore: DateTime.UtcNow,
                                           expires: DateTime.UtcNow.Add(expiration),
                                           signingCredentials: new SigningCredentials(
                                               tokenOptions.SigningKey,
                                               SecurityAlgorithms.HmacSha256));

            var accessToken = new JwtSecurityTokenHandler().WriteToken(jwt);

            return accessToken;
        }

        public TokenWithClaimsPrincipal GenerateAccessTokenWithClaimsPrincipal(string userName,
            IEnumerable<Claim> userClaims)
        {
            var userClaimList = userClaims.ToList();
            var accessToken = GenerateAccessToken(userName, userClaimList);

            return new TokenWithClaimsPrincipal()
            {
                AccessToken = accessToken,
                ClaimsPrincipal = ClaimsPrincipalFactory.CreatePrincipal(
                    MergeUserClaimsWithDefaultClaims(userName, userClaimList)),
                AuthProperties = CreateAuthProperties(accessToken)
            };
        }

        private static AuthenticationProperties CreateAuthProperties(string accessToken)
        {
            var authProps = new AuthenticationProperties();
            authProps.StoreTokens(
                new[]
                {
                    new AuthenticationToken()
                    {
                        Name = TokenConstants.TokenName,
                        Value = accessToken
                    }
                });

            return authProps;
        }

        private static IEnumerable<Claim> MergeUserClaimsWithDefaultClaims(string userName,
            IEnumerable<Claim> userClaims)
        {
            var claims = new List<Claim>(userClaims)
            {
                new Claim(ClaimTypes.Name, userName),
                new Claim(JwtRegisteredClaimNames.Sub, userName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat,
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64)
            };

            return claims;
        }
    }
}
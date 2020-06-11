using JwtAuthenticationHelper.Types;
using Microsoft.IdentityModel.Tokens;
using System;

namespace JwtAuthenticationHelper.Extensions
{
    [Obsolete("Please use the JwtGenerator in combination with Cookies or JwtBearer extension packages. This project will be removed")]
    public static class TokenValidationParametersExtensions
    {
        internal static TokenValidationParameters ToTokenValidationParams(
            this TokenOptions tokenOptions) =>
            new TokenValidationParameters
            {
                ClockSkew = TimeSpan.Zero,

                ValidateAudience = true,
                ValidAudience = tokenOptions.Audience,

                ValidateIssuer = true,
                ValidIssuer = tokenOptions.Issuer,

                IssuerSigningKey = tokenOptions.SigningKey,
                ValidateIssuerSigningKey = true,

                RequireExpirationTime = true,
                ValidateLifetime = true
            };
    }
}
using JwtHelper.Core.Abstractions;
using JwtHelper.Core.Extensions;
using JwtHelper.Core.Types;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;

namespace JwtHelper.Core
{
    public class DefaultJwtTokenValidator : IJwtTokenValidator
    {
        public JwtValidationResult Validate(string jwt, TokenOptions tokenOptions)
        {
            if (string.IsNullOrWhiteSpace(jwt))
                return JwtValidationResult.Failure(
                    reason: "Input JWT cannot be null, empty or white space");

            if (tokenOptions == null)
                return JwtValidationResult.Failure("TokenOptions cannot be null");

            return ValidateToken(jwt, tokenOptions.ToTokenValidationParams());
        }

        private JwtValidationResult ValidateToken(
            string jwt,
            TokenValidationParameters tokenValidationParameters)
        {
            try
            {
                new JwtSecurityTokenHandler().ValidateToken(
                jwt, tokenValidationParameters, out var token);

                if (!(token is JwtSecurityToken _jwt) ||
                    !_jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.Ordinal))
                    return JwtValidationResult.Failure("Signing algorithm does not match");

                return JwtValidationResult.Success();
            }
            catch (Exception exception)
            {
                return JwtValidationResult.Failure(exception.Message);
            }
        }
    }
}
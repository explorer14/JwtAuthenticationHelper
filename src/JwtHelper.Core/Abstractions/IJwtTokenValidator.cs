using JwtHelper.Core.Types;

namespace JwtHelper.Core.Abstractions
{
    public interface IJwtTokenValidator
    {
        JwtValidationResult Validate(string jwt, TokenOptions tokenOptions);
    }
}
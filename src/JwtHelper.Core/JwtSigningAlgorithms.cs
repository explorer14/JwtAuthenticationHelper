using Microsoft.IdentityModel.Tokens;

namespace JwtHelper.Core;

internal static class JwtSigningAlgorithms
{
    internal static string Default() => 
        SecurityAlgorithms.HmacSha512;
}
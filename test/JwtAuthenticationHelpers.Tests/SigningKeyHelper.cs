using System;

namespace JwtAuthenticationHelpers.Tests;

internal static class SigningKeyHelper
{
    internal static string SigningKeyOf512Bits() => 
        $"{Guid.NewGuid():N}_{DateTimeOffset.UtcNow.Ticks}_123456789_987654321";
}
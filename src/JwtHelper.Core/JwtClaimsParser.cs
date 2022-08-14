using JwtHelper.Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace JwtHelper.Core
{
    public static class JwtClaimsParser
    {
        public static IReadOnlyCollection<Claim> GetClaims(string jwt)
        {
            List<Claim> claims = new List<Claim>();
            var tokenClaimsPart = jwt.Split('.')[1];
            var claimsSet = JsonSerializer.Deserialize<Dictionary<string, object>>(
                Encoding.UTF8.GetString(
                    Convert.FromBase64String(EnsureValidLength(tokenClaimsPart))));

            if (claimsSet == null || !claimsSet.Any())
                throw new NoClaimsFoundInToken();

            foreach (var kvp in claimsSet)
            {
                claims.Add(new Claim(kvp.Key, kvp.Value.ToString()));
            }

            return claims;
        }

        public static string GetClaimValueForType(
            this IReadOnlyCollection<Claim> claims, string type) =>
            claims.FirstOrDefault(x => x.Type == type)?.Value ?? null;

        private static string EnsureValidLength(string tokenClaimsPart)
        {
            if (tokenClaimsPart.Length % 4 == 0)
                return tokenClaimsPart;

            var paddedString = new StringBuilder(tokenClaimsPart);
            bool isMultipleOf4 = false;

            while (!isMultipleOf4)
            {
                paddedString.Append('=');

                if (paddedString.Length % 4 == 0)
                    isMultipleOf4 = true;
            }

            return paddedString.ToString();
        }
    }
}
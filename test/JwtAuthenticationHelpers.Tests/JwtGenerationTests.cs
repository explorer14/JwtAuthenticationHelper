using JwtHelper.Core;
using JwtHelper.Core.Types;
using Microsoft.AspNetCore.Authentication;
using System.Linq;
using System.Security.Claims;
using Xunit;

namespace JwtAuthenticationHelpers.Tests
{
    public class JwtGenerationTests
    {
        [Fact]
        public void ShouldGenerateJWTWithClaimsPrincipalsAndOptionalClaims()
        {
            var tokenGen = new JwtTokenGenerator(
                new TokenOptions(
                    "Me",
                    "Me2",
                    "abcdefghijklmnopqr12345"));

            var tcp = tokenGen.GenerateAccessTokenWithClaimsPrincipal("username@org.com",
                new[]
                {
                    new Claim(ClaimTypes.GivenName, "FName"),
                    new Claim(ClaimTypes.Surname, "LName")
                });

            Assert.True(tcp.ClaimsPrincipal != null);
            Assert.True(!string.IsNullOrWhiteSpace(tcp.AccessToken));
            Assert.True(tcp.ClaimsPrincipal.FindFirst(ClaimTypes.GivenName).Value == "FName");
            Assert.True(tcp.ClaimsPrincipal.FindFirst(ClaimTypes.Surname).Value == "LName");
            Assert.True(tcp.AuthProperties != null &&
                tcp.AuthProperties.Items.Any() &&
                !string.IsNullOrWhiteSpace(tcp.AuthProperties.GetTokenValue(TokenConstants.TokenName)));
        }
    }
}
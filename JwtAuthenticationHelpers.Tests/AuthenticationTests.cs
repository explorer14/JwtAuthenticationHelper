using JwtAuthenticationHelper;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using JwtAuthenticationHelper.Types;
using Microsoft.AspNetCore.Authentication;
using Xunit;

namespace JwtAuthenticationHelpers.Tests
{
    public class AuthenticationTests
    {
        [Fact(DisplayName = "Can generate token with claims principal given user claims")]
        public void Test2()
        {
            var tokenGen = new JwtTokenGenerator(
                new TokenOptions(
                    "Me",
                    "Me2",
                    new SymmetricSecurityKey(
                        Encoding.ASCII.GetBytes(
                            "abcdefghijklmnopqr12345"))));

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
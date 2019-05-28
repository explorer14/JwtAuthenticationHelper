using JwtAuthenticationHelper;
using JwtAuthenticationHelper.Types;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Security.Claims;
using System.Text;
using System.Threading;
using Xunit;

namespace JwtAuthenticationHelpers.Tests
{
    public class TicketFormatTests
    {
        [Fact]
        public void Protecting_an_Auth_ticket_converts_it_into_a_secure_string()
        {
            // Arrange
            var ticketFormat = TicketFormat();
            var token = Jwt();

            // Act
            var encryptedString = ticketFormat.Protect(
                new AuthenticationTicket(
                    token.ClaimsPrincipal,
                    token.AuthProperties,
                    "Cookies"));

            // Assert
            Assert.True(!string.IsNullOrWhiteSpace(encryptedString));
        }

        [Fact]
        public void Unprotecting_an_encrypted_ticket_converts_it_back_into_AuthTicket()
        {
            // Arrange
            var ticketFormat = TicketFormat();
            var token = Jwt();
            var encryptedString = ticketFormat.Protect(
                new AuthenticationTicket(
                    token.ClaimsPrincipal,
                    token.AuthProperties,
                    "Cookies"));

            // Act
            var authTicket = ticketFormat.Unprotect(encryptedString);

            // Assert
            Assert.NotNull(authTicket);
        }

        [Fact]
        public void Expired_JWT_Throws_ExpiredException()
        {
            // Arrange
            var ticketFormat = TicketFormat();
            var options = new TokenOptions(
                "Token.WebApp",
                "Token.WebApp.Clients",
                SecurityKey(), 1);

            var token = Jwt(options);
            var encryptedString = ticketFormat.Protect(
                new AuthenticationTicket(
                    token.ClaimsPrincipal,
                    token.AuthProperties,
                    "Cookies"));

            // let the token expire
            Thread.Sleep(options.TokenExpiryInMinutes * 60 * 1000);

            // Act & // Assert
            Assert.Null(ticketFormat.Unprotect(encryptedString));
        }

        [Fact]
        public void Unprotecting_Ticket_With_Invalid_JWT_Throws_SecurityException()
        {
            // Arrange
            var ticketFormat = TicketFormat();
            var authProps = new AuthenticationProperties();
            authProps.StoreTokens(new[]
            {
                new AuthenticationToken(){ Name = "jwt", Value = "Evil Token"}
            });

            var encryptedString = ticketFormat.Protect(
                new AuthenticationTicket(
                    ClaimsPrincipalFactory.CreatePrincipal(new[]
                    {
                        new Claim(ClaimTypes.GivenName, "Blah")
                    }),
                    authProps,
                    "Cookies"));

            // Act & assert
            Assert.Null(ticketFormat.Unprotect(encryptedString));
        }

        [Fact]
        public void Unprotecting_Ticket_With_Null_JWT_Throws_ArgumentNullException()
        {
            // Arrange
            var ticketFormat = TicketFormat();
            var authProps = new AuthenticationProperties();
            authProps.StoreTokens(new[]
            {
                new AuthenticationToken(){ Name = "jwt", Value = null}
            });

            var encryptedString = ticketFormat.Protect(
                new AuthenticationTicket(
                    ClaimsPrincipalFactory.CreatePrincipal(new[]
                    {
                        new Claim(ClaimTypes.GivenName, "Blah")
                    }),
                    authProps,
                    "Cookies"));

            // Act & assert
            Assert.Null(ticketFormat.Unprotect(encryptedString));
        }

        [Fact]
        public void Unprotecting_Ticket_With_Empty_AuthProps_Throws_ArgumentNullException()
        {
            // Arrange
            var ticketFormat = TicketFormat();

            var encryptedString = ticketFormat.Protect(
                new AuthenticationTicket(
                    ClaimsPrincipalFactory.CreatePrincipal(new[]
                    {
                        new Claim(ClaimTypes.GivenName, "Blah")
                    }),
                    new AuthenticationProperties(),
                    "Cookies"));

            // Act & assert
            Assert.Null(ticketFormat.Unprotect(encryptedString));
        }

        private static TokenWithClaimsPrincipal Jwt(TokenOptions options = null)
        {
            var tokenGenerator =
                new JwtTokenGenerator(
                    options ?? new TokenOptions(
                                "Token.WebApp",
                                "Token.WebApp.Clients",
                                SecurityKey()));

            var token = tokenGenerator.GenerateAccessTokenWithClaimsPrincipal("Aman",
                new[]
                {
                    new Claim("FName", "Aman")
                });

            return token;
        }

        private static JwtAuthTicketFormat TicketFormat()
        {
            var ticketFormat = new JwtAuthTicketFormat(
                new TokenValidationParameters
                {
                    ClockSkew = TimeSpan.Zero,

                    ValidateAudience = true,
                    ValidAudience = "Token.WebApp.Clients",

                    ValidateIssuer = true,
                    ValidIssuer = "Token.WebApp",

                    IssuerSigningKey = SecurityKey(),
                    ValidateIssuerSigningKey = true,

                    RequireExpirationTime = true,
                    ValidateLifetime = true
                },
                new TicketSerializer(),
                GetDataProtector());

            return ticketFormat;
        }

        private static IDataProtector GetDataProtector()
        {
            var services = new ServiceCollection();
            services.AddDataProtection(options => options.ApplicationDiscriminator = $"{nameof(TicketFormatTests)}")
                .SetApplicationName($"{nameof(TicketFormatTests)}");

            var dataProtector = services
                .BuildServiceProvider()
                .GetDataProtector(new[]
                {
                    $"{nameof(TicketFormatTests)}-Auth1"
                });

            return dataProtector;
        }

        private static SymmetricSecurityKey SecurityKey()
        {
            var symmetricSecurityKey = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes(
                    "f47b558d-7654-458c-99f2-13b190ef0199"));

            return symmetricSecurityKey;
        }
    }
}
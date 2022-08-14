using FluentAssertions;
using JwtHelper.Core;
using JwtHelper.Core.Types;
using System;
using System.Security.Claims;
using Xunit;

namespace JwtAuthenticationHelpers.Tests
{
    public class TokenValidationTests
    {
        [Theory]
        [InlineData(" ")]
        [InlineData(null)]
        [InlineData("")]
        public void ShouldFailWithReasonIfJwtIsNullOrEmpty(string inputJwt)
        {
            var validator = new DefaultJwtTokenValidator();
            var result = validator.Validate(inputJwt, DefaultTokenOptions());
            result.IsTokenValid.Should().BeFalse();
            result.Reason.Should().NotBeNull().And.Be("Input JWT cannot be null, empty or white space");
        }

        [Fact]
        public void ShouldFailWithReasonIfTokenOptionsAreNull()
        {
            var validator = new DefaultJwtTokenValidator();
            var result = validator.Validate("inputJwt", null);
            result.IsTokenValid.Should().BeFalse();
            result.Reason.Should().NotBeNull().And.Be("TokenOptions cannot be null");
        }

        [Fact]
        public void ShouldFailWithReasonIfJwtSignedUsingAnUnexpectedAlgorithm()
        {
            var tokenSignedWithUnexpectedAlgorithm =
                "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzM4NCJ9.eyJpc3MiOiJJU1NVRVIiLCJpYXQiOjE2NjA0Nzc4MjMsImV4cCI6MTY5MjAxNDEyMywiYXVkIjoiQVVESUVOQ0UiLCJzdWIiOiJTVUJKRUNUIn0.HAPE20LacGZQoyyKDPHBOePXy6OKlAOx6TxKQpUPp69A85kmPsQimO3dEJCnA5Bp";

            var validator = new DefaultJwtTokenValidator();
            var result = validator.Validate(tokenSignedWithUnexpectedAlgorithm, DefaultTokenOptions());
            result.IsTokenValid.Should().BeFalse();
            result.Reason.Should().NotBeNull().And.Be("Signing algorithm does not match");
        }

        [Fact]
        public void ShouldFailWithReasonIfValidationThrowsException()
        {
            var inValidJwt =
                "eXAiOiJKV1QiLCJhbGciOiJIUzM4NCJ9.eyJpc3MiOiJJU1NVRVIiLCJpYXQiOjE2NjA0Nzc4MjMsImV4cCI6MTY5MjAxNDEyMywiYXVkIjoiQVVESUVOQ0UiLCJzdWIiOiJTVUJKRUNUIn0.HAPE20LacGZQoyyKDPHBOePXy6OKlAOx6TxKQpUPp69A85kmPsQimO3dEJCnA5Bp";

            var validator = new DefaultJwtTokenValidator();
            var result = validator.Validate(inValidJwt, DefaultTokenOptions());
            result.IsTokenValid.Should().BeFalse();
            result.Reason.Should().NotBeNull();
        }

        [Fact]
        public void ShouldSucceedIfTokenIsValid()
        {
            var validator = new DefaultJwtTokenValidator();
            var defaultTokenOptions = DefaultTokenOptions();

            var result = validator.Validate(
                new DefaultJwtTokenGenerator(defaultTokenOptions)
                .GenerateAccessToken("test", Array.Empty<Claim>()),
                defaultTokenOptions);

            result.IsTokenValid.Should().BeTrue();
            result.Reason.Should().BeEmpty();
        }

        private TokenOptions DefaultTokenOptions() =>
            new TokenOptions("ISSUER", "AUDIENCE", "abcdefghijklmnopqr12345");
    }
}
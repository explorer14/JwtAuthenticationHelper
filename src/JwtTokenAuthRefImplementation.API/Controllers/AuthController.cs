using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using JwtAuthenticationHelper.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JwtTokenAuthRefImplementation.API.Controllers
{
    [Produces("application/json")]
    [Route("api/auth")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly IJwtTokenGenerator jwtTokenGenerator;

        public AuthController(IJwtTokenGenerator jwtTokenGenerator)
        {
            this.jwtTokenGenerator = jwtTokenGenerator;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody]UserCredentials userCredentials)
        {
            // Replace this with your custom authentication logic which will
            // securely return the authenticated user's details including
            // any role specific info
            if (userCredentials.Username == "user1" && userCredentials.Password == "pass1")
            {
                var userInfo = new UserInfo
                {
                    FirstName = "UserFName",
                    LastName = "UserLName"
                };

                var accessTokenResult = jwtTokenGenerator.GenerateAccessTokenWithClaimsPrincipal(
                    userCredentials.Username,
                    AddMyClaims(userInfo));

                return Ok(accessTokenResult.AccessToken);
            }
            else
            {
                return Unauthorized();
            }
        }

        private static IEnumerable<Claim> AddMyClaims(UserInfo authenticatedUser)
        {
            var myClaims = new List<Claim>
            {
                new Claim(ClaimTypes.GivenName, authenticatedUser.FirstName),
                new Claim(ClaimTypes.Surname, authenticatedUser.LastName)
            };

            return myClaims;
        }
    }

    internal class UserInfo
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }     
    }

    public class UserCredentials
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
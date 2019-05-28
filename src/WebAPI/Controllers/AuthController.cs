using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JwtAuthenticationHelper.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly IJwtTokenGenerator jwtTokenGenerator;

        public AuthController(IJwtTokenGenerator jwtTokenGenerator)
        {
            this.jwtTokenGenerator = jwtTokenGenerator;
        }

        [HttpGet("boob")]
        public string Get()
        {
            return "This one works!";
        }

        //[HttpPost]
        //[AllowAnonymous]
        //public IActionResult Login([FromBody]UserCredentials userCredentials)
        //{
        //    // Replace this with your custom authentication logic which will
        //    // securely return the authenticated user's details including
        //    // any role specific info
        //    if (userCredentials.Username == "user1" && userCredentials.Password == "pass1")
        //    {
        //        var userInfo = new UserInfo
        //        {
        //            FirstName = "UserFName",
        //            LastName = "UserLName",
        //            HasAdminRights = true
        //        };

        //        var accessTokenResult = jwtTokenGenerator.GenerateAccessTokenWithClaimsPrincipal(
        //            userCredentials.Username,
        //            AddMyClaims(userInfo));
        //        //await HttpContext.SignInAsync(accessTokenResult.ClaimsPrincipal,
        //        //    accessTokenResult.AuthProperties);

        //        return Ok(accessTokenResult.AccessToken);
        //    }
        //    else
        //    {
        //        return Unauthorized();
        //    }
        //}

        //private static IEnumerable<Claim> AddMyClaims(UserInfo authenticatedUser)
        //{
        //    var myClaims = new List<Claim>
        //    {
        //        new Claim(ClaimTypes.GivenName, authenticatedUser.FirstName),
        //        new Claim(ClaimTypes.Surname, authenticatedUser.LastName),
        //        new Claim("HasAdminRights", authenticatedUser.HasAdminRights ? "Y" : "N")
        //    };

        //    return myClaims;
        //}
    }

    internal class UserInfo
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool HasAdminRights { get; set; }
    }

    public class UserCredentials
    {
        public string Username { get; internal set; }
        public string Password { get; internal set; }
    }
}
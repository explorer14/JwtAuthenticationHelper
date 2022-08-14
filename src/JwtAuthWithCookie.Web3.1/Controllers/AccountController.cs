using JwtGenerator.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace JwtAuthWithCookie.Web3._1.Controllers
{
    public class AccountController : Controller
    {
        private readonly IJwtTokenGenerator tokenGenerator;

        public AccountController(IJwtTokenGenerator tokenGenerator)
        {
            this.tokenGenerator = tokenGenerator;
        }

        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            ViewBag.returnUrl = returnUrl;

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(UserCredentials userCredentials, string returnUrl = null)
        {
            ViewBag.returnUrl = returnUrl;
            var returnTo = "/Account/Login";

            // Replace this with your custom authentication logic which will
            // securely return the authenticated user's details including
            // any role specific info
            if (userCredentials.Username == "user1" && userCredentials.Password == "badpassword")
            {
                var userInfo = new UserInfo
                {
                    FirstName = "UserFName",
                    LastName = "UserLName",
                    HasAdminRights = true
                };

                var accessTokenResult = tokenGenerator.GenerateAccessTokenWithClaimsPrincipal(
                    userCredentials.Username,
                    AddMyClaims(userInfo));
                await HttpContext.SignInAsync(accessTokenResult.ClaimsPrincipal,
                    accessTokenResult.AuthProperties);
                returnTo = returnUrl;
            }

            return RedirectToLocal(returnTo);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

        private static IEnumerable<Claim> AddMyClaims(UserInfo authenticatedUser)
        {
            var myClaims = new List<Claim>
            {
                new Claim(ClaimTypes.GivenName, authenticatedUser.FirstName),
                new Claim(ClaimTypes.Surname, authenticatedUser.LastName),
                new Claim("HasAdminRights", authenticatedUser.HasAdminRights ? "Y" : "N")
            };

            return myClaims;
        }
    }
}

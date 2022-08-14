using JwtHelper.Core.Abstractions;
using JwtHelper.Core.Types;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace VanillaJwtUsage.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VanillaJwtController : ControllerBase
    {
        private readonly IJwtTokenGenerator jwtTokenGenerator;
        private readonly IJwtTokenValidator jwtTokenValidator;
        private readonly TokenOptions tokenOptions;

        public VanillaJwtController(
            IJwtTokenGenerator jwtTokenGenerator,
            IJwtTokenValidator jwtTokenValidator,
            TokenOptions tokenOptions)
        {
            this.jwtTokenGenerator = jwtTokenGenerator;
            this.jwtTokenValidator = jwtTokenValidator;
            this.tokenOptions = tokenOptions;
        }

        [HttpGet]
        public ActionResult GenerateToken()
        {
            var jwt = jwtTokenGenerator.GenerateAccessToken("test", Array.Empty<Claim>());
            return Ok(jwt);
        }

        [HttpGet("validate/{jwt}")]
        public ActionResult ValidateToken(string jwt)
        {
            var result = jwtTokenValidator.Validate(jwt, tokenOptions);

            return Ok(result);
        }
    }
}
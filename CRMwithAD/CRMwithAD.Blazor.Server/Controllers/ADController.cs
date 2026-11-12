using CRMwithAD.Blazor.Server.Models.ADModels;
using CRMwithAD.Blazor.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRMwithAD.Blazor.Server.Controllers
{
    [Route("api/ad")]
    [AllowAnonymous]
    public class ADController : ControllerBase
    {
        private readonly ADAuthService _adAuthService;

        public ADController(ADAuthService adAuthService)
        {
            _adAuthService = adAuthService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] AdLogin login)
        {
            var isAuthenicated = _adAuthService.Login(login);
            if (isAuthenicated)
            {
                return Ok(new { message = "Authentication successful" });
            }
            else
            {
                return Unauthorized(new { message = "Authentication failed" });
            }
        }

        [HttpPost("authenticate")]
        public IActionResult AuthenticateFromAD([FromBody] AdLogin login)
        {
            var isAuthenicated = _adAuthService.AuthenticateFromAD(login, out List<string> roles);
            if (isAuthenicated)
            {
                return Ok(new { message = "Authentication successful", userRole = roles });
            }
            else
            {
                return Unauthorized(new { message = "Authentication failed" });
            }
        }

        [HttpGet("{ouName}/ou-users")]
        public IActionResult GetUsersFromOU(string ouName)
        {
            var users = _adAuthService.GetUsersFromOU(ouName);
            if (!users.Any()) return NotFound(new { message = "OU not found or no users in the specified OU" });
            return Ok(users);
        }
    }
}

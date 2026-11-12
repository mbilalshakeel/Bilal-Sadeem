using DevExpress.ExpressApp;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CRMwithAD.Blazor.Server.Controllers
{
    [Route("api/roles")]
    [ApiController]
    [Authorize]
    public class RolesController : ControllerBase
    {
        IObjectSpaceFactory objectSpaceFactory;
        public RolesController(IObjectSpaceFactory objectSpaceFactory)
        {
            this.objectSpaceFactory = objectSpaceFactory;
        }

        [HttpGet]
        public IActionResult GetAllRoles()
        {
            try
            {
                using var objectSpace = objectSpaceFactory.CreateObjectSpace<PermissionPolicyRole>();

                var roles = objectSpace.GetObjectsQuery<PermissionPolicyRole>()
                    .Select(r => new
                    {
                        r.Name
                    })
                    .ToList();

                return Ok(roles);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpGet("user")]
        public IActionResult GetCurrentUserRoles()
        {
            if (!User.Identity.IsAuthenticated)
                return Unauthorized();

            var roles = User.Claims
                          .Where(c => c.Type == ClaimTypes.Role)
                          .Select(c => c.Value)
                          .Distinct()
                          .ToList();

            return Ok(new
            {
                User = User.Identity.Name,
                Roles = roles
            });
        }
    }
}

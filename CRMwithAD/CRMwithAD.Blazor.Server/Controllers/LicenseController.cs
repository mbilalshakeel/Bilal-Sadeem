using CRMwithAD.Module.BusinessObjects.LicenseManagement;
using DevExpress.Xpo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRMwithAD.Blazor.Server.Controllers
{
    [Route("api/license")]
    [ApiController]
    [AllowAnonymous]
    public class LicenseController : ControllerBase
    {
        // Inject your XPO data layer/session here
        private readonly IDataLayer dataLayer;
        public LicenseController(IDataLayer dataLayer)
        {
            this.dataLayer = dataLayer;
        }

        [HttpGet]
        public IActionResult GetLicense()
        {
            try
            {
                using var session = new Session(dataLayer);
                var licenses = new XPQuery<License>(session)
                    .Select(a => new { EnablementDate = DateOnly.FromDateTime(a.EnablementDate), ExpiryDate = DateOnly.FromDateTime(a.ExpiryDate), a.Status })
                    .ToList();

                return Ok(licenses);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}
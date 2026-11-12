using CRMwithAD.Module.BusinessObjects.CampaignsManagement;
using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRMwithAD.Blazor.Server.Controllers
{
    [Route("api/campaign")]
    [ApiController]
    [Authorize]
    public class CampaignController : ControllerBase
    {
        IObjectSpaceFactory objectSpaceFactory;
        public CampaignController(IObjectSpaceFactory objectSpaceFactory)
        {
            this.objectSpaceFactory = objectSpaceFactory;
        }

        [HttpGet("{campaignid}/attachment")]
        public IActionResult GetAttachment(string campaignid)
        {
            try
            {
                using var objectSpace = objectSpaceFactory.CreateObjectSpace<Campaign>();
                var campaign = objectSpace.GetObjectsQuery<Campaign>()
                    .FirstOrDefault(t => t.Oid == Guid.Parse(campaignid));

                if (campaign?.Banner?.Content != null)
                {
                    byte[] bannerBytes = campaign.Banner.Content;
                    string bannerName = campaign.Banner.FileName ?? "banner.jpg";
                    string mimeType = GetMimeType(bannerName);

                    return File(bannerBytes, mimeType, bannerName);
                }

                return NotFound("Campaign or banner not found.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        private string GetMimeType(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLower();
            return ext switch
            {
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                _ => "application/octet-stream"
            };
        }
    }
}


using CRMwithAD.Blazor.Server.Models.CRMApiModels;
using CRMwithAD.Module.BusinessObjects.Configurations;
using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRMwithAD.Blazor.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SubjectsApiController : ControllerBase
    {
        IObjectSpaceFactory objectSpaceFactory;
        public SubjectsApiController(IObjectSpaceFactory objectSpaceFactory)
        {
            this.objectSpaceFactory = objectSpaceFactory;
        }
        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                List<ActiveSubjects> activeAccounts = new List<ActiveSubjects>();
                using (IObjectSpace newObjectSpace = objectSpaceFactory.CreateObjectSpace<Category>())
                {
                    IQueryable<Category> outdatedTasks = newObjectSpace.GetObjectsQuery<Category>();
                    foreach (Category account in outdatedTasks)
                    {
                        ActiveSubjects activeAccounts1 = new ActiveSubjects();
                        activeAccounts1.Oid = account.Oid.ToString();
                        activeAccounts1.Title = account.Title;
                        activeAccounts.Add(activeAccounts1);
                    }
                    return Ok(activeAccounts.ToArray());
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}

using CRMwithAD.Blazor.Server.Models.CRMApiModels;
using CRMwithAD.Blazor.Server.Services;
using CRMwithAD.Module.BusinessObjects.Configurations;
using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRMwithAD.Blazor.Server.Controllers
{
    [Route("api/")]
    [ApiController]
    [Authorize]
    public class DropdownController : ControllerBase
    {
        IObjectSpaceFactory objectSpaceFactory;
        public DropdownController(IObjectSpaceFactory objectSpaceFactory)
        {
            this.objectSpaceFactory = objectSpaceFactory;
        }

        [HttpGet("dropdown/{categoryOid}/sub-categories")]
        public IActionResult GetSubCategories(string categoryOid)
        {
            try
            {
                if (string.IsNullOrEmpty(categoryOid) || !Guid.TryParse(categoryOid, out Guid categoryGuid))
                {
                    return BadRequest("Invalid category OID.");
                }

                List<ActiveSubjects> activeSubCategories = [];
                using (IObjectSpace newObjectSpace = objectSpaceFactory.CreateObjectSpace<SubCategory>())
                {
                    // Fetch sub-categories associated with the specified category OID
                    IQueryable<SubCategory> subCategories = newObjectSpace.GetObjectsQuery<SubCategory>().Where(x => x.Categories.Oid == Guid.Parse(categoryOid));
                    foreach (SubCategory category in subCategories)
                    {
                        ActiveSubjects activeSubCategory = new()
                        {
                            Oid = category.Oid.ToString(),
                            Title = category.Title
                        };
                        activeSubCategories.Add(activeSubCategory);
                    }
                    return Ok(activeSubCategories.ToArray());
                }
            }
            catch (Exception ex)
            {
                Logging.LogError(ex, "DropdownController/GetSubCategories");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("categories")]
        public IActionResult GetAllCategoriesWithSubCategories()
        {
            try
            {
                using (IObjectSpace objectSpace = objectSpaceFactory.CreateObjectSpace<Category>())
                {
                    var categories = objectSpace.GetObjectsQuery<Category>()
                        .Select(c => new
                        {
                            Oid = c.Oid.ToString(),
                            c.Title,
                            SubCategories = c.SubCategories.Select(sc => new
                            {
                                Oid = sc.Oid.ToString(),
                                sc.Title
                            }).ToList()
                        })
                        .ToList();

                    return Ok(categories);
                }
            }
            catch (Exception ex)
            {
                Logging.LogError(ex, "DropdownController/GetAllCategoriesWithSubCategories");
                return BadRequest(ex.Message);
            }
        }
    }
}
using CRMwithAD.Blazor.Server.Models.CRMApiModels;
using CRMwithAD.Module.BusinessObjects.CustomerManagement;
using CRMwithAD.Module.BusinessObjects.SLAsManagement;
using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRMwithAD.Blazor.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountApiController : ControllerBase
    {
        IObjectSpaceFactory objectSpaceFactory;
        public AccountApiController(IObjectSpaceFactory objectSpaceFactory)
        {
            this.objectSpaceFactory = objectSpaceFactory;
        }

        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                List<ActiveAccounts> activeAccounts = new List<ActiveAccounts>();
                using (IObjectSpace newObjectSpace = objectSpaceFactory.CreateObjectSpace<Account>())
                {
                    IQueryable<Account> outdatedTasks = newObjectSpace.GetObjectsQuery<Account>();
                    foreach (Account account in outdatedTasks)
                    {
                        ActiveAccounts activeAccounts1 = new ActiveAccounts();
                        activeAccounts1.oid = account.Oid.ToString();
                        activeAccounts1.Name = account.Name;
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

        [HttpPost]
        public IActionResult Post([FromBody] AccountCreationRequest request)
        {
            try
            {
                using (IObjectSpace objectSpace = objectSpaceFactory.CreateObjectSpace(typeof(Account)))
                {


                    var eaccounts = objectSpace.GetObjects<Account>();
                    var sla = objectSpace.GetObjects<SLA>();
                    var eaccount = eaccounts.SingleOrDefault(c => c.Name.ToString().ToLower() == request.Name.ToLower());
                    if (eaccount != null)
                    {
                        // Object with the same unique property already exists
                        return Conflict(new { Message = "An account with the same name already exists." });
                    }

                    Account account = objectSpace.CreateObject<Account>();
                    account.Name = request.Name;
                    account.AssignedSLA = sla.FirstOrDefault();
                    objectSpace.CommitChanges();
                    var createdOid = account.Oid;
                    return Ok(new { oid = createdOid });
                }
            }
            catch (Exception ex)
            {

                return Ok(ex);
            }

        }

        [HttpDelete("{id}")]
        public IActionResult DeleteObject(Guid id)
        {
            using (IObjectSpace objectSpace = objectSpaceFactory.CreateObjectSpace(typeof(Account)))
            {
                // Ensure user is authenticated

                try
                {
                    var account = objectSpace.GetObjectByKey<Account>(id);

                    if (account == null)
                    {
                        return NotFound(new { Message = "Account not found." });
                    }

                    objectSpace.Delete(account);
                    objectSpace.CommitChanges();

                    // Return a success message
                    return Ok(new { Message = "Account deleted successfully." });
                }
                catch (Exception ex)
                {
                    // Handle exceptions and return an error response
                    return BadRequest(new { Message = ex.Message });
                }

            }
        }
    }
}

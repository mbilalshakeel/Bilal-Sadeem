using CRMwithAD.Blazor.Server.Models.CRMApiModels;
using CRMwithAD.Blazor.Server.Services;
using CRMwithAD.Module.BusinessObjects.CustomerManagement;
using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRMwithAD.Blazor.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ContactsApiController : ControllerBase
    {
        IObjectSpaceFactory objectSpaceFactory;
        public ContactsApiController(IObjectSpaceFactory objectSpaceFactory)
        {
            this.objectSpaceFactory = objectSpaceFactory;
        }

        [HttpGet]
        public IActionResult Get(string Username)
        {
            try
            {
                List<ActiveContacts> activeContacts = new List<ActiveContacts>();
                using (IObjectSpace newObjectSpace = objectSpaceFactory.CreateObjectSpace<Contact>())
                {

                    Contact Tasks = newObjectSpace.GetObjectsQuery<Contact>().Where(p => p.Username == Username || p.Email == Username).FirstOrDefault();

                    ActiveContacts activeContacts1 = new ActiveContacts();
                    activeContacts1.oid = Tasks.Oid.ToString();
                    activeContacts1.firstname = Tasks.FirstName.ToString();
                    activeContacts1.lastname = Tasks.LastName.ToString();
                    activeContacts1.email = Tasks.Email;
                    activeContacts1.accountoid = Tasks.Accounts.Oid.ToString();
                    activeContacts1.accountname = Tasks.Accounts.Name.ToString();
                    activeContacts.Add(activeContacts1);
                    return Ok(activeContacts.ToArray());
                }
            }
            catch (Exception ex)
            {
                Logging.LogError(ex, "ContactsApi/Get");
                return BadRequest(ex);
            }
        }

        [HttpPost]
        public IActionResult Post([FromBody] ContactCreationRequest request)
        {
            try
            {
                using (IObjectSpace objectSpace = objectSpaceFactory.CreateObjectSpace(typeof(Contact)))
                {
                    //var econtacts = objectSpace.GetObjects<Contact>();
                    //var econtact = econtacts.SingleOrDefault(c => c.Username.ToString().ToLower() == request.username.ToLower() && c.Accounts.Oid.ToString() == request.accountoid.ToString());
                    //if (econtact != null)
                    //{
                    //    // Object with the same unique property already exists
                    //    return Conflict(new { Message = "An object with the same username already exists." });
                    //}

                    var accounts = objectSpace.GetObjects<Account>();
                    var account = accounts.SingleOrDefault(c => c.Oid.ToString() == request.accountoid);
                    Contact contact = objectSpace.CreateObject<Contact>();
                    contact.FirstName = request.firstname;
                    contact.LastName = request.lastname;
                    contact.Email = request.email;
                    contact.Username = request.username;
                    contact.Accounts = account;
                    objectSpace.CommitChanges();
                    var createdOid = contact.Oid;
                    return Ok(new { oid = createdOid });
                }
            }
            catch (Exception)
            {

                return BadRequest();
            }

        }
        [HttpDelete("{id}")]
        public IActionResult DeleteObject(Guid id)
        {
            using (IObjectSpace objectSpace = objectSpaceFactory.CreateObjectSpace(typeof(Contact)))
            {
                // Ensure user is authenticated

                try
                {
                    var contact = objectSpace.GetObjectByKey<Contact>(id);

                    if (contact == null)
                    {
                        return NotFound(new { Message = "Object not found." });
                    }

                    objectSpace.Delete(contact);
                    objectSpace.CommitChanges();

                    // Return a success message
                    return Ok(new { Message = "Object deleted successfully." });
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

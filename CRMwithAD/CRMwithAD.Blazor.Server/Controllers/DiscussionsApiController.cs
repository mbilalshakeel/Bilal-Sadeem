using CRMwithAD.Blazor.Server.Models.CRMApiModels;
using CRMwithAD.Blazor.Server.Services;
using CRMwithAD.Module.BusinessObjects;
using CRMwithAD.Module.BusinessObjects.CustomerManagement;
using CRMwithAD.Module.BusinessObjects.Helpdesk_Tickets;
using CRMwithAD.Module.BusinessObjects.HelpdeskTickets;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Xpo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;

namespace CRMwithAD.Blazor.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DiscussionsApiController : ControllerBase
    {

        IObjectSpaceFactory objectSpaceFactory;
        private readonly IConfiguration _configuration;

        public DiscussionsApiController(IObjectSpaceFactory objectSpaceFactory, IConfiguration configuration)
        {
            this.objectSpaceFactory = objectSpaceFactory;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Get(string caseID)
        {
            try
            {
                List<DiscussionsModel> discussionsModels = new List<DiscussionsModel>();

                using (IObjectSpace newObjectSpace = objectSpaceFactory.CreateObjectSpace<Comment>())
                {
                    IQueryable<Comment> outdatedTasks = newObjectSpace.GetObjectsQuery<Comment>().Where(p => p.Tickets.CaseID.ToString() == caseID).OrderByDescending(p => p.Date.ToString("HH:mm:ss"));
                    foreach (Comment contacts in outdatedTasks)
                    {

                        string inputAsString = "";

                        DiscussionsModel discussionsModel = new DiscussionsModel();
                        discussionsModel.Comment = contacts.Reply;


                        if (contacts.Attachment == null)
                        {
                            discussionsModel.AttachmentName = null;
                            discussionsModel.Attachment = null;
                        }
                        else
                        {
                            string fileData = string.Empty;
                            var fileStream = new MemoryStream();
                            contacts.Attachment.SaveToStream(fileStream);
                            inputAsString = Convert.ToBase64String(fileStream.ToArray());
                            discussionsModel.AttachmentName = contacts.Attachment.FileName;

                            discussionsModel.Attachment = inputAsString;
                        }


                        if (contacts.TenantUser == null)
                        {
                            discussionsModel.RepliedBy = null;
                            discussionsModel.RepliedByOid = null;
                        }
                        else
                        {
                            discussionsModel.RepliedBy = contacts.TenantUser.FirstName + " " + contacts.TenantUser.LastName;
                            discussionsModel.RepliedByOid = contacts.TenantUser.Oid.ToString();
                        }
                        if (contacts.CRMSupport == null)
                        {
                            discussionsModel.PostedBy = null;
                        }
                        else
                        {
                            discussionsModel.PostedBy = contacts.CRMSupport.Name;
                        }
                        discussionsModel.time = contacts.Date.ToString();
                        discussionsModels.Add(discussionsModel);


                    }
                    return Ok(discussionsModels.ToArray());

                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }

        }

        [HttpPost]
        public async Task<IActionResult> PostComments([FromForm] DiscussionRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {

                int maxSizeMb = _configuration.GetValue<int>("MaxUploadSizeInMBs");
                long maxSizeBytes = maxSizeMb * 1024 * 1024;

                if (request.Attachment != null && request.Attachment.Length > maxSizeBytes)
                {
                    return BadRequest($"Attachment size cannot exceed {maxSizeMb} MB.");
                }

                string filename = null;
                MemoryStream stream = null;

                if (request.Attachment != null && request.Attachment.Length > 0)
                {
                    var allowedExtensions = new[] { ".png", ".jpg", ".jpeg", ".pdf", ".txt", ".xls", ".xlsx", ".log", ".zip", ".rar", ".tar" };
                    var fileExtension = Path.GetExtension(request.Attachment.FileName)?.ToLower();

                    if (string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension))
                    {
                        return BadRequest("Only PNG, JPG, JPEG, PDF, TXT, XLS, XLSX, LOG, ZIP, RAR, TAR files are allowed as attachments.");
                    }

                    filename = Guid.NewGuid().ToString() + fileExtension;
                    stream = new MemoryStream();
                    await request.Attachment.CopyToAsync(stream);
                    stream.Position = 0;
                }

                using (IObjectSpace objectSpace = objectSpaceFactory.CreateObjectSpace(typeof(Comment)))
                {
                    var cases = objectSpace.GetObjects<Ticket>();
                    var vCase = cases.SingleOrDefault(c => c.CaseID.ToString() == request.CaseID);

                    var contacts = objectSpace.GetObjects<Contact>();
                    var contact = contacts.SingleOrDefault(c => c.Oid.ToString() == request.ContactID);

                    Comment discussions = objectSpace.CreateObject<Comment>();
                    discussions.Tickets = vCase;
                    discussions.TenantUser = contact;
                    discussions.Reply = request.Replay;
                    discussions.CRMSupport = null;

                    if (stream != null)
                    {
                        discussions.Attachment = new DevExpress.Persistent.BaseImpl.FileData(((XPObjectSpace)objectSpace).Session);
                        discussions.Attachment.LoadFromStream(filename, stream);
                        discussions.Attachment.Save();
                    }

                    objectSpace.CommitChanges();

                    // Disable email notification in Comment.OnSaved() to prevent duplicate
                    discussions.DisableEmailNotification();

                    objectSpace.CommitChanges();

                    if (vCase.Technician != null)
                    {
                        await SendEmailNotificationAsync(vCase.Technician, vCase, request.Replay);
                    }
                }

                return Ok("Reply posted");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private async Task SendEmailNotificationAsync(ApplicationUser supportUser, Ticket ticket, string comment)
        {
            if (supportUser == null || string.IsNullOrEmpty(supportUser.Email) || !IsValidEmail(supportUser.Email))
                return;

            var email = _configuration.GetValue<string>("SMTPConnection:FromEmail");
            var password = _configuration.GetValue<string>("SMTPConnection:Password");
            var server = _configuration.GetValue<string>("SMTPConnection:Server");
            var port = _configuration.GetValue<int>("SMTPConnection:Port");
            var maxRetries = _configuration.GetValue<int>("SMTPConnection:MaxRetries", 3);
            var delayMs = _configuration.GetValue<int>("SMTPConnection:RetryDelayMs", 2000);
            var ccList = _configuration.GetSection("SMTPConnection:CC").Get<List<string>>();

            if (!IsValidEmail(email))
                return;

            string subject = $"Ticket: {ticket.TicketId}";
            string body =
                $"Hello {supportUser.Name},\n\n" +
                $"A new comment has been added to your assigned ticket.\n\n" +
                $"Ticket ID: {ticket.TicketId}.\n" +
                $"Message: {comment}.\n\n" +
                $"Please log in to the CRM system to review.\n\n" +
                $"Regards,\nAs-Sadeem Support Team";

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    var fromAddress = new MailAddress(email, "CRM");

                    using var smtp = new SmtpClient(server, port)
                    {
                        EnableSsl = true,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(fromAddress.Address, password)
                    };

                    using var message = new MailMessage()
                    {
                        From = fromAddress,
                        Subject = subject,
                        Body = body
                    };

                    message.To.Add(supportUser.Email);

                    if (ccList != null)
                    {
                        foreach (var ccEmail in ccList)
                        {
                            if (!string.IsNullOrEmpty(ccEmail) && IsValidEmail(ccEmail))
                                message.CC.Add(ccEmail);
                        }
                    }

                    await smtp.SendMailAsync(message);
                    return; // success — exit
                }
                catch (Exception ex) when (attempt < maxRetries)
                {
                    Logging.LogError(ex, $"DiscussionsApiController/SendEmailNotificationAsync attempt {attempt}/{maxRetries} failed for {supportUser.Email}. Retrying in {delayMs}ms...");
                    await Task.Delay(delayMs);
                    delayMs *= 2; // exponential backoff
                }
                catch (Exception ex)
                {
                    Logging.LogError(ex, $"DiscussionsApiController/SendEmailNotificationAsync failed after {maxRetries} attempts for {supportUser.Email}.");
                }
            }
        }
        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new MailAddress(email);   // throws if invalid
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
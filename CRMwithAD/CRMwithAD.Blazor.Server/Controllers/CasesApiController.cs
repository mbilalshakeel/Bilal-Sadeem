using CRMwithAD.Blazor.Server.Models.CRMApiModels;
using CRMwithAD.Blazor.Server.Services;
using CRMwithAD.Module.BusinessObjects;
using CRMwithAD.Module.BusinessObjects.Configurations;
using CRMwithAD.Module.BusinessObjects.CustomerManagement;
using CRMwithAD.Module.BusinessObjects.Helpdesk_Tickets;
using CRMwithAD.Module.Models;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.BaseImpl;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;
using Task = System.Threading.Tasks.Task;

namespace CRMwithAD.Blazor.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CasesApiController : ControllerBase
    {
        IObjectSpaceFactory objectSpaceFactory;
        private readonly IConfiguration _configuration;

        public CasesApiController(IObjectSpaceFactory objectSpaceFactory, IConfiguration configuration)
        {
            this.objectSpaceFactory = objectSpaceFactory;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Get(string AccountID, string ContactID, string TicketType)
        {
            try
            {
                List<ActiveCases> activeCases = new List<ActiveCases>();

                using (IObjectSpace newObjectSpace = objectSpaceFactory.CreateObjectSpace<Ticket>())
                {
                    IQueryable<Ticket> ticketsQuery = newObjectSpace.GetObjectsQuery<Ticket>()
                        .Where(p => p.Account.Oid.ToString() == AccountID &&
                                    p.Contact.Oid.ToString() == ContactID);

                    // -------------------------------
                    //   HANDLE TicketType NULL
                    // -------------------------------
                    if (!string.IsNullOrWhiteSpace(TicketType))
                    {
                        // Validate enum
                        if (!Enum.TryParse(typeof(TicketStatus), TicketType, true, out var parsedEnum))
                        {
                            return BadRequest("Invalid TicketType value");
                        }

                        TicketStatus selectedStatus = (TicketStatus)parsedEnum;

                        // Filter based on ticket status enum
                        ticketsQuery = ticketsQuery.Where(p => p.Status == selectedStatus);
                    }

                    // -----------------------------------
                    //  Now process all tickets
                    // -----------------------------------
                    foreach (var task in ticketsQuery)
                    {
                        var activeCase = new ActiveCases();

                        // Attachment handling
                        if (task.Attachment != null)
                        {
                            using var ms = new MemoryStream();
                            task.Attachment.SaveToStream(ms);

                            activeCase.Attachment = Convert.ToBase64String(ms.ToArray());
                            activeCase.AttachmentName = task.Attachment.FileName;
                        }

                        activeCase.Oid = task.Oid.ToString();
                        activeCase.Description = task.Description;
                        activeCase.TicketId = task.TicketId;
                        activeCase.Title = task.Title;
                        activeCase.Category = task.Category?.Title;
                        activeCase.SubCategory = task.SubCategory?.Title;
                        activeCase.Status = task.Status.ToString();
                        activeCase.Priority = task.Priority.ToString();
                        activeCase.TicketDate = task.Date.ToString("MM/dd/yyyy hh:mm tt");

                        activeCase.ClosingDate = task.CloseDate == DateTime.MinValue
                            ? "N/A"
                            : task.CloseDate.ToString("MM/dd/yyyy hh:mm tt");

                        activeCases.Add(activeCase);
                    }

                    return Ok(activeCases.ToArray());
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }
        [HttpPost]
        public async Task<IActionResult> PostAsync([FromForm] CaseRequest request)
        {
            try
            {
                // Validate attachment
                var hasAttachment = request.Attachment != null && request.Attachment.Length > 0;
                string fileExtension = hasAttachment ? Path.GetExtension(request.Attachment.FileName)?.ToLower() : "";

                if (hasAttachment && !new[] { ".png", ".jpg", ".jpeg", ".pdf", ".txt", ".xls", ".xlsx", ".log", ".zip", ".rar", ".tar" }.Contains(fileExtension))
                {
                    return BadRequest("Only PNG, JPG, JPEG, PDF, TXT, XLS, XLSX, LOG, ZIP, RAR, TAR files are allowed.");
                }

                var maxSizeMb = _configuration.GetValue<int>("MaxUploadSizeInMBs");
                var maxSizeBytes = maxSizeMb * 1024 * 1024;

                if (hasAttachment && request.Attachment.Length > maxSizeBytes)
                {
                    return BadRequest($"Attachment size cannot exceed {maxSizeMb} MB.");
                }

                using var objectSpace = objectSpaceFactory.CreateObjectSpace(typeof(Ticket));

                // Load related objects
                var account = objectSpace.FirstOrDefault<Account>(a => a.Oid.ToString() == request.AccountOid);
                var contact = objectSpace.FirstOrDefault<Contact>(c => c.Oid.ToString() == request.ContactOid);
                var subject = objectSpace.FirstOrDefault<Category>(s => s.Oid.ToString() == request.SubjectOid);
                var subCategory = objectSpace.FirstOrDefault<SubCategory>(s => s.Oid.ToString() == request.SubCategoryOid);

                if (account == null || contact == null || subject == null)
                {
                    return BadRequest("Invalid Account, Contact, or Subject.");
                }

                // Create ticket
                var ticket = objectSpace.CreateObject<Ticket>();
                ticket.Title = request.Title;
                ticket.Description = request.Description;
                ticket.Category = subject;
                ticket.SubCategory = subCategory;
                ticket.Account = account;
                ticket.Contact = contact;
                //ticket.AssignTeam = team;
                ticket.Date = DateTime.Now;
                var requestedPriority = request.Priority?.Trim();
                ticket.Priority = requestedPriority switch
                {
                    "Critical" => Priority.Critical,
                    "Severe" => Priority.Severe,
                    "High" => Priority.High,
                    _ => Priority.Moderate
                };

                // Save attachment
                if (hasAttachment)
                {
                    ticket.Attachment = new FileData(((XPObjectSpace)objectSpace).Session);
                    await using var stream = request.Attachment.OpenReadStream();
                    ticket.Attachment.LoadFromStream(Guid.NewGuid().ToString() + fileExtension, stream);
                    ticket.Attachment.Save();
                }

                objectSpace.CommitChanges();

                // Fetch team members with email
                var members = subject.Technicians
                    .Where(m => !string.IsNullOrEmpty(m.Email))
                    .ToList();

                await SendEmailNotificationAsync(members, ticket, hasAttachment ? request.Attachment : null);

                await SendTicketAcknowledgementEmailAsync(contact.Email, ticket);

                return Ok("Ticket created successfully.");
            }
            catch (Exception ex)
            {
                Logging.LogError(ex, "CasesApi/Post");
                return BadRequest("An error occurred while processing the request.");
            }
        }

        private async Task SendEmailNotificationAsync(List<ApplicationUser> members, Ticket ticket, IFormFile attachmentFile = null)
        {
            try
            {
                var toList = members
                    .Where(m => !string.IsNullOrWhiteSpace(m.Email) && IsValidEmail(m.Email))
                    .Select(m => m.Email)
                    .ToList();

                var ccList = _configuration.GetSection("SMTPConnection:CC").Get<List<string>>();

                string subject = $"NewTicket: {ticket.TicketId}";
                string body =
                    $"A new case has been assigned to your team.\n\n" +
                    $"Ticket ID: {ticket.TicketId}\n\n" +
                    $"Tenant: {ticket.Account.Name}\n" +
                    $"Case Title: {ticket.Title}\n" +
                    $"Description: {ticket.Description}\n\n" +
                    $"Please log in to the CRM system to review.\n\n" +
                    $"Regards,\nAs-Sadeem Support Team";

                // Read attachment once — reused across retries
                byte[] attachmentBytes = null;
                string attachmentFileName = null;

                if (attachmentFile != null && attachmentFile.Length > 0)
                {
                    await using var stream = attachmentFile.OpenReadStream();
                    attachmentBytes = new byte[attachmentFile.Length];
                    await stream.ReadExactlyAsync(attachmentBytes, 0, (int)attachmentFile.Length);
                    attachmentFileName = attachmentFile.FileName;
                }

                await SendEmailAsync(toList, ccList, subject, body, attachmentBytes, attachmentFileName);
            }
            catch (Exception ex)
            {
                Logging.LogError(ex, "CasesApi/SendEmailNotificationAsync");
            }
        }
        private async Task SendTicketAcknowledgementEmailAsync(string email, Ticket ticket)
        {
            var toList = new List<string> { email };
            List<string> ccList = null;

            string subject = $"Ticket Received: {ticket.TicketId}";

            string body =
                $"Your support ticket has been received.\n\n" +
                $"Ticket ID: {ticket.TicketId}\n\n" +
                $"Our cloud support team will get back to you shortly. Please check the comment section in ticket details of Helpdesk for further updates.\n\n" +
                $"This is an automated message. Please do not reply to this email.\n\n" +
                $"Regards,\nAs-Sadeem Support Team";

            await SendEmailAsync(toList, ccList, subject, body);
        }
        private async Task SendEmailAsync(List<string> toList, List<string> ccList, string subject, string body, byte[] attachmentBytes = null, string attachmentFileName = null)
        {
            var email = _configuration.GetValue<string>("SMTPConnection:FromEmail");
            var password = _configuration.GetValue<string>("SMTPConnection:Password");
            var server = _configuration.GetValue<string>("SMTPConnection:Server");
            var port = _configuration.GetValue<int>("SMTPConnection:Port");
            var maxRetries = _configuration.GetValue<int>("SMTPConnection:MaxRetries", 3);
            var delayMs = _configuration.GetValue<int>("SMTPConnection:RetryDelayMs", 2000);

            if (!IsValidEmail(email))
                return;

            // Read attachment bytes once outside the retry loop
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

                    if (toList != null)
                    {
                        foreach (var toEmail in toList)
                        {
                            if (!string.IsNullOrWhiteSpace(toEmail) && IsValidEmail(toEmail))
                                message.To.Add(toEmail);
                        }
                    }

                    if (ccList != null)
                    {
                        foreach (var ccEmail in ccList)
                        {
                            if (!string.IsNullOrWhiteSpace(ccEmail) && IsValidEmail(ccEmail))
                                message.CC.Add(ccEmail);
                        }
                    }

                    // Attach file if provided
                    if (attachmentBytes != null && !string.IsNullOrEmpty(attachmentFileName))
                    {
                        var stream = new MemoryStream(attachmentBytes);
                        var attachment = new Attachment(stream, attachmentFileName);
                        message.Attachments.Add(attachment);
                    }

                    await smtp.SendMailAsync(message);
                    return; // success — exit
                }
                catch (Exception ex) when (attempt < maxRetries)
                {
                    Logging.LogError(ex, $"SendEmailAsync attempt {attempt}/{maxRetries} failed. Retrying in {delayMs}ms...");
                    await Task.Delay(delayMs);
                    delayMs *= 2;
                }
                catch (Exception ex)
                {
                    Logging.LogError(ex, $"SendEmailAsync failed after {maxRetries} attempts.");
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
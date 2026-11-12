using CRMwithAD.Module.BusinessObjects.CustomerManagement;
using CRMwithAD.Module.BusinessObjects.Helpdesk_Tickets;
using CRMwithAD.Module.Services;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using System.ComponentModel;
using System.Net;
using System.Net.Mail;

namespace CRMwithAD.Module.BusinessObjects.HelpdeskTickets
{
    [DefaultClassOptions]
    public class Comment : BaseObject
    {
        private bool isNewComment = false;
        private bool skipEmailNotification = false;
        private bool emailSent = false; // Add this flag

        public Comment(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            CRMSupport = Session.FindObject<ApplicationUser>(CriteriaOperator.Parse("Oid=CurrentUserId()"));
        }


        // Add this persistent property to track if email was sent
        private bool _EmailSent;
        [Browsable(false)] // Hide from UI
        public bool EmailSent
        {
            get { return _EmailSent; }
            set { SetPropertyValue(nameof(EmailSent), ref _EmailSent, value); }
        }

        private ApplicationUser _CRMSupport;

        [VisibleInDetailView(false)]  // Hide in DetailView (record creation/editing)
        [VisibleInListView(true)]
        [ModelDefault("AllowEdit", "False")]
        public ApplicationUser CRMSupport
        {
            get { return _CRMSupport; }
            set { SetPropertyValue<ApplicationUser>(nameof(CRMSupport), ref _CRMSupport, value); }
        }

        private string _Reply;

        [Size(SizeAttribute.Unlimited)] // Unlimited size in DB
        [ModelDefault("RowCount", "5")] // Display 5 rows (textarea)
        [ModelDefault("PropertyEditorType", "DevExpress.ExpressApp.Blazor.Editors.BlazorStringPropertyEditor")] // Ensure using Blazor string editor

        public string Reply
        {
            get { return _Reply; }
            set { SetPropertyValue<string>(nameof(Reply), ref _Reply, value); }
        }

        private FileData _Attachment;

        [FileTypeFilter("Allowed files", 0, "*.bmp", "*.png", "*.gif", "*.jpg", "*.jpeg", "*.pdf", "*.txt", "*.log", "*.xls", "*.xlsx", "*.zip", "*.rar", "*.tar")]
        public FileData Attachment
        {
            get { return _Attachment; }
            set { SetPropertyValue<FileData>(nameof(Attachment), ref _Attachment, value); }
        }

        //[PersistentAlias("Attachment.Oid")]
        //[ImageEditor(ListViewImageEditorMode = ImageEditorMode.PictureEdit,
        //ListViewImageEditorCustomHeight = 300)]
        //[ModelDefault("AllowEdit", "False")]

        //public byte[] AttachmentThumbnail
        //{
        //    get
        //    {
        //        if (Attachment == null || Attachment.Content == null)
        //            return null;

        //        // Optional: Resize thumbnail
        //        using (var ms = new MemoryStream(Attachment.Content))
        //        {
        //            using (var image = Image.FromStream(ms))
        //            {
        //                using (var thumb = image.GetThumbnailImage(600, 600, () => false, IntPtr.Zero))
        //                {
        //                    using (var thumbStream = new MemoryStream())
        //                    {
        //                        thumb.Save(thumbStream, ImageFormat.Png);
        //                        return thumbStream.ToArray();
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        private Contact _TenantUser;
        [VisibleInDetailView(false)]  // Hide in DetailView (record creation/editing)
        [VisibleInListView(true)]
        public Contact TenantUser
        {
            get { return _TenantUser; }
            set { SetPropertyValue<Contact>(nameof(TenantUser), ref _TenantUser, value); }
        }

        private Ticket _Tickets;
        [Association]
        public Ticket Tickets
        {
            get { return _Tickets; }
            set { SetPropertyValue<Ticket>(nameof(Tickets), ref _Tickets, value); }
        }
        private DateTime _Date;

        [VisibleInDetailView(false)]  // Hide in DetailView (record creation/editing)
        [VisibleInListView(false)]
        [ModelDefault("DisplayFormat", "{0:yyyy-MM-dd HH:mm}")]
        [ModelDefault("AllowEdit", "False")]
        public DateTime Date
        {
            get { return _Date; }
            set { SetPropertyValue<DateTime>(nameof(Date), ref _Date, value); }
        }

        //Method to disable email notification (called from API)
        public void DisableEmailNotification()
        {
            skipEmailNotification = true;
        }
        protected override void OnSaving()
        {
            base.OnSaving();

            // Mark if this is a new comment
            isNewComment = Session.IsNewObject(this);

            Date = DateTime.Now;

            // Set CRMSupport if needed
            if (isNewComment && CRMSupport == null && TenantUser == null)
            {
                CRMSupport = Session.FindObject<ApplicationUser>(
                    CriteriaOperator.Parse("Oid = CurrentUserId()"));
            }

            // Ensure allowed attachments
            if (Attachment != null)
            {
                var ext = Path.GetExtension(Attachment.FileName)?.ToLowerInvariant();
                var allowed = new[] { ".png", ".jpg", ".jpeg", ".pdf", ".txt", ".xls", ".xlsx", ".log", ".zip", ".rar", ".tar" };
                if (!allowed.Contains(ext))
                    throw new UserFriendlyException("Only PNG, JPG, JPEG, PDF, TXT, XLS, XLSX, LOG, ZIP, RAR, TAR files are allowed.");
            }

            // SEND EMAIL LOGIC HERE (ONLY ONCE)
            if (isNewComment && !skipEmailNotification && !EmailSent)
            {
                if (Tickets?.Contact != null && CRMSupport != null)
                {
                    SendEmailNotificationAsync(Tickets.Contact, Tickets);
                    EmailSent = true;   // saved in SAME commit
                }
            }
        }

        [PersistentAlias("Concat(Iif(CRMSupport.Oid = CurrentUserOid(), 'Me: ', CRMSupport.UserName + ': '), Replay)")]
        [ModelDefault("DisplayFormat", "{0}")]
        [VisibleInDetailView(false)]
        [VisibleInListView(true)]
        [ModelDefault("AllowEdit", "False")]
        public string Comments
        {
            get
            {
                string sender;

                if (CRMSupport != null)
                    sender = "HEC Support " + CRMSupport.UserName;
                else if (TenantUser != null)
                    sender = "Tenant Admin " + TenantUser.FirstName + " " + TenantUser.LastName;
                else
                    sender = "Unknown";

                return $"{sender}: {Reply}";
            }


        }
        public override string ToString()
        {
            return Comments;
        }

        private void SendEmailNotificationAsync(Contact tenanttUser, Ticket ticket)
        {
            if (string.IsNullOrEmpty(tenanttUser.Email) || !IsValidEmail(tenanttUser.Email))
                return;

            var config = AppSettingsProvider.Configuration;
            var host = config["SMTPConnection:Server"];
            var port = int.Parse(config["SMTPConnection:Port"]);
            var email = config["SMTPConnection:FromEmail"];
            var password = config["SMTPConnection:Password"];
            var maxRetries = int.TryParse(config["SMTPConnection:MaxRetries"], out var r) ? r : 3;
            var delayMs = int.TryParse(config["SMTPConnection:RetryDelayMs"], out var d) ? d : 2000;
            var ccList = config.GetSection("SMTPConnection:CC")
                .GetChildren()
                .Select(x => x.Value)
                .ToList();

            if (!IsValidEmail(email))
                return;

            string subject = $"Ticket: {ticket.TicketId}";
            string body =
                $"Hello {tenanttUser.FirstName} {tenanttUser.LastName},\n\n" +
                $"Ticket ID: {ticket.TicketId}.\n\n" +
                $"A new comment has been added to your ticket.\n\n" +
                $"Comment:\n{Reply}\n\n" +
                $"Please log in to the Helpdesk to review.\n\n" +
                $"Regards,\nAs-Sadeem Support Team";

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    var fromAddress = new MailAddress(email, "CRM");

                    using var smtp = new SmtpClient(host, port)
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

                    message.To.Add(tenanttUser.Email);

                    if (ccList != null)
                    {
                        foreach (var ccEmail in ccList)
                        {
                            if (!string.IsNullOrEmpty(ccEmail) && IsValidEmail(ccEmail))
                                message.CC.Add(ccEmail);
                        }
                    }

                    smtp.Send(message);
                    return; // success — exit
                }
                catch (Exception ex) when (attempt < maxRetries)
                {
                    Tracing.Tracer.LogError(ex);
                    Thread.Sleep(delayMs);
                    delayMs *= 2; // exponential backoff
                }
                catch (Exception ex)
                {
                    Tracing.Tracer.LogError(ex);
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
using CRMwithAD.Module.BusinessObjects.Configurations;
using CRMwithAD.Module.BusinessObjects.CustomerManagement;
using CRMwithAD.Module.BusinessObjects.HelpdeskTickets;
using CRMwithAD.Module.Models;
using CRMwithAD.Module.Services;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using System.ComponentModel;
using System.Net;
using System.Net.Mail;

namespace CRMwithAD.Module.BusinessObjects.Helpdesk_Tickets
{
    [DefaultClassOptions]
    public class Ticket : BaseObject
    {
        private bool isNewObject = false;
        private Account _Account;
        //private Team _AssignTeam;
        private Contact _Contact;
        private DateTime _Date;
        private DateTime _CloseDate;
        private string _Description;
        private TicketStatus _Status;
        private Category _Category;
        //private SubCategory _AsignSubCategory;
        private string _Title;
        private SubCategory _SubCategory;
        private ApplicationUser _Technician;
        private bool _isNew;
        private Priority _Priority;
        private string _TicketId;
        private bool technicianChanged = false;
        private bool isTicketResolved = false;
        private bool wasResolvedAndSaved = false; // NEW: Track if ticket was saved as resolved

        public Ticket(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            isNewObject = true; // Mark as new
            if (Session.IsNewObject(this))
            {
                TicketId = GenerateCustomId();
            }
            // Place your initialization code here (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument112834.aspx).
        }
        private string GenerateCustomId()
        {
            return DateTime.Now.ToString("yyyyMMdd-HHmmss");
        }

        [Appearance("TitleEnabledForNew", AppearanceItemType = "ViewItem", Criteria = "!IsNewObject(this)", Enabled = false)]
        public string Title
        {
            get { return _Title; }
            set { SetPropertyValue<string>(nameof(Title), ref _Title, value); }
        }

        [Appearance("CategoryEnabledForNew", AppearanceItemType = "ViewItem", Criteria = "!IsNewObject(this)", Enabled = false)]
        [ImmediatePostData]
        public Category Category
        {
            get { return _Category; }
            set { SetPropertyValue<Category>(nameof(Category), ref _Category, value); }

            //set
            //{
            //    if (SetPropertyValue(nameof(Category), ref _Category, value))
            //    {
            //        // Clear any previously selected team
            //        AsignSubCategory = null;
            //        // Tell XAF that AvailableTeams has changed
            //        OnChanged(nameof(AvailableSubCategories));
            //        // And refresh the AssignTeam editor
            //        OnChanged(nameof(AsignSubCategory));
            //    }
            //}

        }

        [Appearance("DescriptionEnabledForNew", AppearanceItemType = "ViewItem", Criteria = "!IsNewObject(this)", Enabled = false)]
        [Size(SizeAttribute.Unlimited)] // Unlimited size in DB
        [ModelDefault("RowCount", "5")] // Display 5 rows (textarea)
        [ModelDefault("PropertyEditorType", "DevExpress.ExpressApp.Blazor.Editors.BlazorStringPropertyEditor")] // Ensure using Blazor string editor
        public string Description
        {
            get { return _Description; }
            set { SetPropertyValue<string>(nameof(Description), ref _Description, value); }
        }
        //[Appearance("AccountDisabled", Context = "Cases_DetailView", AppearanceItemType = "ViewItem", Criteria = "Status = 'Resolved' ", Enabled = false)]

        //[ModelDefault("AllowEdit", "False")]

        [Appearance("PriorityEnabledForNew", AppearanceItemType = "ViewItem", Criteria = "!IsNewObject(this)", Enabled = false)]
        public Priority Priority
        {
            get { return _Priority; }
            set { SetPropertyValue<Priority>(nameof(Type), ref _Priority, value); }
        }

        private FileData _Attachment;

        public FileData Attachment
        {
            get { return _Attachment; }
            set { SetPropertyValue<FileData>(nameof(Attachment), ref _Attachment, value); }
        }

        [Appearance("AccountEnabledForNew", AppearanceItemType = "ViewItem", Criteria = "!IsNewObject(this)", Enabled = false)]
        public Account Account
        {
            get { return _Account; }
            set { SetPropertyValue<Account>(nameof(Account), ref _Account, value); }
        }

        //public SubCategory SubCategory
        //{
        //    get { return _SubCategory; }
        //    set { SetPropertyValue<SubCategory>(nameof(SubCategory), ref _SubCategory, value); }
        //}


        // SubCategory property — ensure it triggers AvailableTechnicians refresh when changed
        [Appearance("SubCategoryEnabledForNew", AppearanceItemType = "ViewItem", Criteria = "!IsNewObject(this)", Enabled = false)]
        [ImmediatePostData]
        public SubCategory SubCategory
        {
            get { return _SubCategory; }
            set
            {
                if (SetPropertyValue<SubCategory>(nameof(SubCategory), ref _SubCategory, value))
                {
                    // Refresh available technicians when SubCategory changes
                    OnChanged(nameof(AvailableTechnicians));
                    OnChanged(nameof(Technician));
                }
            }
        }

        //[Appearance("AssignTeamDisabled", Context = "Cases_DetailView", AppearanceItemType = "ViewItem", Criteria = "Status = 'Resolved' ", Enabled = false)]
        //[ModelDefault("AllowEdit", "False")]

        //[ImmediatePostData]
        //[DataSourceProperty(nameof(AvailableTeams))]
        //public Team AssignTeam
        //{
        //    get { return _AssignTeam; }
        //    set
        //    {
        //        SetPropertyValue<Team>(nameof(AssignTeam), ref _AssignTeam, value);
        //        OnChanged(nameof(AvailableTechnicians));
        //        // 4) Also signal that the bound Technician property has changed
        //        OnChanged(nameof(Technician));
        //    }
        //}


        //[ImmediatePostData]
        //[DataSourceProperty(nameof(AvailableSubCategories))]
        //public SubCategory AsignSubCategory
        //{
        //    get { return _AsignSubCategory; }
        //    set
        //    {
        //        SetPropertyValue<SubCategory>(nameof(AsignSubCategory), ref _AsignSubCategory, value);
        //        OnChanged(nameof(AvailableTechnicians));
        //        // 4) Also signal that the bound Technician property has changed
        //        OnChanged(nameof(Technician));
        //    }
        //}

        [Browsable(false)]
        public XPCollection<SubCategory> AvailableSubCategories
        {
            get
            {
                // If no category is chosen, return an empty list
                if (Category == null)
                {
                    return new XPCollection<SubCategory>(Session);
                }
                // Filter: Team.Title = Category.Title
                var crit = new BinaryOperator("Title", Category.Title);
                return new XPCollection<SubCategory>(Session, crit);
            }
        }

        [Browsable(false)]
        [NonPersistent]
        public bool IsResolvedAndSaved
        {
            get { return wasResolvedAndSaved; }
        }


        [DataSourceProperty(nameof(AvailableTechnicians))]
        [Appearance("TechnicianEnabledForNew", AppearanceItemType = "ViewItem", Criteria = "IsResolvedAndSaved = true", Enabled = false)]
        public ApplicationUser Technician
        {
            get { return _Technician; }
            set { SetPropertyValue<ApplicationUser>(nameof(Technician), ref _Technician, value); }
        }


        [Browsable(false)]
        public XPCollection<ApplicationUser> AvailableTechnicians
        {
            //get
            //{
            //    // This will pull only those users in the Technician role:
            //    return new XPCollection<ApplicationUser>(Session,
            //        CriteriaOperator.Parse("Roles[Name = 'Technician']"));
            //}

            get
            {
                //// 1) Must be in the Technician role:
                //var criteria = CriteriaOperator.Parse("Roles[Name = 'Technician']");

                //// 2) And, if a team is selected, match the user's Teams property:
                //if (SubCategory != null)
                //{
                //    // filter ApplicationUser.Teams.Oid == AssignTeam.Oid
                //    var teamCrit = new BinaryOperator("SubCategories.Oid", SubCategory.Oid);
                //    criteria = CriteriaOperator.And(criteria, teamCrit);
                //}

                //return new XPCollection<ApplicationUser>(Session, criteria);

                // Get all technicians from the selected SubCategory
                //var allTechs = SubCategory.Technicians;

                // Filter only those with Technician role
                //var criteria = CriteriaOperator.Parse("Roles[Name = 'Technician']");
                //var filtered = allTechs.Cast<ApplicationUser>()
                //    .Where(u => u.Roles.Any(r => r.Name == "Technician"))
                //    .ToList();

                //var result = new XPCollection<ApplicationUser>(Session, false);
                //foreach (var tech in filtered)
                //{
                //    result.Add(tech);
                //}
                //return result;

                if (Category == null)
                {
                    return new XPCollection<ApplicationUser>(Session);
                }

                var allTechs = Category.Technicians ?? new XPCollection<ApplicationUser>(Session);

                // Filter only those with Technician role
                var filtered = allTechs.Cast<ApplicationUser>()
                    .Where(u => u.Roles.Any(r => r.Name == "Technician"))
                    .ToList();

                var result = new XPCollection<ApplicationUser>(Session, false);
                foreach (var tech in filtered)
                {
                    result.Add(tech);
                }
                return result;

            }
        }

        [PersistentAlias("Oid")] // Make sure this refers to the underlying OID property
        [Browsable(false)]
        //[ModelDefault("AllowEdit", "False")] // Set it as read-only in the UI
        // Hides from Property Editor (Designer)
        [ModelDefault("IsVisible", "False")]

        public Guid CaseID
        {
            get { return Oid; } // Return the OID value
        }
        //[Appearance("ContactDisabled", Context = "Cases_DetailView", AppearanceItemType = "ViewItem", Criteria = "Status = 'Resolved' ", Enabled = false)]

        //[ModelDefault("AllowEdit", "False")]


        [Appearance("ContactEnabledForNew", AppearanceItemType = "ViewItem", Criteria = "!IsNewObject(this)", Enabled = false)]
        public Contact Contact
        {
            get { return _Contact; }
            set { SetPropertyValue<Contact>(nameof(Contact), ref _Contact, value); }
        }
        //[ModelDefault("AllowEdit", "False")]

        [Appearance("DateEnabledForNew", AppearanceItemType = "ViewItem", Criteria = "!IsNewObject(this)", Enabled = false)]
        [ModelDefault("DisplayFormat", "yyyy-MM-dd HH:mm")]
        [ModelDefault("EditMask", "yyyy-MM-dd HH:mm")]
        public DateTime Date
        {
            get { return _Date; }
            set { SetPropertyValue<DateTime>(nameof(Date), ref _Date, value); }
        }


        [ModelDefault("AllowEdit", "False")]
        [ModelDefault("DisplayFormat", "yyyy-MM-dd HH:mm")]
        [ModelDefault("EditMask", "yyyy-MM-dd HH:mm")]
        public DateTime CloseDate
        {
            get { return _CloseDate; }
            set { SetPropertyValue<DateTime>(nameof(CloseDate), ref _CloseDate, value); }
        }

        [ModelDefault("AllowEdit", "False")]
        public string TicketId
        {
            get { return _TicketId; }
            set { SetPropertyValue<string>(nameof(TicketId), ref _TicketId, value); }
        }

        // Disable for regular users when resolved
        //[Appearance("StatusDisabledForUsers",
        //    Context = "Ticket_DetailView",
        //    AppearanceItemType = "ViewItem",
        //    Criteria = "Status = 'Resolved'",
        //    Enabled = false,
        //    Priority = 1)]


        // Enable for Administrators (higher priority overrides)
        //[Appearance("StatusEnabledForAdmin",
        //    Context = "Cases_DetailView",
        //    AppearanceItemType = "ViewItem",
        //    Criteria = "Status = 'Resolved' AND IsCurrentUserInRole('Administrators')",
        //    Enabled = true,   
        //    Priority = 2)]

        [Appearance("StatusDisabled", AppearanceItemType = "ViewItem", Criteria = "IsResolvedAndSaved = true", Enabled = false)]
        public TicketStatus Status
        {
            get { return _Status; }
            set { SetPropertyValue(nameof(Status), ref _Status, value); }
        }

        [NonPersistent]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool IsNotAdmin
        {
            get
            {
                var user = SecuritySystem.CurrentUser as ApplicationUser;
                if (user == null) return true;
                return !user.Roles.Any(r => r.Name == "Administrators"); // Adjust role name if needed
            }
        }

        [Appearance("CommentsDisabled", Context = "Ticket_DetailView", AppearanceItemType = "ViewItem", Criteria = "Status = 'Resolved' ", Enabled = false)]
        [Aggregated, Association]
        public XPCollection<Comment> Comments
        {
            get { return GetCollection<Comment>(nameof(Comments)); }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [PersistentAlias("SubCategory.Oid")]
        public Guid? SubCategoryOid
        {
            get
            {
                return SubCategory != null ? (Guid?)SubCategory.Oid : null;
            }
        }

        protected override void OnSaving()
        {
            _isNew = Session.IsNewObject(this);  // Set flag BEFORE saving

            if (isTicketResolved && (_CloseDate == default || _CloseDate == DateTime.MinValue))
            {
                SetPropertyValue(nameof(CloseDate), ref _CloseDate, DateTime.Now);
            }

            if (Status == TicketStatus.Resolved)
            {
                wasResolvedAndSaved = true;
            }

            base.OnSaving();
        }
        protected override void OnSaved()
        {
            base.OnSaved();

            bool sendToNewObject = isNewObject;
            bool sendToTechnician = technicianChanged && Technician != null;
            bool sendToCustomer = isTicketResolved && Contact != null;

            // Reset the flags BEFORE sending emails to avoid re-triggering
            isNewObject = false;
            technicianChanged = false;
            isTicketResolved = false;

            if (sendToNewObject)
            {
                SendEmailNotification();
            }

            if (sendToTechnician)
            {
                SendTicketAssignedEmail();
            }

            if (sendToCustomer)
            {
                SendTicketResolvedEmail();
            }

            // Notify UI that IsResolvedAndSaved changed (to trigger appearance rules)
            OnChanged(nameof(IsResolvedAndSaved));
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();

            // When loading from database, check if it's already resolved
            if (Status == TicketStatus.Resolved)
            {
                wasResolvedAndSaved = true;
            }
        }

        protected override void OnChanged(string propertyName, object oldValue, object newValue)
        {
            base.OnChanged(propertyName, oldValue, newValue);

            if (propertyName == nameof(Technician) && newValue != null)
            {
                technicianChanged = true;  // Mark to send email
            }

            if (propertyName == nameof(Status) && newValue is TicketStatus status && status == TicketStatus.Resolved)
            {
                isTicketResolved = true;
            }

        }

        private void SendEmailNotification()
        {
            if (Technician == null || string.IsNullOrEmpty(Technician.Email))
                return;

            try
            {
                var config = AppSettingsProvider.Configuration;

                var host = config["SMTPConnection:Server"];
                var port = int.Parse(config["SMTPConnection:Port"]);
                var email = config["SMTPConnection:FromEmail"];
                var password = config["SMTPConnection:Password"];

                var fromAddress = new MailAddress(email, "CRM");
                var toAddress = new MailAddress(Technician.Email, Technician.Name);

                string subject = $"Ticket Assigned: {TicketId}";
                string body = $"Hello {Technician.Name},\n\n" +
                              $"A new case has been assigned to you.\n\n" +
                              $"Case Title: {Title}\n" +
                              $"Description: {Description}\n" +
                              $"Status: {Status}\n" +
                              $"Date: {Date}\n\n" +
                              $"TicketId: {TicketId}\n\n" +
                              $"Please log in to the system to review the details.\n\n" +
                              $"Regards,\nSupport System";

                var smtp = new SmtpClient
                {
                    Host = host,
                    Port = port,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, password)
                };

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body
                })
                {
                    smtp.Send(message);
                }
            }
            catch (Exception ex)
            {
                Tracing.Tracer.LogError(ex);
            }
        }
        private void SendTicketAssignedEmail()
        {
            if (Technician == null || string.IsNullOrEmpty(Technician.Email) || !IsValidEmail(Technician.Email))
                return;

            string subject = $"Ticket Assigned: {TicketId}";
            string body =
                $"Hello {Technician.Name},\n\n" +
                $"A new ticket has been assigned to you.\n\n" +
                $"TicketId: {TicketId}\n\n" +
                $"Title: {Title}\n" +
                $"Description: {Description}\n\n" +
                $"Please log in to CRM portal to review the details.\n\n" +
                $"Regards,\nAs-Sadeem Support Team";

            SendEmail(Technician.Email, subject, body);
        }
        private void SendTicketResolvedEmail()
        {
            if (Contact == null || string.IsNullOrEmpty(Contact.Email) || !IsValidEmail(Contact.Email))
                return;

            string subject = $"Ticket Resolved: {TicketId}";
            string body =
                $"Hello {Contact.FirstName} {Contact.LastName},\n\n" +
                $"Your ticket with Ticket ID: {TicketId} has been resolved.\n\n" +
                $"This is an automated message. Please do not reply to this email.\n\n" +
                $"Regards,\nAs-Sadeem Support Team";

            SendEmail(Contact.Email, subject, body, true);
        }
        private void SendEmail(string toEmail, string subject, string body, bool isResolvedTicketEmail = false)
        {
            if (string.IsNullOrEmpty(toEmail) || !IsValidEmail(toEmail))
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

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    var fromAddress = new MailAddress(email, "CRM");
                    var toAddress = new MailAddress(toEmail);

                    using var smtp = new SmtpClient
                    {
                        Host = host,
                        Port = port,
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

                    message.To.Add(toAddress);

                    if (!isResolvedTicketEmail && ccList != null)
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
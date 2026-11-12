using CRMwithAD.Module.BusinessObjects.CampaignsManagement;
using CRMwithAD.Module.BusinessObjects.CustomerManagement;
using CRMwithAD.Module.Models;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using System.Net;
using System.Net.Mail;

namespace CRMwithAD.Module.Controllers
{
    // For more typical usage scenarios, be sure to check out https://documentation.devexpress.com/eXpressAppFramework/clsDevExpressExpressAppViewControllertopic.aspx.
    public partial class CampaignViewController : ObjectViewController<DetailView, Campaign>
    {
        // Use CodeRush to create Controllers and Actions with a few keystrokes.
        // https://docs.devexpress.com/CodeRushForRoslyn/403133/
        public CampaignViewController()
        {
            InitializeComponent();
            SimpleAction startCampaignAction = new SimpleAction(this, "StartCampaign", PredefinedCategory.RecordEdit)
            {
                Caption = "Start Campaign",
                ConfirmationMessage = "Are you sure you want to start this campaign?",
                ImageName = "Action_Grant",
                TargetObjectType = typeof(Campaign)
            };
            startCampaignAction.Execute += StartCampaignAction_Execute;
            // Target required Views (via the TargetXXX properties) and create their Actions.
        }
        protected override void OnActivated()
        {
            base.OnActivated();
            var action = Actions["StartCampaign"] as SimpleAction;
            if (action != null)
            {
                UpdateActionState(action);
                View.CurrentObjectChanged += (s, e) => UpdateActionState(action);
            }
            // Perform various tasks depending on the target View.
        }
        private void UpdateActionState(SimpleAction action)
        {
            if (View?.CurrentObject is Campaign campaign)
            {
                action.Active["CanStart"] = campaign.Status == CampaignStatus.Created;
            }
            else
            {
                action.Active["CanStart"] = false;
            }
        }

        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
            // Access and customize the target View control.
        }
        protected override void OnDeactivated()
        {
            // Unsubscribe from previously subscribed events and release other references and resources.
            base.OnDeactivated();
        }
        //private async void StartCampaignAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        //{
        //    Campaign campaign = View.CurrentObject as Campaign;
        //    if (campaign != null && campaign.Status == CampaignStatus.Created)
        //    {
        //        campaign.Status = CampaignStatus.Created;
        //        campaign.StartDate = DateTime.Now;
        //        // Trigger email sending logic here

        //        var contacts = ObjectSpace.GetObjects<Contact>(); // Assuming Contact is defined in your app
        //        foreach (var contact in contacts)
        //        {
        //            //SendEmail(contact.Email, campaign.Name, campaign.Description);

        //            SmtpClient client = new SmtpClient("smtp.office365.com", int.Parse("587"));
        //            client.Credentials = new NetworkCredential("test.user@appinsnap.com", "Login@786");
        //            client.EnableSsl = true;
        //            MailMessage message = new MailMessage();
        //            message.Subject = campaign.Name;
        //            message.Body = campaign.Description;
        //            if (campaign.Banner != null && campaign.Banner.Content != null)
        //            {
        //                byte[] bannerBytes = campaign.Banner.Content;
        //                MemoryStream bannerStream = new MemoryStream(bannerBytes);
        //                string bannerName = campaign.Banner.FileName ?? "banner.jpg";

        //                Attachment bannerAttachment = new Attachment(bannerStream, bannerName);
        //                message.Attachments.Add(bannerAttachment);
        //            }
        //            message.IsBodyHtml = true;
        //            message.To.Add(contact.Email);
        //            message.From = new MailAddress("test.user@appinsnap.com", string.Empty);
        //            //if (!string.IsNullOrEmpty(cc))
        //            //    message.CC.Add(cc);
        //            await client.SendMailAsync(message);

        //        }

        //        ObjectSpace.CommitChanges();
        //        campaign.Status = CampaignStatus.Completed;
        //        campaign.ExecuteDate = DateTime.Now;
        //        var action = sender as SimpleAction;
        //        if (action != null)
        //            UpdateActionState(action);
        //    }
        //}
        private async void StartCampaignAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            Campaign campaign = View.CurrentObject as Campaign;
            if (campaign != null && campaign.Status == CampaignStatus.Created)
            {
                if (campaign.StartDate < DateTime.Now)
                {
                    Application.ShowViewStrategy.ShowMessage(
                        "Cannot start campaign. The start date has already passed.",
                        InformationType.Error,
                        2000,   // message duration in ms
                        InformationPosition.Top
                    );
                    return;
                }

                var contacts = ObjectSpace.GetObjects<Contact>();
                foreach (var contact in contacts)
                {
                    using var client = new SmtpClient("smtp.office365.com", 587)
                    {
                        Credentials = new NetworkCredential("cloud@pern.edu.pk", "G/132597703328as"),
                        EnableSsl = true
                    };

                    using var message = new MailMessage
                    {
                        Subject = campaign.Name,
                        Body = campaign.Description,
                        IsBodyHtml = true,
                        From = new MailAddress("cloud@pern.edu.pk", string.Empty)
                    };

                    message.To.Add(contact.Email);

                    if (campaign.Banner != null && campaign.Banner.Content != null)
                    {
                        var bannerStream = new MemoryStream(campaign.Banner.Content);
                        var bannerName = campaign.Banner.FileName ?? "banner.jpg";
                        message.Attachments.Add(new Attachment(bannerStream, bannerName));
                    }

                    await client.SendMailAsync(message);
                }

                campaign.Status = CampaignStatus.Completed;
                campaign.ExecuteDate = DateTime.Now;

                ObjectSpace.CommitChanges();

                var action = sender as SimpleAction;
                if (action != null)
                    UpdateActionState(action);
            }
        }
    }
}

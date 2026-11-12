using DevExpress.ExpressApp;

namespace CRMwithAD.Module.Controllers
{
    // For more typical usage scenarios, be sure to check out https://documentation.devexpress.com/eXpressAppFramework/clsDevExpressExpressAppViewControllertopic.aspx.
    public partial class DiscussionsReplyController : ViewController
    {
        //// Use CodeRush to create Controllers and Actions with a few keystrokes.
        //// https://docs.devexpress.com/CodeRushForRoslyn/403133/
        //public DiscussionsReplyController()
        //{
        //    InitializeComponent();
        //    var reply = new SimpleAction(this, "Reply", PredefinedCategory.Edit)
        //    {
        //        Caption = "Reply",
        //        TargetObjectType = typeof(Ticket)
        //    };
        //    reply.Execute += Reply_Execute;
        //    // Target required Views (via the TargetXXX properties) and create their Actions.
        //}
        //private void Reply_Execute(object sender, SimpleActionExecuteEventArgs e)
        //{
        //    var os = Application.CreateObjectSpace(typeof(Comment));
        //    var newMsg = os.CreateObject<Comment>();
        //    newMsg.CRMSupport = os.FindObject<ApplicationUser>(
        //        new BinaryOperator("UserName", SecuritySystem.CurrentUserName));
        //    newMsg.Reply = "Type your message here"; // or show popup if needed
        //    newMsg.Date = DateTime.Now;

        //    os.CommitChanges();
        //    View.ObjectSpace.Refresh();
        //}
        //protected override void OnActivated()
        //{
        //    base.OnActivated();
        //    // Perform various tasks depending on the target View.
        //}
        //protected override void OnViewControlsCreated()
        //{
        //    base.OnViewControlsCreated();
        //    // Access and customize the target View control.
        //}
        //protected override void OnDeactivated()
        //{
        //    // Unsubscribe from previously subscribed events and release other references and resources.
        //    base.OnDeactivated();
        //}
    }
}

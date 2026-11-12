using CRMwithAD.Module.BusinessObjects.SubscriptionsManagement;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.SystemModule;

namespace CRMwithAD.Module.Controllers
{
    // For more typical usage scenarios, be sure to check out https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.ViewController.
    public partial class SubscriptionPlanReadOnlyController : ViewController<ListView>
    {
        // Use CodeRush to create Controllers and Actions with a few keystrokes.
        // https://docs.devexpress.com/CodeRushForRoslyn/403133/
        public SubscriptionPlanReadOnlyController()
        {
            InitializeComponent();
            TargetObjectType = typeof(SubscriptionPlan);
            TargetViewType = ViewType.ListView;
        }
        protected override void OnActivated()
        {
            base.OnActivated();

            // Disable New action
            var newObjectViewController = Frame.GetController<NewObjectViewController>();
            if (newObjectViewController != null)
            {
                newObjectViewController.NewObjectAction.Active["SubscriptionPlanReadOnly"] = false;
            }

            // Disable Delete action
            var deleteObjectsViewController = Frame.GetController<DeleteObjectsViewController>();
            if (deleteObjectsViewController != null)
            {
                deleteObjectsViewController.DeleteAction.Active["SubscriptionPlanReadOnly"] = false;
            }

            // Disable Double-Click / Open
            var processController = Frame.GetController<ListViewProcessCurrentObjectController>();
            if (processController != null)
            {
                processController.ProcessCurrentObjectAction.Enabled["SubscriptionPlanReadOnly"] = false;
            }

            // Set the view to read-only
            View.AllowEdit["SubscriptionPlanReadOnly"] = false;
        }

        protected override void OnDeactivated()
        {
            // Re-enable actions when controller is deactivated
            var newObjectViewController = Frame.GetController<NewObjectViewController>();
            if (newObjectViewController != null)
            {
                newObjectViewController.NewObjectAction.Active.RemoveItem("SubscriptionPlanReadOnly");
            }

            var deleteObjectsViewController = Frame.GetController<DeleteObjectsViewController>();
            if (deleteObjectsViewController != null)
            {
                deleteObjectsViewController.DeleteAction.Active.RemoveItem("SubscriptionPlanReadOnly");
            }

            var processController = Frame.GetController<ListViewProcessCurrentObjectController>();
            if (processController != null)
            {
                processController.ProcessCurrentObjectAction.Enabled.RemoveItem("SubscriptionPlanReadOnly");
            }

            base.OnDeactivated();
        }
    }
}

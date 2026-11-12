using CRMwithAD.Module.BusinessObjects.SubscriptionsManagement;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.SystemModule;

namespace CRMwithAD.Module.Controllers
{
    public partial class UserSubscribedPlanReadOnlyController : ViewController<ListView>
    {
        public UserSubscribedPlanReadOnlyController()
        {
            InitializeComponent();
            TargetObjectType = typeof(UserSubscribedPlan);
            TargetViewType = ViewType.ListView;
        }
        protected override void OnActivated()
        {
            base.OnActivated();

            var newObjectViewController = Frame.GetController<NewObjectViewController>();
            if (newObjectViewController != null)
            {
                newObjectViewController.NewObjectAction.Active["UserSubscribedPlanReadOnly"] = false;
            }

            var deleteObjectsViewController = Frame.GetController<DeleteObjectsViewController>();
            if (deleteObjectsViewController != null)
            {
                deleteObjectsViewController.DeleteAction.Active["UserSubscribedPlanReadOnly"] = false;
            }

            var processController = Frame.GetController<ListViewProcessCurrentObjectController>();
            if (processController != null)
            {
                processController.ProcessCurrentObjectAction.Enabled["UserSubscribedPlanReadOnly"] = false;
            }

            View.AllowEdit["UserSubscribedPlanReadOnly"] = false;
        }

        protected override void OnDeactivated()
        {
            var newObjectViewController = Frame.GetController<NewObjectViewController>();
            if (newObjectViewController != null)
            {
                newObjectViewController.NewObjectAction.Active.RemoveItem("UserSubscribedPlanReadOnly");
            }

            var deleteObjectsViewController = Frame.GetController<DeleteObjectsViewController>();
            if (deleteObjectsViewController != null)
            {
                deleteObjectsViewController.DeleteAction.Active.RemoveItem("UserSubscribedPlanReadOnly");
            }

            var processController = Frame.GetController<ListViewProcessCurrentObjectController>();
            if (processController != null)
            {
                processController.ProcessCurrentObjectAction.Enabled.RemoveItem("UserSubscribedPlanReadOnly");
            }

            base.OnDeactivated();
        }
    }
}

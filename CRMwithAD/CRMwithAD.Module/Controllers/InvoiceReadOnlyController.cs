using CRMwithAD.Module.BusinessObjects.BillingManagement;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.SystemModule;

namespace CRMwithAD.Module.Controllers
{
    public class InvoiceReadOnlyController : ViewController<ListView>
    {
        public InvoiceReadOnlyController()
        {
            TargetObjectType = typeof(Invoice);
            TargetViewType = ViewType.ListView;
        }

        protected override void OnActivated()
        {
            base.OnActivated();

            // Disable New action
            var newObjectViewController = Frame.GetController<NewObjectViewController>();
            if (newObjectViewController != null)
            {
                newObjectViewController.NewObjectAction.Active["InvoiceReadOnly"] = false;
            }

            // Disable Delete action
            var deleteObjectsViewController = Frame.GetController<DeleteObjectsViewController>();
            if (deleteObjectsViewController != null)
            {
                deleteObjectsViewController.DeleteAction.Active["InvoiceReadOnly"] = false;
            }
            // 1. Disable the Double-Click
            var processController = Frame.GetController<ListViewProcessCurrentObjectController>();
            if (processController != null)
            {
                processController.ProcessCurrentObjectAction.Enabled["InvoiceReadOnly"] = false;
            }

            // 2. Set the View to Read-Only at the View level
            View.AllowEdit["InvoiceReadOnly"] = false;
        }

        protected override void OnDeactivated()
        {
            // Re-enable actions when controller is deactivated
            var newObjectViewController = Frame.GetController<NewObjectViewController>();
            if (newObjectViewController != null)
            {
                newObjectViewController.NewObjectAction.Active.RemoveItem("InvoiceReadOnly");
            }

            var deleteObjectsViewController = Frame.GetController<DeleteObjectsViewController>();
            if (deleteObjectsViewController != null)
            {
                deleteObjectsViewController.DeleteAction.Active.RemoveItem("InvoiceReadOnly");
            }

            base.OnDeactivated();
        }
    }
}

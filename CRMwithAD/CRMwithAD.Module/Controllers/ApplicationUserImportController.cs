using CRMwithAD.Module.BusinessObjects;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;

namespace CRMwithAD.Module.Controllers
{
    // Non-persistent object to host the Blazor import popup
    [DomainComponent]
    public class ImportUsersBlazorHost
    {
        //[Browsable(false)]
        [EditorAlias("ImportUsersBlazorHostPropertyEditor")]
        public string AD_Users { get; set; } // Dummy property for the property editor
    }

    public class ApplicationUserImportController : ViewController<ListView>
    {
        private readonly PopupWindowShowAction importUsersAction;
        public ApplicationUserImportController()
        {
            TargetObjectType = typeof(ApplicationUser);
            TargetViewType = ViewType.ListView;
            importUsersAction = new PopupWindowShowAction(this, "ImportUsers", PredefinedCategory.View)
            {
                Caption = "Import Users",
                ImageName = "Import"
            };
            importUsersAction.CustomizePopupWindowParams += ImportUsersAction_CustomizePopupWindowParams;
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            // Deactivate (hide) the Import Users action so it does not appear in the ApplicationUser ListView.
            // If you want to hide conditionally (e.g., only for non-admins or only in Blazor), add checks here and set the Active state accordingly.
            importUsersAction.Active["HideImportUsers"] = false;
        }

        private void ImportUsersAction_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            var objectSpace = Application.CreateObjectSpace(typeof(ImportUsersBlazorHost));
            var host = objectSpace.CreateObject<ImportUsersBlazorHost>();
            var detailView = Application.CreateDetailView(objectSpace, host);
            e.View = detailView;
            e.DialogController.SaveOnAccept = false;
            e.DialogController.AcceptAction.Caption = "Close";
            e.DialogController.CancelAction.Active["Hide"] = false;
        }
    }
}

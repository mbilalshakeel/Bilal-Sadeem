using CRMwithAD.Module.BusinessObjects.HelpdeskTickets;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Layout;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Model.NodeGenerators;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Templates;
using DevExpress.ExpressApp.Utils;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRMwithAD.Module.Controllers
{
    // For more typical usage scenarios, be sure to check out https://documentation.devexpress.com/eXpressAppFramework/clsDevExpressExpressAppViewControllertopic.aspx.
    public partial class HideDiscussionsColumnsController : ViewController
    {
        // Use CodeRush to create Controllers and Actions with a few keystrokes.
        // https://docs.devexpress.com/CodeRushForRoslyn/403133/
        public HideDiscussionsColumnsController()
        {
            InitializeComponent();
            // Target required Views (via the TargetXXX properties) and create their Actions.
        }
        protected override void OnActivated()
        {
            base.OnActivated();
            if (View.ObjectTypeInfo.Type != typeof(Comment))
                return;

            // Cast to IModelListView
            var modelListView = View.Model as IModelListView;
            if (modelListView == null)
                return;

            foreach (var column in modelListView.Columns)
            {
                var modelColumn = column as IModelColumn;
                if (modelColumn == null) continue;

                if (modelColumn.PropertyName == "Comments")
                {
                    modelColumn.Index = 1;

                }
                else if (modelColumn.PropertyName == "Attachment")
                {
                    modelColumn.Index = 2;

                    modelColumn.DisplayFormat = "Html";
                }
                else if (modelColumn.PropertyName == "Date")
                {
                    modelColumn.Index = 0;
                    modelColumn.DisplayFormat = "yyyy-MM-dd HH:mm:ss";
                }
                else
                {
                    modelColumn.Index = -1;
                }
            }
            // Perform various tasks depending on the target View.
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
    }
}

using DevExpress.ExpressApp;

namespace CRMwithAD.Module.Controllers
{
    // For more typical usage scenarios, be sure to check out https://documentation.devexpress.com/eXpressAppFramework/clsDevExpressExpressAppViewControllertopic.aspx.
    public partial class CaseListViewTeamFilterController : ViewController<ListView>
    {
        // Use CodeRush to create Controllers and Actions with a few keystrokes.
        // https://docs.devexpress.com/CodeRushForRoslyn/403133/
        public CaseListViewTeamFilterController()
        {
            InitializeComponent();
            // Target required Views (via the TargetXXX properties) and create their Actions.
        }
        //protected override void OnActivated()
        //{
        //    base.OnActivated();
        //    var user = SecuritySystem.CurrentUser as ApplicationUser;

        //    if (user?.SubCategoryOid != null && View.ObjectTypeInfo.Type == typeof(Ticket))
        //    {
        //        View.CollectionSource.Criteria["TeamFilter"] =
        //        //CriteriaOperator.Parse("AssignTeamOid = ?", user.SubCategoryOid);
        //        CriteriaOperator.Parse("SubCategoryOid = ?", user.SubCategoryOid);
        //    }
        //    // Perform various tasks depending on the target View.
        //}

        //protected override void OnActivated()
        //{
        //    base.OnActivated();
        //    var user = SecuritySystem.CurrentUser as ApplicationUser;
        //    if (user == null || View.ObjectTypeInfo.Type != typeof(Ticket))
        //        return;

        //    var criteria = View.CollectionSource.Criteria;

        //    // Find and remove existing "TeamFilter" entry without LINQ
        //    KeyValuePair<string, CriteriaOperator>? existing = null;
        //    foreach (object item in (IEnumerable)criteria)
        //    {
        //        if (item is KeyValuePair<string, CriteriaOperator> kvp && kvp.Key == "TeamFilter")
        //        {
        //            existing = kvp;
        //            break;
        //        }
        //    }
        //    if (existing.HasValue)
        //    {
        //        criteria.Remove(existing.Value);
        //    }

        //    var subCategoryOids = user.SubCategories?.Select(s => (object)s.Oid).ToArray();
        //    if (subCategoryOids != null && subCategoryOids.Length > 0)
        //    {
        //        // Add or replace the filter using the indexer supported by LightDictionary
        //        criteria["TeamFilter"] = new InOperator("SubCategoryOid", subCategoryOids);
        //    }
        //}

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

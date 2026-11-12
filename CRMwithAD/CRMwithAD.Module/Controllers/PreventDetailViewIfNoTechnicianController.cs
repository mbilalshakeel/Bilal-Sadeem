//using CRMwithAD.Module.BusinessObjects;
//using CRMwithAD.Module.BusinessObjects.Helpdesk_Tickets;
//using DevExpress.ExpressApp;
//using DevExpress.ExpressApp.SystemModule;

//namespace CRMwithAD.Module.Controllers
//{
//    public partial class PreventDetailViewIfNoTechnicianController : ViewController<ListView>
//    {
//        public PreventDetailViewIfNoTechnicianController()
//        {
//            InitializeComponent();
//            // Target required Views (via the TargetXXX properties) and create their Actions.
//        }
//        protected override void OnActivated()
//        {
//            base.OnActivated();
//            var processCurrentObjectController = Frame.GetController<ListViewProcessCurrentObjectController>();
//            if (processCurrentObjectController != null)
//            {
//                processCurrentObjectController.CustomProcessSelectedItem += ProcessCurrentObjectController_CustomProcessSelectedItem;
//            }
//            // Perform various tasks depending on the target View.
//        }
//        private void ProcessCurrentObjectController_CustomProcessSelectedItem(object sender, CustomProcessListViewSelectedItemEventArgs e)
//        {
//            var listView = View as ListView;
//            if (listView.Id == "Ticket_ListView")
//            {
//                var selectedCase = listView?.SelectedObjects?.Cast<Ticket>().FirstOrDefault();

//                if (selectedCase != null)
//                {
//                    if (selectedCase.Technician != null && !IsCurrentUserAssignedToTicket(selectedCase) && !IsCurrentUserAdmin() && !IsCurrentUserReadOnlyAdmin())
//                    {
//                        e.Handled = true;

//                        Application.ShowViewStrategy.ShowMessage(
//                            "Other technician is already assigned to this ticket.",
//                            InformationType.Warning,
//                            5000,
//                            InformationPosition.Bottom);
//                    }

//                    if (selectedCase.Technician == null && !IsCurrentUserAdmin())
//                    {
//                        if (!IsCurrentUserReadOnlyAdmin())
//                        {
//                            e.Handled = true;

//                            Application.ShowViewStrategy.ShowMessage(
//                                "No technician is assigned to this ticket. Please contact your admin.",
//                                InformationType.Warning,
//                                5000,
//                                InformationPosition.Bottom);
//                        }
//                    }
//                }
//            }
//        }
//        private bool IsCurrentUserAdmin()
//        {
//            var currentUser = SecuritySystem.CurrentUser as ApplicationUser;
//            return currentUser != null && currentUser.Roles.Any(r => r.Name == "Administrators");
//        }
//        private bool IsCurrentUserReadOnlyAdmin()
//        {
//            var currentUser = SecuritySystem.CurrentUser as ApplicationUser;
//            return currentUser != null && currentUser.Roles.Any(r => r.Name == "Admin");
//        }
//        private bool IsCurrentUserAssignedToTicket(Ticket ticket)
//        {
//            var currentUser = SecuritySystem.CurrentUser as ApplicationUser;
//            return currentUser != null && ticket.Technician != null && ticket.Technician.Oid == currentUser.Oid;
//        }
//        protected override void OnViewControlsCreated()
//        {
//            base.OnViewControlsCreated();
//            // Access and customize the target View control.
//        }
//        protected override void OnDeactivated()
//        {
//            // Unsubscribe from previously subscribed events and release other references and resources.
//            base.OnDeactivated();
//        }
//    }
//}

using CRMwithAD.Module.BusinessObjects;
using CRMwithAD.Module.BusinessObjects.Helpdesk_Tickets;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.SystemModule;

namespace CRMwithAD.Module.Controllers
{
    public partial class PreventDetailViewIfNoTechnicianController : ViewController<ListView>
    {
        public PreventDetailViewIfNoTechnicianController()
        {
            InitializeComponent();
            // Target the specific Ticket ListView
            TargetViewId = "Ticket_ListView";
        }

        protected override void OnActivated()
        {
            base.OnActivated();

            // 1. Filter the list so technicians only see their own assigned tickets
            ApplySecurityFilter();

            // 2. Keep your existing logic to prevent opening via double-click/enter
            var processCurrentObjectController = Frame.GetController<ListViewProcessCurrentObjectController>();
            if (processCurrentObjectController != null)
            {
                processCurrentObjectController.CustomProcessSelectedItem += ProcessCurrentObjectController_CustomProcessSelectedItem;
            }
        }

        private void ApplySecurityFilter()
        {
            // If the user is an Admin or ReadOnly Admin, don't filter anything
            if (IsCurrentUserAdmin() || IsCurrentUserReadOnlyAdmin())
            {
                View.CollectionSource.Criteria["TechnicianFilter"] = null;
            }
            else
            {
                // Filter: Technician must match current user Oid
                // If no tickets match, the list will naturally be empty
                var currentUser = SecuritySystem.CurrentUser as ApplicationUser;
                if (currentUser != null)
                {
                    View.CollectionSource.Criteria["TechnicianFilter"] = CriteriaOperator.Parse("Technician.Oid = ?", currentUser.Oid);
                }
            }
        }

        private void ProcessCurrentObjectController_CustomProcessSelectedItem(object sender, CustomProcessListViewSelectedItemEventArgs e)
        {
            var selectedCase = e.InnerArgs.CurrentObject as Ticket;

            if (selectedCase != null)
            {
                // Logic: Block if assigned to someone else AND user is not admin
                if (selectedCase.Technician != null && !IsCurrentUserAssignedToTicket(selectedCase) && !IsCurrentUserAdmin() && !IsCurrentUserReadOnlyAdmin())
                {
                    e.Handled = true;
                    ShowWarning("Other technician is already assigned to this ticket.");
                }
                // Logic: Block if no technician assigned AND user is not admin
                else if (selectedCase.Technician == null && !IsCurrentUserAdmin() && !IsCurrentUserReadOnlyAdmin())
                {
                    e.Handled = true;
                    ShowWarning("No technician is assigned to this ticket. Please contact your admin.");
                }
            }
        }

        private void ShowWarning(string message)
        {
            Application.ShowViewStrategy.ShowMessage(message, InformationType.Warning, 5000, InformationPosition.Bottom);
        }

        private bool IsCurrentUserAdmin()
        {
            var currentUser = SecuritySystem.CurrentUser as ApplicationUser;
            return currentUser != null && currentUser.Roles.Any(r => r.Name == "Administrators");
        }

        private bool IsCurrentUserReadOnlyAdmin()
        {
            var currentUser = SecuritySystem.CurrentUser as ApplicationUser;
            return currentUser != null && currentUser.Roles.Any(r => r.Name == "Admin");
        }

        private bool IsCurrentUserAssignedToTicket(Ticket ticket)
        {
            var currentUser = SecuritySystem.CurrentUser as ApplicationUser;
            return currentUser != null && ticket.Technician != null && ticket.Technician.Oid == currentUser.Oid;
        }

        protected override void OnDeactivated()
        {
            var processCurrentObjectController = Frame.GetController<ListViewProcessCurrentObjectController>();
            if (processCurrentObjectController != null)
            {
                processCurrentObjectController.CustomProcessSelectedItem -= ProcessCurrentObjectController_CustomProcessSelectedItem;
            }
            base.OnDeactivated();
        }
    }
}
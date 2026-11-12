using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;

namespace CRMwithAD.Module.BusinessObjects.SubscriptionsManagement
{
    [DomainComponent]
    [DefaultClassOptions]
    public class UserSubscribedPlan : NonPersistentBaseObject
    {
        public String TenantName { get; set; }
        public String PlanName { get; set; }
    }
}
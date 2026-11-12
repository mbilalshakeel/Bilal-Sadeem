using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;

namespace CRMwithAD.Module.BusinessObjects.SubscriptionsManagement
{
    [DomainComponent]
    [DefaultClassOptions]
    public class SubscriptionPlan : NonPersistentBaseObject
    {
        public String CloudName { get; set; }
        public String PlanName { get; set; }
        public String Price { get; set; }
    }
}
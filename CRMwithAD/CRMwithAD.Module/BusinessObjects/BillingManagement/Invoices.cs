using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;

namespace CRMwithAD.Module.BusinessObjects.BillingManagement
{
    [DomainComponent]
    [DefaultClassOptions]
    public class Invoices : NonPersistentBaseObject
    {
        public String TenantName { get; set; }
        public String StartDate { get; set; }
        public String EndDate { get; set; }
        public String Amount { get; set; }
    }
}
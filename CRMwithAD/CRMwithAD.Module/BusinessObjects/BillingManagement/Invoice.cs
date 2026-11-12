using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;

namespace CRMwithAD.Module.BusinessObjects.BillingManagement
{
    [DomainComponent]
    [DefaultClassOptions]
    public class Invoice : NonPersistentBaseObject
    {
        public String InvoiceNumber { get; set; }
        public String TenantName { get; set; }
        //public String InvoiceDate { get; set; }
        public Double Amount { get; set; }
        public String Currency { get; set; }
        public String Status { get; set; }
    }
}
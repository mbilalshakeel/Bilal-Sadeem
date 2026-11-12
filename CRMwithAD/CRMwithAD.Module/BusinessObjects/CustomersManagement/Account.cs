using CRMwithAD.Module.BusinessObjects.SLAsManagement;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;

namespace CRMwithAD.Module.BusinessObjects.CustomerManagement
{
    [DefaultClassOptions]

    public class Account : BaseObject
    {

        [Indexed(Unique = true)]
        private string _Name;
        private SLA _AssignedSLA;

        public Account(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();

        }
        [PersistentAlias("Oid")] // Make sure this refers to the underlying OID property
        [ModelDefault("AllowEdit", "False")] // Set it as read-only in the UI
        public Guid AccountID
        {
            get { return Oid; } // Return the OID value
        }

        public string Name
        {
            get { return _Name; }
            set { SetPropertyValue<string>(nameof(Name), ref _Name, value); }
        }

        public SLA AssignedSLA
        {
            get { return _AssignedSLA; }
            set { SetPropertyValue<SLA>(nameof(AssignedSLA), ref _AssignedSLA, value); }
        }


        [DevExpress.Xpo.Aggregated, Association]
        public XPCollection<Contact> Contacts
        {
            get { return GetCollection<Contact>(nameof(Contacts)); }
        }


    }
}
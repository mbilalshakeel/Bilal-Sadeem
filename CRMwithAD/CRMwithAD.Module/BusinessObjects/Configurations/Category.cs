using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;

namespace CRMwithAD.Module.BusinessObjects.Configurations
{
    [DefaultClassOptions]
    public class Category : BaseObject
    {
        public Category(Session session)
           : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
        }


        [PersistentAlias("Oid")] // Make sure this refers to the underlying OID property
        [ModelDefault("AllowEdit", "False")] // Set it as read-only in the UI
        public Guid SubjectID
        {
            get { return Oid; } // Return the OID value
        }
        private string _Title;
        public string Title
        {
            get { return _Title; }
            set { SetPropertyValue<string>(nameof(Title), ref _Title, value); }
        }

        [DevExpress.Xpo.Aggregated, Association]
        public XPCollection<SubCategory> SubCategories
        {
            get { return GetCollection<SubCategory>(nameof(SubCategories)); }
        }

        [Association("Category-Users")]
        [XafDisplayName("Technicians")]
        public XPCollection<ApplicationUser> Technicians
        {
            get { return GetCollection<ApplicationUser>(nameof(Technicians)); }
        }
    }
}
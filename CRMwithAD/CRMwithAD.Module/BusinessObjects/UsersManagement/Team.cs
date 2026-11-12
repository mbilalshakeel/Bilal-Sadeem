using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;

namespace CRMwithAD.Module.BusinessObjects.UsersManagement
{
    [DefaultClassOptions]
    public class Team : BaseObject
    {
        public Team(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();

        }
        private string _Title;
        public string Title
        {
            get { return _Title; }
            set { SetPropertyValue<string>(nameof(Title), ref _Title, value); }
        }

        //[Association("Team-Users")]
        //public XPCollection<ApplicationUser> Members
        //{
        //    get { return GetCollection<ApplicationUser>(nameof(Members)); }
        //}

    }
}
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;

namespace CRMwithAD.Module.BusinessObjects.CustomerManagement
{
    [DefaultClassOptions]
    public class Contact : BaseObject
    {
        private Account _Accounts;
        [Indexed(Unique = true)]
        private string _Email;

        private string _FirstName;

        private string _LastName;

        private string _Username;

        public Contact(Session session) : base(session)
        { }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
        }


        [PersistentAlias("Oid")] // Make sure this refers to the underlying OID property
        [ModelDefault("AllowEdit", "False")] // Set it as read-only in the UI
        public Guid ContactID
        {
            get { return Oid; } // Return the OID value
        }
        [RuleRequiredField]

        public string Email
        {
            get { return _Email; }
            set { SetPropertyValue<string>(nameof(Email), ref _Email, value); }
        }
        [RuleRequiredField]
        public string FirstName
        {
            get { return _FirstName; }
            set { SetPropertyValue<string>(nameof(FirstName), ref _FirstName, value); }
        }
        [RuleRequiredField]
        public string LastName
        {
            get { return _LastName; }
            set { SetPropertyValue<string>(nameof(LastName), ref _LastName, value); }
        }
        [RuleRequiredField]
        public string Username
        {
            get { return _Username; }
            set { SetPropertyValue<string>(nameof(Username), ref _Username, value); }
        }

        [Association]
        public Account Accounts
        {
            get { return _Accounts; }
            set { SetPropertyValue<Account>(nameof(Accounts), ref _Accounts, value); }
        }
    }
}
using CRMwithAD.Module.Models;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;

namespace CRMwithAD.Module.BusinessObjects.SLAsManagement
{
    [DefaultClassOptions]
    public class SLA : BaseObject
    {
        public SLA(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();

        }
        private SLAType _Title;
        public SLAType Title
        {
            get { return _Title; }
            set { SetPropertyValue<SLAType>(nameof(Title), ref _Title, value); }
        }

        private string _Description;
        public string Description
        {
            get { return _Description; }
            set { SetPropertyValue<string>(nameof(Description), ref _Description, value); }
        }

        private int _FirstResponseTime;
        [RuleValueComparison(DefaultContexts.Save, ValueComparisonType.GreaterThanOrEqual, 1, CustomMessageTemplate = "First Response Time cannot be less than 1.")]
        public int FirstResponseTime
        {
            get { return _FirstResponseTime; }
            set { SetPropertyValue<int>(nameof(FirstResponseTime), ref _FirstResponseTime, value); }
        }
        private int _FailureTime;
        [RuleValueComparison(DefaultContexts.Save, ValueComparisonType.GreaterThanOrEqual, 1, CustomMessageTemplate = "Failure Time cannot be less than 1.")]
        public int FailureTime
        {
            get { return _FailureTime; }
            set { SetPropertyValue<int>(nameof(FailureTime), ref _FailureTime, value); }
        }
        private int _WarningTime;
        [RuleValueComparison(DefaultContexts.Save, ValueComparisonType.GreaterThanOrEqual, 1, CustomMessageTemplate = "Warning Time cannot be less than 1.")]
        public int WarningTime
        {
            get { return _WarningTime; }
            set { SetPropertyValue<int>(nameof(WarningTime), ref _WarningTime, value); }
        }
    }
}
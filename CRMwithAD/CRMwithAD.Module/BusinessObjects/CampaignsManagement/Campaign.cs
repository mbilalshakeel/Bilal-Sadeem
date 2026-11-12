using CRMwithAD.Module.Models;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;

namespace CRMwithAD.Module.BusinessObjects.CampaignsManagement
{
    [DefaultClassOptions]

    [RuleCriteria("EndDateOnOrAfterStartDate", "Save", "EndDate >= StartDate", SkipNullOrEmptyValues = false)]
    [RuleCriteria("ExecuteDateOnOrBeforeStartDate", "Save", "ExecuteDate <= StartDate", SkipNullOrEmptyValues = false)]

    public class Campaign : BaseObject
    {
        public Campaign(Session session)
            : base(session)
        {
        }
        private string name;
        [RuleRequiredField(DefaultContexts.Save)]
        public string Name
        {
            get => name;
            set => SetPropertyValue(nameof(Name), ref name, value);
        }

        private DateTime startDate;

        [RuleRequiredField(DefaultContexts.Save)]
        public DateTime StartDate
        {
            get => startDate;
            set => SetPropertyValue(nameof(StartDate), ref startDate, value);
        }

        private DateTime endDate;
        [RuleRequiredField(DefaultContexts.Save)]
        public DateTime EndDate
        {
            get => endDate;
            set => SetPropertyValue(nameof(EndDate), ref endDate, value);
        }

        private DateTime? executeDate;
        public DateTime? ExecuteDate
        {
            get => executeDate;
            set => SetPropertyValue(nameof(ExecuteDate), ref executeDate, value);
        }

        private string description;
        [RuleRequiredField(DefaultContexts.Save)]
        [Size(SizeAttribute.Unlimited)]
        public string Description
        {
            get => description;
            set => SetPropertyValue(nameof(Description), ref description, value);
        }

        private CampaignStatus _Status;

        [Appearance("StatusDisabled", Context = "Campaign_DetailView", AppearanceItemType = "ViewItem", Criteria = "Status = 'Completed' ", Enabled = false)]
        public CampaignStatus Status
        {
            get => _Status;
            set => SetPropertyValue(nameof(Status), ref _Status, value);
        }

        private FileData _Banner;

        public FileData Banner
        {
            get { return _Banner; }
            set { SetPropertyValue<FileData>(nameof(Banner), ref _Banner, value); }
        }
    }
}
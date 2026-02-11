using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using ComplyEA.Module.BusinessObjects.Regulatory;

namespace ComplyEA.Module.BusinessObjects.Lookups
{
    [DefaultClassOptions]
    [NavigationItem("Administration")]
    [ImageName("TimelineType")]
    public class TimelineType : BaseLookup
    {
        public TimelineType(Session session) : base(session) { }

        int? frequencyDays;
        [ToolTip("Number of days between occurrences, if applicable")]
        public int? FrequencyDays
        {
            get => frequencyDays;
            set => SetPropertyValue(nameof(FrequencyDays), ref frequencyDays, value);
        }

        [Association("TimelineType-Requirements")]
        public XPCollection<ComplianceRequirement> Requirements => GetCollection<ComplianceRequirement>(nameof(Requirements));
    }
}

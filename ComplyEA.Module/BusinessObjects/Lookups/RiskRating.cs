using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using ComplyEA.Module.BusinessObjects.Compliance;
using ComplyEA.Module.BusinessObjects.Regulatory;

namespace ComplyEA.Module.BusinessObjects.Lookups
{
    [DefaultClassOptions]
    [NavigationItem("Administration")]
    [ImageName("RiskRating")]
    public class RiskRating : BaseLookup
    {
        public RiskRating(Session session) : base(session) { }

        int severityLevel;
        [ToolTip("Numeric severity level for sorting and calculations (higher = more severe)")]
        public int SeverityLevel
        {
            get => severityLevel;
            set => SetPropertyValue(nameof(SeverityLevel), ref severityLevel, value);
        }

        [Association("RiskRating-Requirements")]
        public XPCollection<ComplianceRequirement> Requirements => GetCollection<ComplianceRequirement>(nameof(Requirements));

        [Association("RiskRating-Obligations")]
        public XPCollection<ComplianceObligation> Obligations => GetCollection<ComplianceObligation>(nameof(Obligations));
    }
}

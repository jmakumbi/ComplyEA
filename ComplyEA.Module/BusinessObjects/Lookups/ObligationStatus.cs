using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using ComplyEA.Module.BusinessObjects.Compliance;

namespace ComplyEA.Module.BusinessObjects.Lookups
{
    [DefaultClassOptions]
    [NavigationItem("Administration")]
    [ImageName("State_Task_Completed")]
    public class ObligationStatus : BaseLookup
    {
        public ObligationStatus(Session session) : base(session) { }

        bool isTerminal;
        [ToolTip("Indicates if this status represents a final state")]
        public bool IsTerminal
        {
            get => isTerminal;
            set => SetPropertyValue(nameof(IsTerminal), ref isTerminal, value);
        }

        bool requiresAction;
        [ToolTip("Indicates if this status requires user action")]
        public bool RequiresAction
        {
            get => requiresAction;
            set => SetPropertyValue(nameof(RequiresAction), ref requiresAction, value);
        }

        [Association("ObligationStatus-Obligations")]
        public XPCollection<ComplianceObligation> Obligations => GetCollection<ComplianceObligation>(nameof(Obligations));
    }
}

using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using ComplyEA.Module.BusinessObjects.Compliance;

namespace ComplyEA.Module.BusinessObjects.Lookups
{
    [DefaultClassOptions]
    [NavigationItem("Administration")]
    [ImageName("State_Priority_Normal")]
    public class DeliveryStatus : BaseLookup
    {
        public DeliveryStatus(Session session) : base(session) { }

        bool isSuccessful;
        [ToolTip("Indicates if this status represents successful delivery")]
        public bool IsSuccessful
        {
            get => isSuccessful;
            set => SetPropertyValue(nameof(IsSuccessful), ref isSuccessful, value);
        }

        [Association("DeliveryStatus-Reminders")]
        public XPCollection<ComplianceReminder> Reminders => GetCollection<ComplianceReminder>(nameof(Reminders));
    }
}

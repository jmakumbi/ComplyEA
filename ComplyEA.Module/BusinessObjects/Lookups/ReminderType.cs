using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using ComplyEA.Module.BusinessObjects.Compliance;
using ComplyEA.Module.BusinessObjects.Configuration;

namespace ComplyEA.Module.BusinessObjects.Lookups
{
    [DefaultClassOptions]
    [NavigationItem("Administration")]
    [ImageName("BO_Scheduler")]
    public class ReminderType : BaseLookup
    {
        public ReminderType(Session session) : base(session) { }

        int defaultDaysBeforeDue;
        [ToolTip("Default number of days before due date to send this reminder")]
        public int DefaultDaysBeforeDue
        {
            get => defaultDaysBeforeDue;
            set => SetPropertyValue(nameof(DefaultDaysBeforeDue), ref defaultDaysBeforeDue, value);
        }

        bool isEscalation;
        [ToolTip("Indicates if this reminder type triggers escalation")]
        public bool IsEscalation
        {
            get => isEscalation;
            set => SetPropertyValue(nameof(IsEscalation), ref isEscalation, value);
        }

        [Association("ReminderType-Reminders")]
        public XPCollection<ComplianceReminder> Reminders => GetCollection<ComplianceReminder>(nameof(Reminders));

        [Association("ReminderType-CompanySettings")]
        public XPCollection<CompanyReminderSettings> CompanySettings => GetCollection<CompanyReminderSettings>(nameof(CompanySettings));

        [Association("ReminderType-EmailTemplates")]
        public XPCollection<EmailTemplate> EmailTemplates => GetCollection<EmailTemplate>(nameof(EmailTemplates));
    }
}

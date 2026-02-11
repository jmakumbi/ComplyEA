using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using ComplyEA.Module.BusinessObjects.Compliance;
using ComplyEA.Module.BusinessObjects.Configuration;
using ComplyEA.Module.BusinessObjects.Organization;

namespace ComplyEA.Module.BusinessObjects.Lookups
{
    [DefaultClassOptions]
    [NavigationItem("Administration")]
    [ImageName("NotificationChannel")]
    public class NotificationChannel : BaseLookup
    {
        public NotificationChannel(Session session) : base(session) { }

        [Association("NotificationChannel-Contacts")]
        public XPCollection<CompanyContact> Contacts => GetCollection<CompanyContact>(nameof(Contacts));

        [Association("NotificationChannel-Reminders")]
        public XPCollection<ComplianceReminder> Reminders => GetCollection<ComplianceReminder>(nameof(Reminders));

        [Association("NotificationChannel-ReminderSettings")]
        public XPCollection<CompanyReminderSettings> ReminderSettings => GetCollection<CompanyReminderSettings>(nameof(ReminderSettings));
    }
}

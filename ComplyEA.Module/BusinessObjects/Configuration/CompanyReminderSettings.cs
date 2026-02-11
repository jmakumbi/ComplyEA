using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using ComplyEA.Module.BusinessObjects.Lookups;
using ComplyEA.Module.BusinessObjects.Organization;

namespace ComplyEA.Module.BusinessObjects.Configuration
{
    [DefaultClassOptions]
    [NavigationItem("Configuration")]
    [ImageName("CompanyReminderSettings")]
    public class CompanyReminderSettings : BaseObject
    {
        public CompanyReminderSettings(Session session) : base(session) { }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            IsEnabled = true;
            InitialReminderDays = 30;
            FirstReminderDays = 14;
            SecondReminderDays = 7;
            FinalNoticeDays = 3;
        }

        Company company;
        [RuleRequiredField]
        [Association("Company-ReminderSettings")]
        public Company Company
        {
            get => company;
            set => SetPropertyValue(nameof(Company), ref company, value);
        }

        ReminderType reminderType;
        [Association("ReminderType-CompanySettings")]
        [ToolTip("Leave empty for default settings applying to all reminder types")]
        public ReminderType ReminderType
        {
            get => reminderType;
            set => SetPropertyValue(nameof(ReminderType), ref reminderType, value);
        }

        bool isEnabled;
        public bool IsEnabled
        {
            get => isEnabled;
            set => SetPropertyValue(nameof(IsEnabled), ref isEnabled, value);
        }

        int initialReminderDays;
        [ToolTip("Days before due date for initial reminder")]
        public int InitialReminderDays
        {
            get => initialReminderDays;
            set => SetPropertyValue(nameof(InitialReminderDays), ref initialReminderDays, value);
        }

        int firstReminderDays;
        [ToolTip("Days before due date for first follow-up reminder")]
        public int FirstReminderDays
        {
            get => firstReminderDays;
            set => SetPropertyValue(nameof(FirstReminderDays), ref firstReminderDays, value);
        }

        int secondReminderDays;
        [ToolTip("Days before due date for second follow-up reminder")]
        public int SecondReminderDays
        {
            get => secondReminderDays;
            set => SetPropertyValue(nameof(SecondReminderDays), ref secondReminderDays, value);
        }

        int finalNoticeDays;
        [ToolTip("Days before due date for final notice")]
        public int FinalNoticeDays
        {
            get => finalNoticeDays;
            set => SetPropertyValue(nameof(FinalNoticeDays), ref finalNoticeDays, value);
        }

        NotificationChannel defaultChannel;
        [Association("NotificationChannel-ReminderSettings")]
        public NotificationChannel DefaultChannel
        {
            get => defaultChannel;
            set => SetPropertyValue(nameof(DefaultChannel), ref defaultChannel, value);
        }

        bool escalateToManager;
        [ToolTip("Escalate to manager on final notice")]
        public bool EscalateToManager
        {
            get => escalateToManager;
            set => SetPropertyValue(nameof(EscalateToManager), ref escalateToManager, value);
        }

        CompanyContact escalationContact;
        [Association("CompanyContact-EscalationSettings")]
        [ToolTip("Contact to receive escalation notifications")]
        public CompanyContact EscalationContact
        {
            get => escalationContact;
            set => SetPropertyValue(nameof(EscalationContact), ref escalationContact, value);
        }

        public override string ToString()
        {
            return $"{Company?.Name} - {(ReminderType?.Name ?? "Default")}";
        }
    }
}

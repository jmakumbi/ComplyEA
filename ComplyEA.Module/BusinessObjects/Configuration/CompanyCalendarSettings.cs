using System;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using ComplyEA.Module.BusinessObjects.Lookups;
using ComplyEA.Module.BusinessObjects.Organization;
using ComplyEA.Module.BusinessObjects.Security;

namespace ComplyEA.Module.BusinessObjects.Configuration
{
    [DefaultClassOptions]
    [NavigationItem("Configuration")]
    [ImageName("BO_Appointment")]
    public class CompanyCalendarSettings : BaseObject
    {
        public CompanyCalendarSettings(Session session) : base(session) { }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            IsEnabled = true;
            SyncDueDates = true;
            SyncReminders = true;
        }

        Company company;
        [RuleRequiredField]
        [Association("Company-CalendarSettings")]
        public Company Company
        {
            get => company;
            set => SetPropertyValue(nameof(Company), ref company, value);
        }

        CalendarProvider calendarProvider;
        [Association("CalendarProvider-CalendarSettings")]
        public CalendarProvider CalendarProvider
        {
            get => calendarProvider;
            set => SetPropertyValue(nameof(CalendarProvider), ref calendarProvider, value);
        }

        bool isEnabled;
        public bool IsEnabled
        {
            get => isEnabled;
            set => SetPropertyValue(nameof(IsEnabled), ref isEnabled, value);
        }

        bool syncDueDates;
        [ToolTip("Create calendar events for due dates")]
        public bool SyncDueDates
        {
            get => syncDueDates;
            set => SetPropertyValue(nameof(SyncDueDates), ref syncDueDates, value);
        }

        bool syncReminders;
        [ToolTip("Create calendar reminders")]
        public bool SyncReminders
        {
            get => syncReminders;
            set => SetPropertyValue(nameof(SyncReminders), ref syncReminders, value);
        }

        string calendarId;
        [Size(500)]
        [ToolTip("External calendar identifier")]
        public string CalendarId
        {
            get => calendarId;
            set => SetPropertyValue(nameof(CalendarId), ref calendarId, value);
        }

        ApplicationUser connectedUser;
        [Association("ApplicationUser-CalendarSettings")]
        [ToolTip("User whose calendar account is connected")]
        public ApplicationUser ConnectedUser
        {
            get => connectedUser;
            set => SetPropertyValue(nameof(ConnectedUser), ref connectedUser, value);
        }

        DateTime? lastSyncDate;
        public DateTime? LastSyncDate
        {
            get => lastSyncDate;
            set => SetPropertyValue(nameof(LastSyncDate), ref lastSyncDate, value);
        }

        string lastSyncError;
        [Size(SizeAttribute.Unlimited)]
        [VisibleInListView(false)]
        public string LastSyncError
        {
            get => lastSyncError;
            set => SetPropertyValue(nameof(LastSyncError), ref lastSyncError, value);
        }

        public override string ToString()
        {
            return $"{Company?.Name} - {CalendarProvider?.Name}";
        }
    }
}

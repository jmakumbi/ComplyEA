using System;
using System.Linq;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using ComplyEA.Module.BusinessObjects.Compliance;
using ComplyEA.Module.BusinessObjects.Configuration;
using ComplyEA.Module.BusinessObjects.Lookups;
using ComplyEA.Module.BusinessObjects.Organization;

namespace ComplyEA.Module.Services
{
    public class ReminderGenerationService : IReminderGenerationService
    {
        // Reminder type codes
        private const string REMINDER_INITIAL = "INITIAL";
        private const string REMINDER_FIRST = "FIRST";
        private const string REMINDER_SECOND = "SECOND";
        private const string REMINDER_FINAL = "FINAL";
        private const string REMINDER_ESCALATION = "ESCALATION";

        // Delivery status code
        private const string STATUS_PENDING = "PENDING";

        // Notification channel code
        private const string CHANNEL_EMAIL = "EMAIL";

        // Status codes for non-terminal obligations
        private const string OBLIGATION_PENDING = "PENDING";
        private const string OBLIGATION_INPROGRESS = "INPROGRESS";

        public int GenerateRemindersForObligation(IObjectSpace os, ComplianceObligation obligation)
        {
            if (obligation == null || !obligation.DueDate.HasValue)
                return 0;

            // Don't generate reminders for completed/waived obligations
            if (obligation.Status?.IsTerminal == true)
                return 0;

            var company = obligation.Company;
            var settings = GetEffectiveReminderSettings(os, company);
            var dueDate = obligation.DueDate.Value;

            int count = 0;

            // Get reminder types
            var reminderTypes = os.GetObjects<ReminderType>(
                new BinaryOperator("IsActive", true))
                .OrderBy(r => r.SortOrder)
                .ToList();

            // Get notification channel (default to email)
            var defaultChannel = settings?.DefaultChannel ??
                os.FindObject<NotificationChannel>(new BinaryOperator("Code", CHANNEL_EMAIL));

            // Get pending delivery status
            var pendingStatus = os.FindObject<DeliveryStatus>(new BinaryOperator("Code", STATUS_PENDING));

            // Determine recipient
            var recipient = obligation.AssignedTo;
            string recipientEmail = recipient?.Email ?? company?.Email;

            foreach (var reminderType in reminderTypes)
            {
                // Skip escalation if not enabled for this company
                if (reminderType.IsEscalation && settings?.EscalateToManager != true)
                    continue;

                // Check if reminder already exists
                if (ReminderExists(os, obligation, reminderType.Code))
                    continue;

                // Calculate days before due based on settings or type default
                int daysBeforeDue = GetDaysBeforeDue(settings, reminderType);
                if (daysBeforeDue <= 0)
                    continue;

                var scheduledDate = dueDate.AddDays(-daysBeforeDue);

                // Don't schedule reminders in the past
                if (scheduledDate < DateTime.Today)
                    continue;

                var reminder = os.CreateObject<ComplianceReminder>();
                reminder.ComplianceObligation = os.GetObject(obligation);
                reminder.ReminderType = reminderType;
                reminder.ScheduledDate = scheduledDate;
                reminder.DeliveryStatus = pendingStatus;
                reminder.NotificationChannel = defaultChannel;

                // Set recipient
                if (reminderType.IsEscalation && settings?.EscalationContact != null)
                {
                    reminder.Recipient = os.GetObject(settings.EscalationContact);
                    reminder.RecipientEmail = settings.EscalationContact.Email;
                }
                else
                {
                    if (recipient != null)
                    {
                        reminder.Recipient = os.GetObject(recipient);
                    }
                    reminder.RecipientEmail = recipientEmail;
                }

                count++;
            }

            return count;
        }

        public int GenerateRemindersForPendingObligations(IObjectSpace os, Company company = null)
        {
            // Find obligations that are pending or in progress and have a due date
            var criteria = CriteriaOperator.And(
                new NotOperator(new NullOperator("DueDate")),
                CriteriaOperator.Or(
                    new BinaryOperator("Status.Code", OBLIGATION_PENDING),
                    new BinaryOperator("Status.Code", OBLIGATION_INPROGRESS)
                )
            );

            if (company != null)
            {
                criteria = CriteriaOperator.And(criteria,
                    new BinaryOperator("Company.Oid", company.Oid));
            }

            var obligations = os.GetObjects<ComplianceObligation>(criteria);

            int totalCount = 0;
            foreach (var obligation in obligations)
            {
                totalCount += GenerateRemindersForObligation(os, obligation);
            }

            return totalCount;
        }

        public CompanyReminderSettings GetEffectiveReminderSettings(IObjectSpace os, Company company)
        {
            if (company == null)
                return null;

            // First try to find company-specific default settings (no specific reminder type)
            var settings = os.FindObject<CompanyReminderSettings>(
                CriteriaOperator.And(
                    new BinaryOperator("Company.Oid", company.Oid),
                    new NullOperator("ReminderType"),
                    new BinaryOperator("IsEnabled", true)
                ));

            if (settings != null)
                return settings;

            // If no settings found, return null - callers should use ReminderType.DefaultDaysBeforeDue
            return null;
        }

        public int RegenerateReminders(IObjectSpace os, ComplianceObligation obligation)
        {
            if (obligation == null)
                return 0;

            // Delete existing unsent reminders
            var existingReminders = os.GetObjects<ComplianceReminder>(
                CriteriaOperator.And(
                    new BinaryOperator("ComplianceObligation.Oid", obligation.Oid),
                    new NullOperator("SentDate")
                )).ToList();

            foreach (var reminder in existingReminders)
            {
                os.Delete(reminder);
            }

            // Generate new reminders
            return GenerateRemindersForObligation(os, obligation);
        }

        public bool ReminderExists(IObjectSpace os, ComplianceObligation obligation, string reminderTypeCode)
        {
            var existing = os.FindObject<ComplianceReminder>(
                CriteriaOperator.And(
                    new BinaryOperator("ComplianceObligation.Oid", obligation.Oid),
                    new BinaryOperator("ReminderType.Code", reminderTypeCode)
                ));

            return existing != null;
        }

        private int GetDaysBeforeDue(CompanyReminderSettings settings, ReminderType reminderType)
        {
            if (settings == null)
                return reminderType.DefaultDaysBeforeDue;

            // Map reminder type code to settings property
            switch (reminderType.Code)
            {
                case REMINDER_INITIAL:
                    return settings.InitialReminderDays;
                case REMINDER_FIRST:
                    return settings.FirstReminderDays;
                case REMINDER_SECOND:
                    return settings.SecondReminderDays;
                case REMINDER_FINAL:
                    return settings.FinalNoticeDays;
                case REMINDER_ESCALATION:
                    // Escalation uses type default (typically 1 day)
                    return reminderType.DefaultDaysBeforeDue;
                default:
                    return reminderType.DefaultDaysBeforeDue;
            }
        }
    }
}

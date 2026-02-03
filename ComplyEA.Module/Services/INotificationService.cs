using System.Threading.Tasks;
using DevExpress.ExpressApp;
using ComplyEA.Module.BusinessObjects.Compliance;

namespace ComplyEA.Module.Services
{
    /// <summary>
    /// Result of notification processing.
    /// </summary>
    public class NotificationResult
    {
        public int Processed { get; set; }
        public int Sent { get; set; }
        public int Failed { get; set; }
    }

    /// <summary>
    /// Service for processing and sending compliance reminders.
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Processes all due reminders and sends notifications.
        /// </summary>
        Task<NotificationResult> ProcessDueRemindersAsync(IObjectSpace os);

        /// <summary>
        /// Sends a specific reminder notification.
        /// </summary>
        Task<bool> SendReminderAsync(IObjectSpace os, ComplianceReminder reminder);

        /// <summary>
        /// Retries sending failed reminders up to maxRetries times.
        /// </summary>
        Task<int> RetryFailedRemindersAsync(IObjectSpace os, int maxRetries = 3);

        /// <summary>
        /// Processes email/SMS template placeholders with reminder data.
        /// Supported placeholders: {{CompanyName}}, {{RequirementTitle}}, {{DueDate}},
        /// {{DaysUntilDue}}, {{RecipientName}}, {{ObligationStatus}}, {{RegulatoryAct}},
        /// {{PeriodYear}}, {{PeriodQuarter}}, {{PeriodMonth}}, {{ActionUrl}}
        /// </summary>
        string ProcessTemplate(string template, ComplianceReminder reminder);

        /// <summary>
        /// Populates reminder message subject and body from email template.
        /// </summary>
        void PopulateReminderMessage(IObjectSpace os, ComplianceReminder reminder);
    }
}

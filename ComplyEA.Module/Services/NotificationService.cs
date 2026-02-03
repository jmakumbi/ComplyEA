using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using ComplyEA.Module.BusinessObjects.Compliance;
using ComplyEA.Module.BusinessObjects.Configuration;
using ComplyEA.Module.BusinessObjects.Lookups;

namespace ComplyEA.Module.Services
{
    public class NotificationService : INotificationService
    {
        // Delivery status codes
        private const string STATUS_PENDING = "PENDING";
        private const string STATUS_SENT = "SENT";
        private const string STATUS_FAILED = "FAILED";

        // Notification channel codes
        private const string CHANNEL_EMAIL = "EMAIL";
        private const string CHANNEL_SMS = "SMS";

        // Placeholder pattern: {{PlaceholderName}}
        private static readonly Regex PlaceholderPattern = new Regex(@"\{\{(\w+)\}\}", RegexOptions.Compiled);

        private readonly IEmailService _emailService;

        public NotificationService(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task<NotificationResult> ProcessDueRemindersAsync(IObjectSpace os)
        {
            var result = new NotificationResult();

            // Find all due reminders that are pending
            var pendingStatus = os.FindObject<DeliveryStatus>(new BinaryOperator("Code", STATUS_PENDING));
            if (pendingStatus == null)
                return result;

            var dueReminders = os.GetObjects<ComplianceReminder>(
                CriteriaOperator.And(
                    new BinaryOperator("DeliveryStatus.Code", STATUS_PENDING),
                    new BinaryOperator("ScheduledDate", DateTime.Today, BinaryOperatorType.LessOrEqual),
                    new NullOperator("SentDate")
                )).ToList();

            result.Processed = dueReminders.Count;

            foreach (var reminder in dueReminders)
            {
                var success = await SendReminderAsync(os, reminder);
                if (success)
                {
                    result.Sent++;
                }
                else
                {
                    result.Failed++;
                }
            }

            return result;
        }

        public async Task<bool> SendReminderAsync(IObjectSpace os, ComplianceReminder reminder)
        {
            if (reminder == null)
                return false;

            try
            {
                // Populate message if not already populated
                if (string.IsNullOrEmpty(reminder.MessageSubject) || string.IsNullOrEmpty(reminder.MessageBody))
                {
                    PopulateReminderMessage(os, reminder);
                }

                var channelCode = reminder.NotificationChannel?.Code ?? CHANNEL_EMAIL;

                bool success = false;

                if (channelCode == CHANNEL_EMAIL || channelCode == "BOTH")
                {
                    if (!string.IsNullOrEmpty(reminder.RecipientEmail))
                    {
                        var emailResult = await _emailService.SendEmailAsync(
                            reminder.RecipientEmail,
                            reminder.MessageSubject,
                            reminder.MessageBody,
                            null);

                        success = emailResult.Success;
                        if (!success)
                        {
                            reminder.ErrorMessage = emailResult.ErrorMessage;
                        }
                    }
                    else
                    {
                        reminder.ErrorMessage = "No recipient email address configured.";
                    }
                }

                // Update reminder status
                if (success)
                {
                    var sentStatus = os.FindObject<DeliveryStatus>(new BinaryOperator("Code", STATUS_SENT));
                    reminder.DeliveryStatus = sentStatus;
                    reminder.SentDate = DateTime.Now;
                    reminder.ErrorMessage = null;
                }
                else
                {
                    var failedStatus = os.FindObject<DeliveryStatus>(new BinaryOperator("Code", STATUS_FAILED));
                    reminder.DeliveryStatus = failedStatus;
                    reminder.RetryCount++;
                }

                return success;
            }
            catch (Exception ex)
            {
                var failedStatus = os.FindObject<DeliveryStatus>(new BinaryOperator("Code", STATUS_FAILED));
                reminder.DeliveryStatus = failedStatus;
                reminder.ErrorMessage = ex.Message;
                reminder.RetryCount++;
                return false;
            }
        }

        public async Task<int> RetryFailedRemindersAsync(IObjectSpace os, int maxRetries = 3)
        {
            // Find failed reminders that haven't exceeded retry limit
            var failedReminders = os.GetObjects<ComplianceReminder>(
                CriteriaOperator.And(
                    new BinaryOperator("DeliveryStatus.Code", STATUS_FAILED),
                    new BinaryOperator("RetryCount", maxRetries, BinaryOperatorType.Less)
                )).ToList();

            int successCount = 0;

            // Reset status to pending for retry
            var pendingStatus = os.FindObject<DeliveryStatus>(new BinaryOperator("Code", STATUS_PENDING));

            foreach (var reminder in failedReminders)
            {
                reminder.DeliveryStatus = pendingStatus;
                var success = await SendReminderAsync(os, reminder);
                if (success)
                {
                    successCount++;
                }
            }

            return successCount;
        }

        public string ProcessTemplate(string template, ComplianceReminder reminder)
        {
            if (string.IsNullOrEmpty(template) || reminder == null)
                return template;

            var obligation = reminder.ComplianceObligation;
            var company = obligation?.Company;
            var requirement = obligation?.ComplianceRequirement;
            var recipient = reminder.Recipient;

            return PlaceholderPattern.Replace(template, match =>
            {
                var placeholder = match.Groups[1].Value;

                switch (placeholder)
                {
                    case "CompanyName":
                        return company?.Name ?? "";
                    case "CompanyShortName":
                        return company?.ShortName ?? company?.Name ?? "";
                    case "RequirementTitle":
                        return requirement?.Title ?? obligation?.Title ?? "";
                    case "ObligationTitle":
                        return obligation?.Title ?? "";
                    case "DueDate":
                        return obligation?.DueDate?.ToString("MMMM dd, yyyy") ?? "";
                    case "DueDateShort":
                        return obligation?.DueDate?.ToString("yyyy-MM-dd") ?? "";
                    case "DaysUntilDue":
                        if (obligation?.DueDate.HasValue == true)
                        {
                            var days = (obligation.DueDate.Value - DateTime.Today).Days;
                            return days.ToString();
                        }
                        return "";
                    case "RecipientName":
                        if (recipient != null)
                            return $"{recipient.FirstName} {recipient.LastName}".Trim();
                        return "";
                    case "RecipientFirstName":
                        return recipient?.FirstName ?? "";
                    case "ObligationStatus":
                        return obligation?.Status?.Name ?? "";
                    case "RegulatoryAct":
                        return requirement?.RegulatoryAct?.ShortName ?? requirement?.RegulatoryAct?.Name ?? "";
                    case "RegulatoryActFull":
                        return requirement?.RegulatoryAct?.Name ?? "";
                    case "PeriodYear":
                        return obligation?.PeriodYear?.ToString() ?? "";
                    case "PeriodQuarter":
                        return obligation?.PeriodQuarter.HasValue == true ? $"Q{obligation.PeriodQuarter}" : "";
                    case "PeriodMonth":
                        return obligation?.PeriodMonth.HasValue == true
                            ? new DateTime(2000, obligation.PeriodMonth.Value, 1).ToString("MMMM")
                            : "";
                    case "SectionReference":
                        return requirement?.SectionReference ?? "";
                    case "RiskRating":
                        return obligation?.RiskRating?.Name ?? "";
                    case "PenaltyAmount":
                        return requirement?.PenaltyAmount?.ToString("N0") ?? "";
                    case "ReminderType":
                        return reminder.ReminderType?.Name ?? "";
                    case "ScheduledDate":
                        return reminder.ScheduledDate.ToString("MMMM dd, yyyy");
                    case "CurrentDate":
                        return DateTime.Today.ToString("MMMM dd, yyyy");
                    case "ActionUrl":
                        // Placeholder for application URL - could be configured in system settings
                        return "[Application Link]";
                    default:
                        return match.Value; // Keep original if unknown
                }
            });
        }

        public void PopulateReminderMessage(IObjectSpace os, ComplianceReminder reminder)
        {
            if (reminder == null)
                return;

            // Find email template for this reminder type
            var template = os.FindObject<EmailTemplate>(
                CriteriaOperator.And(
                    new BinaryOperator("ReminderType.Code", reminder.ReminderType?.Code),
                    new BinaryOperator("IsActive", true),
                    new BinaryOperator("IsDefault", true)
                ));

            // Fall back to any active template for this type
            if (template == null)
            {
                template = os.FindObject<EmailTemplate>(
                    CriteriaOperator.And(
                        new BinaryOperator("ReminderType.Code", reminder.ReminderType?.Code),
                        new BinaryOperator("IsActive", true)
                    ));
            }

            // Fall back to default template
            if (template == null)
            {
                template = os.FindObject<EmailTemplate>(
                    CriteriaOperator.And(
                        new BinaryOperator("Code", "DEFAULT"),
                        new BinaryOperator("IsActive", true)
                    ));
            }

            if (template != null)
            {
                reminder.MessageSubject = ProcessTemplate(template.Subject, reminder);
                reminder.MessageBody = ProcessTemplate(template.BodyHtml, reminder);
            }
            else
            {
                // Generate default message if no template found
                reminder.MessageSubject = GenerateDefaultSubject(reminder);
                reminder.MessageBody = GenerateDefaultBody(reminder);
            }
        }

        private string GenerateDefaultSubject(ComplianceReminder reminder)
        {
            var obligation = reminder.ComplianceObligation;
            var reminderType = reminder.ReminderType?.Name ?? "Reminder";

            return $"[{reminderType}] Compliance Obligation Due: {obligation?.Title ?? "Unknown"}";
        }

        private string GenerateDefaultBody(ComplianceReminder reminder)
        {
            var obligation = reminder.ComplianceObligation;
            var company = obligation?.Company;
            var requirement = obligation?.ComplianceRequirement;

            return $@"
<html>
<body>
<h2>Compliance Reminder</h2>

<p>Dear {reminder.Recipient?.FirstName ?? "Compliance Officer"},</p>

<p>This is a reminder that the following compliance obligation is due:</p>

<table style='border-collapse: collapse; margin: 20px 0;'>
<tr><td style='padding: 8px; border: 1px solid #ddd; background: #f5f5f5;'><strong>Company:</strong></td>
<td style='padding: 8px; border: 1px solid #ddd;'>{company?.Name ?? "N/A"}</td></tr>
<tr><td style='padding: 8px; border: 1px solid #ddd; background: #f5f5f5;'><strong>Requirement:</strong></td>
<td style='padding: 8px; border: 1px solid #ddd;'>{obligation?.Title ?? "N/A"}</td></tr>
<tr><td style='padding: 8px; border: 1px solid #ddd; background: #f5f5f5;'><strong>Regulatory Act:</strong></td>
<td style='padding: 8px; border: 1px solid #ddd;'>{requirement?.RegulatoryAct?.Name ?? "N/A"}</td></tr>
<tr><td style='padding: 8px; border: 1px solid #ddd; background: #f5f5f5;'><strong>Due Date:</strong></td>
<td style='padding: 8px; border: 1px solid #ddd;'>{obligation?.DueDate?.ToString("MMMM dd, yyyy") ?? "N/A"}</td></tr>
<tr><td style='padding: 8px; border: 1px solid #ddd; background: #f5f5f5;'><strong>Status:</strong></td>
<td style='padding: 8px; border: 1px solid #ddd;'>{obligation?.Status?.Name ?? "N/A"}</td></tr>
</table>

<p>Please ensure this obligation is addressed before the due date to maintain compliance.</p>

<p>Best regards,<br/>ComplyEA Notification System</p>
</body>
</html>";
        }
    }
}

using System;
using System.Threading.Tasks;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using ComplyEA.Module.BusinessObjects.Compliance;
using ComplyEA.Module.BusinessObjects.Lookups;
using ComplyEA.Module.Services;

namespace ComplyEA.Module.Controllers.Compliance
{
    /// <summary>
    /// Controller for sending notifications from ComplianceReminder list view.
    /// </summary>
    public class NotificationController : ObjectViewController<ListView, ComplianceReminder>
    {
        private SimpleAction sendDueRemindersAction;
        private SimpleAction resendReminderAction;
        private SimpleAction retryFailedAction;

        private const string STATUS_PENDING = "PENDING";
        private const string STATUS_FAILED = "FAILED";

        public NotificationController()
        {
            // Send all due reminders
            sendDueRemindersAction = new SimpleAction(this, "SendDueReminders", PredefinedCategory.RecordEdit)
            {
                Caption = "Send Due Reminders",
                ToolTip = "Process and send all reminders that are due today or earlier",
                ImageName = "NotificationChannel",
                SelectionDependencyType = SelectionDependencyType.Independent
            };
            sendDueRemindersAction.Execute += SendDueReminders_Execute;

            // Resend specific reminder
            resendReminderAction = new SimpleAction(this, "ResendReminder", PredefinedCategory.RecordEdit)
            {
                Caption = "Resend",
                ToolTip = "Resend the selected reminder(s)",
                ImageName = "Action_Refresh",
                SelectionDependencyType = SelectionDependencyType.RequireMultipleObjects
            };
            resendReminderAction.Execute += ResendReminder_Execute;

            // Retry all failed reminders
            retryFailedAction = new SimpleAction(this, "RetryFailedReminders", PredefinedCategory.RecordEdit)
            {
                Caption = "Retry Failed",
                ToolTip = "Retry sending all failed reminders",
                ImageName = "Action_Refresh",
                SelectionDependencyType = SelectionDependencyType.Independent
            };
            retryFailedAction.Execute += RetryFailed_Execute;
        }

        private async void SendDueReminders_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var emailService = new SmtpEmailService(ObjectSpace);
            var notificationService = new NotificationService(emailService);

            try
            {
                var result = await notificationService.ProcessDueRemindersAsync(ObjectSpace);
                ObjectSpace.CommitChanges();
                View.ObjectSpace.Refresh();

                var message = $"Processed {result.Processed} reminder(s): {result.Sent} sent, {result.Failed} failed.";
                Application.ShowViewStrategy.ShowMessage(message);
            }
            catch (Exception ex)
            {
                Application.ShowViewStrategy.ShowMessage($"Error processing reminders: {ex.Message}");
            }
        }

        private async void ResendReminder_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var emailService = new SmtpEmailService(ObjectSpace);
            var notificationService = new NotificationService(emailService);

            int sentCount = 0;
            int failedCount = 0;

            // Reset selected reminders to pending and send
            var pendingStatus = ObjectSpace.FindObject<DeliveryStatus>(
                new BinaryOperator("Code", STATUS_PENDING));

            foreach (var obj in e.SelectedObjects)
            {
                var reminder = obj as ComplianceReminder;
                if (reminder != null)
                {
                    // Reset status to allow resend
                    reminder.DeliveryStatus = pendingStatus;
                    reminder.SentDate = null;
                    reminder.ErrorMessage = null;

                    try
                    {
                        var success = await notificationService.SendReminderAsync(ObjectSpace, reminder);
                        if (success)
                            sentCount++;
                        else
                            failedCount++;
                    }
                    catch
                    {
                        failedCount++;
                    }
                }
            }

            ObjectSpace.CommitChanges();
            View.ObjectSpace.Refresh();

            Application.ShowViewStrategy.ShowMessage($"Resent {sentCount} reminder(s), {failedCount} failed.");
        }

        private async void RetryFailed_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var emailService = new SmtpEmailService(ObjectSpace);
            var notificationService = new NotificationService(emailService);

            try
            {
                var successCount = await notificationService.RetryFailedRemindersAsync(ObjectSpace, 3);
                ObjectSpace.CommitChanges();
                View.ObjectSpace.Refresh();

                Application.ShowViewStrategy.ShowMessage($"Retried failed reminders: {successCount} sent successfully.");
            }
            catch (Exception ex)
            {
                Application.ShowViewStrategy.ShowMessage($"Error retrying reminders: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Controller for notification actions on ComplianceReminder detail view.
    /// </summary>
    public class NotificationDetailController : ObjectViewController<DetailView, ComplianceReminder>
    {
        private SimpleAction sendReminderAction;
        private SimpleAction previewMessageAction;

        private const string STATUS_PENDING = "PENDING";

        public NotificationDetailController()
        {
            // Send this reminder
            sendReminderAction = new SimpleAction(this, "SendReminderNow", PredefinedCategory.RecordEdit)
            {
                Caption = "Send Now",
                ToolTip = "Send this reminder immediately",
                ImageName = "NotificationChannel"
            };
            sendReminderAction.Execute += SendReminder_Execute;

            // Preview message
            previewMessageAction = new SimpleAction(this, "PreviewMessage", PredefinedCategory.RecordEdit)
            {
                Caption = "Preview Message",
                ToolTip = "Generate and preview the message that will be sent",
                ImageName = "Action_Search"
            };
            previewMessageAction.Execute += PreviewMessage_Execute;
        }

        private async void SendReminder_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var reminder = ViewCurrentObject;
            if (reminder == null)
                return;

            var emailService = new SmtpEmailService(ObjectSpace);
            var notificationService = new NotificationService(emailService);

            // Reset to pending if already sent/failed
            var pendingStatus = ObjectSpace.FindObject<DeliveryStatus>(
                new BinaryOperator("Code", STATUS_PENDING));
            reminder.DeliveryStatus = pendingStatus;
            reminder.SentDate = null;
            reminder.ErrorMessage = null;

            try
            {
                var success = await notificationService.SendReminderAsync(ObjectSpace, reminder);
                ObjectSpace.CommitChanges();
                View.ObjectSpace.Refresh();

                if (success)
                {
                    Application.ShowViewStrategy.ShowMessage("Reminder sent successfully.");
                }
                else
                {
                    Application.ShowViewStrategy.ShowMessage($"Failed to send reminder: {reminder.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                Application.ShowViewStrategy.ShowMessage($"Error sending reminder: {ex.Message}");
            }
        }

        private void PreviewMessage_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var reminder = ViewCurrentObject;
            if (reminder == null)
                return;

            var emailService = new SmtpEmailService(ObjectSpace);
            var notificationService = new NotificationService(emailService);

            // Generate message content
            notificationService.PopulateReminderMessage(ObjectSpace, reminder);

            // Refresh view to show populated fields
            View.ObjectSpace.Refresh();

            Application.ShowViewStrategy.ShowMessage("Message preview generated. Check MessageSubject and MessageBody fields.");
        }
    }
}

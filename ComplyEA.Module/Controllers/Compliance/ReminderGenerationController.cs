using System;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using ComplyEA.Module.BusinessObjects.Compliance;
using ComplyEA.Module.Services;

namespace ComplyEA.Module.Controllers.Compliance
{
    /// <summary>
    /// Controller for generating reminders from ComplianceObligation list view.
    /// </summary>
    public class ReminderGenerationController : ObjectViewController<ListView, ComplianceObligation>
    {
        private SimpleAction generateRemindersAction;
        private SimpleAction regenerateRemindersAction;
        private SimpleAction generateAllRemindersAction;

        public ReminderGenerationController()
        {
            // Generate reminders for selected obligations
            generateRemindersAction = new SimpleAction(this, "GenerateReminders", PredefinedCategory.RecordEdit)
            {
                Caption = "Generate Reminders",
                ToolTip = "Generate reminder schedule for selected obligation(s)",
                ImageName = "ComplianceReminder",
                SelectionDependencyType = SelectionDependencyType.RequireMultipleObjects
            };
            generateRemindersAction.Execute += GenerateReminders_Execute;

            // Regenerate reminders (deletes unsent and creates new)
            regenerateRemindersAction = new SimpleAction(this, "RegenerateReminders", PredefinedCategory.RecordEdit)
            {
                Caption = "Regenerate Reminders",
                ToolTip = "Delete unsent reminders and regenerate schedule (use when due date changes)",
                ImageName = "Action_Refresh",
                SelectionDependencyType = SelectionDependencyType.RequireMultipleObjects,
                ConfirmationMessage = "This will delete all unsent reminders and create new ones. Continue?"
            };
            regenerateRemindersAction.Execute += RegenerateReminders_Execute;

            // Generate reminders for all pending obligations
            generateAllRemindersAction = new SimpleAction(this, "GenerateAllReminders", PredefinedCategory.RecordEdit)
            {
                Caption = "Generate All Reminders",
                ToolTip = "Generate reminders for all pending obligations",
                ImageName = "ComplianceReminder",
                SelectionDependencyType = SelectionDependencyType.Independent
            };
            generateAllRemindersAction.Execute += GenerateAllReminders_Execute;
        }

        private void GenerateReminders_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var service = new ReminderGenerationService();
            int totalCount = 0;

            foreach (var obj in e.SelectedObjects)
            {
                var obligation = obj as ComplianceObligation;
                if (obligation != null)
                {
                    totalCount += service.GenerateRemindersForObligation(ObjectSpace, obligation);
                }
            }

            if (totalCount > 0)
            {
                ObjectSpace.CommitChanges();
            }

            var message = totalCount > 0
                ? $"Generated {totalCount} reminder(s) for the selected obligation(s)."
                : "No new reminders were generated. Reminders may already exist or obligations are complete.";

            Application.ShowViewStrategy.ShowMessage(message);
        }

        private void RegenerateReminders_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var service = new ReminderGenerationService();
            int totalCount = 0;

            foreach (var obj in e.SelectedObjects)
            {
                var obligation = obj as ComplianceObligation;
                if (obligation != null)
                {
                    totalCount += service.RegenerateReminders(ObjectSpace, obligation);
                }
            }

            ObjectSpace.CommitChanges();

            var message = totalCount > 0
                ? $"Regenerated {totalCount} reminder(s) for the selected obligation(s)."
                : "No reminders were generated. Obligations may be complete or have no future due dates.";

            Application.ShowViewStrategy.ShowMessage(message);
        }

        private void GenerateAllReminders_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var service = new ReminderGenerationService();
            int totalCount = service.GenerateRemindersForPendingObligations(ObjectSpace, null);

            if (totalCount > 0)
            {
                ObjectSpace.CommitChanges();
            }

            var message = totalCount > 0
                ? $"Generated {totalCount} reminder(s) for all pending obligations."
                : "No new reminders were generated.";

            Application.ShowViewStrategy.ShowMessage(message);
        }
    }

    /// <summary>
    /// Controller for generating reminders from ComplianceObligation detail view.
    /// </summary>
    public class ReminderGenerationDetailController : ObjectViewController<DetailView, ComplianceObligation>
    {
        private SimpleAction generateRemindersAction;
        private SimpleAction regenerateRemindersAction;

        public ReminderGenerationDetailController()
        {
            generateRemindersAction = new SimpleAction(this, "GenerateRemindersDetail", PredefinedCategory.RecordEdit)
            {
                Caption = "Generate Reminders",
                ToolTip = "Generate reminder schedule for this obligation",
                ImageName = "ComplianceReminder"
            };
            generateRemindersAction.Execute += GenerateReminders_Execute;

            regenerateRemindersAction = new SimpleAction(this, "RegenerateRemindersDetail", PredefinedCategory.RecordEdit)
            {
                Caption = "Regenerate Reminders",
                ToolTip = "Delete unsent reminders and regenerate schedule",
                ImageName = "Action_Refresh",
                ConfirmationMessage = "This will delete all unsent reminders and create new ones. Continue?"
            };
            regenerateRemindersAction.Execute += RegenerateReminders_Execute;
        }

        private void GenerateReminders_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var service = new ReminderGenerationService();
            var obligation = ViewCurrentObject;

            if (obligation == null)
                return;

            int count = service.GenerateRemindersForObligation(ObjectSpace, obligation);

            if (count > 0)
            {
                ObjectSpace.CommitChanges();
                View.ObjectSpace.Refresh();
            }

            var message = count > 0
                ? $"Generated {count} reminder(s)."
                : "No new reminders were generated. Reminders may already exist or obligation is complete.";

            Application.ShowViewStrategy.ShowMessage(message);
        }

        private void RegenerateReminders_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var service = new ReminderGenerationService();
            var obligation = ViewCurrentObject;

            if (obligation == null)
                return;

            int count = service.RegenerateReminders(ObjectSpace, obligation);
            ObjectSpace.CommitChanges();
            View.ObjectSpace.Refresh();

            var message = count > 0
                ? $"Regenerated {count} reminder(s)."
                : "No reminders were generated.";

            Application.ShowViewStrategy.ShowMessage(message);
        }
    }
}

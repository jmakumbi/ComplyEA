using System;
using System.Collections.Generic;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using ComplyEA.Module.BusinessObjects.Compliance;
using ComplyEA.Module.BusinessObjects.Lookups;

namespace ComplyEA.Module.Controllers.Compliance
{
    public class ObligationStatusController : ObjectViewController<ListView, ComplianceObligation>
    {
        private SingleChoiceAction changeStatusAction;
        private SimpleAction markCompleteAction;
        private SimpleAction markOverdueAction;

        // Status codes
        private const string STATUS_PENDING = "PENDING";
        private const string STATUS_INPROGRESS = "INPROGRESS";
        private const string STATUS_SUBMITTED = "SUBMITTED";
        private const string STATUS_COMPLETED = "COMPLETED";
        private const string STATUS_OVERDUE = "OVERDUE";
        private const string STATUS_WAIVED = "WAIVED";

        public ObligationStatusController()
        {
            // Single choice action for status dropdown
            changeStatusAction = new SingleChoiceAction(this, "ChangeObligationStatus", PredefinedCategory.RecordEdit)
            {
                Caption = "Change Status",
                ToolTip = "Change the status of selected obligation(s)",
                ImageName = "State_Task_Completed",
                SelectionDependencyType = SelectionDependencyType.RequireMultipleObjects,
                ItemType = SingleChoiceActionItemType.ItemIsOperation
            };
            changeStatusAction.Execute += ChangeStatus_Execute;

            // Mark Complete action
            markCompleteAction = new SimpleAction(this, "MarkObligationComplete", PredefinedCategory.RecordEdit)
            {
                Caption = "Mark Complete",
                ToolTip = "Mark selected obligation(s) as completed",
                ImageName = "State_Task_Completed",
                SelectionDependencyType = SelectionDependencyType.RequireMultipleObjects,
                ConfirmationMessage = "Are you sure you want to mark the selected obligation(s) as complete?"
            };
            markCompleteAction.Execute += MarkComplete_Execute;

            // Mark Overdue action
            markOverdueAction = new SimpleAction(this, "MarkObligationsOverdue", PredefinedCategory.RecordEdit)
            {
                Caption = "Mark Overdue",
                ToolTip = "Mark past-due obligations as overdue",
                ImageName = "State_Task_WaitingForSomeoneElse",
                SelectionDependencyType = SelectionDependencyType.Independent
            };
            markOverdueAction.Execute += MarkOverdue_Execute;
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            PopulateStatusChoices();
        }

        private void PopulateStatusChoices()
        {
            changeStatusAction.Items.Clear();

            var statuses = ObjectSpace.GetObjects<ObligationStatus>(
                new BinaryOperator("IsActive", true));

            foreach (var status in statuses)
            {
                var item = new ChoiceActionItem(status.Code, status.Name, status);
                changeStatusAction.Items.Add(item);
            }
        }

        private void ChangeStatus_Execute(object sender, SingleChoiceActionExecuteEventArgs e)
        {
            var newStatus = e.SelectedChoiceActionItem.Data as ObligationStatus;
            if (newStatus == null)
                return;

            int count = 0;
            foreach (var obj in e.SelectedObjects)
            {
                var obligation = obj as ComplianceObligation;
                if (obligation != null)
                {
                    obligation.Status = ObjectSpace.GetObject(newStatus);

                    // If status is terminal, set completed date
                    if (newStatus.IsTerminal && !obligation.CompletedDate.HasValue)
                    {
                        obligation.CompletedDate = DateTime.Now;
                    }

                    count++;
                }
            }

            if (count > 0)
            {
                ObjectSpace.CommitChanges();
                View.ObjectSpace.Refresh();
            }

            Application.ShowViewStrategy.ShowMessage($"Updated status for {count} obligation(s) to '{newStatus.Name}'.");
        }

        private void MarkComplete_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var completedStatus = ObjectSpace.FindObject<ObligationStatus>(
                new BinaryOperator("Code", STATUS_COMPLETED));

            if (completedStatus == null)
            {
                Application.ShowViewStrategy.ShowMessage("Error: Completed status not found in the system.");
                return;
            }

            int count = 0;
            foreach (var obj in e.SelectedObjects)
            {
                var obligation = obj as ComplianceObligation;
                if (obligation != null && !obligation.Status?.IsTerminal == true)
                {
                    obligation.Status = completedStatus;
                    obligation.CompletedDate = DateTime.Now;
                    count++;
                }
            }

            if (count > 0)
            {
                ObjectSpace.CommitChanges();
                View.ObjectSpace.Refresh();
            }

            Application.ShowViewStrategy.ShowMessage($"Marked {count} obligation(s) as complete.");
        }

        private void MarkOverdue_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var overdueStatus = ObjectSpace.FindObject<ObligationStatus>(
                new BinaryOperator("Code", STATUS_OVERDUE));

            if (overdueStatus == null)
            {
                Application.ShowViewStrategy.ShowMessage("Error: Overdue status not found in the system.");
                return;
            }

            // Find all obligations that are past due and not in a terminal status
            var pastDueObligations = ObjectSpace.GetObjects<ComplianceObligation>(
                CriteriaOperator.And(
                    new BinaryOperator("DueDate", DateTime.Today, BinaryOperatorType.Less),
                    new BinaryOperator("Status.IsTerminal", false),
                    CriteriaOperator.Or(
                        new BinaryOperator("Status.Code", STATUS_PENDING),
                        new BinaryOperator("Status.Code", STATUS_INPROGRESS)
                    )
                ));

            int count = 0;
            foreach (var obligation in pastDueObligations)
            {
                obligation.Status = overdueStatus;
                count++;
            }

            if (count > 0)
            {
                ObjectSpace.CommitChanges();
                View.ObjectSpace.Refresh();
            }

            Application.ShowViewStrategy.ShowMessage($"Marked {count} past-due obligation(s) as overdue.");
        }
    }

    /// <summary>
    /// Controller for detail view of ComplianceObligation
    /// </summary>
    public class ObligationStatusDetailController : ObjectViewController<DetailView, ComplianceObligation>
    {
        private SingleChoiceAction changeStatusAction;
        private SimpleAction markCompleteAction;

        private const string STATUS_COMPLETED = "COMPLETED";

        public ObligationStatusDetailController()
        {
            // Single choice action for status dropdown
            changeStatusAction = new SingleChoiceAction(this, "ChangeObligationStatusDetail", PredefinedCategory.RecordEdit)
            {
                Caption = "Change Status",
                ToolTip = "Change the status of this obligation",
                ImageName = "State_Task_Completed",
                ItemType = SingleChoiceActionItemType.ItemIsOperation
            };
            changeStatusAction.Execute += ChangeStatus_Execute;

            // Mark Complete action
            markCompleteAction = new SimpleAction(this, "MarkObligationCompleteDetail", PredefinedCategory.RecordEdit)
            {
                Caption = "Mark Complete",
                ToolTip = "Mark this obligation as completed",
                ImageName = "State_Task_Completed",
                ConfirmationMessage = "Are you sure you want to mark this obligation as complete?"
            };
            markCompleteAction.Execute += MarkComplete_Execute;
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            PopulateStatusChoices();
        }

        private void PopulateStatusChoices()
        {
            changeStatusAction.Items.Clear();

            var statuses = ObjectSpace.GetObjects<ObligationStatus>(
                new BinaryOperator("IsActive", true));

            foreach (var status in statuses)
            {
                var item = new ChoiceActionItem(status.Code, status.Name, status);
                changeStatusAction.Items.Add(item);
            }
        }

        private void ChangeStatus_Execute(object sender, SingleChoiceActionExecuteEventArgs e)
        {
            var newStatus = e.SelectedChoiceActionItem.Data as ObligationStatus;
            if (newStatus == null)
                return;

            var obligation = ViewCurrentObject;
            if (obligation != null)
            {
                obligation.Status = ObjectSpace.GetObject(newStatus);

                // If status is terminal, set completed date
                if (newStatus.IsTerminal && !obligation.CompletedDate.HasValue)
                {
                    obligation.CompletedDate = DateTime.Now;
                }

                ObjectSpace.CommitChanges();
            }

            Application.ShowViewStrategy.ShowMessage($"Status changed to '{newStatus.Name}'.");
        }

        private void MarkComplete_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var completedStatus = ObjectSpace.FindObject<ObligationStatus>(
                new BinaryOperator("Code", STATUS_COMPLETED));

            if (completedStatus == null)
            {
                Application.ShowViewStrategy.ShowMessage("Error: Completed status not found in the system.");
                return;
            }

            var obligation = ViewCurrentObject;
            if (obligation != null && !obligation.Status?.IsTerminal == true)
            {
                obligation.Status = completedStatus;
                obligation.CompletedDate = DateTime.Now;
                ObjectSpace.CommitChanges();
                Application.ShowViewStrategy.ShowMessage("Obligation marked as complete.");
            }
        }
    }
}

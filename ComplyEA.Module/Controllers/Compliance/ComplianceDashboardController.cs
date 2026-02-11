using System;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.Persistent.Base;
using ComplyEA.Module.BusinessObjects.Compliance;

namespace ComplyEA.Module.Controllers.Compliance
{
    /// <summary>
    /// Controller providing dashboard navigation actions for compliance management.
    /// Active on all views to provide quick navigation to key compliance areas.
    /// </summary>
    public class ComplianceDashboardController : WindowController
    {
        private SimpleAction viewOverdueObligationsAction;
        private SimpleAction viewUpcomingDeadlinesAction;
        private SimpleAction viewPendingRemindersAction;
        private SimpleAction viewAllObligationsAction;

        private const string STATUS_PENDING = "PENDING";
        private const string STATUS_INPROGRESS = "INPROGRESS";
        private const string STATUS_OVERDUE = "OVERDUE";
        private const string DELIVERY_PENDING = "PENDING";

        public ComplianceDashboardController()
        {
            TargetWindowType = WindowType.Main;

            // View overdue obligations
            viewOverdueObligationsAction = new SimpleAction(this, "ViewOverdueObligations", "Dashboard")
            {
                Caption = "Overdue Obligations",
                ToolTip = "View all overdue compliance obligations",
                ImageName = "Status_Overdue"
            };
            viewOverdueObligationsAction.Execute += ViewOverdueObligations_Execute;

            // View upcoming deadlines (next 30 days)
            viewUpcomingDeadlinesAction = new SimpleAction(this, "ViewUpcomingDeadlines", "Dashboard")
            {
                Caption = "Upcoming Deadlines",
                ToolTip = "View obligations due in the next 30 days",
                ImageName = "ComplianceReminder"
            };
            viewUpcomingDeadlinesAction.Execute += ViewUpcomingDeadlines_Execute;

            // View pending reminders
            viewPendingRemindersAction = new SimpleAction(this, "ViewPendingReminders", "Dashboard")
            {
                Caption = "Pending Reminders",
                ToolTip = "View unsent reminders",
                ImageName = "NotificationChannel"
            };
            viewPendingRemindersAction.Execute += ViewPendingReminders_Execute;

            // View all obligations (quick access)
            viewAllObligationsAction = new SimpleAction(this, "ViewAllObligations", "Dashboard")
            {
                Caption = "All Obligations",
                ToolTip = "View all compliance obligations",
                ImageName = "ComplianceObligation"
            };
            viewAllObligationsAction.Execute += ViewAllObligations_Execute;
        }

        private void ViewOverdueObligations_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            // Create criteria for overdue obligations
            // Overdue = DueDate < Today AND Status is not terminal
            var criteria = CriteriaOperator.And(
                new BinaryOperator("DueDate", DateTime.Today, BinaryOperatorType.Less),
                new BinaryOperator("Status.IsTerminal", false)
            );

            ShowFilteredObligationListView("Overdue Obligations", criteria);
        }

        private void ViewUpcomingDeadlines_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            // Create criteria for upcoming deadlines (next 30 days)
            var today = DateTime.Today;
            var thirtyDaysOut = today.AddDays(30);

            var criteria = CriteriaOperator.And(
                new BinaryOperator("DueDate", today, BinaryOperatorType.GreaterOrEqual),
                new BinaryOperator("DueDate", thirtyDaysOut, BinaryOperatorType.LessOrEqual),
                new BinaryOperator("Status.IsTerminal", false)
            );

            ShowFilteredObligationListView("Upcoming Deadlines (Next 30 Days)", criteria);
        }

        private void ViewPendingReminders_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            // Create criteria for pending reminders
            var criteria = CriteriaOperator.And(
                new BinaryOperator("DeliveryStatus.Code", DELIVERY_PENDING),
                new NullOperator("SentDate")
            );

            ShowFilteredReminderListView("Pending Reminders", criteria);
        }

        private void ViewAllObligations_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            ShowFilteredObligationListView("All Compliance Obligations", null);
        }

        private void ShowFilteredObligationListView(string caption, CriteriaOperator criteria)
        {
            var os = Application.CreateObjectSpace(typeof(ComplianceObligation));

            var listViewId = Application.FindListViewId(typeof(ComplianceObligation));
            var cv = Application.CreateCollectionSource(os, typeof(ComplianceObligation), listViewId);

            if (!ReferenceEquals(criteria, null))
            {
                cv.Criteria["DashboardFilter"] = criteria;
            }

            var listView = Application.CreateListView(listViewId, cv, false);
            listView.Caption = caption;

            var svp = new ShowViewParameters(listView);
            svp.TargetWindow = TargetWindow.Current;
            svp.Context = TemplateContext.View;

            Application.ShowViewStrategy.ShowView(svp, new ShowViewSource(null, null));
        }

        private void ShowFilteredReminderListView(string caption, CriteriaOperator criteria)
        {
            var os = Application.CreateObjectSpace(typeof(ComplianceReminder));

            var listViewId = Application.FindListViewId(typeof(ComplianceReminder));
            var cv = Application.CreateCollectionSource(os, typeof(ComplianceReminder), listViewId);

            if (!ReferenceEquals(criteria, null))
            {
                cv.Criteria["DashboardFilter"] = criteria;
            }

            var listView = Application.CreateListView(listViewId, cv, false);
            listView.Caption = caption;

            var svp = new ShowViewParameters(listView);
            svp.TargetWindow = TargetWindow.Current;
            svp.Context = TemplateContext.View;

            Application.ShowViewStrategy.ShowView(svp, new ShowViewSource(null, null));
        }
    }
}

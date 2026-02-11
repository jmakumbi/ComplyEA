using System;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Editors;

namespace ComplyEA.Module.Controllers.Compliance
{
    /// <summary>
    /// Controller for the 4-quadrant Compliance Cockpit DashboardView.
    /// Applies runtime criteria filters to each quadrant's ListView.
    /// </summary>
    public class ComplianceCockpitController : ViewController<DashboardView>
    {
        private const string FILTER_KEY = "CockpitFilter";

        public ComplianceCockpitController()
        {
            TargetViewId = "ComplianceCockpit_DashboardView";
        }

        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
            ApplyQuadrantFilters();
        }

        private void ApplyQuadrantFilters()
        {
            // Top-left: Overdue obligations (DueDate < Today AND not terminal), sorted by days overdue desc
            ApplyListViewFilter("OverdueObligations", CriteriaOperator.And(
                new BinaryOperator("DueDate", DateTime.Today, BinaryOperatorType.Less),
                new BinaryOperator("Status.IsTerminal", false)
            ));

            // Top-right: Upcoming 30 days (DueDate >= Today AND DueDate <= Today+30 AND not terminal)
            ApplyListViewFilter("UpcomingObligations", CriteriaOperator.And(
                new BinaryOperator("DueDate", DateTime.Today, BinaryOperatorType.GreaterOrEqual),
                new BinaryOperator("DueDate", DateTime.Today.AddDays(30), BinaryOperatorType.LessOrEqual),
                new BinaryOperator("Status.IsTerminal", false)
            ));

            // Bottom-left: Pending reminders (DeliveryStatus = PENDING AND SentDate IS NULL)
            ApplyListViewFilter("PendingReminders", CriteriaOperator.And(
                new BinaryOperator("DeliveryStatus.Code", "PENDING"),
                new NullOperator("SentDate")
            ));

            // Bottom-right: Status Summary — all active obligations (no filter, shows status distribution)
            // No criteria needed; users can group by Status in the UI
        }

        private void ApplyListViewFilter(string dashboardItemId, CriteriaOperator criteria)
        {
            var item = View.FindItem(dashboardItemId) as DashboardViewItem;
            if (item?.InnerView is ListView listView)
            {
                listView.CollectionSource.Criteria[FILTER_KEY] = criteria;
            }
        }
    }
}

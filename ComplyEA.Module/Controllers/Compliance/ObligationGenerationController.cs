using System;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using ComplyEA.Module.BusinessObjects.NonPersistent;
using ComplyEA.Module.BusinessObjects.Organization;
using ComplyEA.Module.Services;

namespace ComplyEA.Module.Controllers.Compliance
{
    public class ObligationGenerationController : ViewController<ListView>
    {
        private PopupWindowShowAction generateObligationsForPeriodAction;
        private SimpleAction generateObligationsForCompanyAction;

        public ObligationGenerationController()
        {
            TargetObjectType = typeof(Company);

            // Action to show popup dialog for period selection
            generateObligationsForPeriodAction = new PopupWindowShowAction(this,
                "GenerateObligationsForPeriod", PredefinedCategory.RecordEdit)
            {
                Caption = "Generate Obligations",
                ToolTip = "Generate compliance obligations for selected period",
                ImageName = "BO_Task",
                SelectionDependencyType = SelectionDependencyType.RequireSingleObject
            };
            generateObligationsForPeriodAction.CustomizePopupWindowParams += GenerateObligationsForPeriod_CustomizePopupWindowParams;
            generateObligationsForPeriodAction.Execute += GenerateObligationsForPeriod_Execute;

            // Quick action to generate for current year
            generateObligationsForCompanyAction = new SimpleAction(this,
                "GenerateObligationsForCurrentYear", PredefinedCategory.RecordEdit)
            {
                Caption = "Generate for Current Year",
                ToolTip = "Generate all compliance obligations for the current year",
                ImageName = "Action_Grant",
                SelectionDependencyType = SelectionDependencyType.RequireSingleObject
            };
            generateObligationsForCompanyAction.Execute += GenerateObligationsForCurrentYear_Execute;
        }

        private void GenerateObligationsForPeriod_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            var os = Application.CreateObjectSpace(typeof(ObligationGenerationParameters));
            var parameters = os.CreateObject<ObligationGenerationParameters>();
            parameters.SetObjectSpace(os);

            // Pre-select the current company
            var currentCompany = View.CurrentObject as Company;
            if (currentCompany != null)
            {
                parameters.Company = os.GetObject(currentCompany);
            }

            e.View = Application.CreateDetailView(os, parameters);
            e.DialogController.SaveOnAccept = false;
        }

        private void GenerateObligationsForPeriod_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            var parameters = e.PopupWindowViewCurrentObject as ObligationGenerationParameters;
            if (parameters == null)
                return;

            var service = new ObligationGenerationService();
            int count = 0;

            using (var os = Application.CreateObjectSpace(typeof(Company)))
            {
                if (parameters.Company != null)
                {
                    var company = os.GetObject(parameters.Company);
                    count = service.GenerateRecurringObligations(os, company,
                        parameters.Year, parameters.Quarter, parameters.Month);
                }
                else
                {
                    count = service.GenerateObligationsForPeriod(os,
                        parameters.Year, parameters.Quarter, parameters.Month);
                }

                if (count > 0)
                {
                    os.CommitChanges();
                }
            }

            var message = count > 0
                ? $"Successfully generated {count} compliance obligation(s)."
                : "No new obligations were generated. Obligations may already exist for the selected period.";

            Application.ShowViewStrategy.ShowMessage(message);
            View.ObjectSpace.Refresh();
        }

        private void GenerateObligationsForCurrentYear_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var company = View.CurrentObject as Company;
            if (company == null)
                return;

            var service = new ObligationGenerationService();
            int count = 0;

            using (var os = Application.CreateObjectSpace(typeof(Company)))
            {
                var targetCompany = os.GetObject(company);
                count = service.GenerateRecurringObligations(os, targetCompany, DateTime.Now.Year, null, null);

                if (count > 0)
                {
                    os.CommitChanges();
                }
            }

            var message = count > 0
                ? $"Successfully generated {count} compliance obligation(s) for {DateTime.Now.Year}."
                : "No new obligations were generated. Obligations may already exist for the current year.";

            Application.ShowViewStrategy.ShowMessage(message);
            View.ObjectSpace.Refresh();
        }
    }
}

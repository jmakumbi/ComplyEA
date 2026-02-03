using DevExpress.ExpressApp;
using ComplyEA.Module.BusinessObjects.Compliance;
using ComplyEA.Module.BusinessObjects.Configuration;
using ComplyEA.Module.BusinessObjects.Organization;

namespace ComplyEA.Module.Services
{
    /// <summary>
    /// Service for generating compliance reminders based on obligation due dates.
    /// </summary>
    public interface IReminderGenerationService
    {
        /// <summary>
        /// Generates reminders for a specific obligation based on company reminder settings.
        /// </summary>
        int GenerateRemindersForObligation(IObjectSpace os, ComplianceObligation obligation);

        /// <summary>
        /// Generates reminders for all pending obligations, optionally filtered by company.
        /// </summary>
        int GenerateRemindersForPendingObligations(IObjectSpace os, Company company = null);

        /// <summary>
        /// Gets the effective reminder settings for a company, falling back to defaults.
        /// </summary>
        CompanyReminderSettings GetEffectiveReminderSettings(IObjectSpace os, Company company);

        /// <summary>
        /// Regenerates reminders for an obligation (e.g., when due date changes).
        /// Deletes existing unsent reminders and creates new ones.
        /// </summary>
        int RegenerateReminders(IObjectSpace os, ComplianceObligation obligation);

        /// <summary>
        /// Checks if a reminder already exists for the obligation and reminder type.
        /// </summary>
        bool ReminderExists(IObjectSpace os, ComplianceObligation obligation, string reminderTypeCode);
    }
}

using System;
using DevExpress.ExpressApp;
using ComplyEA.Module.BusinessObjects.Compliance;
using ComplyEA.Module.BusinessObjects.Organization;
using ComplyEA.Module.BusinessObjects.Regulatory;

namespace ComplyEA.Module.Services
{
    /// <summary>
    /// Service for generating compliance obligations from requirements.
    /// </summary>
    public interface IObligationGenerationService
    {
        /// <summary>
        /// Generates obligations for all requirements in an applicable regulation.
        /// </summary>
        int GenerateObligationsForApplicableRegulation(IObjectSpace os, ApplicableRegulation reg, int year, int? quarter, int? month);

        /// <summary>
        /// Generates recurring obligations for a company for the specified period.
        /// </summary>
        int GenerateRecurringObligations(IObjectSpace os, Company company, int year, int? quarter, int? month);

        /// <summary>
        /// Generates obligations for all companies for the specified period.
        /// </summary>
        int GenerateObligationsForPeriod(IObjectSpace os, int year, int? quarter, int? month);

        /// <summary>
        /// Calculates the due date based on requirement timeline type and period.
        /// </summary>
        DateTime? CalculateDueDate(ComplianceRequirement req, int year, int? quarter, int? month, DateTime? eventDate);

        /// <summary>
        /// Checks if an obligation already exists for the given company, requirement, and period.
        /// </summary>
        bool ObligationExistsForPeriod(IObjectSpace os, Company company, ComplianceRequirement req, int year, int? quarter, int? month);

        /// <summary>
        /// Creates a single obligation for a company and requirement.
        /// </summary>
        ComplianceObligation CreateObligation(IObjectSpace os, Company company, ComplianceRequirement req, int year, int? quarter, int? month, DateTime? eventDate);
    }
}

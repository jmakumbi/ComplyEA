using System;
using System.Linq;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using ComplyEA.Module.BusinessObjects.Compliance;
using ComplyEA.Module.BusinessObjects.Lookups;
using ComplyEA.Module.BusinessObjects.Organization;
using ComplyEA.Module.BusinessObjects.Regulatory;

namespace ComplyEA.Module.Services
{
    public class ObligationGenerationService : IObligationGenerationService
    {
        // Timeline type codes
        private const string TIMELINE_ALWAYS = "ALWAYS";
        private const string TIMELINE_ANNUAL = "ANNUAL";
        private const string TIMELINE_QUARTERLY = "QUARTERLY";
        private const string TIMELINE_MONTHLY = "MONTHLY";
        private const string TIMELINE_EVENT = "EVENT";
        private const string TIMELINE_FIXED = "FIXED";

        // Default status code for new obligations
        private const string STATUS_PENDING = "PENDING";

        public int GenerateObligationsForApplicableRegulation(IObjectSpace os, ApplicableRegulation reg, int year, int? quarter, int? month)
        {
            if (reg == null || !reg.IsActive)
                return 0;

            var company = reg.Company;
            var requirements = os.GetObjects<ComplianceRequirement>(
                CriteriaOperator.And(
                    new BinaryOperator("RegulatoryAct.Oid", reg.RegulatoryAct.Oid),
                    new BinaryOperator("IsActive", true)
                ));

            int count = 0;
            foreach (var requirement in requirements)
            {
                // Check if requirement applies to this company type
                if (requirement.ApplicableCompanyType != null &&
                    company.CompanyType != null &&
                    requirement.ApplicableCompanyType.Oid != company.CompanyType.Oid)
                {
                    continue;
                }

                var timelineCode = requirement.TimelineType?.Code;
                if (string.IsNullOrEmpty(timelineCode))
                    continue;

                // Determine which periods to generate based on timeline type
                switch (timelineCode)
                {
                    case TIMELINE_ANNUAL:
                        if (CreateObligationIfNotExists(os, company, requirement, year, null, null, null))
                            count++;
                        break;

                    case TIMELINE_QUARTERLY:
                        if (quarter.HasValue)
                        {
                            if (CreateObligationIfNotExists(os, company, requirement, year, quarter, null, null))
                                count++;
                        }
                        else
                        {
                            // Generate for all quarters
                            for (int q = 1; q <= 4; q++)
                            {
                                if (CreateObligationIfNotExists(os, company, requirement, year, q, null, null))
                                    count++;
                            }
                        }
                        break;

                    case TIMELINE_MONTHLY:
                        if (month.HasValue)
                        {
                            if (CreateObligationIfNotExists(os, company, requirement, year, null, month, null))
                                count++;
                        }
                        else if (quarter.HasValue)
                        {
                            // Generate for all months in the quarter
                            int startMonth = (quarter.Value - 1) * 3 + 1;
                            for (int m = startMonth; m < startMonth + 3; m++)
                            {
                                if (CreateObligationIfNotExists(os, company, requirement, year, null, m, null))
                                    count++;
                            }
                        }
                        else
                        {
                            // Generate for all months
                            for (int m = 1; m <= 12; m++)
                            {
                                if (CreateObligationIfNotExists(os, company, requirement, year, null, m, null))
                                    count++;
                            }
                        }
                        break;

                    case TIMELINE_ALWAYS:
                    case TIMELINE_FIXED:
                        // These are typically one-time or ongoing, generate once per year
                        if (CreateObligationIfNotExists(os, company, requirement, year, null, null, null))
                            count++;
                        break;

                    // EVENT type requires an event date, skip automatic generation
                }
            }

            return count;
        }

        public int GenerateRecurringObligations(IObjectSpace os, Company company, int year, int? quarter, int? month)
        {
            if (company == null || !company.IsActive)
                return 0;

            var applicableRegs = os.GetObjects<ApplicableRegulation>(
                CriteriaOperator.And(
                    new BinaryOperator("Company.Oid", company.Oid),
                    new BinaryOperator("IsActive", true)
                ));

            int totalCount = 0;
            foreach (var reg in applicableRegs)
            {
                totalCount += GenerateObligationsForApplicableRegulation(os, reg, year, quarter, month);
            }

            return totalCount;
        }

        public int GenerateObligationsForPeriod(IObjectSpace os, int year, int? quarter, int? month)
        {
            var activeCompanies = os.GetObjects<Company>(
                new BinaryOperator("IsActive", true));

            int totalCount = 0;
            foreach (var company in activeCompanies)
            {
                totalCount += GenerateRecurringObligations(os, company, year, quarter, month);
            }

            return totalCount;
        }

        public DateTime? CalculateDueDate(ComplianceRequirement req, int year, int? quarter, int? month, DateTime? eventDate)
        {
            if (req?.TimelineType == null)
                return null;

            var timelineCode = req.TimelineType.Code;
            int dueDayOfMonth = req.DueDayOfMonth ?? 15; // Default to 15th

            switch (timelineCode)
            {
                case TIMELINE_ANNUAL:
                    int dueMonth = req.DueMonth ?? 12; // Default to December
                    return CreateDateSafe(year, dueMonth, dueDayOfMonth);

                case TIMELINE_QUARTERLY:
                    if (!quarter.HasValue)
                        return null;
                    // Due date is at the end of the quarter month
                    int quarterEndMonth = quarter.Value * 3;
                    return CreateDateSafe(year, quarterEndMonth, dueDayOfMonth);

                case TIMELINE_MONTHLY:
                    if (!month.HasValue)
                        return null;
                    return CreateDateSafe(year, month.Value, dueDayOfMonth);

                case TIMELINE_EVENT:
                    if (!eventDate.HasValue || !req.DaysAfterEvent.HasValue)
                        return null;
                    return eventDate.Value.AddDays(req.DaysAfterEvent.Value);

                case TIMELINE_FIXED:
                    // Fixed requirements use DueMonth and DueDayOfMonth as the fixed date each year
                    int fixedMonth = req.DueMonth ?? 12;
                    return CreateDateSafe(year, fixedMonth, dueDayOfMonth);

                case TIMELINE_ALWAYS:
                    // Ongoing requirement, use end of year as nominal due date
                    return new DateTime(year, 12, 31);

                default:
                    return null;
            }
        }

        private DateTime CreateDateSafe(int year, int month, int day)
        {
            // Clamp day to valid range for the month
            int daysInMonth = DateTime.DaysInMonth(year, month);
            int safeDay = Math.Min(day, daysInMonth);
            return new DateTime(year, month, safeDay);
        }

        public bool ObligationExistsForPeriod(IObjectSpace os, Company company, ComplianceRequirement req, int year, int? quarter, int? month)
        {
            var criteria = CriteriaOperator.And(
                new BinaryOperator("Company.Oid", company.Oid),
                new BinaryOperator("ComplianceRequirement.Oid", req.Oid),
                new BinaryOperator("PeriodYear", year)
            );

            if (quarter.HasValue)
            {
                criteria = CriteriaOperator.And(criteria, new BinaryOperator("PeriodQuarter", quarter.Value));
            }
            else
            {
                criteria = CriteriaOperator.And(criteria, new NullOperator("PeriodQuarter"));
            }

            if (month.HasValue)
            {
                criteria = CriteriaOperator.And(criteria, new BinaryOperator("PeriodMonth", month.Value));
            }
            else
            {
                criteria = CriteriaOperator.And(criteria, new NullOperator("PeriodMonth"));
            }

            var existing = os.FindObject<ComplianceObligation>(criteria);
            return existing != null;
        }

        public ComplianceObligation CreateObligation(IObjectSpace os, Company company, ComplianceRequirement req, int year, int? quarter, int? month, DateTime? eventDate)
        {
            var dueDate = CalculateDueDate(req, year, quarter, month, eventDate);
            if (!dueDate.HasValue)
                return null;

            var pendingStatus = os.FindObject<ObligationStatus>(
                new BinaryOperator("Code", STATUS_PENDING));

            var obligation = os.CreateObject<ComplianceObligation>();
            obligation.Company = os.GetObject(company);
            obligation.ComplianceRequirement = os.GetObject(req);
            obligation.PeriodYear = year;
            obligation.PeriodQuarter = quarter;
            obligation.PeriodMonth = month;
            obligation.DueDate = dueDate;
            obligation.Status = pendingStatus;

            // Copy risk rating from requirement if set
            if (req.RiskRating != null)
            {
                obligation.RiskRating = os.GetObject(req.RiskRating);
            }

            return obligation;
        }

        private bool CreateObligationIfNotExists(IObjectSpace os, Company company, ComplianceRequirement req, int year, int? quarter, int? month, DateTime? eventDate)
        {
            if (ObligationExistsForPeriod(os, company, req, year, quarter, month))
                return false;

            var obligation = CreateObligation(os, company, req, year, quarter, month, eventDate);
            return obligation != null;
        }
    }
}

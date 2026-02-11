using System;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using ComplyEA.Module.BusinessObjects.Lookups;
using ComplyEA.Module.BusinessObjects.Organization;
using ComplyEA.Module.BusinessObjects.Regulatory;

namespace ComplyEA.Module.BusinessObjects.Compliance
{
    [DefaultClassOptions]
    [NavigationItem("Compliance")]
    [ImageName("BO_Task")]
    [Appearance("Overdue", AppearanceItemType = "ViewItem", TargetItems = "*",
        Context = "ListView", Criteria = "[Status.Code] = 'OVERDUE'",
        BackColor = "255, 205, 210", FontColor = "198, 40, 40", FontStyle = System.Drawing.FontStyle.Bold)]
    [Appearance("DueSoon", AppearanceItemType = "ViewItem", TargetItems = "*",
        Context = "ListView",
        Criteria = "[Status.Code] != 'OVERDUE' And [Status.IsTerminal] = false And [DueDate] < AddDays(LocalDateTimeNow(), 7)",
        BackColor = "255, 243, 224", FontColor = "230, 81, 0")]
    [Appearance("Completed", AppearanceItemType = "ViewItem", TargetItems = "*",
        Context = "ListView", Criteria = "[Status.Code] = 'COMPLETED'",
        BackColor = "232, 245, 233", FontColor = "46, 125, 50")]
    [Appearance("Waived", AppearanceItemType = "ViewItem", TargetItems = "*",
        Context = "ListView", Criteria = "[Status.Code] = 'WAIVED'",
        BackColor = "245, 245, 245", FontColor = "158, 158, 158", FontStyle = System.Drawing.FontStyle.Italic)]
    [Appearance("InProgress", AppearanceItemType = "ViewItem", TargetItems = "*",
        Context = "ListView", Criteria = "[Status.Code] = 'INPROGRESS'",
        BackColor = "227, 242, 253", FontColor = "21, 101, 192")]
    public class ComplianceObligation : BaseObject
    {
        public ComplianceObligation(Session session) : base(session) { }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            CreatedOn = DateTime.Now;
        }

        Company company;
        [RuleRequiredField]
        [Association("Company-Obligations")]
        public Company Company
        {
            get => company;
            set => SetPropertyValue(nameof(Company), ref company, value);
        }

        ComplianceRequirement complianceRequirement;
        [RuleRequiredField]
        [Association("ComplianceRequirement-Obligations")]
        public ComplianceRequirement ComplianceRequirement
        {
            get => complianceRequirement;
            set => SetPropertyValue(nameof(ComplianceRequirement), ref complianceRequirement, value);
        }

        string title;
        [Size(300)]
        [ToolTip("Leave empty to use requirement title")]
        public string Title
        {
            get => string.IsNullOrEmpty(title) ? ComplianceRequirement?.Title : title;
            set => SetPropertyValue(nameof(Title), ref title, value);
        }

        string description;
        [Size(SizeAttribute.Unlimited)]
        public string Description
        {
            get => description;
            set => SetPropertyValue(nameof(Description), ref description, value);
        }

        int? periodYear;
        [ToolTip("Year this obligation applies to (e.g., 2024 for annual filings)")]
        public int? PeriodYear
        {
            get => periodYear;
            set => SetPropertyValue(nameof(PeriodYear), ref periodYear, value);
        }

        int? periodQuarter;
        [ToolTip("Quarter this obligation applies to (1-4 for quarterly filings)")]
        public int? PeriodQuarter
        {
            get => periodQuarter;
            set => SetPropertyValue(nameof(PeriodQuarter), ref periodQuarter, value);
        }

        int? periodMonth;
        [ToolTip("Month this obligation applies to (1-12 for monthly filings)")]
        public int? PeriodMonth
        {
            get => periodMonth;
            set => SetPropertyValue(nameof(PeriodMonth), ref periodMonth, value);
        }

        DateTime? dueDate;
        [RuleRequiredField]
        public DateTime? DueDate
        {
            get => dueDate;
            set => SetPropertyValue(nameof(DueDate), ref dueDate, value);
        }

        DateTime? completedDate;
        public DateTime? CompletedDate
        {
            get => completedDate;
            set => SetPropertyValue(nameof(CompletedDate), ref completedDate, value);
        }

        ObligationStatus status;
        [RuleRequiredField]
        [Association("ObligationStatus-Obligations")]
        public ObligationStatus Status
        {
            get => status;
            set => SetPropertyValue(nameof(Status), ref status, value);
        }

        CompanyContact assignedTo;
        [Association("CompanyContact-AssignedObligations")]
        public CompanyContact AssignedTo
        {
            get => assignedTo;
            set => SetPropertyValue(nameof(AssignedTo), ref assignedTo, value);
        }

        RiskRating riskRating;
        [Association("RiskRating-Obligations")]
        [ToolTip("Override requirement risk rating if needed")]
        public RiskRating RiskRating
        {
            get => riskRating ?? ComplianceRequirement?.RiskRating;
            set => SetPropertyValue(nameof(RiskRating), ref riskRating, value);
        }

        string submissionReference;
        [Size(100)]
        [ToolTip("Reference number from regulatory submission")]
        public string SubmissionReference
        {
            get => submissionReference;
            set => SetPropertyValue(nameof(SubmissionReference), ref submissionReference, value);
        }

        DateTime createdOn;
        [VisibleInListView(false)]
        public DateTime CreatedOn
        {
            get => createdOn;
            set => SetPropertyValue(nameof(CreatedOn), ref createdOn, value);
        }

        DateTime? lastModifiedOn;
        [VisibleInListView(false)]
        public DateTime? LastModifiedOn
        {
            get => lastModifiedOn;
            set => SetPropertyValue(nameof(LastModifiedOn), ref lastModifiedOn, value);
        }

        string notes;
        [Size(SizeAttribute.Unlimited)]
        public string Notes
        {
            get => notes;
            set => SetPropertyValue(nameof(Notes), ref notes, value);
        }

        [PersistentAlias("Iif([DueDate] Is Not Null And [DueDate] < LocalDateTimeNow() And ([Status] Is Null Or Not [Status.IsTerminal]), True, False)")]
        public bool IsOverdue => (bool)EvaluateAlias(nameof(IsOverdue));

        [PersistentAlias("Iif([DueDate] Is Not Null, DateDiffDay(LocalDateTimeNow(), [DueDate]), Null)")]
        public int? DaysUntilDue => (int?)EvaluateAlias(nameof(DaysUntilDue));

        [Association("ComplianceObligation-Reminders")]
        public XPCollection<ComplianceReminder> Reminders => GetCollection<ComplianceReminder>(nameof(Reminders));

        [Association("ComplianceObligation-Documents")]
        public XPCollection<ComplianceDocument> Documents => GetCollection<ComplianceDocument>(nameof(Documents));

        protected override void OnSaving()
        {
            base.OnSaving();
            LastModifiedOn = DateTime.Now;
        }

        public override string ToString()
        {
            return $"{Company?.ShortName ?? Company?.Name} - {Title}";
        }
    }
}

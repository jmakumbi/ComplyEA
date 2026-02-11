using System;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using ComplyEA.Module.BusinessObjects.Lookups;

namespace ComplyEA.Module.BusinessObjects.Regulatory
{
    [DefaultClassOptions]
    [NavigationItem("Regulatory")]
    [ImageName("ComplianceRequirement")]
    public class ComplianceRequirement : BaseObject
    {
        public ComplianceRequirement(Session session) : base(session) { }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            CreatedOn = DateTime.Now;
            IsActive = true;
        }

        RegulatoryAct regulatoryAct;
        [RuleRequiredField]
        [Association("RegulatoryAct-Requirements")]
        public RegulatoryAct RegulatoryAct
        {
            get => regulatoryAct;
            set => SetPropertyValue(nameof(RegulatoryAct), ref regulatoryAct, value);
        }

        string title;
        [RuleRequiredField]
        [Size(300)]
        public string Title
        {
            get => title;
            set => SetPropertyValue(nameof(Title), ref title, value);
        }

        string sectionReference;
        [Size(100)]
        [ToolTip("Section/Article reference in the act (e.g., 'Section 125(1)')")]
        public string SectionReference
        {
            get => sectionReference;
            set => SetPropertyValue(nameof(SectionReference), ref sectionReference, value);
        }

        string description;
        [Size(SizeAttribute.Unlimited)]
        public string Description
        {
            get => description;
            set => SetPropertyValue(nameof(Description), ref description, value);
        }

        TimelineType timelineType;
        [RuleRequiredField]
        [Association("TimelineType-Requirements")]
        public TimelineType TimelineType
        {
            get => timelineType;
            set => SetPropertyValue(nameof(TimelineType), ref timelineType, value);
        }

        int? dueDayOfMonth;
        [ToolTip("Day of month when due (for monthly/annual requirements)")]
        public int? DueDayOfMonth
        {
            get => dueDayOfMonth;
            set => SetPropertyValue(nameof(DueDayOfMonth), ref dueDayOfMonth, value);
        }

        int? dueMonth;
        [ToolTip("Month when due (1-12, for annual requirements)")]
        public int? DueMonth
        {
            get => dueMonth;
            set => SetPropertyValue(nameof(DueMonth), ref dueMonth, value);
        }

        int? daysAfterEvent;
        [ToolTip("Number of days after triggering event")]
        public int? DaysAfterEvent
        {
            get => daysAfterEvent;
            set => SetPropertyValue(nameof(DaysAfterEvent), ref daysAfterEvent, value);
        }

        string triggerEvent;
        [Size(300)]
        [ToolTip("Event that triggers this requirement (for event-driven timelines)")]
        public string TriggerEvent
        {
            get => triggerEvent;
            set => SetPropertyValue(nameof(TriggerEvent), ref triggerEvent, value);
        }

        RiskRating riskRating;
        [Association("RiskRating-Requirements")]
        public RiskRating RiskRating
        {
            get => riskRating;
            set => SetPropertyValue(nameof(RiskRating), ref riskRating, value);
        }

        decimal? penaltyAmount;
        [ToolTip("Potential penalty for non-compliance")]
        public decimal? PenaltyAmount
        {
            get => penaltyAmount;
            set => SetPropertyValue(nameof(PenaltyAmount), ref penaltyAmount, value);
        }

        string penaltyDescription;
        [Size(500)]
        public string PenaltyDescription
        {
            get => penaltyDescription;
            set => SetPropertyValue(nameof(PenaltyDescription), ref penaltyDescription, value);
        }

        TemplateCategory templateCategory;
        [Association("TemplateCategory-Requirements")]
        [ToolTip("Category of templates applicable to this requirement")]
        public TemplateCategory TemplateCategory
        {
            get => templateCategory;
            set => SetPropertyValue(nameof(TemplateCategory), ref templateCategory, value);
        }

        CompanyType applicableCompanyType;
        [Association("CompanyType-Requirements")]
        [ToolTip("Leave empty if applies to all company types")]
        public CompanyType ApplicableCompanyType
        {
            get => applicableCompanyType;
            set => SetPropertyValue(nameof(ApplicableCompanyType), ref applicableCompanyType, value);
        }

        DateTime createdOn;
        [VisibleInListView(false)]
        public DateTime CreatedOn
        {
            get => createdOn;
            set => SetPropertyValue(nameof(CreatedOn), ref createdOn, value);
        }

        bool isActive;
        public bool IsActive
        {
            get => isActive;
            set => SetPropertyValue(nameof(IsActive), ref isActive, value);
        }

        [Association("ComplianceRequirement-Obligations")]
        public XPCollection<Compliance.ComplianceObligation> Obligations => GetCollection<Compliance.ComplianceObligation>(nameof(Obligations));

        [Association("ComplianceRequirement-Templates")]
        public XPCollection<Compliance.ComplianceTemplate> Templates => GetCollection<Compliance.ComplianceTemplate>(nameof(Templates));

        public override string ToString()
        {
            return Title;
        }
    }
}

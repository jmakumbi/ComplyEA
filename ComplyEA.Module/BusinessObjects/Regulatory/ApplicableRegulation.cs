using System;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using ComplyEA.Module.BusinessObjects.Organization;

namespace ComplyEA.Module.BusinessObjects.Regulatory
{
    [DefaultClassOptions]
    [NavigationItem("Regulatory")]
    [ImageName("BO_Transition")]
    public class ApplicableRegulation : BaseObject
    {
        public ApplicableRegulation(Session session) : base(session) { }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            AppliedOn = DateTime.Now;
            IsActive = true;
        }

        Company company;
        [RuleRequiredField]
        [Association("Company-ApplicableRegulations")]
        public Company Company
        {
            get => company;
            set => SetPropertyValue(nameof(Company), ref company, value);
        }

        RegulatoryAct regulatoryAct;
        [RuleRequiredField]
        [Association("RegulatoryAct-ApplicableRegulations")]
        public RegulatoryAct RegulatoryAct
        {
            get => regulatoryAct;
            set => SetPropertyValue(nameof(RegulatoryAct), ref regulatoryAct, value);
        }

        DateTime appliedOn;
        public DateTime AppliedOn
        {
            get => appliedOn;
            set => SetPropertyValue(nameof(AppliedOn), ref appliedOn, value);
        }

        DateTime? effectiveFrom;
        [ToolTip("Date from which this regulation applies to the company")]
        public DateTime? EffectiveFrom
        {
            get => effectiveFrom;
            set => SetPropertyValue(nameof(EffectiveFrom), ref effectiveFrom, value);
        }

        DateTime? effectiveTo;
        [ToolTip("Date until which this regulation applies (leave empty if ongoing)")]
        public DateTime? EffectiveTo
        {
            get => effectiveTo;
            set => SetPropertyValue(nameof(EffectiveTo), ref effectiveTo, value);
        }

        bool isActive;
        public bool IsActive
        {
            get => isActive;
            set => SetPropertyValue(nameof(IsActive), ref isActive, value);
        }

        string notes;
        [Size(SizeAttribute.Unlimited)]
        public string Notes
        {
            get => notes;
            set => SetPropertyValue(nameof(Notes), ref notes, value);
        }

        public override string ToString()
        {
            return $"{Company?.Name} - {RegulatoryAct?.ShortName ?? RegulatoryAct?.Name}";
        }
    }
}

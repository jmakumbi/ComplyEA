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
    [ImageName("RegulatoryAct")]
    public class RegulatoryAct : BaseObject
    {
        public RegulatoryAct(Session session) : base(session) { }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            CreatedOn = DateTime.Now;
            IsActive = true;
        }

        string name;
        [RuleRequiredField]
        [Size(300)]
        public string Name
        {
            get => name;
            set => SetPropertyValue(nameof(Name), ref name, value);
        }

        string shortName;
        [Size(100)]
        [ToolTip("Common abbreviation (e.g., 'Companies Act')")]
        public string ShortName
        {
            get => shortName;
            set => SetPropertyValue(nameof(ShortName), ref shortName, value);
        }

        ActType actType;
        [Association("ActType-RegulatoryActs")]
        public ActType ActType
        {
            get => actType;
            set => SetPropertyValue(nameof(ActType), ref actType, value);
        }

        RegulationScope regulationScope;
        [Association("RegulationScope-RegulatoryActs")]
        public RegulationScope RegulationScope
        {
            get => regulationScope;
            set => SetPropertyValue(nameof(RegulationScope), ref regulationScope, value);
        }

        Sector applicableSector;
        [Association("Sector-RegulatoryActs")]
        [ToolTip("Leave empty if applies to all sectors")]
        public Sector ApplicableSector
        {
            get => applicableSector;
            set => SetPropertyValue(nameof(ApplicableSector), ref applicableSector, value);
        }

        int? year;
        public int? Year
        {
            get => year;
            set => SetPropertyValue(nameof(Year), ref year, value);
        }

        string jurisdiction;
        [Size(100)]
        public string Jurisdiction
        {
            get => jurisdiction;
            set => SetPropertyValue(nameof(Jurisdiction), ref jurisdiction, value);
        }

        string regulatoryBody;
        [Size(200)]
        [ToolTip("Governing authority (e.g., 'Registrar of Companies')")]
        public string RegulatoryBody
        {
            get => regulatoryBody;
            set => SetPropertyValue(nameof(RegulatoryBody), ref regulatoryBody, value);
        }

        DateTime? effectiveDate;
        public DateTime? EffectiveDate
        {
            get => effectiveDate;
            set => SetPropertyValue(nameof(EffectiveDate), ref effectiveDate, value);
        }

        string description;
        [Size(SizeAttribute.Unlimited)]
        public string Description
        {
            get => description;
            set => SetPropertyValue(nameof(Description), ref description, value);
        }

        string sourceUrl;
        [Size(500)]
        [ToolTip("URL to official source document")]
        public string SourceUrl
        {
            get => sourceUrl;
            set => SetPropertyValue(nameof(SourceUrl), ref sourceUrl, value);
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

        [Association("RegulatoryAct-Acronyms")]
        public XPCollection<ActAcronym> Acronyms => GetCollection<ActAcronym>(nameof(Acronyms));

        [Association("RegulatoryAct-Requirements")]
        public XPCollection<ComplianceRequirement> Requirements => GetCollection<ComplianceRequirement>(nameof(Requirements));

        [Association("RegulatoryAct-ApplicableRegulations")]
        public XPCollection<ApplicableRegulation> ApplicableRegulations => GetCollection<ApplicableRegulation>(nameof(ApplicableRegulations));

        public override string ToString()
        {
            return !string.IsNullOrEmpty(ShortName) ? ShortName : Name;
        }
    }
}

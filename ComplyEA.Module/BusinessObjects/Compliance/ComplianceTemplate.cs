using System;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using ComplyEA.Module.BusinessObjects.Lookups;
using ComplyEA.Module.BusinessObjects.Regulatory;

namespace ComplyEA.Module.BusinessObjects.Compliance
{
    [DefaultClassOptions]
    [NavigationItem("Compliance")]
    [ImageName("ComplianceTemplate")]
    public class ComplianceTemplate : BaseObject
    {
        public ComplianceTemplate(Session session) : base(session) { }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            CreatedOn = DateTime.Now;
            IsActive = true;
        }

        string name;
        [RuleRequiredField]
        [Size(200)]
        public string Name
        {
            get => name;
            set => SetPropertyValue(nameof(Name), ref name, value);
        }

        string code;
        [Size(50)]
        [Indexed(Unique = true)]
        public string Code
        {
            get => code;
            set => SetPropertyValue(nameof(Code), ref code, value);
        }

        string description;
        [Size(SizeAttribute.Unlimited)]
        public string Description
        {
            get => description;
            set => SetPropertyValue(nameof(Description), ref description, value);
        }

        TemplateCategory category;
        [Association("TemplateCategory-Templates")]
        public TemplateCategory Category
        {
            get => category;
            set => SetPropertyValue(nameof(Category), ref category, value);
        }

        ComplianceRequirement complianceRequirement;
        [Association("ComplianceRequirement-Templates")]
        [ToolTip("Specific requirement this template satisfies")]
        public ComplianceRequirement ComplianceRequirement
        {
            get => complianceRequirement;
            set => SetPropertyValue(nameof(ComplianceRequirement), ref complianceRequirement, value);
        }

        FileFormat fileFormat;
        [Association("FileFormat-Templates")]
        public FileFormat FileFormat
        {
            get => fileFormat;
            set => SetPropertyValue(nameof(FileFormat), ref fileFormat, value);
        }

        byte[] templateContent;
        [Size(SizeAttribute.Unlimited)]
        [Delayed(true)]
        [VisibleInListView(false)]
        [VisibleInDetailView(false)]
        public byte[] TemplateContent
        {
            get => GetDelayedPropertyValue<byte[]>(nameof(TemplateContent));
            set => SetDelayedPropertyValue(nameof(TemplateContent), value);
        }

        string fileName;
        [Size(300)]
        public string FileName
        {
            get => fileName;
            set => SetPropertyValue(nameof(FileName), ref fileName, value);
        }

        long fileSize;
        public long FileSize
        {
            get => fileSize;
            set => SetPropertyValue(nameof(FileSize), ref fileSize, value);
        }

        string version;
        [Size(20)]
        public string Version
        {
            get => version;
            set => SetPropertyValue(nameof(Version), ref version, value);
        }

        DateTime createdOn;
        [VisibleInListView(false)]
        public DateTime CreatedOn
        {
            get => createdOn;
            set => SetPropertyValue(nameof(CreatedOn), ref createdOn, value);
        }

        DateTime? lastUpdatedOn;
        public DateTime? LastUpdatedOn
        {
            get => lastUpdatedOn;
            set => SetPropertyValue(nameof(LastUpdatedOn), ref lastUpdatedOn, value);
        }

        bool isActive;
        public bool IsActive
        {
            get => isActive;
            set => SetPropertyValue(nameof(IsActive), ref isActive, value);
        }

        string instructions;
        [Size(SizeAttribute.Unlimited)]
        [ToolTip("Instructions for using this template")]
        public string Instructions
        {
            get => instructions;
            set => SetPropertyValue(nameof(Instructions), ref instructions, value);
        }

        protected override void OnSaving()
        {
            base.OnSaving();
            LastUpdatedOn = DateTime.Now;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}

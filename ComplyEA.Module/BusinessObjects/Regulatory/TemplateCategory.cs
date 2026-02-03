using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;

namespace ComplyEA.Module.BusinessObjects.Regulatory
{
    [DefaultClassOptions]
    [NavigationItem("Regulatory")]
    [ImageName("BO_Folder")]
    public class TemplateCategory : BaseObject
    {
        public TemplateCategory(Session session) : base(session) { }

        string name;
        [RuleRequiredField]
        [Size(100)]
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
        [Size(500)]
        public string Description
        {
            get => description;
            set => SetPropertyValue(nameof(Description), ref description, value);
        }

        TemplateCategory parentCategory;
        [Association("TemplateCategory-SubCategories")]
        public TemplateCategory ParentCategory
        {
            get => parentCategory;
            set => SetPropertyValue(nameof(ParentCategory), ref parentCategory, value);
        }

        int sortOrder;
        public int SortOrder
        {
            get => sortOrder;
            set => SetPropertyValue(nameof(SortOrder), ref sortOrder, value);
        }

        bool isActive = true;
        public bool IsActive
        {
            get => isActive;
            set => SetPropertyValue(nameof(IsActive), ref isActive, value);
        }

        [Association("TemplateCategory-SubCategories")]
        public XPCollection<TemplateCategory> SubCategories => GetCollection<TemplateCategory>(nameof(SubCategories));

        [Association("TemplateCategory-Requirements")]
        public XPCollection<ComplianceRequirement> Requirements => GetCollection<ComplianceRequirement>(nameof(Requirements));

        [Association("TemplateCategory-Templates")]
        public XPCollection<Compliance.ComplianceTemplate> Templates => GetCollection<Compliance.ComplianceTemplate>(nameof(Templates));

        public override string ToString()
        {
            return Name;
        }
    }
}

using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;

namespace ComplyEA.Module.BusinessObjects.Regulatory
{
    [DefaultClassOptions]
    [NavigationItem("Regulatory")]
    [ImageName("BO_List")]
    public class ActAcronym : BaseObject
    {
        public ActAcronym(Session session) : base(session) { }

        RegulatoryAct regulatoryAct;
        [RuleRequiredField]
        [Association("RegulatoryAct-Acronyms")]
        public RegulatoryAct RegulatoryAct
        {
            get => regulatoryAct;
            set => SetPropertyValue(nameof(RegulatoryAct), ref regulatoryAct, value);
        }

        string acronym;
        [RuleRequiredField]
        [Size(50)]
        [Indexed]
        public string Acronym
        {
            get => acronym;
            set => SetPropertyValue(nameof(Acronym), ref acronym, value);
        }

        string fullForm;
        [RuleRequiredField]
        [Size(300)]
        public string FullForm
        {
            get => fullForm;
            set => SetPropertyValue(nameof(FullForm), ref fullForm, value);
        }

        string definition;
        [Size(SizeAttribute.Unlimited)]
        public string Definition
        {
            get => definition;
            set => SetPropertyValue(nameof(Definition), ref definition, value);
        }

        string sectionReference;
        [Size(100)]
        [ToolTip("Section of the act where this term is defined")]
        public string SectionReference
        {
            get => sectionReference;
            set => SetPropertyValue(nameof(SectionReference), ref sectionReference, value);
        }

        public override string ToString()
        {
            return $"{Acronym} - {FullForm}";
        }
    }
}

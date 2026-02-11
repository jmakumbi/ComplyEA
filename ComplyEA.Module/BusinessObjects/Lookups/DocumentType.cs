using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using ComplyEA.Module.BusinessObjects.Compliance;

namespace ComplyEA.Module.BusinessObjects.Lookups
{
    [DefaultClassOptions]
    [NavigationItem("Administration")]
    [ImageName("DocumentType")]
    public class DocumentType : BaseLookup
    {
        public DocumentType(Session session) : base(session) { }

        bool isEvidence;
        [ToolTip("Indicates if this document type serves as compliance evidence")]
        public bool IsEvidence
        {
            get => isEvidence;
            set => SetPropertyValue(nameof(IsEvidence), ref isEvidence, value);
        }

        [Association("DocumentType-Documents")]
        public XPCollection<ComplianceDocument> Documents => GetCollection<ComplianceDocument>(nameof(Documents));
    }
}

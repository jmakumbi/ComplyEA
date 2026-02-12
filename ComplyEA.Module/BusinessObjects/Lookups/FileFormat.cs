using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using ComplyEA.Module.BusinessObjects.Compliance;

namespace ComplyEA.Module.BusinessObjects.Lookups
{
    [DefaultClassOptions]
    [NavigationItem("Administration")]
    [ImageName("BO_Document")]
    public class FileFormat : BaseLookup
    {
        public FileFormat(Session session) : base(session) { }

        string extension;
        [Size(10)]
        [ToolTip("File extension without dot (e.g., 'pdf', 'docx')")]
        public string Extension
        {
            get => extension;
            set => SetPropertyValue(nameof(Extension), ref extension, value);
        }

        string mimeType;
        [Size(100)]
        public string MimeType
        {
            get => mimeType;
            set => SetPropertyValue(nameof(MimeType), ref mimeType, value);
        }

        [Association("FileFormat-Documents")]
        public XPCollection<ComplianceDocument> Documents => GetCollection<ComplianceDocument>(nameof(Documents));

        [Association("FileFormat-Templates")]
        public XPCollection<ComplianceTemplate> Templates => GetCollection<ComplianceTemplate>(nameof(Templates));
    }
}

using System;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using ComplyEA.Module.BusinessObjects.Lookups;
using ComplyEA.Module.BusinessObjects.Security;

namespace ComplyEA.Module.BusinessObjects.Compliance
{
    [DefaultClassOptions]
    [NavigationItem("Compliance")]
    [ImageName("BO_FileAttachment")]
    public class ComplianceDocument : BaseObject
    {
        public ComplianceDocument(Session session) : base(session) { }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            UploadedOn = DateTime.Now;
        }

        ComplianceObligation complianceObligation;
        [RuleRequiredField]
        [Association("ComplianceObligation-Documents")]
        public ComplianceObligation ComplianceObligation
        {
            get => complianceObligation;
            set => SetPropertyValue(nameof(ComplianceObligation), ref complianceObligation, value);
        }

        string fileName;
        [RuleRequiredField]
        [Size(300)]
        public string FileName
        {
            get => fileName;
            set => SetPropertyValue(nameof(FileName), ref fileName, value);
        }

        string displayName;
        [Size(300)]
        [ToolTip("User-friendly name for the document")]
        public string DisplayName
        {
            get => string.IsNullOrEmpty(displayName) ? FileName : displayName;
            set => SetPropertyValue(nameof(DisplayName), ref displayName, value);
        }

        DocumentType documentType;
        [Association("DocumentType-Documents")]
        public DocumentType DocumentType
        {
            get => documentType;
            set => SetPropertyValue(nameof(DocumentType), ref documentType, value);
        }

        FileFormat fileFormat;
        [Association("FileFormat-Documents")]
        public FileFormat FileFormat
        {
            get => fileFormat;
            set => SetPropertyValue(nameof(FileFormat), ref fileFormat, value);
        }

        byte[] fileContent;
        [Size(SizeAttribute.Unlimited)]
        [Delayed(true)]
        [VisibleInListView(false)]
        [VisibleInDetailView(false)]
        public byte[] FileContent
        {
            get => GetDelayedPropertyValue<byte[]>(nameof(FileContent));
            set => SetDelayedPropertyValue(nameof(FileContent), value);
        }

        long fileSize;
        public long FileSize
        {
            get => fileSize;
            set => SetPropertyValue(nameof(FileSize), ref fileSize, value);
        }

        string mimeType;
        [Size(100)]
        [VisibleInListView(false)]
        public string MimeType
        {
            get => mimeType;
            set => SetPropertyValue(nameof(MimeType), ref mimeType, value);
        }

        string description;
        [Size(SizeAttribute.Unlimited)]
        public string Description
        {
            get => description;
            set => SetPropertyValue(nameof(Description), ref description, value);
        }

        DateTime uploadedOn;
        public DateTime UploadedOn
        {
            get => uploadedOn;
            set => SetPropertyValue(nameof(UploadedOn), ref uploadedOn, value);
        }

        ApplicationUser uploadedBy;
        [Association("ApplicationUser-UploadedDocuments")]
        public ApplicationUser UploadedBy
        {
            get => uploadedBy;
            set => SetPropertyValue(nameof(UploadedBy), ref uploadedBy, value);
        }

        string version;
        [Size(20)]
        public string Version
        {
            get => version;
            set => SetPropertyValue(nameof(Version), ref version, value);
        }

        bool isCurrentVersion = true;
        public bool IsCurrentVersion
        {
            get => isCurrentVersion;
            set => SetPropertyValue(nameof(IsCurrentVersion), ref isCurrentVersion, value);
        }

        ComplianceDocument previousVersion;
        [Association("ComplianceDocument-Versions")]
        public ComplianceDocument PreviousVersion
        {
            get => previousVersion;
            set => SetPropertyValue(nameof(PreviousVersion), ref previousVersion, value);
        }

        [Association("ComplianceDocument-Versions")]
        public XPCollection<ComplianceDocument> NewerVersions => GetCollection<ComplianceDocument>(nameof(NewerVersions));

        public override string ToString()
        {
            return DisplayName;
        }
    }
}

using System;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using DevExpress.Xpo;
using ComplyEA.Module.BusinessObjects.Organization;

namespace ComplyEA.Module.BusinessObjects.Security
{
    [DefaultClassOptions]
    [NavigationItem("Security")]
    [ImageName("ApplicationUser")]
    public class ApplicationUser : PermissionPolicyUser
    {
        public ApplicationUser(Session session) : base(session) { }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            CreatedOn = DateTime.Now;
        }

        string firstName;
        [Size(100)]
        public string FirstName
        {
            get => firstName;
            set => SetPropertyValue(nameof(FirstName), ref firstName, value);
        }

        string lastName;
        [Size(100)]
        public string LastName
        {
            get => lastName;
            set => SetPropertyValue(nameof(LastName), ref lastName, value);
        }

        [PersistentAlias("Concat([FirstName], ' ', [LastName])")]
        public string FullName => (string)EvaluateAlias(nameof(FullName));

        string email;
        [Size(200)]
        public string Email
        {
            get => email;
            set => SetPropertyValue(nameof(Email), ref email, value);
        }

        string phone;
        [Size(50)]
        public string Phone
        {
            get => phone;
            set => SetPropertyValue(nameof(Phone), ref phone, value);
        }

        LegalFirm legalFirm;
        [Association("LegalFirm-Users")]
        [ToolTip("Legal firm this user belongs to (for multi-tenancy)")]
        public LegalFirm LegalFirm
        {
            get => legalFirm;
            set => SetPropertyValue(nameof(LegalFirm), ref legalFirm, value);
        }

        Company defaultCompany;
        [Association("Company-DefaultUsers")]
        [ToolTip("Default company context for this user")]
        public Company DefaultCompany
        {
            get => defaultCompany;
            set => SetPropertyValue(nameof(DefaultCompany), ref defaultCompany, value);
        }

        DateTime createdOn;
        [VisibleInListView(false)]
        public DateTime CreatedOn
        {
            get => createdOn;
            set => SetPropertyValue(nameof(CreatedOn), ref createdOn, value);
        }

        DateTime? lastLoginDate;
        [VisibleInListView(false)]
        public DateTime? LastLoginDate
        {
            get => lastLoginDate;
            set => SetPropertyValue(nameof(LastLoginDate), ref lastLoginDate, value);
        }

        bool isSystemAdmin;
        [ToolTip("System administrators can access all tenants")]
        public bool IsSystemAdmin
        {
            get => isSystemAdmin;
            set => SetPropertyValue(nameof(IsSystemAdmin), ref isSystemAdmin, value);
        }

        [Association("ApplicationUser-UploadedDocuments")]
        public XPCollection<Compliance.ComplianceDocument> UploadedDocuments => GetCollection<Compliance.ComplianceDocument>(nameof(UploadedDocuments));

        [Association("ApplicationUser-CalendarSettings")]
        public XPCollection<Configuration.CompanyCalendarSettings> CalendarSettings => GetCollection<Configuration.CompanyCalendarSettings>(nameof(CalendarSettings));
    }
}

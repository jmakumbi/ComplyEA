using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using ComplyEA.Module.BusinessObjects.Organization;

namespace ComplyEA.Module.BusinessObjects.Lookups
{
    [DefaultClassOptions]
    [NavigationItem("Administration")]
    [ImageName("ComplianceRole")]
    public class ComplianceRole : BaseLookup
    {
        public ComplianceRole(Session session) : base(session) { }

        bool canReceiveReminders = true;
        public bool CanReceiveReminders
        {
            get => canReceiveReminders;
            set => SetPropertyValue(nameof(CanReceiveReminders), ref canReceiveReminders, value);
        }

        bool canApproveSubmissions;
        public bool CanApproveSubmissions
        {
            get => canApproveSubmissions;
            set => SetPropertyValue(nameof(CanApproveSubmissions), ref canApproveSubmissions, value);
        }

        [Association("ComplianceRole-Contacts")]
        public XPCollection<CompanyContact> Contacts => GetCollection<CompanyContact>(nameof(Contacts));
    }
}

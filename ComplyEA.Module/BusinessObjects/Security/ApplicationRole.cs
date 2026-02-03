using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using DevExpress.Xpo;

namespace ComplyEA.Module.BusinessObjects.Security
{
    [DefaultClassOptions]
    [NavigationItem("Security")]
    [ImageName("BO_Role")]
    public class ApplicationRole : PermissionPolicyRole
    {
        public ApplicationRole(Session session) : base(session) { }

        string description;
        [Size(500)]
        public string Description
        {
            get => description;
            set => SetPropertyValue(nameof(Description), ref description, value);
        }

        bool isSystemRole;
        [ToolTip("System roles cannot be deleted")]
        public bool IsSystemRole
        {
            get => isSystemRole;
            set => SetPropertyValue(nameof(IsSystemRole), ref isSystemRole, value);
        }

        bool canManageOwnFirm;
        [ToolTip("Can manage legal firm settings")]
        public bool CanManageOwnFirm
        {
            get => canManageOwnFirm;
            set => SetPropertyValue(nameof(CanManageOwnFirm), ref canManageOwnFirm, value);
        }

        bool canManageClients;
        [ToolTip("Can manage client companies")]
        public bool CanManageClients
        {
            get => canManageClients;
            set => SetPropertyValue(nameof(CanManageClients), ref canManageClients, value);
        }

        bool canManageCompliance;
        [ToolTip("Can manage compliance obligations")]
        public bool CanManageCompliance
        {
            get => canManageCompliance;
            set => SetPropertyValue(nameof(CanManageCompliance), ref canManageCompliance, value);
        }

        bool canViewReports;
        [ToolTip("Can view compliance reports")]
        public bool CanViewReports
        {
            get => canViewReports;
            set => SetPropertyValue(nameof(CanViewReports), ref canViewReports, value);
        }

        bool canUploadDocuments;
        [ToolTip("Can upload compliance documents")]
        public bool CanUploadDocuments
        {
            get => canUploadDocuments;
            set => SetPropertyValue(nameof(CanUploadDocuments), ref canUploadDocuments, value);
        }

        bool canConfigureIntegrations;
        [ToolTip("Can configure email/calendar integrations")]
        public bool CanConfigureIntegrations
        {
            get => canConfigureIntegrations;
            set => SetPropertyValue(nameof(CanConfigureIntegrations), ref canConfigureIntegrations, value);
        }
    }
}

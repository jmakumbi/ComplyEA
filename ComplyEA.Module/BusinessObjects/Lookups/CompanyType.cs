using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using ComplyEA.Module.BusinessObjects.Organization;
using ComplyEA.Module.BusinessObjects.Regulatory;

namespace ComplyEA.Module.BusinessObjects.Lookups
{
    [DefaultClassOptions]
    [NavigationItem("Administration")]
    [ImageName("BO_Organization")]
    public class CompanyType : BaseLookup
    {
        public CompanyType(Session session) : base(session) { }

        [Association("CompanyType-Companies")]
        public XPCollection<Company> Companies => GetCollection<Company>(nameof(Companies));

        [Association("CompanyType-Requirements")]
        public XPCollection<ComplianceRequirement> Requirements => GetCollection<ComplianceRequirement>(nameof(Requirements));
    }
}

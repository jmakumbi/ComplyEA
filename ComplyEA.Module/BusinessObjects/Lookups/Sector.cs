using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using ComplyEA.Module.BusinessObjects.Organization;
using ComplyEA.Module.BusinessObjects.Regulatory;

namespace ComplyEA.Module.BusinessObjects.Lookups
{
    [DefaultClassOptions]
    [NavigationItem("Administration")]
    [ImageName("Sector")]
    public class Sector : BaseLookup
    {
        public Sector(Session session) : base(session) { }

        bool requiresSpecificRegulation;
        public bool RequiresSpecificRegulation
        {
            get => requiresSpecificRegulation;
            set => SetPropertyValue(nameof(RequiresSpecificRegulation), ref requiresSpecificRegulation, value);
        }

        [Association("Sector-Companies")]
        public XPCollection<Company> Companies => GetCollection<Company>(nameof(Companies));

        [Association("Sector-RegulatoryActs")]
        public XPCollection<RegulatoryAct> RegulatoryActs => GetCollection<RegulatoryAct>(nameof(RegulatoryActs));
    }
}

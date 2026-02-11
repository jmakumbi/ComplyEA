using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using ComplyEA.Module.BusinessObjects.Regulatory;

namespace ComplyEA.Module.BusinessObjects.Lookups
{
    [DefaultClassOptions]
    [NavigationItem("Administration")]
    [ImageName("RegulationScope")]
    public class RegulationScope : BaseLookup
    {
        public RegulationScope(Session session) : base(session) { }

        [Association("RegulationScope-RegulatoryActs")]
        public XPCollection<RegulatoryAct> RegulatoryActs => GetCollection<RegulatoryAct>(nameof(RegulatoryActs));
    }
}

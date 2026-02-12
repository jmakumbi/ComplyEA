using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using ComplyEA.Module.BusinessObjects.Regulatory;

namespace ComplyEA.Module.BusinessObjects.Lookups
{
    [DefaultClassOptions]
    [NavigationItem("Administration")]
    [ImageName("BO_Contract")]
    public class ActType : BaseLookup
    {
        public ActType(Session session) : base(session) { }

        [Association("ActType-RegulatoryActs")]
        public XPCollection<RegulatoryAct> RegulatoryActs => GetCollection<RegulatoryAct>(nameof(RegulatoryActs));
    }
}

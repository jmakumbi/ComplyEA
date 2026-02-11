using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using ComplyEA.Module.BusinessObjects.Organization;

namespace ComplyEA.Module.BusinessObjects.Lookups
{
    [DefaultClassOptions]
    [NavigationItem("Administration")]
    [ImageName("SubscriptionType")]
    public class SubscriptionType : BaseLookup
    {
        public SubscriptionType(Session session) : base(session) { }

        decimal monthlyPrice;
        public decimal MonthlyPrice
        {
            get => monthlyPrice;
            set => SetPropertyValue(nameof(MonthlyPrice), ref monthlyPrice, value);
        }

        int maxCompanies;
        public int MaxCompanies
        {
            get => maxCompanies;
            set => SetPropertyValue(nameof(MaxCompanies), ref maxCompanies, value);
        }

        int maxUsers;
        public int MaxUsers
        {
            get => maxUsers;
            set => SetPropertyValue(nameof(MaxUsers), ref maxUsers, value);
        }

        [Association("SubscriptionType-LegalFirms")]
        public XPCollection<LegalFirm> LegalFirms => GetCollection<LegalFirm>(nameof(LegalFirms));
    }
}

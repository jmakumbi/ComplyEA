using DevExpress.Persistent.Base;
using DevExpress.Xpo;

namespace ComplyEA.Module.BusinessObjects.Lookups
{
    [DefaultClassOptions]
    [NavigationItem("Administration")]
    [ImageName("BO_Phone")]
    public class SMSProvider : BaseLookup
    {
        public SMSProvider(Session session) : base(session) { }

        string apiEndpoint;
        [Size(500)]
        public string ApiEndpoint
        {
            get => apiEndpoint;
            set => SetPropertyValue(nameof(ApiEndpoint), ref apiEndpoint, value);
        }

        bool supportsUnicode;
        public bool SupportsUnicode
        {
            get => supportsUnicode;
            set => SetPropertyValue(nameof(SupportsUnicode), ref supportsUnicode, value);
        }
    }
}

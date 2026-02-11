using DevExpress.Persistent.Base;
using DevExpress.Xpo;

namespace ComplyEA.Module.BusinessObjects.Lookups
{
    [DefaultClassOptions]
    [NavigationItem("Administration")]
    [ImageName("EmailProvider")]
    public class EmailProvider : BaseLookup
    {
        public EmailProvider(Session session) : base(session) { }

        string oAuthEndpoint;
        [Size(500)]
        public string OAuthEndpoint
        {
            get => oAuthEndpoint;
            set => SetPropertyValue(nameof(OAuthEndpoint), ref oAuthEndpoint, value);
        }

        string apiEndpoint;
        [Size(500)]
        public string ApiEndpoint
        {
            get => apiEndpoint;
            set => SetPropertyValue(nameof(ApiEndpoint), ref apiEndpoint, value);
        }
    }
}

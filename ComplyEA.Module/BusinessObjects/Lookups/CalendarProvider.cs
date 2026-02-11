using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using ComplyEA.Module.BusinessObjects.Configuration;

namespace ComplyEA.Module.BusinessObjects.Lookups
{
    [DefaultClassOptions]
    [NavigationItem("Administration")]
    [ImageName("CalendarProvider")]
    public class CalendarProvider : BaseLookup
    {
        public CalendarProvider(Session session) : base(session) { }

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

        [Association("CalendarProvider-CalendarSettings")]
        public XPCollection<CompanyCalendarSettings> CalendarSettings => GetCollection<CompanyCalendarSettings>(nameof(CalendarSettings));
    }
}

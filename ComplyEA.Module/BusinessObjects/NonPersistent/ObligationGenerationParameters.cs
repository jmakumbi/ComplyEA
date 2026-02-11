using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using ComplyEA.Module.BusinessObjects.Organization;

namespace ComplyEA.Module.BusinessObjects.NonPersistent
{
    [DomainComponent]
    [DefaultClassOptions]
    [ImageName("ObligationGenerationParameters")]
    public class ObligationGenerationParameters : IXafEntityObject
    {
        private IObjectSpace objectSpace;

        public ObligationGenerationParameters()
        {
            Year = DateTime.Now.Year;
            Quarter = (DateTime.Now.Month - 1) / 3 + 1;
            IncludeAnnual = true;
            IncludeQuarterly = true;
            IncludeMonthly = true;
        }

        [ImmediatePostData]
        public int Year { get; set; }

        [ImmediatePostData]
        [Description("Leave empty to generate for all quarters")]
        public int? Quarter { get; set; }

        [ImmediatePostData]
        [Description("Leave empty to generate for all months (or months in selected quarter)")]
        public int? Month { get; set; }

        [DataSourceProperty("AvailableCompanies")]
        [Description("Leave empty to generate for all companies")]
        public Company Company { get; set; }

        [Browsable(false)]
        public IList<Company> AvailableCompanies
        {
            get
            {
                if (objectSpace == null)
                    return new List<Company>();
                return objectSpace.GetObjects<Company>(
                    new DevExpress.Data.Filtering.BinaryOperator("IsActive", true))
                    .OrderBy(c => c.Name)
                    .ToList();
            }
        }

        [Description("Include annual filing requirements")]
        public bool IncludeAnnual { get; set; }

        [Description("Include quarterly filing requirements")]
        public bool IncludeQuarterly { get; set; }

        [Description("Include monthly filing requirements")]
        public bool IncludeMonthly { get; set; }

        [Description("Include ad-hoc requirements (event-driven and always/ongoing timelines)")]
        public bool IncludeAdhoc { get; set; }

        [Description("Estimated number of obligations that will be generated")]
        public int? PreviewCount { get; set; }

        [Browsable(false)]
        public int? ResultCount { get; set; }

        #region IXafEntityObject Members

        void IXafEntityObject.OnCreated()
        {
        }

        void IXafEntityObject.OnLoaded()
        {
        }

        void IXafEntityObject.OnSaving()
        {
        }

        #endregion

        public void SetObjectSpace(IObjectSpace os)
        {
            objectSpace = os;
        }

        public override string ToString()
        {
            var period = $"Year: {Year}";
            if (Quarter.HasValue)
                period += $", Q{Quarter}";
            if (Month.HasValue)
                period += $", Month {Month}";
            return period;
        }
    }
}

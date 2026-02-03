using System;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using ComplyEA.Module.BusinessObjects.Compliance;
using ComplyEA.Module.BusinessObjects.Configuration;
using ComplyEA.Module.BusinessObjects.Lookups;
using ComplyEA.Module.BusinessObjects.Security;

namespace ComplyEA.Module.BusinessObjects.Organization
{
    [DefaultClassOptions]
    [NavigationItem("Organization")]
    [ImageName("BO_Company")]
    public class Company : BaseObject
    {
        public Company(Session session) : base(session) { }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            CreatedOn = DateTime.Now;
            IsActive = true;
        }

        LegalFirm legalFirm;
        [RuleRequiredField]
        [Association("LegalFirm-Companies")]
        public LegalFirm LegalFirm
        {
            get => legalFirm;
            set => SetPropertyValue(nameof(LegalFirm), ref legalFirm, value);
        }

        string name;
        [RuleRequiredField]
        [Size(200)]
        public string Name
        {
            get => name;
            set => SetPropertyValue(nameof(Name), ref name, value);
        }

        string shortName;
        [Size(50)]
        public string ShortName
        {
            get => shortName;
            set => SetPropertyValue(nameof(ShortName), ref shortName, value);
        }

        string registrationNumber;
        [Size(100)]
        public string RegistrationNumber
        {
            get => registrationNumber;
            set => SetPropertyValue(nameof(RegistrationNumber), ref registrationNumber, value);
        }

        string taxPin;
        [Size(50)]
        public string TaxPin
        {
            get => taxPin;
            set => SetPropertyValue(nameof(TaxPin), ref taxPin, value);
        }

        CompanyType companyType;
        [Association("CompanyType-Companies")]
        public CompanyType CompanyType
        {
            get => companyType;
            set => SetPropertyValue(nameof(CompanyType), ref companyType, value);
        }

        Sector sector;
        [Association("Sector-Companies")]
        public Sector Sector
        {
            get => sector;
            set => SetPropertyValue(nameof(Sector), ref sector, value);
        }

        DateTime? incorporationDate;
        public DateTime? IncorporationDate
        {
            get => incorporationDate;
            set => SetPropertyValue(nameof(IncorporationDate), ref incorporationDate, value);
        }

        DateTime? financialYearEnd;
        [ToolTip("Month and day of financial year end")]
        public DateTime? FinancialYearEnd
        {
            get => financialYearEnd;
            set => SetPropertyValue(nameof(FinancialYearEnd), ref financialYearEnd, value);
        }

        string email;
        [Size(200)]
        [RuleRegularExpression(null, DefaultContexts.Save, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", CustomMessageTemplate = "Invalid email format")]
        public string Email
        {
            get => email;
            set => SetPropertyValue(nameof(Email), ref email, value);
        }

        string phone;
        [Size(50)]
        public string Phone
        {
            get => phone;
            set => SetPropertyValue(nameof(Phone), ref phone, value);
        }

        string address;
        [Size(500)]
        public string Address
        {
            get => address;
            set => SetPropertyValue(nameof(Address), ref address, value);
        }

        string city;
        [Size(100)]
        public string City
        {
            get => city;
            set => SetPropertyValue(nameof(City), ref city, value);
        }

        string country;
        [Size(100)]
        public string Country
        {
            get => country;
            set => SetPropertyValue(nameof(Country), ref country, value);
        }

        DateTime createdOn;
        [VisibleInListView(false)]
        public DateTime CreatedOn
        {
            get => createdOn;
            set => SetPropertyValue(nameof(CreatedOn), ref createdOn, value);
        }

        bool isActive;
        public bool IsActive
        {
            get => isActive;
            set => SetPropertyValue(nameof(IsActive), ref isActive, value);
        }

        string notes;
        [Size(SizeAttribute.Unlimited)]
        public string Notes
        {
            get => notes;
            set => SetPropertyValue(nameof(Notes), ref notes, value);
        }

        [Association("Company-Contacts")]
        public XPCollection<CompanyContact> Contacts => GetCollection<CompanyContact>(nameof(Contacts));

        [Association("Company-ApplicableRegulations")]
        public XPCollection<Regulatory.ApplicableRegulation> ApplicableRegulations => GetCollection<Regulatory.ApplicableRegulation>(nameof(ApplicableRegulations));

        [Association("Company-Obligations")]
        public XPCollection<ComplianceObligation> Obligations => GetCollection<ComplianceObligation>(nameof(Obligations));

        [Association("Company-ReminderSettings")]
        public XPCollection<CompanyReminderSettings> ReminderSettings => GetCollection<CompanyReminderSettings>(nameof(ReminderSettings));

        [Association("Company-CalendarSettings")]
        public XPCollection<CompanyCalendarSettings> CalendarSettings => GetCollection<CompanyCalendarSettings>(nameof(CalendarSettings));

        [Association("Company-DefaultUsers")]
        public XPCollection<ApplicationUser> DefaultUsers => GetCollection<ApplicationUser>(nameof(DefaultUsers));

        public override string ToString()
        {
            return Name;
        }
    }
}

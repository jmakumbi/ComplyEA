using System;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using ComplyEA.Module.BusinessObjects.Lookups;
using ComplyEA.Module.BusinessObjects.Security;

namespace ComplyEA.Module.BusinessObjects.Organization
{
    [DefaultClassOptions]
    [NavigationItem("Organization")]
    [ImageName("BO_Organization")]
    public class LegalFirm : BaseObject
    {
        public LegalFirm(Session session) : base(session) { }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            CreatedOn = DateTime.Now;
            IsActive = true;
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

        SubscriptionType subscriptionType;
        [Association("SubscriptionType-LegalFirms")]
        public SubscriptionType SubscriptionType
        {
            get => subscriptionType;
            set => SetPropertyValue(nameof(SubscriptionType), ref subscriptionType, value);
        }

        DateTime? subscriptionStartDate;
        public DateTime? SubscriptionStartDate
        {
            get => subscriptionStartDate;
            set => SetPropertyValue(nameof(SubscriptionStartDate), ref subscriptionStartDate, value);
        }

        DateTime? subscriptionEndDate;
        public DateTime? SubscriptionEndDate
        {
            get => subscriptionEndDate;
            set => SetPropertyValue(nameof(SubscriptionEndDate), ref subscriptionEndDate, value);
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

        [Association("LegalFirm-Companies")]
        public XPCollection<Company> ManagedCompanies => GetCollection<Company>(nameof(ManagedCompanies));

        [Association("LegalFirm-Users")]
        public XPCollection<ApplicationUser> Users => GetCollection<ApplicationUser>(nameof(Users));

        public override string ToString()
        {
            return Name;
        }
    }
}

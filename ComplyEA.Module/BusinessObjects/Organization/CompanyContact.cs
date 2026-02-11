using System;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using ComplyEA.Module.BusinessObjects.Compliance;
using ComplyEA.Module.BusinessObjects.Configuration;
using ComplyEA.Module.BusinessObjects.Lookups;

namespace ComplyEA.Module.BusinessObjects.Organization
{
    [DefaultClassOptions]
    [NavigationItem("Organization")]
    [ImageName("CompanyContact")]
    public class CompanyContact : BaseObject
    {
        public CompanyContact(Session session) : base(session) { }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            CreatedOn = DateTime.Now;
            IsActive = true;
            ReceiveReminders = true;
        }

        Company company;
        [RuleRequiredField]
        [Association("Company-Contacts")]
        public Company Company
        {
            get => company;
            set => SetPropertyValue(nameof(Company), ref company, value);
        }

        string firstName;
        [RuleRequiredField]
        [Size(100)]
        public string FirstName
        {
            get => firstName;
            set => SetPropertyValue(nameof(FirstName), ref firstName, value);
        }

        string lastName;
        [RuleRequiredField]
        [Size(100)]
        public string LastName
        {
            get => lastName;
            set => SetPropertyValue(nameof(LastName), ref lastName, value);
        }

        [PersistentAlias("Concat([FirstName], ' ', [LastName])")]
        public string FullName => (string)EvaluateAlias(nameof(FullName));

        string title;
        [Size(50)]
        public string Title
        {
            get => title;
            set => SetPropertyValue(nameof(Title), ref title, value);
        }

        string jobTitle;
        [Size(100)]
        public string JobTitle
        {
            get => jobTitle;
            set => SetPropertyValue(nameof(JobTitle), ref jobTitle, value);
        }

        ComplianceRole complianceRole;
        [Association("ComplianceRole-Contacts")]
        public ComplianceRole ComplianceRole
        {
            get => complianceRole;
            set => SetPropertyValue(nameof(ComplianceRole), ref complianceRole, value);
        }

        string email;
        [Size(200)]
        [RuleRequiredField]
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

        string mobilePhone;
        [Size(50)]
        public string MobilePhone
        {
            get => mobilePhone;
            set => SetPropertyValue(nameof(MobilePhone), ref mobilePhone, value);
        }

        bool isPrimaryContact;
        [ToolTip("Primary contact for compliance matters")]
        public bool IsPrimaryContact
        {
            get => isPrimaryContact;
            set => SetPropertyValue(nameof(IsPrimaryContact), ref isPrimaryContact, value);
        }

        bool receiveReminders;
        public bool ReceiveReminders
        {
            get => receiveReminders;
            set => SetPropertyValue(nameof(ReceiveReminders), ref receiveReminders, value);
        }

        NotificationChannel preferredNotificationChannel;
        [Association("NotificationChannel-Contacts")]
        public NotificationChannel PreferredNotificationChannel
        {
            get => preferredNotificationChannel;
            set => SetPropertyValue(nameof(PreferredNotificationChannel), ref preferredNotificationChannel, value);
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

        [Association("CompanyContact-AssignedObligations")]
        public XPCollection<ComplianceObligation> AssignedObligations => GetCollection<ComplianceObligation>(nameof(AssignedObligations));

        [Association("CompanyContact-Reminders")]
        public XPCollection<ComplianceReminder> Reminders => GetCollection<ComplianceReminder>(nameof(Reminders));

        [Association("CompanyContact-EscalationSettings")]
        public XPCollection<CompanyReminderSettings> EscalationSettings => GetCollection<CompanyReminderSettings>(nameof(EscalationSettings));

        public override string ToString()
        {
            return FullName ?? $"{FirstName} {LastName}";
        }
    }
}

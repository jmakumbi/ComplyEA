using System;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using ComplyEA.Module.BusinessObjects.Lookups;

namespace ComplyEA.Module.BusinessObjects.Configuration
{
    [DefaultClassOptions]
    [NavigationItem("Configuration")]
    [ImageName("EmailTemplate")]
    public class EmailTemplate : BaseObject
    {
        public EmailTemplate(Session session) : base(session) { }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            CreatedOn = DateTime.Now;
            IsActive = true;
        }

        string name;
        [RuleRequiredField]
        [Size(100)]
        public string Name
        {
            get => name;
            set => SetPropertyValue(nameof(Name), ref name, value);
        }

        string code;
        [Size(50)]
        [Indexed(Unique = true)]
        [ToolTip("Unique code for programmatic reference")]
        public string Code
        {
            get => code;
            set => SetPropertyValue(nameof(Code), ref code, value);
        }

        string description;
        [Size(500)]
        public string Description
        {
            get => description;
            set => SetPropertyValue(nameof(Description), ref description, value);
        }

        ReminderType reminderType;
        [Association("ReminderType-EmailTemplates")]
        [ToolTip("Associated reminder type")]
        public ReminderType ReminderType
        {
            get => reminderType;
            set => SetPropertyValue(nameof(ReminderType), ref reminderType, value);
        }

        string subject;
        [RuleRequiredField]
        [Size(300)]
        public string Subject
        {
            get => subject;
            set => SetPropertyValue(nameof(Subject), ref subject, value);
        }

        string bodyHtml;
        [Size(SizeAttribute.Unlimited)]
        [ToolTip("HTML body of the email. Supports placeholders like {{CompanyName}}, {{DueDate}}, {{RequirementTitle}}")]
        public string BodyHtml
        {
            get => bodyHtml;
            set => SetPropertyValue(nameof(BodyHtml), ref bodyHtml, value);
        }

        string bodyText;
        [Size(SizeAttribute.Unlimited)]
        [ToolTip("Plain text body of the email (fallback)")]
        public string BodyText
        {
            get => bodyText;
            set => SetPropertyValue(nameof(BodyText), ref bodyText, value);
        }

        string smsTemplate;
        [Size(500)]
        [ToolTip("SMS message template (max 160 characters recommended)")]
        public string SmsTemplate
        {
            get => smsTemplate;
            set => SetPropertyValue(nameof(SmsTemplate), ref smsTemplate, value);
        }

        string availablePlaceholders;
        [Size(SizeAttribute.Unlimited)]
        [VisibleInListView(false)]
        [ToolTip("Documentation of available placeholders")]
        public string AvailablePlaceholders
        {
            get => availablePlaceholders;
            set => SetPropertyValue(nameof(AvailablePlaceholders), ref availablePlaceholders, value);
        }

        DateTime createdOn;
        [VisibleInListView(false)]
        public DateTime CreatedOn
        {
            get => createdOn;
            set => SetPropertyValue(nameof(CreatedOn), ref createdOn, value);
        }

        DateTime? lastModifiedOn;
        public DateTime? LastModifiedOn
        {
            get => lastModifiedOn;
            set => SetPropertyValue(nameof(LastModifiedOn), ref lastModifiedOn, value);
        }

        bool isActive;
        public bool IsActive
        {
            get => isActive;
            set => SetPropertyValue(nameof(IsActive), ref isActive, value);
        }

        bool isDefault;
        [ToolTip("Default template for this reminder type")]
        public bool IsDefault
        {
            get => isDefault;
            set => SetPropertyValue(nameof(IsDefault), ref isDefault, value);
        }

        protected override void OnSaving()
        {
            base.OnSaving();
            LastModifiedOn = DateTime.Now;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}

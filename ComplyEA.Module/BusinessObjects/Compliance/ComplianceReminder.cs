using System;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using ComplyEA.Module.BusinessObjects.Lookups;
using ComplyEA.Module.BusinessObjects.Organization;

namespace ComplyEA.Module.BusinessObjects.Compliance
{
    [DefaultClassOptions]
    [NavigationItem("Compliance")]
    [ImageName("BO_Scheduler")]
    public class ComplianceReminder : BaseObject
    {
        public ComplianceReminder(Session session) : base(session) { }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            CreatedOn = DateTime.Now;
        }

        ComplianceObligation complianceObligation;
        [RuleRequiredField]
        [Association("ComplianceObligation-Reminders")]
        public ComplianceObligation ComplianceObligation
        {
            get => complianceObligation;
            set => SetPropertyValue(nameof(ComplianceObligation), ref complianceObligation, value);
        }

        ReminderType reminderType;
        [RuleRequiredField]
        [Association("ReminderType-Reminders")]
        public ReminderType ReminderType
        {
            get => reminderType;
            set => SetPropertyValue(nameof(ReminderType), ref reminderType, value);
        }

        DateTime scheduledDate;
        [RuleRequiredField]
        public DateTime ScheduledDate
        {
            get => scheduledDate;
            set => SetPropertyValue(nameof(ScheduledDate), ref scheduledDate, value);
        }

        DateTime? sentDate;
        public DateTime? SentDate
        {
            get => sentDate;
            set => SetPropertyValue(nameof(SentDate), ref sentDate, value);
        }

        DeliveryStatus deliveryStatus;
        [Association("DeliveryStatus-Reminders")]
        public DeliveryStatus DeliveryStatus
        {
            get => deliveryStatus;
            set => SetPropertyValue(nameof(DeliveryStatus), ref deliveryStatus, value);
        }

        NotificationChannel notificationChannel;
        [Association("NotificationChannel-Reminders")]
        public NotificationChannel NotificationChannel
        {
            get => notificationChannel;
            set => SetPropertyValue(nameof(NotificationChannel), ref notificationChannel, value);
        }

        CompanyContact recipient;
        [Association("CompanyContact-Reminders")]
        public CompanyContact Recipient
        {
            get => recipient;
            set => SetPropertyValue(nameof(Recipient), ref recipient, value);
        }

        string recipientEmail;
        [Size(200)]
        public string RecipientEmail
        {
            get => recipientEmail;
            set => SetPropertyValue(nameof(RecipientEmail), ref recipientEmail, value);
        }

        string recipientPhone;
        [Size(50)]
        public string RecipientPhone
        {
            get => recipientPhone;
            set => SetPropertyValue(nameof(RecipientPhone), ref recipientPhone, value);
        }

        string messageSubject;
        [Size(300)]
        public string MessageSubject
        {
            get => messageSubject;
            set => SetPropertyValue(nameof(MessageSubject), ref messageSubject, value);
        }

        string messageBody;
        [Size(SizeAttribute.Unlimited)]
        public string MessageBody
        {
            get => messageBody;
            set => SetPropertyValue(nameof(MessageBody), ref messageBody, value);
        }

        string errorMessage;
        [Size(SizeAttribute.Unlimited)]
        [VisibleInListView(false)]
        public string ErrorMessage
        {
            get => errorMessage;
            set => SetPropertyValue(nameof(ErrorMessage), ref errorMessage, value);
        }

        int retryCount;
        [VisibleInListView(false)]
        public int RetryCount
        {
            get => retryCount;
            set => SetPropertyValue(nameof(RetryCount), ref retryCount, value);
        }

        DateTime? acknowledgedDate;
        [ToolTip("Date when recipient acknowledged the reminder")]
        public DateTime? AcknowledgedDate
        {
            get => acknowledgedDate;
            set => SetPropertyValue(nameof(AcknowledgedDate), ref acknowledgedDate, value);
        }

        DateTime createdOn;
        [VisibleInListView(false)]
        public DateTime CreatedOn
        {
            get => createdOn;
            set => SetPropertyValue(nameof(CreatedOn), ref createdOn, value);
        }

        public override string ToString()
        {
            return $"{ReminderType?.Name} - {ScheduledDate:d}";
        }
    }
}

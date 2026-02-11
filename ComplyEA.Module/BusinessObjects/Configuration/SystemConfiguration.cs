using System;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;

namespace ComplyEA.Module.BusinessObjects.Configuration
{
    [DefaultClassOptions]
    [NavigationItem("Administration")]
    [ImageName("SystemConfiguration")]
    public class SystemConfiguration : BaseObject
    {
        public SystemConfiguration(Session session) : base(session) { }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            CreatedOn = DateTime.Now;
        }

        string key;
        [RuleRequiredField]
        [Size(100)]
        [Indexed(Unique = true)]
        public string Key
        {
            get => key;
            set => SetPropertyValue(nameof(Key), ref key, value);
        }

        string value;
        [Size(SizeAttribute.Unlimited)]
        public string Value
        {
            get => value;
            set => SetPropertyValue(nameof(Value), ref this.value, value);
        }

        string description;
        [Size(500)]
        public string Description
        {
            get => description;
            set => SetPropertyValue(nameof(Description), ref description, value);
        }

        string category;
        [Size(100)]
        [ToolTip("Configuration category for grouping")]
        public string Category
        {
            get => category;
            set => SetPropertyValue(nameof(Category), ref category, value);
        }

        string dataType;
        [Size(50)]
        [ToolTip("Data type: String, Int, Bool, DateTime, Json")]
        public string DataType
        {
            get => dataType;
            set => SetPropertyValue(nameof(DataType), ref dataType, value);
        }

        bool isEncrypted;
        [ToolTip("Indicates if the value is encrypted")]
        public bool IsEncrypted
        {
            get => isEncrypted;
            set => SetPropertyValue(nameof(IsEncrypted), ref isEncrypted, value);
        }

        bool isSystemOnly;
        [ToolTip("If true, only visible to system administrators")]
        public bool IsSystemOnly
        {
            get => isSystemOnly;
            set => SetPropertyValue(nameof(IsSystemOnly), ref isSystemOnly, value);
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

        protected override void OnSaving()
        {
            base.OnSaving();
            LastModifiedOn = DateTime.Now;
        }

        public override string ToString()
        {
            return Key;
        }

        // Helper methods for typed access
        public int GetIntValue(int defaultValue = 0)
        {
            return int.TryParse(Value, out int result) ? result : defaultValue;
        }

        public bool GetBoolValue(bool defaultValue = false)
        {
            return bool.TryParse(Value, out bool result) ? result : defaultValue;
        }

        public DateTime? GetDateTimeValue()
        {
            return DateTime.TryParse(Value, out DateTime result) ? result : (DateTime?)null;
        }
    }
}

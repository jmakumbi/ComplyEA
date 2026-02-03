using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;

namespace ComplyEA.Module.BusinessObjects.Lookups
{
    /// <summary>
    /// Base class for all lookup tables providing common fields.
    /// </summary>
    [NonPersistent]
    public abstract class BaseLookup : BaseObject
    {
        protected BaseLookup(Session session) : base(session) { }

        string name;
        [RuleRequiredField]
        [Size(100)]
        public string Name
        {
            get => name;
            set => SetPropertyValue(nameof(Name), ref name, value);
        }

        string code;
        [Indexed(Unique = true)]
        [Size(50)]
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

        int sortOrder;
        public int SortOrder
        {
            get => sortOrder;
            set => SetPropertyValue(nameof(SortOrder), ref sortOrder, value);
        }

        bool isActive = true;
        public bool IsActive
        {
            get => isActive;
            set => SetPropertyValue(nameof(IsActive), ref isActive, value);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}

using System;

namespace CostEffectiveCode.BackOffice.Models
{
    [Flags]
    public enum ScaffoldActions
    {
        List = 0x01,
        Create = 0x02,
        Edit = 0x03,
        Delete = 0x04
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ScaffoldVisibleAttribute : Attribute
    {
        public ScaffoldActions Actions { get; set; }

        public ScaffoldVisibleAttribute(ScaffoldActions actions)
        {
            Actions = actions;
        }
    }
}

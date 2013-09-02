using System;

namespace CompositeC1Contrib.FormBuilder.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class FieldHelpAttribute : Attribute
    {
        public virtual string Help { get; protected set; }

        public FieldHelpAttribute(string help)
        {
            Help = help;
        }
    }
}

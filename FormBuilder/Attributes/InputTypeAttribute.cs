using System;

namespace CompositeC1Contrib.FormBuilder.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class InputFieldTypeAttribute : Attribute
    {
        public InputType InputType { get; private set; }

        public InputFieldTypeAttribute(InputType type)
        {
            InputType = type;
        }
    }
}

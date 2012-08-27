using System;

namespace CompositeC1Contrib.FormBuilder.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class FormTagAttributesAttribute : Attribute
    {
        public string Attributes { get; private set; }

        public FormTagAttributesAttribute(string attributes)
        {
            Attributes = attributes;
        }
    }
}

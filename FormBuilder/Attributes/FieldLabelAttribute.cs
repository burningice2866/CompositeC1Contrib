using System;

namespace CompositeC1Contrib.FormBuilder.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class FieldLabelAttribute : Attribute
    {
        public string Label { get; private set; }
        
        public string Link { get; set; }
        public bool OpenLinkInNewWindow { get; set; }

        public FieldLabelAttribute(string label)
        {
            Label = label;
            
            OpenLinkInNewWindow = false;
        }
    }
}

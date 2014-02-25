using System;

namespace CompositeC1Contrib.FunctionProvider
{
    [AttributeUsage(AttributeTargets.Class)]
    public class FunctionDescriptionAttribute : Attribute
    {
        public string Description { get; set; }

        public FunctionDescriptionAttribute() { }

        public FunctionDescriptionAttribute(string description)
        {
            Description = description;
        }
    }
}

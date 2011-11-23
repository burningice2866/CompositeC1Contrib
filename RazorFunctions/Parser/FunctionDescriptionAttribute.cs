using System;

namespace CompositeC1Contrib.RazorFunctions.Parser
{
    [AttributeUsage(AttributeTargets.Class)]
    public class FunctionDescriptionAttribute : Attribute
    {
        public string Description { get; set; }

        public FunctionDescriptionAttribute()
        {            
        }

        public FunctionDescriptionAttribute(string description)
        {
            Description = description;
        }
    }
}

using System;

namespace CompositeC1Contrib.Rendering.Mvc.Functions
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class MvcFunctionAttribute : Attribute
    {
        public string Namespace { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}

using System;

namespace CompositeC1Contrib.Rendering.Nancy.Functions
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class NancyFunctionAttribute : Attribute
    {
        public string Namespace { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}

using System;

namespace CompositeC1Contrib.Rendering.Mvc.Templates
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class MvcTemplateAttribute : Attribute
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string ViewName { get; set; }
    }
}

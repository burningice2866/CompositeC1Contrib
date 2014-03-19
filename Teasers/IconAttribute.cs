using System;

namespace CompositeC1Contrib.Teasers
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
    public class IconAttribute : Attribute
    {
        public string Name { get; set; }
        public string OpenedName { get; set; }

        public IconAttribute(string name)
        {
            Name = name;
        }

        public IconAttribute(string name, string openedName)
        {
            Name = name;
            OpenedName = openedName;
        }
    }
}

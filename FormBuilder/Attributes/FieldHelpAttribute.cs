using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompositeC1Contrib.FormBuilder.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class FieldHelpAttribute : Attribute
    {
        public FieldHelpAttribute(string help)
        {
            this.Help = help;
        }

        public virtual string Help { get; protected set; }
    }
}

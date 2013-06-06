using System;

namespace CompositeC1Contrib.FormBuilder.Dependencies
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public abstract class FormDependencyAttribute: Attribute
    {
        public string ReadFromFieldName { get; private set; }

        protected FormDependencyAttribute(string readFromFieldName)
        {
            this.ReadFromFieldName = readFromFieldName;
        }

        public abstract bool DependencyMet(object dataObject);
        public abstract string[] RequiredFieldValues();
    }
}

using System;

namespace CompositeC1Contrib.FormBuilder.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public abstract class SubmitHandlerAttribute : Attribute
    {
        public abstract void Submit(BaseForm form);
    }
}

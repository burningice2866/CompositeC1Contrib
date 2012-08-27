using System;
using System.Reflection;

namespace CompositeC1Contrib.FormBuilder.Attributes
{
    public class MethodBasedSubmitHandlerAttribute : SubmitHandlerAttribute
    {
        private Type _declaringType;
        private string _methodName;

        public MethodBasedSubmitHandlerAttribute(string methodName)
        {
            _methodName = methodName;
        }

        public MethodBasedSubmitHandlerAttribute(Type declaringType, string methodName)
        {
            _declaringType = declaringType;
            _methodName = methodName;
        }

        public override void Submit(BaseForm form)
        {
            var type = _declaringType ?? form.GetType();
            var method = type.GetMethod(_methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            method.Invoke(form, null);
        }
    }
}

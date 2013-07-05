using System;
using System.Reflection;
using Composite;

namespace CompositeC1Contrib.FormBuilder.Attributes
{
    public class MethodBasedDataSourceAttribute : DataSourceAttribute
    {
        private readonly string _methodName;
        private readonly Type _declaringType;

        public MethodBasedDataSourceAttribute(string methodName)
        {
            _methodName = methodName;
        }

        public MethodBasedDataSourceAttribute(Type declaringType, string methodName)
        {
            _declaringType = declaringType;
            _methodName = methodName;
        }

        public override object GetData(BaseForm form)
        {
            var type = _declaringType ?? form.GetType();
            var method = type.GetMethod(_methodName, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            Verify.IsNotNull(method, "Failed to find method '{0}' on type '{1}'", _methodName, type.FullName);

            return method.Invoke(form, null);
        }
    }
}

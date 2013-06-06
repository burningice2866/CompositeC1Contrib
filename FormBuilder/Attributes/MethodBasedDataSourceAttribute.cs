using System;
using System.Reflection;

namespace CompositeC1Contrib.FormBuilder.Attributes
{
    public class MethodBasedDataSourceAttribute : DataSourceAttribute
    {
        private string _methodName;
        private Type _declaringType;

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

            return method.Invoke(form, null);
        }
    }
}

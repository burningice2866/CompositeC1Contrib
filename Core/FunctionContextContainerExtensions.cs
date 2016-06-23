using System.Collections.Generic;
using System.Reflection;

using Composite.Functions;

namespace CompositeC1Contrib
{
    public static class FunctionContextContainerExtensions
    {
        private static FieldInfo _field = typeof(FunctionContextContainer).GetField("_parameterDictionary", BindingFlags.Instance | BindingFlags.NonPublic);

        public static T GetParameter<T>(this FunctionContextContainer container, string name)
        {
            try
            {
                return (T)container.GetParameterValue(name, typeof(T));
            }
            catch
            {
                return default(T);
            }
        }

        public static bool TryGetParameter<T>(this FunctionContextContainer container, string name, out T value)
        {
            try
            {
                value = (T)container.GetParameterValue(name, typeof(T));

                return true;
            }
            catch
            {
                value = default(T);

                return false;
            }
        }

        public static void AddParameter(this FunctionContextContainer container, string name, object parameter)
        {
            var díctionary = (Dictionary<string, object>)_field.GetValue(container);
            if (díctionary == null)
            {
                díctionary = new Dictionary<string, object>();

                _field.SetValue(container, díctionary);
            }

            díctionary.Add(name, parameter);
        }
    }
}

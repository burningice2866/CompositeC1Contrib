using System;
using System.Collections.Generic;
using System.ComponentModel;

using Composite.Functions;

namespace CompositeC1Contrib.RazorFunctions
{
    public static class Functions
    {
        public static void ExecuteFunction(string name)
        {
            ExecuteFunction(name, null);
        }

        public static object ExecuteFunction(string name, IDictionary<string, object> @params)
        {
            IFunction function;
            if (!FunctionFacade.TryGetFunction(out function, name))
            {
                throw new ArgumentException("Invalid function name", "name");
            }

            return FunctionFacade.Execute<object>(function, @params, new FunctionContextContainer());
        }

        public static object ExecuteFunction(string name, object @params)
        {
            return ExecuteFunction(name, objectToDictionary(@params));
        }

        private static IDictionary<string, object> objectToDictionary(object instance)
        {
            var dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            if (instance != null)
            {
                foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(instance))
                {
                    object obj = descriptor.GetValue(instance);

                    dictionary.Add(descriptor.Name, obj);
                }
            }

            return dictionary;
        }
    }
}

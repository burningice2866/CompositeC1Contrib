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

        public static object ExecuteFunction(string name, object parameters)
        {
            return ExecuteFunction(name, ObjectToDictionary(parameters));
        }

        public static object ExecuteFunction(string name, IDictionary<string, object> parameters)
        {
            IFunction function;
            if (!FunctionFacade.TryGetFunction(out function, name))
            {
                throw new ArgumentException("Invalid function name", "name");
            }

            return FunctionFacade.Execute<object>(function, parameters, new FunctionContextContainer());
        }

        public static IDictionary<string, object> ObjectToDictionary(object instance)
        {
            if (instance == null)
            {
                return null;
            }

            var dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(instance))
            {
                object obj = descriptor.GetValue(instance);

                dictionary.Add(descriptor.Name, obj);
            }

            return dictionary;
        }
    }
}

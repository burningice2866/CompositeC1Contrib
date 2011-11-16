using System;
using System.Collections.Generic;

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
            return ExecuteFunction(name, C1HtmlHelper.ObjectToDictionary(parameters));
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
    }
}

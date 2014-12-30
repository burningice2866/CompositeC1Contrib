using System.Collections.Generic;

using Composite.Functions;
using Composite.Functions.Plugins.FunctionProvider;

namespace CompositeC1Contrib.Rendering.Mvc.Functions
{
    public class MvcFunctionsProvider : IFunctionProvider
    {
        private static FunctionNotifier _functionNotifier;

        public FunctionNotifier FunctionNotifier
        {
            set { _functionNotifier = value; }
        }

        public IEnumerable<IFunction> Functions
        {
            get { return GlobalConfiguration.Current.Functions; }
        }

        public static void Reload()
        {
            _functionNotifier.FunctionsUpdated();
        }
    }
}

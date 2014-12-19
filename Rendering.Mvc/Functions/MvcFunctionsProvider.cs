using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

using Composite.Functions;
using Composite.Functions.Plugins.FunctionProvider;

namespace CompositeC1Contrib.Rendering.Mvc.Functions
{
    public class MvcFunctionsProvider : IFunctionProvider
    {
        private FunctionNotifier _functionNotifier;

        public FunctionNotifier FunctionNotifier
        {
            set { _functionNotifier = value; }
        }

        public IEnumerable<IFunction> Functions
        {
            get
            {
                var list = new List<MvcFunction>();

                try
                {
                    var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    foreach (var assembly in assemblies)
                    {
                        try
                        {
                            var types = assembly.GetTypes();
                            foreach (var type in types)
                            {
                                try
                                {
                                    var attribute = type.GetCustomAttributes<MvcFunctionAttribute>(false).SingleOrDefault();
                                    if (attribute == null)
                                    {
                                        continue;
                                    }

                                    var controllerDescriptor = new ReflectedControllerDescriptor(type);
                                    var function = new MvcFunction(controllerDescriptor);

                                    list.Add(function);
                                }
                                catch { }
                            }
                        }
                        catch { }
                    }
                }
                catch { }

                return list;
            }
        }
    }
}

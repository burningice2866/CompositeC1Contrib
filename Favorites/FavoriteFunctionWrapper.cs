using System;
using System.Collections.Generic;

using Composite.C1Console.Security;
using Composite.Data;
using Composite.Data.Types;
using Composite.Functions;

namespace CompositeC1Contrib.Favorites
{
    public class FavoriteFunctionWrapper : IFunction
    {
        private readonly IFunction _function;

        public string Description
        {
            get { return _function.Description; }
        }

        public EntityToken EntityToken
        {
            get { return _function.EntityToken; }
        }

        private readonly string _name;
        public string Name
        {
            get { return _name; }
        }

        public string Namespace
        {
            get { return "__Favorites"; }
        }

        public IEnumerable<ParameterProfile> ParameterProfiles
        {
            get { return _function.ParameterProfiles; }
        }

        public Type ReturnType
        {
            get { return _function.ReturnType; }
        }

        public FavoriteFunctionWrapper(string name, IFunction function)
        {
            _name = name;
            _function = function;
        }

        public object Execute(ParameterList parameters, FunctionContextContainer context)
        {
            return _function.Execute(parameters, context);
        }

        public static string GetFunctionNameFromEntityToken(EntityToken entityToken)
        {
            var dataToken = entityToken as DataEntityToken;
            if (dataToken != null)
            {
                if (dataToken.InterfaceType == typeof(IXsltFunction))
                {
                    var xsltFunction = (IXsltFunction)dataToken.Data;

                    return FunctionFacade.GetFunctionCompositionName(xsltFunction.Namespace, xsltFunction.Name);
                }

                if (dataToken.InterfaceType == typeof(IMethodBasedFunctionInfo))
                {
                    var methodBasedFunction = (IMethodBasedFunctionInfo)dataToken.Data;

                    return FunctionFacade.GetFunctionCompositionName(methodBasedFunction.Namespace, methodBasedFunction.UserMethodName);
                }
            }

            return entityToken.Id;
        }
    }
}

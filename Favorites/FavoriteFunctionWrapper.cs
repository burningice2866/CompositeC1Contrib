using System;
using System.Collections.Generic;

using Composite.C1Console.Security;
using Composite.Data;
using Composite.Data.Types;
using Composite.Functions;

using CompositeC1Contrib.Favorites.Data.Types;

namespace CompositeC1Contrib.Favorites
{
    public class FavoriteFunctionWrapper : IFunction
    {
        private IFunction _function;

        public string Description
        {
            get { return _function.Description; }
        }

        public EntityToken EntityToken
        {
            get { return _function.EntityToken; }
        }

        private string _name;
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

        private FavoriteFunctionWrapper(string name, IFunction function)
        {
            _name = name;
            _function = function;
        }

        public object Execute(ParameterList parameters, FunctionContextContainer context)
        {
            return _function.Execute(parameters, context);
        }

        public static IFunction Create(IFavoriteFunction function)
        {
            string name = function.Name;

            string fullName = String.Empty;
            var entityToken = EntityTokenSerializer.Deserialize(function.SerializedEntityToken);

            var dataToken = entityToken as DataEntityToken;
            if (dataToken != null)
            {
                if (dataToken.InterfaceType == typeof(IXsltFunction))
                {
                    var xsltFunction = (IXsltFunction)dataToken.Data;

                    fullName = String.Join(".", xsltFunction.Namespace, xsltFunction.Name);
                }

                if (dataToken.InterfaceType == typeof(IMethodBasedFunctionInfo))
                {
                    var methodBasedFunction = (IMethodBasedFunctionInfo)dataToken.Data;

                    fullName = String.Join(".", methodBasedFunction.Namespace, methodBasedFunction.UserMethodName);
                }
            }
            else
            {
                fullName = entityToken.Id;
            }

            IFunction iFunction;
            if (FunctionFacade.TryGetFunction(out iFunction, fullName))
            {
                return new FavoriteFunctionWrapper(function.Name, iFunction);
            }

            return null;
        }
    }
}

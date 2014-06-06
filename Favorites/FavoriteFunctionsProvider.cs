using System.Collections.Generic;

using Composite.Data;
using Composite.Functions;

using Composite.Functions.Plugins.FunctionProvider;
using CompositeC1Contrib.Favorites.Data.Types;

namespace CompositeC1Contrib.Favorites
{
    public class FavoriteFunctionsProvider : IFunctionProvider
    {
        private static FunctionNotifier _globalNotifier;

        public FunctionNotifier FunctionNotifier
        {
            set { _globalNotifier = value; }
        }

        public IEnumerable<IFunction> Functions
        {
            get
            {
                using (var data = new DataConnection())
                {                    
                    var favoriteFunctions = data.Get<IFavoriteFunction>();
                    foreach (var function in favoriteFunctions)
                    {
                        IFunction iFunction;
                        if (FunctionFacade.TryGetFunction(out iFunction, function.FunctionName))
                        {
                            yield return new FavoriteFunctionWrapper(function.Name, iFunction);
                        }
                    }
                }
            }
        }

        public FavoriteFunctionsProvider()
        {
            DataEventSystemFacade.SubscribeToDataAfterAdd<IFavoriteFunction>(OnDataChanged, false);
            DataEventSystemFacade.SubscribeToDataDeleted<IFavoriteFunction>(OnDataChanged, false);
        }

        private static void OnDataChanged(object sender, DataEventArgs e)
        {
            _globalNotifier.FunctionsUpdated();
        }

        public static void Update()
        {
            if (_globalNotifier != null)
            {
                _globalNotifier.FunctionsUpdated();
            }
        }
    }
}

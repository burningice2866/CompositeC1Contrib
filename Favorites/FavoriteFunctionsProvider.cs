using System.Collections.Generic;
using System.Linq;

using Composite.C1Console.Security;
using Composite.Data;
using Composite.Functions;

using Composite.Functions.Plugins.FunctionProvider;
using CompositeC1Contrib.Favorites.Data.Types;

namespace CompositeC1Contrib.Favorites
{
    public class FavoriteFunctionsProvider : IFunctionProvider
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
                using (var data = new DataConnection())
                {                    
                    var favoriteFunctions = data.Get<IFavoriteFunction>();
                    foreach (var function in favoriteFunctions)
                    {
                        var iFunction = FavoriteFunctionWrapper.Create(function);
                        if (iFunction != null)
                        {
                            yield return iFunction;
                        }
                    }
                }
            }
        }

        public FavoriteFunctionsProvider()
        {
            DataEventSystemFacade.SubscribeToDataAfterAdd<IFavoriteFunction>(onDataChanged, false);
            DataEventSystemFacade.SubscribeToDataDeleted<IFavoriteFunction>(onDataChanged, false);
        }

        private void onDataChanged(object sender, DataEventArgs e)
        {
            this._functionNotifier.FunctionsUpdated();
        }
    }
}

using System.Linq;

using Composite.C1Console.Actions;
using Composite.C1Console.Security;
using Composite.Data;

using CompositeC1Contrib.Favorites.Data.Types;

namespace CompositeC1Contrib.Favorites
{
    public class RemoveFromFavoritesActionExecutor : IActionExecutor
    {
        public FlowToken Execute(EntityToken entityToken, ActionToken actionToken, FlowControllerServicesContainer flowControllerServicesContainer)
        {
            var functionName = FavoriteFunctionWrapper.GetFunctionNameFromEntityToken(entityToken);

            using (var data = new DataConnection())
            {
                var favorite = data.Get<IFavoriteFunction>().Single(f => f.FunctionName == functionName);

                data.Delete(favorite);
            }

            var treeRefresher = new ParentTreeRefresher(flowControllerServicesContainer);
            treeRefresher.PostRefreshMesseges(entityToken);

            return null;
        }
    }
}

using System;
using System.Linq;

using Composite.Data;
using Composite.Functions;

using CompositeC1Contrib.Favorites.Data.Types;
using CompositeC1Contrib.Workflows;

namespace CompositeC1Contrib.Favorites.Workflows
{
    public sealed class AddToFavoritesQueueWorkflow : Basic1StepDialogWorkflow
    {
        public AddToFavoritesQueueWorkflow() : base("\\InstalledPackages\\CompositeC1Contrib.Favorites\\AddToFavorites.xml") { }

        public override bool Validate()
        {
            var favoriteFunction = GetBinding<IFavoriteFunction>("FavoriteFunction");

            using (var data = new DataConnection())
            {
                var nameExists = data.Get<IFavoriteFunction>().Any(q => q.Name == favoriteFunction.Name || q.FunctionName == favoriteFunction.FunctionName);
                if (nameExists)
                {
                    ShowFieldMessage("Favorite function", "Favorite with this name or function already exists");

                    return false;
                }
            }

            return base.Validate();
        }

        public override void OnInitialize(object sender, EventArgs e)
        {
            if (BindingExist("FavoriteFunction"))
            {
                return;
            }

            using (var data = new DataConnection())
            {
                var favoriteFunction = data.CreateNew<IFavoriteFunction>();

                var fullName = FavoriteFunctionWrapper.GetFunctionNameFromEntityToken(EntityToken);
                var iFunction = FunctionFacade.GetFunction(fullName);

                favoriteFunction.Id = Guid.NewGuid();
                favoriteFunction.FunctionName = fullName;
                favoriteFunction.Name = iFunction.Name;

                Bindings.Add("FavoriteFunction", favoriteFunction);
            }
        }

        public override void OnFinish(object sender, EventArgs e)
        {
            var favoriteFunction = GetBinding<IFavoriteFunction>("FavoriteFunction");

            using (var data = new DataConnection())
            {
                data.Add(favoriteFunction);
            }

            var treeRefresher = CreateParentTreeRefresher();
            treeRefresher.PostRefreshMesseges(EntityToken);
        }
    }
}

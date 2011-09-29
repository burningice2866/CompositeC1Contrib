using System;
using System.Linq;
using System.Workflow.Activities;

using Composite.Data;
using Composite.Functions;

using CompositeC1Contrib.Favorites.Data.Types;

namespace CompositeC1Contrib.Favorites.Workflows
{
    public sealed partial class AddToFavoritesQueueWorkflow : Composite.C1Console.Workflow.Activities.FormsWorkflow
    {
        public AddToFavoritesQueueWorkflow()
        {
            InitializeComponent();
        }

        private void validateSave(object sender, ConditionalEventArgs e)
        {
            var favoriteFunction = GetBinding<IFavoriteFunction>("FavoriteFunction");

            using (var data = new DataConnection())
            {
                var nameExists = data.Get<IFavoriteFunction>().Any(q => q.Name == favoriteFunction.Name || q.FunctionName == favoriteFunction.FunctionName);
                if (nameExists)
                {
                    ShowFieldMessage("Favorite function", "Favorite with this name or function already exists");

                    e.Result = false;

                    return;
                }
            }

            e.Result = true;
        }

        private void initCodeActivity_ExecuteCode(object sender, EventArgs e)
        {
            if (!BindingExist("FavoriteFunction"))
            {
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
        }

        private void saveCodeActivity_ExecuteCode(object sender, EventArgs e)
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

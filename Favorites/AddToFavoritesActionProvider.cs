using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;

using Composite.C1Console.Elements;
using Composite.C1Console.Elements.Plugins.ElementActionProvider;
using Composite.C1Console.Security;
using Composite.C1Console.Workflow;
using Composite.Core.ResourceSystem;
using Composite.Data;
using Composite.Functions;

using CompositeC1Contrib.Favorites.Data.Types;
using CompositeC1Contrib.Favorites.Workflows;

namespace CompositeC1Contrib.Favorites
{
    [ConfigurationElementType(typeof(NonConfigurableElementActionProvider))]
    public class AddToFavoritesActionProvider : IElementActionProvider
    {
        private static readonly ActionGroup _actionGroup = new ActionGroup("Default", ActionGroupPriority.PrimaryLow);
        private static readonly ActionLocation _actionLocation = new ActionLocation { ActionType = ActionType.Add, IsInFolder = false, IsInToolbar = true, ActionGroup = _actionGroup };

        public IEnumerable<ElementAction> GetActions(EntityToken entityToken)
        {
            string fullName = FavoriteFunctionWrapper.GetFunctionNameFromEntityToken(entityToken);
            if (!String.IsNullOrEmpty(fullName))
            {
                IFunction function;

                if (FunctionFacade.TryGetFunction(out function, fullName))
                {
                    yield return createAction(fullName);
                }
            }
        }

        private ElementAction createAction(string fullName)
        {
            using (var data = new DataConnection())
            {
                var isFavorite = data.Get<IFavoriteFunction>().Any(f => f.FunctionName == fullName);

                var label = isFavorite ? "Remove from favorites" : "Add to favorites";
                var actionToken = isFavorite ? new ConfirmWorkflowActionToken("Are you sure?", typeof(RemoveFromFavoritesActionToken)) : new WorkflowActionToken(typeof(AddToFavoritesQueueWorkflow));
                var toggleIcon = isFavorite ? "generated-type-data-delete" : "accept";
                
                return new ElementAction(new ActionHandle(actionToken))
                {
                    VisualData = new ActionVisualizedData
                    {
                        Label = label,
                        ToolTip = label,
                        Icon = new ResourceHandle("Composite.Icons", toggleIcon),
                        ActionLocation = _actionLocation
                    }
                };
            }            
        }
    }
}

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
using Composite.Data.Types;
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
            var dataToken = entityToken as DataEntityToken;
            if (dataToken != null)
            {
                if (dataToken.InterfaceType == typeof(IXsltFunction))
                {
                    yield return createAction(entityToken);
                }

                if (dataToken.InterfaceType == typeof(IMethodBasedFunctionInfo))
                {
                    yield return createAction(entityToken);
                }
            }

            string id = entityToken.Id;
            if (!String.IsNullOrEmpty(id))
            {
                IFunction function;

                if (FunctionFacade.TryGetFunction(out function, id))
                {
                    yield return createAction(entityToken);
                }
            }
        }

        private ElementAction createAction(EntityToken entityToken)
        {
            string serializedEntityToken = entityToken.Serialize();
            string userName = UserValidationFacade.GetUsername();

            using (var data = new DataConnection())
            {
                var isFavorite = data.Get<IFavoriteFunction>().Any(f => f.SerializedEntityToken == serializedEntityToken);

                var label = isFavorite ? "Remove from favorites" : "Add to favorites";

                var deleteActionToken = new WorkflowActionToken(typeof(AddToFavoritesQueueWorkflow));
                return new ElementAction(new ActionHandle(deleteActionToken))
                {
                    VisualData = new ActionVisualizedData
                    {
                        Label = label,
                        ToolTip = label,
                        Icon = new ResourceHandle("Composite.Icons", "generated-type-data-delete"),
                        ActionLocation = _actionLocation
                    }
                };
            }            
        }
    }
}

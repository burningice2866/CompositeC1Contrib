using System.Collections.Generic;

using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;

using Composite.C1Console.Elements;
using Composite.C1Console.Elements.Plugins.ElementActionProvider;
using Composite.C1Console.Security;
using Composite.Core.ResourceSystem;
using Composite.Data;
using Composite.Data.Types;

namespace CompositeC1Contrib.GetItemLink
{
    [ConfigurationElementType(typeof(NonConfigurableElementActionProvider))]
    public class ActionProvider : IElementActionProvider
    {
        private static readonly ActionGroup ActionGroup = new ActionGroup("Default", ActionGroupPriority.PrimaryLow);
        private static readonly ActionLocation ActionLocation = new ActionLocation { ActionType = ActionType.Add, IsInFolder = false, IsInToolbar = false, ActionGroup = ActionGroup };

        public IEnumerable<ElementAction> GetActions(EntityToken entityToken)
        {
            var dataEntityToken = entityToken as DataEntityToken;
            if (dataEntityToken != null)
            {
                var file = dataEntityToken.Data as IMediaFile;
                if (file != null)
                {
                    var actionToken = new GetMediaLinkActionToken(file);

                    yield return CreateElementAction(actionToken);
                }

                var page = dataEntityToken.Data as IPage;
                if (page != null)
                {
                    var actionToken = new GetPageLinkActionToken(page);

                    yield return CreateElementAction(actionToken);
                }
            }
        }

        private static ElementAction CreateElementAction(ActionToken actionToken)
        {
            return new ElementAction(new ActionHandle(actionToken))
            {
                VisualData = new ActionVisualizedData
                {
                    Label = "Get link",
                    ToolTip = "Get link",
                    Icon = new ResourceHandle("Composite.Icons", "down"),
                    ActionLocation = ActionLocation
                }
            };
        }
    }
}
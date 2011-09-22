using System.Collections.Generic;

using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;

using Composite.C1Console.Elements;
using Composite.C1Console.Elements.Plugins.ElementActionProvider;
using Composite.C1Console.Security;
using Composite.Core.ResourceSystem;
using Composite.Core.Types;
using Composite.Data;
using Composite.Plugins.Elements.ElementProviders.GeneratedDataTypesElementProvider;

namespace CompositeC1Contrib.ChangeHistory
{
    [ConfigurationElementType(typeof(NonConfigurableElementActionProvider))]
    public class ElementChangeHistoryActionProvider : IElementActionProvider
    {
        private static readonly ActionGroup _actionGroup = new ActionGroup("Default", ActionGroupPriority.PrimaryLow);
        private static readonly ActionLocation _actionLocation = new ActionLocation { ActionType = ActionType.Add, IsInFolder = false, IsInToolbar = true, ActionGroup = _actionGroup };

        public IEnumerable<ElementAction> GetActions(EntityToken entityToken)
        {
            var generatedDataTypetoken = entityToken as GeneratedDataTypesElementProviderTypeEntityToken;
            if (generatedDataTypetoken != null)
            {
                if (generatedDataTypetoken.Source == "GeneratedDataTypesElementProvider")
                {
                    var type = TypeManager.GetType(generatedDataTypetoken.SerializedTypeName);

                    if (!typeof(IPageMetaData).IsAssignableFrom(type))
                    {
                        string message;
                        string icon;

                        if (typeof(IChangeHistory).IsAssignableFrom(type))
                        {
                            message = "Disable history";
                            icon = "delete";
                        }
                        else
                        {
                            message = "Enable history";
                            icon = "accept";
                        }

                        var actionToken = new ToggleSuperInterfaceActionToken(typeof(IChangeHistory));

                        yield return new ElementAction(new ActionHandle(actionToken))
                        {
                            VisualData = new ActionVisualizedData
                            {
                                Label = message,
                                ToolTip = message,
                                Icon = new ResourceHandle("Composite.Icons", icon),
                                ActionLocation = _actionLocation
                            }
                        };
                    }
                }
            }
        }
    }
}
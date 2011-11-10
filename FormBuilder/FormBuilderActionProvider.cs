using System;
using System.Collections.Generic;

using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;

using Composite.C1Console.Elements;
using Composite.C1Console.Elements.Plugins.ElementActionProvider;
using Composite.C1Console.Security;
using Composite.Core.ResourceSystem;
using Composite.Core.Types;
using Composite.Data;
using Composite.Plugins.Elements.ElementProviders.GeneratedDataTypesElementProvider;

using CompositeC1Contrib.FormBuilder.Data.Types;

namespace CompositeC1Contrib.FormBuilder
{
    [ConfigurationElementType(typeof(NonConfigurableElementActionProvider))]
    public class FormBuilderActionProvider : IElementActionProvider
    {
        private static readonly Type _formFieldType = typeof(IFormField);
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

                        if (_formFieldType.IsAssignableFrom(type))
                        {
                            message = "Demote as form field type";
                            icon = "delete";
                        }
                        else
                        {
                            message = "Promote to a form field type";
                            icon = "accept";
                        }

                        var actionToken = new ToggleSuperInterfaceActionToken(_formFieldType);

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
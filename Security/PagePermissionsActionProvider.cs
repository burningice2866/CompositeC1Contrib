using System.Collections.Generic;

using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;

using Composite.C1Console.Elements;
using Composite.C1Console.Elements.Plugins.ElementActionProvider;
using Composite.C1Console.Security;
using Composite.C1Console.Workflow;
using Composite.Core.ResourceSystem;
using Composite.Data;
using Composite.Data.Types;

using CompositeC1Contrib.Security.Workflows;

namespace CompositeC1Contrib.Security
{
    [ConfigurationElementType(typeof(NonConfigurableElementActionProvider))]
    public class PagePermissionsActionProvider : IElementActionProvider
    {
        private static readonly ActionGroup ActionGroup = new ActionGroup("Default", ActionGroupPriority.PrimaryLow);
        private static readonly ActionLocation ActionLocation = new ActionLocation { ActionType = ActionType.Add, IsInFolder = false, IsInToolbar = false, ActionGroup = ActionGroup };

        public IEnumerable<ElementAction> GetActions(EntityToken entityToken)
        {
            var dataToken = entityToken as DataEntityToken;
            if (dataToken != null)
            {
                var page = dataToken.Data as IPage;
                if (page != null)
                {
                    var actionToken = new WorkflowActionToken(typeof(EditPagePermissionsWorkflow));

                    yield return new ElementAction(new ActionHandle(actionToken))
                    {
                        VisualData = new ActionVisualizedData
                        {
                            Label = "Edit membership permissions",
                            ToolTip = "Edit membership permissions",
                            Icon = new ResourceHandle("Composite.Icons", "generated-type-data-edit"),
                            ActionLocation = ActionLocation
                        }
                    };
                }
            }
        }
    }
}
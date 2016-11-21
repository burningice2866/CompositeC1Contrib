using Composite.C1Console.Elements;
using Composite.C1Console.Security;
using Composite.C1Console.Workflow;
using Composite.Core.ResourceSystem;

using CompositeC1Contrib.Localization.C1Console.Workflows;

namespace CompositeC1Contrib.Localization.C1Console.ElementProvider
{
    public static class Actions
    {
        public static readonly ActionGroup ActionGroup = new ActionGroup(ActionGroupPriority.PrimaryHigh);
        public static readonly ActionLocation ActionLocation = new ActionLocation { ActionType = ActionType.Add, IsInFolder = false, IsInToolbar = true, ActionGroup = ActionGroup };

        public static ElementAction GetImportAction()
        {
            var actionToken = new WorkflowActionToken(typeof(ImportWorkflow), new[] { PermissionType.Administrate });

            return new ElementAction(new ActionHandle(actionToken))
            {
                VisualData = new ActionVisualizedData
                {
                    Label = "Import",
                    ToolTip = "Import",
                    Icon = new ResourceHandle("Composite.Icons", "generated-type-data-edit"),
                    ActionLocation = ActionLocation
                }
            };
        }

        public static ElementAction GetAddAction()
        {
            var actionToken = new WorkflowActionToken(typeof(AddResourceWorkflow), new[] { PermissionType.Add });

            return new ElementAction(new ActionHandle(actionToken))
            {
                VisualData = new ActionVisualizedData
                {
                    Label = "Add resource",
                    ToolTip = "Add resource",
                    Icon = new ResourceHandle("Composite.Icons", "generated-type-data-edit"),
                    ActionLocation = ActionLocation
                }
            };
        }

        public static ElementAction GetExportAction()
        {
            var actionToken = new WorkflowActionToken(typeof(ExportWorkflow), new[] { PermissionType.Administrate });

            return new ElementAction(new ActionHandle(actionToken))
            {
                VisualData = new ActionVisualizedData
                {
                    Label = "Export",
                    ToolTip = "Export",
                    Icon = new ResourceHandle("Composite.Icons", "generated-type-data-edit"),
                    ActionLocation = ActionLocation
                }
            };
        }

        public static ElementAction GetGenerateClassAction()
        {
            var actionToken = new WorkflowActionToken(typeof(GenerateClassWithKeysWorkflow), new[] { PermissionType.Administrate });

            return new ElementAction(new ActionHandle(actionToken))
            {
                VisualData = new ActionVisualizedData
                {
                    Label = "Generate class file",
                    ToolTip = "Generate class file",
                    Icon = new ResourceHandle("Composite.Icons", "generated-type-data-edit"),
                    ActionLocation = ActionLocation
                }
            };
        }
    }
}

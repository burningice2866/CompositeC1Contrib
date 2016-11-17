using Composite.C1Console.Elements;
using Composite.C1Console.Security;
using Composite.Core.ResourceSystem;

namespace CompositeC1Contrib.DownloadFoldersAsZip
{
    public static class Actions
    {
        private static readonly ActionGroup ActionGroup = new ActionGroup("Default", ActionGroupPriority.PrimaryLow);
        private static readonly ActionLocation ActionLocation = new ActionLocation { ActionType = ActionType.Add, IsInFolder = false, IsInToolbar = false, ActionGroup = ActionGroup };

        public static ElementAction CreateElementAction(ActionToken actionToken)
        {
            return new ElementAction(new ActionHandle(actionToken))
            {
                VisualData = new ActionVisualizedData
                {
                    Label = "Download",
                    ToolTip = "Download",
                    Icon = new ResourceHandle("Composite.Icons", "down"),
                    ActionLocation = ActionLocation
                }
            };
        }
    }
}

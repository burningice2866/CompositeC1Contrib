using System;

using Composite.C1Console.Elements;
using Composite.C1Console.Security;
using Composite.Core.ResourceSystem;
using Composite.Core.WebClient;

namespace CompositeC1Contrib.Sorting
{
    public static class Actions
    {
        private static readonly ActionGroup ActionGroup = new ActionGroup("Default", ActionGroupPriority.PrimaryLow);
        private static readonly ActionLocation ActionLocation = new ActionLocation { ActionType = ActionType.Add, IsInFolder = false, IsInToolbar = false, ActionGroup = ActionGroup };

        private static string baseUrl = UrlUtils.ResolveAdminUrl("InstalledPackages/CompositeC1Contrib.Sorting/");

        public static ElementAction CreateSortAction(string url, string label)
        {
            if (String.IsNullOrEmpty(label))
            {
                label = StringResourceSystemFacade.GetString("CompositeC1Contrib.Sorting", "Sort");
            }
            else
            {
                label = StringResourceSystemFacade.GetString("CompositeC1Contrib.Sorting", "Sort") + " " + label;
            }

            var urlAction = new UrlActionToken(label, baseUrl + url, new[] { PermissionType.Edit, PermissionType.Publish });

            return new ElementAction(new ActionHandle(urlAction))
            {
                VisualData = new ActionVisualizedData
                {
                    Label = label,
                    ToolTip = label,
                    Icon = new ResourceHandle("Composite.Icons", "cut"),
                    ActionLocation = ActionLocation
                }
            };
        }

        public static ElementAction CreateInterfaceToggleAction(ActionToken actionToken, string icon, string message)
        {
            return new ElementAction(new ActionHandle(actionToken))
            {
                VisualData = new ActionVisualizedData
                {
                    Label = message,
                    ToolTip = message,
                    Icon = new ResourceHandle("Composite.Icons", icon),
                    ActionLocation = ActionLocation
                }
            };
        }
    }
}

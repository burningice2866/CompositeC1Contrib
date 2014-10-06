using System.Collections.Generic;
using System.Linq;

using Composite.C1Console.Elements;
using Composite.C1Console.Elements.Plugins.ElementProvider;
using Composite.C1Console.Security;
using Composite.Core.ResourceSystem;
using Composite.Core.WebClient;
using Composite.Data;

using CompositeC1Contrib.SiteUpdate.C1Console.ElementProviders.EntityTokens;

namespace CompositeC1Contrib.SiteUpdate.C1Console.ElementProviders
{
    public class SiteUpdateElementProvider : IHooklessElementProvider, IAuxiliarySecurityAncestorProvider
    {
        private static readonly ActionGroup ActionGroup = new ActionGroup(ActionGroupPriority.PrimaryHigh);
        private static readonly ActionLocation ActionLocation = new ActionLocation { ActionType = ActionType.Add, IsInFolder = false, IsInToolbar = true, ActionGroup = ActionGroup };

        private const string ListUrl = "InstalledPackages/CompositeC1Contrib.SiteUpdate/siteUpdates.aspx";

        private ElementProviderContext _context;
        public ElementProviderContext Context
        {
            set { _context = value; }
        }

        public SiteUpdateElementProvider()
        {
            AuxiliarySecurityAncestorFacade.AddAuxiliaryAncestorProvider<DataEntityToken>(this);
        }

        public IEnumerable<Element> GetChildren(EntityToken entityToken, SearchToken searchToken)
        {
            return Enumerable.Empty<Element>();
        }

        public IEnumerable<Element> GetRoots(SearchToken searchToken)
        {
            var elementHandle = _context.CreateElementHandle(new SiteUpdateElementProviderEntityToken());
            var rootElement = new Element(elementHandle)
            {
                VisualData = new ElementVisualizedData
                {
                    Label = "SiteUpdate",
                    ToolTip = "SiteUpdate",
                    HasChildren = true,
                    Icon = new ResourceHandle("Composite.Icons", "localization-element-closed-root"),
                    OpenedIcon = new ResourceHandle("Composite.Icons", "localization-element-opened-root")
                }
            };

            AddViewAction(rootElement);

            return new[] { rootElement };
        }

        public Dictionary<EntityToken, IEnumerable<EntityToken>> GetParents(IEnumerable<EntityToken> entityTokens)
        {
            var dictionary = new Dictionary<EntityToken, IEnumerable<EntityToken>>();
            foreach (var token in entityTokens)
            {
                var dataToken = token as DataEntityToken;
                if (dataToken == null)
                {
                    continue;
                }
            }

            return dictionary;
        }

        private static void AddViewAction(Element element)
        {
            var url = UrlUtils.ResolveAdminUrl(ListUrl);

            var queuedUrlAction = new UrlActionToken("View", url, new[] { PermissionType.Administrate });
            element.AddAction(new ElementAction(new ActionHandle(queuedUrlAction))
            {
                VisualData = new ActionVisualizedData
                {
                    Label = "View",
                    ToolTip = "View",
                    Icon = new ResourceHandle("Composite.Icons", "localization-element-closed-root"),
                    ActionLocation = ActionLocation
                }
            });
        }
    }
}

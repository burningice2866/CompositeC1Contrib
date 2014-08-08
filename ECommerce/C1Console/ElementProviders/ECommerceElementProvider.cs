using System;
using System.Collections.Generic;
using System.Linq;

using Composite.C1Console.Elements;
using Composite.C1Console.Elements.Plugins.ElementProvider;
using Composite.C1Console.Security;
using Composite.Core.ResourceSystem;
using Composite.Core.WebClient;
using Composite.Data;

using CompositeC1Contrib.ECommerce.C1Console.ElementProviders.EntityTokens;
using CompositeC1Contrib.ECommerce.Data.Types;

namespace CompositeC1Contrib.ECommerce.C1Console.ElementProviders
{
    public class ECommerceElementProvider : IHooklessElementProvider, IAuxiliarySecurityAncestorProvider
    {
        private static readonly ActionGroup ActionGroup = new ActionGroup(ActionGroupPriority.PrimaryHigh);
        private static readonly ActionLocation ActionLocation = new ActionLocation { ActionType = ActionType.Add, IsInFolder = false, IsInToolbar = true, ActionGroup = ActionGroup };

        private const string UrlTemplate = "InstalledPackages/CompositeC1Contrib.ECommerce/ListShopOrders.aspx?status={0}";

        private ElementProviderContext _context;
        public ElementProviderContext Context
        {
            set { _context = value; }
        }

        public ECommerceElementProvider()
        {
            AuxiliarySecurityAncestorFacade.AddAuxiliaryAncestorProvider<DataEntityToken>(this);
        }

        public IEnumerable<Element> GetChildren(EntityToken entityToken, SearchToken searchToken)
        {
            if (entityToken is ECommerceElementProviderEntityToken)
            {
                var pendingOrdersCount = GetOrdersCount(PaymentStatus.Pending);
                var authorizedOrdersCount = GetOrdersCount(PaymentStatus.Authorized);

                var pendingElementHandle = _context.CreateElementHandle(new PendingOrdersEntityToken());
                var pendingElement = new Element(pendingElementHandle)
                {
                    VisualData = new ElementVisualizedData
                    {
                        Label = String.Format("Pending orders ({0})", pendingOrdersCount),
                        ToolTip = String.Format("Pending orders ({0})", pendingOrdersCount),
                        HasChildren = false,
                        Icon = new ResourceHandle("Composite.Icons", "localization-element-closed-root"),
                        OpenedIcon = new ResourceHandle("Composite.Icons", "localization-element-opened-root")
                    }
                };

                AddViewLogAction(PaymentStatus.Pending, pendingElement);

                yield return pendingElement;

                var authorizedElementHandle = _context.CreateElementHandle(new AuthorizedOrdersEntityToken());
                var autorizedElement = new Element(authorizedElementHandle)
                {
                    VisualData = new ElementVisualizedData
                    {
                        Label = String.Format("Autorized orders ({0})", authorizedOrdersCount),
                        ToolTip = String.Format("Autorized orders ({0})", authorizedOrdersCount),
                        HasChildren = false,
                        Icon = new ResourceHandle("Composite.Icons", "localization-element-closed-root"),
                        OpenedIcon = new ResourceHandle("Composite.Icons", "localization-element-opened-root")
                    }
                };

                AddViewLogAction(PaymentStatus.Authorized, autorizedElement);

                yield return autorizedElement;
            }
        }

        public IEnumerable<Element> GetRoots(SearchToken searchToken)
        {
            var elementHandle = _context.CreateElementHandle(new ECommerceElementProviderEntityToken());
            var rootElement = new Element(elementHandle)
            {
                VisualData = new ElementVisualizedData
                {
                    Label = "Shop",
                    ToolTip = "Shop",
                    HasChildren = true,
                    Icon = new ResourceHandle("Composite.Icons", "localization-element-closed-root"),
                    OpenedIcon = new ResourceHandle("Composite.Icons", "localization-element-opened-root")
                }
            };

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

        private static void AddViewLogAction(PaymentStatus status, Element element)
        {
            var url = String.Format(UrlTemplate, (int)status);
            url = UrlUtils.ResolveAdminUrl(url);

            var queuedUrlAction = new UrlActionToken("View log", url, new[] { PermissionType.Administrate });
            element.AddAction(new ElementAction(new ActionHandle(queuedUrlAction))
            {
                VisualData = new ActionVisualizedData
                {
                    Label = "View log",
                    ToolTip = "View log",
                    Icon = new ResourceHandle("Composite.Icons", "localization-element-closed-root"),
                    ActionLocation = ActionLocation
                }
            });
        }

        public int GetOrdersCount(PaymentStatus status)
        {
            using (var data = new DataConnection())
            {
                return data.Get<IShopOrder>().Count(s => s.PaymentStatus == (int)status);
            }
        }
    }
}

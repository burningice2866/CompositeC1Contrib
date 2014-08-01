using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;

using Composite.C1Console.Elements;
using Composite.C1Console.Elements.ElementProviderHelpers.AssociatedDataElementProviderHelper;
using Composite.C1Console.Elements.Plugins.ElementActionProvider;
using Composite.C1Console.Security;
using Composite.Core.ResourceSystem;
using Composite.Core.Types;
using Composite.Core.WebClient;
using Composite.Data;
using Composite.Data.Types;
using Composite.Plugins.Elements.ElementProviders.GeneratedDataTypesElementProvider;

namespace CompositeC1Contrib.Sorting
{
    [ConfigurationElementType(typeof(NonConfigurableElementActionProvider))]
    public class SortableActionProvider : IElementActionProvider
    {
        private static readonly Type SortableType = typeof(IGenericSortable);
        private static readonly ActionGroup ActionGroup = new ActionGroup("Default", ActionGroupPriority.PrimaryLow);
        private static readonly ActionLocation ActionLocation = new ActionLocation { ActionType = ActionType.Add, IsInFolder = false, IsInToolbar = false, ActionGroup = ActionGroup };

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

                        if (typeof(IGenericSortable).IsAssignableFrom(type))
                        {
                            message = "Disable sorting";
                            icon = "delete";
                        }
                        else
                        {
                            message = "Enable sorting";
                            icon = "accept";
                        }

                        var actionToken = new ToggleSuperInterfaceActionToken(SortableType);

                        yield return new ElementAction(new ActionHandle(actionToken))
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

            string url = null;
            string label = "Sort";

            var associatedToken = entityToken as AssociatedDataElementProviderHelperEntityToken;
            if (associatedToken != null)
            {
                var type = TypeManager.GetType(associatedToken.Payload);
                if (SortableType.IsAssignableFrom(type))
                {
                    var pageId = associatedToken.Id;

                    using (new DataScope(DataScopeIdentifier.Administrated))
                    {
                        var instances = DataFacade.GetData(type).Cast<IPageFolderData>().Where(f => f.PageId == Guid.Parse(associatedToken.Id));
                        if (instances.Any())
                        {
                            url = "Sort.aspx?type=" + type.FullName + "&pageId=" + pageId;
                        }
                    }
                }
            }

            var dataToken = entityToken as DataEntityToken;
            if (dataToken != null)
            {
                var type = dataToken.InterfaceType;

                if (typeof(IPage).IsAssignableFrom(type))
                {
                    var page = (IPage)dataToken.Data;

                    using (var data = new DataConnection(PublicationScope.Unpublished))
                    {
                        if (data.Get<IPageStructure>().Count(ps => ps.ParentId == page.Id) > 1)
                        {
                            url = "SortPages.aspx?pageId=" + page.Id;
                            label += " Childpages";
                        }
                    }
                }
            }

            if (String.IsNullOrEmpty(url))
            {
                yield break;
            }

            string baseUrl = UrlUtils.ResolveAdminUrl("InstalledPackages/CompositeC1Contrib.Sorting/");
            var urlAction = new UrlActionToken(label, baseUrl + url, new[] { PermissionType.Edit, PermissionType.Publish });

            yield return new ElementAction(new ActionHandle(urlAction))
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
    }
}
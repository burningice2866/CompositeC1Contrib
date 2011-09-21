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
using Composite.Data;
using Composite.Data.Types;
using Composite.Plugins.Elements.ElementProviders.GeneratedDataTypesElementProvider;

namespace CompositeC1Contrib.Sorting
{
    [ConfigurationElementType(typeof(NonConfigurableElementActionProvider))]
    public class SortableActionProvider : IElementActionProvider
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

                        yield return new ElementAction(new ActionHandle(new SortableActionToken()))
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

            string url = null;

            var associatedToken = entityToken as AssociatedDataElementProviderHelperEntityToken;
            if (associatedToken != null)
            {
                var type = TypeManager.GetType(associatedToken.Payload);
                var pageId = associatedToken.Id;

                using (new DataScope(DataScopeIdentifier.Administrated))
                {
                    var instances = DataFacade.GetData(type).Cast<IPageFolderData>().Where(f => f.PageId == Guid.Parse(associatedToken.Id));
                    if (instances.Any())
                    {
                        url = "/Composite/InstalledPackages/GenericSortable/Sort.aspx?type="+ type.FullName +"&pageId=" + pageId;
                    }
                }
            }

            var dataToken = entityToken as DataEntityToken;
            if (dataToken != null)
            {
                var type = dataToken.InterfaceType;

                if (typeof(IGenericSortable).IsAssignableFrom(type))
                {
                    if (!typeof(IPageFolderData).IsAssignableFrom(type))
                    {
                        url = "/Composite/InstalledPackages/GenericSortable/Sort.aspx?type=" + dataToken.InterfaceType.FullName;
                    }                    
                }
                else if (typeof(IPage).IsAssignableFrom(type))
                {
                    var page = (IPage)dataToken.Data;

                    url = "/Composite/InstalledPackages/GenericSortable/SortPages.aspx?pageId=" + page.Id;
                }
            }

            if (!String.IsNullOrEmpty(url))
            {
                var urlAction = new UrlActionToken("Sort", url, new[] { PermissionType.Edit, PermissionType.Publish });

                yield return new ElementAction(new ActionHandle(urlAction))
                {
                    VisualData = new ActionVisualizedData
                    {
                        Label = "Sort",
                        ToolTip = "Sort",
                        Icon = new ResourceHandle("Composite.Icons", "cut"),
                        ActionLocation = _actionLocation
                    }
                };
            }
        }
    }
}
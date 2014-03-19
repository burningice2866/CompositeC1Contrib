using System;
using System.Collections.Generic;
using System.Linq;

using Composite.C1Console.Elements;
using Composite.C1Console.Elements.Plugins.ElementAttachingProvider;
using Composite.C1Console.Security;
using Composite.C1Console.Workflow;
using Composite.Core.PageTemplates;
using Composite.Core.ResourceSystem;
using Composite.Core.WebClient;
using Composite.Data;
using Composite.Data.Types;

using CompositeC1Contrib.Teasers.Data;
using CompositeC1Contrib.Teasers.C1Console.Actions;
using CompositeC1Contrib.Teasers.C1Console.EntityTokens;
using CompositeC1Contrib.Teasers.C1Console.WorkFlows;
using CompositeC1Contrib.Teasers.Configuration;

namespace CompositeC1Contrib.Teasers.C1Console
{
    public class TeaserElementAttachingProvider : IElementAttachingProvider
    {
        private static readonly ActionGroup ActionGroup = new ActionGroup(ActionGroupPriority.PrimaryHigh);
        
        public static readonly IDictionary<Guid, IList<Tuple<string, string>>> TemplateTeaserPositions;
        public static readonly ActionLocation ActionLocation = new ActionLocation { ActionType = ActionType.Add, IsInFolder = false, IsInToolbar = true, ActionGroup = ActionGroup };

        private ElementProviderContext _context;
        public ElementProviderContext Context
        {
            set { _context = value; }
        }

        static TeaserElementAttachingProvider()
        {
            TemplateTeaserPositions = new Dictionary<Guid, IList<Tuple<string, string>>>();

            var config = TeasersSection.GetSection();
            var positions = config.Positions.Cast<TeasersPositionElement>().ToDictionary(e => e.Name, e => e.Label);
            var templates = PageTemplateFacade.GetPageTemplates();

            foreach (var template in templates)
            {
                var templatePositions = config.Templates.Cast<TeasersTemplateElement>().SingleOrDefault(e => e.Guid == template.Id);
                if (templatePositions == null)
                {
                    continue;
                }

                var tupleList = templatePositions.Positions.Cast<TeasersTemplatePositionElement>().Select(e => Tuple.Create(e.Name, positions[e.Name])).ToList();

                TemplateTeaserPositions.Add(template.Id, tupleList);
            }
        }

        public ElementAttachingProviderResult GetAlternateElementList(EntityToken parentEntityToken, Dictionary<string, string> piggybag)
        {
            var dataToken = parentEntityToken as DataEntityToken;
            if (dataToken == null || dataToken.InterfaceType != typeof(IPage))
            {
                return null;
            }

            var page = (IPage)dataToken.Data;

            if (!TemplateTeaserPositions.ContainsKey(page.TemplateId))
            {
                return null;
            }

            return new ElementAttachingProviderResult
            {
                Elements = GetRoot(page, piggybag),
                Position = ElementAttachingProviderPosition.Top,
                PositionPriority = 1
            };
        }

        public IEnumerable<Element> GetRoot(IPage page, Dictionary<string, string> piggybag)
        {
            if (!TemplateTeaserPositions.ContainsKey(page.TemplateId))
            {
                return Enumerable.Empty<Element>();
            }

            var elementHandle = _context.CreateElementHandle(new TeasersElementProviderEntityToken(page));
            var rootElement = new Element(elementHandle)
            {
                VisualData = new ElementVisualizedData
                {
                    Label = "Teasers",
                    ToolTip = "Teasers",
                    HasChildren = true,
                    Icon = new ResourceHandle("Composite.Icons", "template"),
                    OpenedIcon = new ResourceHandle("Composite.Icons", "template")
                }
            };

            return new[] { rootElement };
        }

        public bool HaveCustomChildElements(EntityToken parentEntityToken, Dictionary<string, string> piggybag)
        {
            var dataToken = parentEntityToken as DataEntityToken;

            return dataToken != null && dataToken.InterfaceType == typeof(IPage);
        }

        public IEnumerable<Element> GetChildren(EntityToken entityToken, Dictionary<string, string> piggybag)
        {
            var list = new List<Element>();

            var providerToken = entityToken as TeasersElementProviderEntityToken;
            if (providerToken != null)
            {
                AddPositionFolders(entityToken, list);
            }

            var folderToken = entityToken as PageTeaserPositionFolderEntityToken;
            if (folderToken != null)
            {
                AddTeaserInstances(entityToken, list, folderToken);
            }

            return list;
        }

        private void AddTeaserInstances(EntityToken entityToken, List<Element> list, PageTeaserPositionFolderEntityToken folderToken)
        {
            var page = PageManager.GetPageById(new Guid(entityToken.Source));
            var instances = TeaserFacade.GetPageTeasers(page, folderToken.Id, false);

            foreach (var instance in instances)
            {
                var attributes = instance.DataSourceId.InterfaceType.GetCustomAttributes(true).ToList();

                var icon = new ResourceHandle("Composite.Icons", "dataassociation-rootfolder-closed");
                var openedIcon = new ResourceHandle("Composite.Icons", "dataassociation-rootfolder-open");

                var iconAttribute = attributes.OfType<IconAttribute>().FirstOrDefault();
                if (iconAttribute != null)
                {
                    icon = new ResourceHandle("Composite.Icons", iconAttribute.Name);

                    if (!String.IsNullOrEmpty(iconAttribute.OpenedName))
                    {
                        openedIcon = new ResourceHandle("Composite.Icons", iconAttribute.OpenedName);
                    }
                }

                var label = "No label";
                try
                {
                    label = instance.GetLabel();
                }
                catch (Exception) { }

                var elementHandle = _context.CreateElementHandle(new PageTeaserInstanceEntityToken(page, instance));
                var teaserElement = new Element(elementHandle)
                {
                    VisualData = new ElementVisualizedData
                    {
                        Label = label,
                        ToolTip = label,
                        HasChildren = false,
                        Icon = icon,
                        OpenedIcon = openedIcon
                    }
                };

                var editWorkflowAttribute = attributes.OfType<EditWorkflowAttribute>().FirstOrDefault();
                if (editWorkflowAttribute != null)
                {
                    var editActionToken = new WorkflowActionToken(editWorkflowAttribute.EditWorkflowType);
                    teaserElement.AddAction(new ElementAction(new ActionHandle(editActionToken))
                    {
                        VisualData = new ActionVisualizedData
                        {
                            Label = "Edit",
                            ToolTip = "Edit",
                            Icon = new ResourceHandle("Composite.Icons", "generated-type-data-edit"),
                            ActionLocation = ActionLocation
                        }
                    });
                }

                var deleteActionToken = new ConfirmWorkflowActionToken("Delete: " + label, typeof(DeletePageTeaserActionToken));
                teaserElement.AddAction(new ElementAction(new ActionHandle(deleteActionToken))
                {
                    VisualData = new ActionVisualizedData
                    {
                        Label = "Delete",
                        ToolTip = "Delete",
                        Icon = new ResourceHandle("Composite.Icons", "generated-type-data-delete"),
                        ActionLocation = ActionLocation
                    }
                });

                list.Add(teaserElement);
            }
        }

        private void AddPositionFolders(EntityToken entityToken, List<Element> list)
        {
            var page = PageManager.GetPageById(new Guid(entityToken.Source));
            IList<Tuple<string, string>> positions;
            
            if (!TemplateTeaserPositions.TryGetValue(page.TemplateId, out positions))
            {
                return;
            }

            foreach (var position in positions)
            {
                var elementHandle = _context.CreateElementHandle(new PageTeaserPositionFolderEntityToken(page, position.Item1));
                var positionElement = new Element(elementHandle)
                {
                    VisualData = new ElementVisualizedData
                    {
                        Label = position.Item2,
                        ToolTip = position.Item2,
                        HasChildren = TeaserFacade.GetPageTeasers(page, position.Item1, false).Any(),
                        Icon = new ResourceHandle("Composite.Icons", "folder"),
                        OpenedIcon = new ResourceHandle("Composite.Icons", "folder_active"),
                    }
                };

                var addActionToken = new WorkflowActionToken(typeof(AddPageTeaserWorkFlow));
                positionElement.AddAction(new ElementAction(new ActionHandle(addActionToken))
                {
                    VisualData = new ActionVisualizedData
                    {
                        Label = "Add teaser",
                        ToolTip = "Add teaser",
                        Icon = new ResourceHandle("Composite.Icons", "generated-type-data-add"),
                        ActionLocation = ActionLocation
                    }
                });

                var url = String.Format("InstalledPackages/FSD.Teaser/SortPageTeasers.aspx?pageId={0}&position={1}", page.Id, position);
                var sortActionToken = new UrlActionToken("Sort fields", UrlUtils.ResolveAdminUrl(url), new[] { PermissionType.Add, PermissionType.Edit, PermissionType.Administrate, });
                positionElement.AddAction(new ElementAction(new ActionHandle(sortActionToken))
                {
                    VisualData = new ActionVisualizedData
                    {
                        Label = "Sort teasers",
                        ToolTip = "Sort teasers",
                        Icon = new ResourceHandle("Composite.Icons", "cut"),
                        ActionLocation = ActionLocation
                    }
                });

                list.Add(positionElement);
            }
        }
    }
}

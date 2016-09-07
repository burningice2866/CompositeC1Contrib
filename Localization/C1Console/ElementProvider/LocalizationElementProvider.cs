using System;
using System.Collections.Generic;
using System.Linq;

using Composite.C1Console.Elements;
using Composite.C1Console.Elements.Plugins.ElementProvider;
using Composite.C1Console.Security;
using Composite.C1Console.Workflow;
using Composite.Core.ResourceSystem;
using Composite.Data;

using CompositeC1Contrib.Localization.C1Console.Actions;
using CompositeC1Contrib.Localization.C1Console.ElementProvider.EntityTokens;
using CompositeC1Contrib.Localization.C1Console.Workflows;

namespace CompositeC1Contrib.Localization.C1Console.ElementProvider
{
    public class LocalizationElementProvider : IHooklessElementProvider, IAuxiliarySecurityAncestorProvider
    {
        private static readonly ActionGroup ActionGroup = new ActionGroup(ActionGroupPriority.PrimaryHigh);
        private static readonly ActionLocation ActionLocation = new ActionLocation { ActionType = ActionType.Add, IsInFolder = false, IsInToolbar = true, ActionGroup = ActionGroup };

        private ElementProviderContext _context;
        public ElementProviderContext Context
        {
            set { _context = value; }
        }

        public LocalizationElementProvider()
        {
            AuxiliarySecurityAncestorFacade.AddAuxiliaryAncestorProvider<DataEntityToken>(this);
        }

        public IEnumerable<Element> GetChildren(EntityToken entityToken, SearchToken searchToken)
        {
            var elements = new List<Element>();

            var folderToken = entityToken as NamespaceFolderEntityToken;
            if (folderToken != null)
            {
                foreach (var el in GetNamespaceAndResourceElements(_context, folderToken.Namespace))
                {
                    elements.Add(el);
                }
            }

            if (entityToken is LocalizationElementProviderEntityToken)
            {
                foreach (var el in GetNamespaceAndResourceElements(_context, String.Empty))
                {
                    elements.Add(el);
                }
            }

            return elements;
        }

        private static IEnumerable<Element> GetNamespaceAndResourceElements(ElementProviderContext context, string ns)
        {
            var resources = GetResourceKeys(ns);

            var folders = new List<string>();
            var elements = new List<Element>();

            foreach (var key in resources)
            {
                var label = key.Key;

                if (label == ns)
                {
                    continue;
                }


                var labelParts = label.Split('.');

                if (!String.IsNullOrEmpty(ns))
                {
                    var nsParts = ns.Split('.');

                    if (nsParts.Length > labelParts.Length)
                    {
                        continue;
                    }

                    labelParts = labelParts.Skip(nsParts.Length).ToArray();
                    label = String.Join(".", labelParts);
                }

                if (labelParts.Length > 1)
                {
                    var folder = labelParts[0];

                    if (!folders.Contains(folder))
                    {
                        folders.Add(folder);
                    }
                }
                else if (labelParts.Length == 1)
                {
                    var token = key.GetDataEntityToken();

                    var elementHandle = context.CreateElementHandle(token);
                    var element = new Element(elementHandle)
                    {
                        VisualData = new ElementVisualizedData
                        {
                            Label = label,
                            ToolTip = label,
                            HasChildren = false,
                            Icon = ResourceHandle.BuildIconFromDefaultProvider("localization-element-closed-root"),
                            OpenedIcon = ResourceHandle.BuildIconFromDefaultProvider("localization-element-opened-root")
                        }
                    };

                    var editActionToken = new WorkflowActionToken(typeof(EditResourceWorkflow), new[] { PermissionType.Edit });
                    element.AddAction(new ElementAction(new ActionHandle(editActionToken))
                    {
                        VisualData = new ActionVisualizedData
                        {
                            Label = "Edit",
                            ToolTip = "Edit",
                            Icon = new ResourceHandle("Composite.Icons", "generated-type-data-edit"),
                            ActionLocation = ActionLocation
                        }
                    });

                    var deleteActionToken = new ConfirmWorkflowActionToken("Are you sure?", typeof(DeleteResourceActionToken));
                    element.AddAction(new ElementAction(new ActionHandle(deleteActionToken))
                    {
                        VisualData = new ActionVisualizedData
                        {
                            Label = "Delete",
                            ToolTip = "Delete",
                            Icon = new ResourceHandle("Composite.Icons", "generated-type-data-delete"),
                            ActionLocation = ActionLocation
                        }
                    });

                    elements.Add(element);
                }
            }

            foreach (var folder in folders.OrderBy(f => f))
            {
                var handleNamespace = folder;
                if (!String.IsNullOrEmpty(ns))
                {
                    handleNamespace = ns + "." + handleNamespace;
                }

                var folderElement = NamespaceFolderEntityToken.CreateElement(context, folder, handleNamespace);

                var deleteActionToken = new ConfirmWorkflowActionToken("Are you sure?", typeof(DeleteNamespaceActionToken));
                folderElement.AddAction(new ElementAction(new ActionHandle(deleteActionToken))
                {
                    VisualData = new ActionVisualizedData
                    {
                        Label = "Delete",
                        ToolTip = "Delete",
                        Icon = new ResourceHandle("Composite.Icons", "generated-type-data-delete"),
                        ActionLocation = ActionLocation
                    }
                });

                AppendAddAction(folderElement);
                AppendRenameNamespaceAction(folderElement);
                AppendExportAction(folderElement);

                yield return folderElement;
            }

            foreach (var el in elements)
            {
                yield return el;
            }
        }

        public IEnumerable<Element> GetRoots(SearchToken searchToken)
        {
            var elementHandle = _context.CreateElementHandle(new LocalizationElementProviderEntityToken());
            var rootElement = new Element(elementHandle)
            {
                VisualData = new ElementVisualizedData
                {
                    Label = "Localization",
                    ToolTip = "Localization",
                    HasChildren = true,
                    Icon = new ResourceHandle("Composite.Icons", "localization-element-closed-root"),
                    OpenedIcon = new ResourceHandle("Composite.Icons", "localization-element-opened-root")
                }
            };

            AppendImportAction(rootElement);
            AppendExportAction(rootElement);
            AppendGenerateClassAction(rootElement);
            AppendAddAction(rootElement);

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

                var key = dataToken.Data as IResourceKey;
                if (key == null)
                {
                    continue;
                }

                var parts = key.Key.Split('.');
                var ns = String.Join(".", parts.Take(parts.Length - 1));

                dictionary.Add(token, new[] { new NamespaceFolderEntityToken(ns) });
            }

            return dictionary;
        }

        private static void AppendAddAction(Element element)
        {
            var actionToken = new WorkflowActionToken(typeof(AddResourceWorkflow), new[] { PermissionType.Administrate });
            element.AddAction(new ElementAction(new ActionHandle(actionToken))
            {
                VisualData = new ActionVisualizedData
                {
                    Label = "Add resource",
                    ToolTip = "Add resource",
                    Icon = new ResourceHandle("Composite.Icons", "generated-type-data-edit"),
                    ActionLocation = ActionLocation
                }
            });
        }

        private static void AppendGenerateClassAction(Element element)
        {
            var actionToken = new WorkflowActionToken(typeof(GenerateClassWithKeysWorkflow), new[] { PermissionType.Read });
            element.AddAction(new ElementAction(new ActionHandle(actionToken))
            {
                VisualData = new ActionVisualizedData
                {
                    Label = "Generate class file",
                    ToolTip = "Generate class file",
                    Icon = new ResourceHandle("Composite.Icons", "generated-type-data-edit"),
                    ActionLocation = ActionLocation
                }
            });
        }

        private static void AppendExportAction(Element element)
        {
            var actionToken = new WorkflowActionToken(typeof(ExportWorkflow), new[] { PermissionType.Read });
            element.AddAction(new ElementAction(new ActionHandle(actionToken))
            {
                VisualData = new ActionVisualizedData
                {
                    Label = "Export",
                    ToolTip = "Export",
                    Icon = new ResourceHandle("Composite.Icons", "generated-type-data-edit"),
                    ActionLocation = ActionLocation
                }
            });
        }

        private static void AppendImportAction(Element element)
        {
            var actionToken = new WorkflowActionToken(typeof(ImportWorkflow), new[] { PermissionType.Read });
            element.AddAction(new ElementAction(new ActionHandle(actionToken))
            {
                VisualData = new ActionVisualizedData
                {
                    Label = "Import",
                    ToolTip = "Import",
                    Icon = new ResourceHandle("Composite.Icons", "generated-type-data-edit"),
                    ActionLocation = ActionLocation
                }
            });
        }

        private static void AppendRenameNamespaceAction(Element element)
        {
            var actionToken = new WorkflowActionToken(typeof(RenameNamespaceWorkflow), new[] { PermissionType.Edit });
            element.AddAction(new ElementAction(new ActionHandle(actionToken))
            {
                VisualData = new ActionVisualizedData
                {
                    Label = "Rename",
                    ToolTip = "Rename",
                    Icon = new ResourceHandle("Composite.Icons", "generated-type-data-edit"),
                    ActionLocation = ActionLocation
                }
            });
        }

        public static IEnumerable<IResourceKey> GetResourceKeys(string ns)
        {
            using (var data = new DataConnection())
            {
                return from k in data.Get<IResourceKey>()
                       where k.ResourceSet == null && k.Key.StartsWith(ns)
                       orderby k.Key
                       select k;
            }
        }
    }
}
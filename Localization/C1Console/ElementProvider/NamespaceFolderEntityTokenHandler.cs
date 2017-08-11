using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

using Composite.C1Console.Elements;
using Composite.C1Console.Security;
using Composite.C1Console.Workflow;
using Composite.Core.ResourceSystem;
using Composite.Data;

using CompositeC1Contrib.Composition;
using CompositeC1Contrib.Localization.C1Console.Actions;
using CompositeC1Contrib.Localization.C1Console.ElementProvider.EntityTokens;
using CompositeC1Contrib.Localization.C1Console.Workflows;

namespace CompositeC1Contrib.Localization.C1Console.ElementProvider
{
    [Export("Localization", typeof(IElementProviderFor))]
    public class NamespaceFolderEntityTokenHandler : IElementProviderFor
    {
        public IEnumerable<Type> ProviderFor => new[] { typeof(NamespaceFolderEntityToken), typeof(LocalizationElementProviderEntityToken) };

        public IEnumerable<Element> Provide(ElementProviderContext context, EntityToken token)
        {
            var resourceSet = String.Empty;
            var ns = String.Empty;

            if (token is LocalizationEntityToken localizationEntityToken)
            {
                resourceSet = localizationEntityToken.ResourceSet;
            }

            if (token is NamespaceFolderEntityToken namespaceToken)
            {
                ns = namespaceToken.Namespace;
            }

            foreach (var el in GetNamespaceAndResourceElements(context, resourceSet, ns))
            {
                yield return el;
            }
        }

        private static IEnumerable<Element> GetNamespaceAndResourceElements(ElementProviderContext context, string resourceSet, string ns)
        {
            var resources = LocalizationsFacade.GetResourceKeys(ns, resourceSet);

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
                    var element = CreateResourceElement(resourceSet, label, elementHandle);

                    var editActionToken = new WorkflowActionToken(typeof(EditResourceWorkflow), new[] { PermissionType.Edit });
                    element.AddAction(new ElementAction(new ActionHandle(editActionToken))
                    {
                        VisualData = new ActionVisualizedData
                        {
                            Label = "Edit",
                            ToolTip = "Edit",
                            Icon = new ResourceHandle("Composite.Icons", "generated-type-data-edit"),
                            ActionLocation = Actions.ActionLocation
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
                            ActionLocation = Actions.ActionLocation
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

                var folderElement = NamespaceFolderEntityToken.CreateElement(context, folder, resourceSet, handleNamespace);

                var deleteActionToken = new ConfirmWorkflowActionToken("Are you sure?", typeof(DeleteNamespaceActionToken));
                folderElement.AddAction(new ElementAction(new ActionHandle(deleteActionToken))
                {
                    VisualData = new ActionVisualizedData
                    {
                        Label = "Delete",
                        ToolTip = "Delete",
                        Icon = new ResourceHandle("Composite.Icons", "generated-type-data-delete"),
                        ActionLocation = Actions.ActionLocation
                    }
                });

                folderElement.AddAction(Actions.GetAddAction());
                folderElement.AddAction(Actions.GetExportAction());

                yield return folderElement;
            }

            foreach (var el in elements)
            {
                yield return el;
            }
        }

        private static Element CreateResourceElement(string resourceSet, string label, ElementHandle elementHandle)
        {
            if (String.IsNullOrEmpty(resourceSet))
            {
                var dragAndDropInfo = new ElementDragAndDropInfo(typeof(IResourceKey));

                dragAndDropInfo.AddDropType(typeof(NamespaceFolderEntityToken));
                dragAndDropInfo.SupportsIndexedPosition = false;

                return new Element(elementHandle, dragAndDropInfo)
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
            }

            return new Element(elementHandle)
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
        }
    }
}

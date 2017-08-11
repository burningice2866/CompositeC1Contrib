﻿using System;
using System.Collections.Generic;
using System.Linq;

using Composite.C1Console.Actions;
using Composite.C1Console.Elements;
using Composite.C1Console.Elements.Plugins.ElementProvider;
using Composite.C1Console.Security;
using Composite.Core.ResourceSystem;
using Composite.Data;

using CompositeC1Contrib.Composition;
using CompositeC1Contrib.Localization.C1Console.ElementProvider.EntityTokens;

namespace CompositeC1Contrib.Localization.C1Console.ElementProvider
{
    public class LocalizationElementProvider : HooklessElementProvider, IAuxiliarySecurityAncestorProvider, IDragAndDropElementProvider
    {
        public LocalizationElementProvider() : base("Localization")
        {
            AuxiliarySecurityAncestorFacade.AddAuxiliaryAncestorProvider<DataEntityToken>(this);
        }

        protected override IEnumerable<Element> GetRootsImpl(SearchToken searchToken)
        {
            var elementHandle = Context.CreateElementHandle(new LocalizationElementProviderEntityToken(String.Empty));
            var rootElement = new Element(elementHandle)
            {
                VisualData = new ElementVisualizedData
                {
                    Label = "Localization",
                    ToolTip = "Localization",
                    HasChildren = LocalizationsFacade.GetResourceKeys(String.Empty, String.Empty).Any(),
                    Icon = new ResourceHandle("Composite.Icons", "localization-element-closed-root"),
                    OpenedIcon = new ResourceHandle("Composite.Icons", "localization-element-opened-root")
                }
            };

            rootElement.AddAction(Actions.GetAddAction());
            rootElement.AddAction(Actions.GetGenerateClassAction());
            rootElement.AddAction(Actions.GetImportAction());
            rootElement.AddAction(Actions.GetExportAction());

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

                dictionary.Add(token, new[] { new NamespaceFolderEntityToken(key.ResourceSet, ns) });
            }

            return dictionary;
        }

        public bool OnElementDraggedAndDropped(EntityToken draggedEntityToken, EntityToken newParentEntityToken, int dropIndex, DragAndDropType dragAndDropType, FlowControllerServicesContainer draggedElementFlowControllerServicesContainer)
        {
            var destinationNamespace = (NamespaceFolderEntityToken)newParentEntityToken;

            if (draggedEntityToken is DataEntityToken dataToken)
            {
                var draggedResource = (IResourceKey)dataToken.Data;

                draggedResource.Key = destinationNamespace.Namespace + "." + draggedResource.Key.Split('.').Last();

                using (var data = new DataConnection())
                {
                    data.Update(draggedResource);
                }
            }

            if (draggedEntityToken is NamespaceFolderEntityToken namespaceFolderToken)
            {
                var key = namespaceFolderToken.Namespace.Split('.').Last();

                LocalizationsFacade.RenameNamespace(namespaceFolderToken.Namespace, destinationNamespace.Namespace + "." + key, destinationNamespace.ResourceSet);
            }

            var treeRefresher = new SpecificTreeRefresher(draggedElementFlowControllerServicesContainer);

            treeRefresher.PostRefreshMesseges(new LocalizationElementProviderEntityToken(destinationNamespace.ResourceSet));

            return true;
        }
    }
}
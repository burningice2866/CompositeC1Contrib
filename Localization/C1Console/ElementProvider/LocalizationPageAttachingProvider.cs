using System;
using System.Collections.Generic;
using System.Linq;

using Composite.C1Console.Elements;
using Composite.C1Console.Elements.Plugins.ElementAttachingProvider;
using Composite.C1Console.Security;
using Composite.Core.ResourceSystem;
using Composite.Data;
using Composite.Data.Types;

using CompositeC1Contrib.Composition;
using CompositeC1Contrib.Localization.C1Console.ElementProvider.EntityTokens;

namespace CompositeC1Contrib.Localization.C1Console.ElementProvider
{
    public class LocalizationPageAttachingProvider : IElementAttachingProvider, IAuxiliarySecurityAncestorProvider
    {
        private ProviderContainer<IElementProviderFor> EntityTokenHandlers { get; }
        private ProviderContainer<IElementActionProviderFor> ElementActionProviders { get; }

        public ElementProviderContext Context { get; set; }

        public LocalizationPageAttachingProvider()
        {
            EntityTokenHandlers = new ProviderContainer<IElementProviderFor>("Localization");
            ElementActionProviders = new ProviderContainer<IElementActionProviderFor>("Localization");
        }

        public ElementAttachingProviderResult GetAlternateElementList(EntityToken parentEntityToken, Dictionary<string, string> piggybag)
        {
            var dataToken = parentEntityToken as DataEntityToken;
            if (dataToken == null || dataToken.InterfaceType != typeof(IPage))
            {
                return null;
            }

            var page = (IPage)dataToken.Data;
            if (page == null)
            {
                return null;
            }

            if (PageManager.GetParentId(page.Id) != Guid.Empty)
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
            var elementHandle = Context.CreateElementHandle(new LocalizationElementProviderEntityToken("site:" + page.Id));
            var rootElement = new Element(elementHandle)
            {
                VisualData = new ElementVisualizedData
                {
                    Label = "Localization",
                    ToolTip = "Localization",
                    HasChildren = LocalizationsFacade.GetResourceKeys(String.Empty, "site:" + page.Id).Any(),
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

        public bool HaveCustomChildElements(EntityToken parentEntityToken, Dictionary<string, string> piggybag)
        {
            var dataToken = parentEntityToken as DataEntityToken;
            if (dataToken == null || dataToken.InterfaceType != typeof(IPage))
            {
                return false;
            }

            var page = dataToken.Data as IPage;
            if (page == null || PageManager.GetParentId(page.Id) != Guid.Empty)
            {
                return false;
            }

            return true;
        }

        public IEnumerable<Element> GetChildren(EntityToken parentEntityToken, Dictionary<string, string> piggybag)
        {
            var elementProviders = EntityTokenHandlers.GetProvidersFor(parentEntityToken);
            var elements = elementProviders.SelectMany(p => p.Provide(Context, parentEntityToken)).ToList();

            foreach (var el in elements)
            {
                var token = el.ElementHandle.EntityToken;

                var actionProviders = ElementActionProviders.GetProvidersFor(token);
                foreach (var provider in actionProviders)
                {
                    provider.AddActions(el);
                }
            }

            return elements;
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
    }
}

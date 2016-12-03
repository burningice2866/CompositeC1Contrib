using System.Collections.Generic;
using System.Linq;

using Composite.C1Console.Elements;
using Composite.C1Console.Elements.Plugins.ElementProvider;
using Composite.C1Console.Security;

namespace CompositeC1Contrib.Composition
{
    public abstract class HooklessElementProvider : IHooklessElementProvider
    {
        protected ProviderContainer<IElementProviderFor> EntityTokenHandlers { get; }
        protected ProviderContainer<IElementActionProviderFor> ElementActionProviders { get; }

        public ElementProviderContext Context
        {
            protected get; set;
        }

        protected HooklessElementProvider(string contract)
        {
            EntityTokenHandlers = new ProviderContainer<IElementProviderFor>(contract);
            ElementActionProviders = new ProviderContainer<IElementActionProviderFor>(contract);
        }

        public virtual IEnumerable<Element> GetChildren(EntityToken entityToken, SearchToken seachToken)
        {
            var elementProviders = EntityTokenHandlers.GetProvidersFor(entityToken);
            var elements = elementProviders.SelectMany(p => p.Provide(Context, entityToken)).ToList();

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

        public IEnumerable<Element> GetRoots(SearchToken seachToken)
        {
            var elements = GetRootsImpl(seachToken).ToList();

            foreach (var el in elements)
            {
                var providers = ElementActionProviders.GetProvidersFor(el.ElementHandle.EntityToken);
                foreach (var provider in providers)
                {
                    provider.AddActions(el);
                }
            }

            return elements;
        }

        protected abstract IEnumerable<Element> GetRootsImpl(SearchToken seachToken);
    }
}

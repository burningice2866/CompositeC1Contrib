using System.Collections.Generic;
using System.Linq;

using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;

using Composite.C1Console.Elements;
using Composite.C1Console.Elements.Plugins.ElementActionProvider;
using Composite.C1Console.Security;

using CompositeC1Contrib.Composition;

namespace CompositeC1Contrib
{
    [ConfigurationElementType(typeof(NonConfigurableElementActionProvider))]
    public class GenericActionProvider : IElementActionProvider
    {
        private readonly ProviderContainer<IElementActionProviderFor> _providerContainer = new ProviderContainer<IElementActionProviderFor>();

        public IEnumerable<ElementAction> GetActions(EntityToken entityToken)
        {
            var providers = _providerContainer.GetProvidersFor(entityToken);
            var actions = providers.SelectMany(p => p.Provide(entityToken));

            return actions.ToList();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

using Composite.C1Console.Security;

namespace CompositeC1Contrib.Composition
{
    public class ProviderContainer<T> where T : IProviderFor
    {
        private static IDictionary<Type, IList<T>> ElementActionProviders = new Dictionary<Type, IList<T>>();

        public ProviderContainer() : this(null) { }

        public ProviderContainer(string contract)
        {
            var providers = CompositionContainerFacade.GetExportedValues<T>(contract).ToList();

            foreach (var provider in providers)
            {
                foreach (var t in provider.ProviderFor)
                {
                    IList<T> list;

                    if (!ElementActionProviders.TryGetValue(t, out list))
                    {
                        list = new List<T>();

                        ElementActionProviders.Add(t, list);
                    }

                    list.Add(provider);
                }
            }
        }

        public IEnumerable<T> GetProvidersFor(EntityToken entityToken)
        {
            IList<T> providers;
            if (ElementActionProviders.TryGetValue(entityToken.GetType(), out providers))
            {
                return providers;
            }

            return Enumerable.Empty<T>();
        }
    }
}

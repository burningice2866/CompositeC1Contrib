using System;
using System.Collections.Generic;
using System.Linq;

using Composite.C1Console.Security;

namespace CompositeC1Contrib.Composition
{
    public class ProviderContainer<T> where T : IProviderFor
    {
        private readonly IDictionary<Type, IList<T>> _providers = new Dictionary<Type, IList<T>>();

        public ProviderContainer() : this(null) { }

        public ProviderContainer(string contract)
        {
            var exports = CompositionContainerFacade.BuildContainer().GetExports<T, IDictionary<string, object>>(contract).OrderBy(e =>
            {
                object order;
                if (e.Metadata.TryGetValue("Order", out order))
                {
                    return (int)order;
                }

                return -1;
            });

            var providers = exports.Select(e => e.Value);

            foreach (var provider in providers)
            {
                foreach (var t in provider.ProviderFor)
                {
                    IList<T> list;

                    if (!_providers.TryGetValue(t, out list))
                    {
                        list = new List<T>();

                        _providers.Add(t, list);
                    }

                    list.Add(provider);
                }
            }
        }

        public IEnumerable<T> GetProvidersFor(EntityToken entityToken)
        {
            IList<T> providers;
            if (_providers.TryGetValue(entityToken.GetType(), out providers))
            {
                return providers;
            }

            return Enumerable.Empty<T>();
        }
    }
}

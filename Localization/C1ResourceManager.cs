using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Resources;

using Composite.Data;

using CompositeC1Contrib.Localization.Configuration;

namespace CompositeC1Contrib.Localization
{
    public class C1ResourceManager : ResourceManager
    {
        private bool _ignoreCase;
        private readonly ConcurrentDictionary<CultureInfo, ResourceSet> _resourceSets = new ConcurrentDictionary<CultureInfo, ResourceSet>();

        public override Type ResourceSetType => typeof(C1ResourceSet);

        public override bool IgnoreCase
        {
            get => _ignoreCase;
            set => _ignoreCase = value;
        }

        public C1ResourceManager(string resourceSet)
        {
            BaseNameField = resourceSet;

            var config = LocalizationSection.GetSection();
            if (config != null)
            {
                _ignoreCase = config.IgnoreCase;
            }

            void InvalidateCache(object sender, StoreEventArgs e)
            {
                _resourceSets.Clear();
            }

            DataEvents<IResourceKey>.OnStoreChanged += InvalidateCache;
            DataEvents<IResourceValue>.OnStoreChanged += InvalidateCache;
        }

        protected override ResourceSet InternalGetResourceSet(CultureInfo culture, bool createIfNotExists, bool tryParents)
        {
            return _resourceSets.GetOrAdd(culture, ci => new C1ResourceSet(BaseName, ci));
        }
    }
}

using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Resources;

using Composite.Data;

namespace CompositeC1Contrib.Localization
{
    public class C1ResourceManager : ResourceManager
    {
        ConcurrentDictionary<CultureInfo, ResourceSet> _resourceSets = new ConcurrentDictionary<CultureInfo, ResourceSet>();

        private static object AddSyncLock = new object();

        public override Type ResourceSetType
        {
            get { return typeof(C1ResourceSet); }
        }

        public C1ResourceManager(string resourceSet)
        {
            BaseNameField = resourceSet;

            StoreEventHandler invalidateCache = (sender, e) =>
            {
                _resourceSets.Clear();
            };

            DataEvents<IResourceKey>.OnStoreChanged += invalidateCache;
            DataEvents<IResourceValue>.OnStoreChanged += invalidateCache;
        }

        protected override ResourceSet InternalGetResourceSet(CultureInfo culture, bool createIfNotExists, bool tryParents)
        {
            return _resourceSets.GetOrAdd(culture, ci =>
            {
                return new C1ResourceSet(BaseName, ci);
            });
        }
    }
}

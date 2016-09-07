using System;
using System.Globalization;
using System.Resources;

using Composite.Data;

namespace CompositeC1Contrib.Localization
{
    public class C1ResourceSet : ResourceSet
    {
        public C1ResourceSet(string resourceSet, CultureInfo ci)
            : base(new C1ResourceReader(resourceSet, ci))
        {
            StoreEventHandler invalidateCache = (sender, e) => 
            {
                Table.Clear();
            };

            DataEvents<IResourceKey>.OnStoreChanged += invalidateCache;
            DataEvents<IResourceValue>.OnStoreChanged += invalidateCache;
        }

        public override Type GetDefaultReader()
        {
            return typeof(C1ResourceReader);
        }

        public override Type GetDefaultWriter()
        {
            return typeof(C1ResourceWriter);
        }
    }
}

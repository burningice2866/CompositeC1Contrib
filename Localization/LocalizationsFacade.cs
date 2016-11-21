using System.Collections.Generic;
using System.Linq;

using Composite.Data;

namespace CompositeC1Contrib.Localization
{
    public static class LocalizationsFacade
    {
        public static void RenameNamespace(string ns, string newNs, string resourceSet = null)
        {
            using (var data = new DataConnection())
            {
                var resourceKeys = data.Get<IResourceKey>().Where(k => k.ResourceSet == resourceSet && k.Key.StartsWith(ns + ".")).ToList();
                foreach (var resourceKey in resourceKeys)
                {
                    resourceKey.Key = resourceKey.Key.Remove(0, ns.Length).Insert(0, newNs);
                }

                data.Update<IResourceKey>(resourceKeys);
            }
        }

        public static IEnumerable<IResourceKey> GetResourceKeys(string ns, string resourceSet = null)
        {
            if (ns.Length > 0)
            {
                ns = ns + ".";
            }

            using (var data = new DataConnection())
            {
                return from k in data.Get<IResourceKey>()
                       where k.ResourceSet == resourceSet && k.Key.StartsWith(ns)
                       orderby k.Key
                       select k;
            }
        }
    }
}

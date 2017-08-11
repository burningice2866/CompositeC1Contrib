using System;
using System.Collections.Generic;
using System.Linq;

using Composite;
using Composite.Data;
using Composite.Data.Transactions;

namespace CompositeC1Contrib.Localization
{
    public static class LocalizationsFacade
    {
        public static void DeleteNamespace(string ns, string resourceSet)
        {
            if (resourceSet == String.Empty)
            {
                resourceSet = null;
            }

            using (var transaction = TransactionsFacade.CreateNewScope())
            {
                using (var data = new DataConnection())
                {
                    var keys = data.Get<IResourceKey>().Where(r => r.ResourceSet == resourceSet && r.Key.StartsWith(ns)).ToList();
                    foreach (var key in keys)
                    {
                        var values = data.Get<IResourceValue>().Where(v => v.KeyId == key.Id);

                        data.Delete<IResourceValue>(values);
                    }

                    data.Delete<IResourceKey>(keys);
                }

                transaction.Complete();
            }
        }

        public static void RenameNamespace(string ns, string newNs, string resourceSet)
        {
            Verify.ArgumentNotNull(ns, "ns");
            Verify.ArgumentNotNull(newNs, "newNs");

            if (resourceSet == String.Empty)
            {
                resourceSet = null;
            }

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

        public static void CopyNamespace(string ns, string newNs, string resourceSet)
        {
            Verify.ArgumentNotNull(ns, "ns");
            Verify.ArgumentNotNull(newNs, "newNs");

            if (resourceSet == String.Empty)
            {
                resourceSet = null;
            }

            using (var transaction = TransactionsFacade.CreateNewScope())
            {
                using (var data = new DataConnection())
                {
                    var keysToAdd = new List<IResourceKey>();
                    var valuesToAdd = new List<IResourceValue>();

                    var resources = (from key in data.Get<IResourceKey>()
                                     join value in data.Get<IResourceValue>() on key.Id equals value.KeyId into values
                                     where key.ResourceSet == resourceSet && key.Key.StartsWith(ns + ".")
                                     select new
                                     {
                                         Key = key,
                                         Values = values
                                     }).ToList();

                    foreach (var resource in resources)
                    {
                        var keyToAdd = data.CreateNew<IResourceKey>();

                        keyToAdd.Id = Guid.NewGuid();
                        keyToAdd.Key = resource.Key.Key.Remove(0, ns.Length).Insert(0, newNs);
                        keyToAdd.ResourceSet = resource.Key.ResourceSet;
                        keyToAdd.Type = resource.Key.Type;

                        keysToAdd.Add(keyToAdd);

                        foreach (var value in resource.Values)
                        {
                            var valueToAdd = data.CreateNew<IResourceValue>();

                            valueToAdd.Id = Guid.NewGuid();
                            valueToAdd.KeyId = keyToAdd.Id;
                            valueToAdd.Culture = value.Culture;
                            valueToAdd.Value = value.Value;

                            valuesToAdd.Add(valueToAdd);
                        }
                    }

                    data.Add<IResourceKey>(keysToAdd);
                    data.Add<IResourceValue>(valuesToAdd);
                }

                transaction.Complete();
            }
        }

        public static IEnumerable<IResourceKey> GetResourceKeys(string ns, string resourceSet)
        {
            Verify.ArgumentNotNull(ns, "ns");

            if (resourceSet == String.Empty)
            {
                resourceSet = null;
            }

            using (var data = new DataConnection())
            {
                IQueryable<IResourceKey> query = from k in data.Get<IResourceKey>()
                                                 where Equals(k.ResourceSet, resourceSet)
                                                 orderby k.Key
                                                 select k;

                if (ns.Length > 0)
                {
                    ns = ns + ".";

                    query = from k in query where k.Key.StartsWith(ns) select k;
                }

                return query;
            }
        }
    }
}

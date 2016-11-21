using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Composite.Data;
using Composite.Data.Transactions;

namespace CompositeC1Contrib.Localization
{
    public class C1ResourceDataManager
    {
        public static C1ResourceDataManager Instance = new C1ResourceDataManager();

        public string GetResourceObject(string key, string resourceSet, CultureInfo culture)
        {
            var cultureName = GetCultureName(culture);

            if (resourceSet == String.Empty)
            {
                resourceSet = null;
            }

            using (var data = new DataConnection())
            {
                var resourceKey = data.Get<IResourceKey>().SingleOrDefault(r => r.ResourceSet == resourceSet && r.Key == key);
                if (resourceKey == null)
                {
                    return null;
                }

                var resource = data.Get<IResourceValue>().SingleOrDefault(r => r.KeyId == resourceKey.Id && r.Culture == cultureName);
                if (resource == null)
                {
                    return null;
                }

                return resource.Value;
            }
        }

        public IDictionary GetResourceSet(string resourceSet, CultureInfo culture)
        {
            if (resourceSet == String.Empty)
            {
                resourceSet = null;
            }

            var dictionary = new Dictionary<string, string>();

            using (var data = new DataConnection())
            {
                var query = from rv in data.Get<IResourceValue>()
                            join r in data.Get<IResourceKey>() on rv.KeyId equals r.Id
                            where r.ResourceSet == resourceSet
                            where rv.Culture == culture.Name
                            select new { r.Key, rv.Value };

                foreach (var itm in query)
                {
                    if (!dictionary.ContainsKey(itm.Key))
                    {
                        dictionary.Add(itm.Key, itm.Value);
                    }
                }
            }

            return dictionary;
        }

        public void AddResource(string key, object value, CultureInfo culture)
        {
            AddResource(key, value, null, culture);
        }

        public void AddResource(string key, object value, string resourceSet, CultureInfo culture)
        {
            if (resourceSet == String.Empty)
            {
                resourceSet = null;
            }

            using (var transaction = TransactionsFacade.CreateNewScope())
            {
                using (var data = new DataConnection())
                {
                    var resourceKey = data.Get<IResourceKey>().SingleOrDefault(k => k.ResourceSet == resourceSet && k.Key == key);
                    if (resourceKey == null)
                    {
                        resourceKey = data.CreateNew<IResourceKey>();

                        resourceKey.Id = Guid.NewGuid();
                        resourceKey.Key = key;
                        resourceKey.ResourceSet = resourceSet;

                        data.Add(resourceKey);
                    }

                    var resourceValue = data.CreateNew<IResourceValue>();

                    resourceValue.Id = Guid.NewGuid();
                    resourceValue.KeyId = resourceKey.Id;
                    resourceValue.Culture = GetCultureName(culture);
                    resourceValue.Value = value.ToString();

                    data.Add(resourceValue);
                }

                transaction.Complete();
            }
        }

        public void UpdateResource(string key, object value, CultureInfo culture)
        {
            UpdateResource(key, value, null, culture);
        }

        public void UpdateResource(string key, object value, string resourceSet, CultureInfo culture)
        {
            var cultureName = GetCultureName(culture);

            if (resourceSet == String.Empty)
            {
                resourceSet = null;
            }

            using (var data = new DataConnection())
            {
                var resourceKey = data.Get<IResourceKey>().Single(r => r.ResourceSet == resourceSet && r.Key == key);
                var resourceValue = data.Get<IResourceValue>().Single(r => r.KeyId == resourceKey.Id && r.Culture == cultureName);

                resourceValue.Value = value.ToString();

                data.Update(resourceValue);
            }
        }

        public void GenerateResources(IDictionary resourceList, string resourceSet, CultureInfo culture)
        {
            using (var transaction = TransactionsFacade.CreateNewScope())
            {
                foreach (DictionaryEntry entry in resourceList)
                {
                    if (entry.Value != null)
                    {
                        UpdateOrAddResource(entry.Key.ToString(), entry.Value, resourceSet, culture);
                    }
                }

                transaction.Complete();
            }
        }

        public void UpdateOrAddResource(string key, object value, CultureInfo culture)
        {
            UpdateOrAddResource(key, value, null, culture);
        }

        public void UpdateOrAddResource(string key, object value, string resourceSet, CultureInfo culture)
        {
            var exists = GetResourceObject(key, resourceSet, culture) != null;
            if (exists)
            {
                UpdateResource(key, value, resourceSet, culture);
            }
            else
            {
                AddResource(key, value, resourceSet, culture);
            }
        }

        public void DeleteResource(string key, CultureInfo culture)
        {
            DeleteResource(key, null, culture);
        }

        public void DeleteResource(string key, string resourceSet, CultureInfo culture)
        {
            var cultureName = GetCultureName(culture);

            if (resourceSet == String.Empty)
            {
                resourceSet = null;
            }

            using (var transaction = TransactionsFacade.CreateNewScope())
            {
                using (var data = new DataConnection())
                {
                    var resourceKey = data.Get<IResourceKey>().SingleOrDefault(r => r.ResourceSet == resourceSet && r.Key == key);
                    if (resourceKey == null)
                    {
                        return;
                    }

                    var resourceValue = data.Get<IResourceValue>().SingleOrDefault(r => r.KeyId == resourceKey.Id && r.Culture == cultureName);
                    if (resourceValue != null)
                    {
                        data.Delete(resourceValue);
                    }

                    if (!data.Get<IResourceValue>().Any(v => v.KeyId == resourceKey.Id))
                    {
                        data.Delete(resourceKey);
                    }
                }

                transaction.Complete();
            }
        }

        private static string GetCultureName(CultureInfo culture)
        {
            var cultureName = String.Empty;
            if (culture != null && !culture.IsNeutralCulture)
            {
                cultureName = culture.Name;
            }

            return cultureName;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Composite;
using Composite.Data;
using Composite.Data.Transactions;

namespace CompositeC1Contrib.Localization
{
    public class C1ResourceDataManager
    {
        private readonly string _resourceSet;
        private readonly CultureInfo _culture;
        private readonly string _cultureName = String.Empty;

        public C1ResourceDataManager(string resourceSet, CultureInfo culture)
        {
            Verify.ArgumentNotNull(culture, nameof(culture));

            _resourceSet = resourceSet;
            _culture = culture;

            if (_resourceSet == String.Empty)
            {
                _resourceSet = null;
            }

            if (!culture.IsNeutralCulture)
            {
                _cultureName = culture.Name;
            }
        }

        public string GetResourceObject(string key)
        {
            Verify.ArgumentNotNullOrEmpty(key, nameof(key));

            using (var data = new DataConnection())
            {
                var resourceKey = data.Get<IResourceKey>().SingleOrDefault(r => Equals(r.ResourceSet, _resourceSet) && r.Key == key);
                if (resourceKey == null)
                {
                    return null;
                }

                var resource = data.Get<IResourceValue>().SingleOrDefault(r => r.KeyId == resourceKey.Id && r.Culture == _cultureName);
                if (resource == null)
                {
                    return null;
                }

                return resource.Value;
            }
        }

        public IDictionary GetResourceSet()
        {
            var dictionary = new Dictionary<string, string>();

            using (var data = new DataConnection())
            {
                var query = from rv in data.Get<IResourceValue>()
                            join r in data.Get<IResourceKey>() on rv.KeyId equals r.Id
                            where Equals(r.ResourceSet, _resourceSet) && rv.Culture == _culture.Name
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

        public void AddResource(string key, object value)
        {
            Verify.ArgumentNotNullOrEmpty(key, nameof(key));
            Verify.ArgumentNotNull(value, nameof(value));

            using (var transaction = TransactionsFacade.CreateNewScope())
            {
                using (var data = new DataConnection())
                {
                    var resourceKey = data.Get<IResourceKey>().SingleOrDefault(k => Equals(k.ResourceSet, _resourceSet) && k.Key == key);
                    if (resourceKey == null)
                    {
                        resourceKey = data.CreateNew<IResourceKey>();

                        resourceKey.Id = Guid.NewGuid();
                        resourceKey.Key = key;
                        resourceKey.ResourceSet = _resourceSet;

                        data.Add(resourceKey);
                    }

                    var resourceValue = data.CreateNew<IResourceValue>();

                    resourceValue.Id = Guid.NewGuid();
                    resourceValue.KeyId = resourceKey.Id;
                    resourceValue.Culture = _cultureName;
                    resourceValue.Value = value.ToString();

                    data.Add(resourceValue);
                }

                transaction.Complete();
            }
        }

        public void UpdateResource(string key, object value)
        {
            Verify.ArgumentNotNullOrEmpty(key, nameof(key));
            Verify.ArgumentNotNull(value, nameof(value));

            using (var data = new DataConnection())
            {
                var resourceKey = data.Get<IResourceKey>().Single(r => Equals(r.ResourceSet, _resourceSet) && r.Key == key);
                var resourceValue = data.Get<IResourceValue>().Single(r => r.KeyId == resourceKey.Id && r.Culture == _cultureName);

                resourceValue.Value = value.ToString();

                data.Update(resourceValue);
            }
        }

        public void GenerateResources(IDictionary resourceList)
        {
            Verify.ArgumentNotNull(resourceList, nameof(resourceList));

            using (var transaction = TransactionsFacade.CreateNewScope())
            {
                foreach (DictionaryEntry entry in resourceList)
                {
                    var key = entry.Key.ToString();

                    if (entry.Value == null)
                    {
                        DeleteResource(key);

                    }
                    else
                    {
                        UpdateOrAddResource(key, entry.Value);
                    }
                }

                transaction.Complete();
            }
        }

        public void UpdateOrAddResource(string key, object value)
        {
            Verify.ArgumentNotNullOrEmpty(key, nameof(key));
            Verify.ArgumentNotNull(value, nameof(value));

            var exists = GetResourceObject(key) != null;
            if (exists)
            {
                UpdateResource(key, value);
            }
            else
            {
                AddResource(key, value);
            }
        }

        public void DeleteResource(string key)
        {
            Verify.ArgumentNotNullOrEmpty(key, nameof(key));

            using (var transaction = TransactionsFacade.CreateNewScope())
            {
                using (var data = new DataConnection())
                {
                    var resourceKey = data.Get<IResourceKey>().SingleOrDefault(r => Equals(r.ResourceSet, _resourceSet) && r.Key == key);
                    if (resourceKey != null)
                    {
                        var resourceValue = data.Get<IResourceValue>().SingleOrDefault(r => r.KeyId == resourceKey.Id && r.Culture == _cultureName);
                        if (resourceValue != null)
                        {
                            data.Delete(resourceValue);
                        }

                        if (!data.Get<IResourceValue>().Any(v => v.KeyId == resourceKey.Id))
                        {
                            data.Delete(resourceKey);
                        }
                    }
                }

                transaction.Complete();
            }
        }
    }
}

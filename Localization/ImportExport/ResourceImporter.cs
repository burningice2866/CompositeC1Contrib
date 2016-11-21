using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Composite.Data;
using Composite.Data.Transactions;

namespace CompositeC1Contrib.Localization.ImportExport
{
    public class ResourceImporter
    {
        private readonly ImportExportModel _model;

        private IList<IResourceKey> _keysToAdd = new List<IResourceKey>();

        private IList<IResourceValue> _valuesToAdd = new List<IResourceValue>();
        private IList<IResourceValue> _valuesToUpdate = new List<IResourceValue>();

        public ResourceImporter(ImportExportModel model)
        {
            _model = model;
        }

        public ImportResult Import()
        {
            _keysToAdd.Clear();
            _valuesToAdd.Clear();
            _valuesToUpdate.Clear();

            var result = new ImportResult
            {
                Languages = _model.Languages.Keys.ToArray(),
                ResourceSets = _model.Languages.Values.SelectMany(l => l.ResourceSets.Select(r => r.Name)).Distinct().ToArray()
            };

            using (var transaction = TransactionsFacade.CreateNewScope())
            {
                using (var data = new DataConnection())
                {
                    var existingKeys = data.Get<IResourceKey>().ToDictionary(k => k.Key);
                    var existingValues = data.Get<IResourceValue>().ToDictionary(v => Tuple.Create(v.KeyId, v.Culture));

                    foreach (var language in _model.Languages.Values)
                    {
                        foreach (var resourceSet in language.ResourceSets)
                        {
                            EvaluateAddOrUpdates(language.Culture, resourceSet, data, result, existingKeys, existingValues);
                        }
                    }

                    result.KeysAdded = _keysToAdd.Count;
                    result.ValuesAdded = _valuesToAdd.Count;
                    result.ValuesUpdated = _valuesToUpdate.Count;

                    data.Add<IResourceKey>(_keysToAdd);

                    data.Add<IResourceValue>(_valuesToAdd);
                    data.Update<IResourceValue>(_valuesToUpdate);
                }

                transaction.Complete();
            }

            return result;
        }

        private void EvaluateAddOrUpdates(CultureInfo ci,
            ImportExportModelResourceSet resourceSet,
            DataConnection data,
            ImportResult result,
            IDictionary<string, IResourceKey> existingKeys,
            IDictionary<Tuple<Guid, string>, IResourceValue> existingValues)
        {
            foreach (var resource in resourceSet.Resources.Values)
            {
                IResourceKey existingKey;
                if (existingKeys.TryGetValue(resource.Key, out existingKey))
                {
                    var valueKey = Tuple.Create(existingKey.Id, ci.Name);

                    IResourceValue existingValue;
                    if (existingValues.TryGetValue(valueKey, out existingValue))
                    {
                        if (existingValue.Value == resource.Value)
                        {
                            result.ValuesWereTheSame++;
                        }
                        else
                        {
                            existingValue.Value = resource.Value;

                            if (existingValue.DataSourceId.ExistsInStore)
                            {
                                _valuesToUpdate.Add(existingValue);
                            }
                        }
                    }
                    else
                    {
                        var value = data.CreateNew<IResourceValue>();

                        value.Id = Guid.NewGuid();
                        value.KeyId = existingKey.Id;
                        value.Culture = ci.Name;
                        value.Value = resource.Value;

                        existingValues.Add(valueKey, value);
                        _valuesToAdd.Add(value);
                    }
                }
                else
                {
                    var key = data.CreateNew<IResourceKey>();

                    key.Id = Guid.NewGuid();
                    key.Key = resource.Key;
                    key.ResourceSet = resourceSet.Name;
                    key.Type = resource.Type.ToString();

                    existingKeys.Add(resource.Key, key);
                    _keysToAdd.Add(key);

                    var value = data.CreateNew<IResourceValue>();

                    value.Id = Guid.NewGuid();
                    value.KeyId = key.Id;
                    value.Culture = ci.Name;
                    value.Value = resource.Value;

                    existingValues.Add(Tuple.Create(value.Id, ci.Name), value);
                    _valuesToAdd.Add(value);
                }
            }
        }
    }
}

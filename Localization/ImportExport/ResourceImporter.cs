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

        private IList<IResourceKey> keysToAdd = new List<IResourceKey>();

        private IList<IResourceValue> valuesToAdd = new List<IResourceValue>();
        private IList<IResourceValue> valuesToUpdate = new List<IResourceValue>();

        public ResourceImporter(ImportExportModel model)
        {
            _model = model;
        }

        public ImportResult Import()
        {
            keysToAdd.Clear();
            valuesToAdd.Clear();
            valuesToUpdate.Clear();

            var result = new ImportResult
            {
                Languages = _model.Languages.Keys.ToArray(),
                ResourceSets = _model.Languages.Values.SelectMany(l => l.ResourceSets.Select(r => r.Name)).Distinct().ToArray()
            };

            using (var transaction = TransactionsFacade.CreateNewScope())
            {
                using (var data = new DataConnection())
                {
                    foreach (var language in _model.Languages.Values)
                    {
                        foreach (var resourceSet in language.ResourceSets)
                        {
                            EvaluateAddOrUpdates(language.Culture, resourceSet, data, result);
                        }
                    }

                    result.KeysAdded = keysToAdd.Count;
                    result.ValuesAdded = valuesToAdd.Count;
                    result.ValuesUpdated = valuesToUpdate.Count;

                    data.Add<IResourceKey>(keysToAdd);

                    data.Add<IResourceValue>(valuesToAdd);
                    data.Update<IResourceValue>(valuesToUpdate);
                }

                transaction.Complete();
            }

            return result;
        }

        private void EvaluateAddOrUpdates(CultureInfo ci, ImportExportModelResourceSet resourceSet, DataConnection data, ImportResult result)
        {
            var existingKeys = data.Get<IResourceKey>().Where(k => k.ResourceSet == resourceSet.Name).ToDictionary(k => k.Key);
            var existingValues = data.Get<IResourceValue>().Where(v => v.Culture == ci.Name).ToDictionary(v => v.KeyId);

            foreach (var resource in resourceSet.Resources.Values)
            {
                IResourceKey existingKey;
                if (existingKeys.TryGetValue(resource.Key, out existingKey))
                {
                    IResourceValue existingValue;
                    if (existingValues.TryGetValue(existingKey.Id, out existingValue))
                    {
                        if (existingValue.Value == resource.Value)
                        {
                            result.ValuesWereTheSame++;
                        }
                        else
                        {
                            existingValue.Value = resource.Value;

                            valuesToUpdate.Add(existingValue);
                        }
                    }
                    else
                    {
                        var value = data.CreateNew<IResourceValue>();

                        value.Id = Guid.NewGuid();
                        value.KeyId = existingKey.Id;
                        value.Culture = ci.Name;
                        value.Value = resource.Value;

                        valuesToAdd.Add(value);
                    }
                }
                else
                {
                    var key = data.CreateNew<IResourceKey>();

                    key.Id = Guid.NewGuid();
                    key.Key = resource.Key;
                    key.ResourceSet = resourceSet.Name;
                    key.Type = resource.Type.ToString();

                    keysToAdd.Add(key);

                    var value = data.CreateNew<IResourceValue>();

                    value.Id = Guid.NewGuid();
                    value.KeyId = key.Id;
                    value.Culture = ci.Name;
                    value.Value = resource.Value;

                    valuesToAdd.Add(value);
                }
            }
        }
    }
}

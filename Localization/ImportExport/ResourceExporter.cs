using System;
using System.Globalization;
using System.Linq;

using Composite.Data;

namespace CompositeC1Contrib.Localization.ImportExport
{
    public class ResourceExporter
    {
        private readonly string[] _languages;
        private readonly string[] _resourceSets;
        private readonly string _ns;

        public ResourceExporter(string[] languages, string[] resourceSets, string ns)
        {
            _languages = languages;
            _resourceSets = resourceSets;
            _ns = ns;
        }

        public ImportExportModel Export()
        {
            var model = new ImportExportModel();

            using (var data = new DataConnection())
            {
                foreach (var language in _languages)
                {
                    var ci = new CultureInfo(language);

                    var modelLanguage = new ImportExportModelLanguage(ci);

                    foreach (var resourceSet in _resourceSets)
                    {
                        var modelResourceSet = new ImportExportModelResourceSet
                        {
                            Name = String.IsNullOrEmpty(resourceSet) ? null : resourceSet
                        };

                        var keys = from key in data.Get<IResourceKey>()
                                   where key.ResourceSet == (resourceSet == String.Empty ? null : resourceSet)
                                   select key;

                        var query = from key in keys
                                    join value in data.Get<IResourceValue>() on key.Id equals value.KeyId
                                    where value.Culture == ci.Name
                                    select new
                                    {
                                        Key = key.Key,
                                        Type = key.Type,
                                        Value = value.Value
                                    };

                        if (!String.IsNullOrEmpty(_ns))
                        {
                            query = from o in query
                                    where o.Key.StartsWith(_ns)
                                    select o;
                        }

                        foreach (var o in query)
                        {
                            var resourceModel = new ImportExportModelResource
                            {
                                Key = o.Key,
                                Value = o.Value
                            };

                            if (!String.IsNullOrEmpty(o.Type))
                            {
                                resourceModel.Type = (ResourceType)Enum.Parse(typeof(ResourceType), o.Type);
                            }

                            modelResourceSet.Resources.Add(resourceModel.Key, resourceModel);
                        }

                        modelLanguage.ResourceSets.Add(modelResourceSet);
                    }

                    model.Languages.Add(ci, modelLanguage);
                }
            }

            return model;
        }
    }
}

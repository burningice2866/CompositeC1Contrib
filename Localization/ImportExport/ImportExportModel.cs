using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;

namespace CompositeC1Contrib.Localization.ImportExport
{
    public class ImportExportModel
    {
        public IDictionary<CultureInfo, ImportExportModelLanguage> Languages { get; private set; }

        public ImportExportModel()
        {
            Languages = new Dictionary<CultureInfo, ImportExportModelLanguage>();
        }

        public XElement ToXml()
        {
            var doc = new XElement("resources");

            foreach (var lang in Languages.Values)
            {
                var langElement = new XElement("language", new XAttribute("name", lang.Culture.Name));

                foreach (var resourceSet in lang.ResourceSets)
                {
                    var resourceSetElement = new XElement("resourceSet");

                    if (!String.IsNullOrEmpty(resourceSet.Name))
                    {
                        resourceSetElement.Add(new XAttribute("name", resourceSet.Name));
                    }

                    foreach (var res in resourceSet.Resources.Values)
                    {
                        var resourceElement = new XElement("resource",
                                new XAttribute("key", res.Key),
                                new XAttribute("type", res.Type),
                                new XAttribute("value", res.Value ?? String.Empty));

                        resourceSetElement.Add(resourceElement);
                    }

                    langElement.Add(resourceSetElement);
                }

                doc.Add(langElement);
            }

            return doc;
        }

        public static ImportExportModel FromXml(string xml)
        {
            var doc = XElement.Parse(xml);
            var model = new ImportExportModel();

            foreach (var language in doc.Elements("language"))
            {
                var ci = new CultureInfo((string)language.Attribute("name"));

                var languageModel = new ImportExportModelLanguage(ci);

                foreach (var resourceSet in language.Elements("resourceSet"))
                {
                    var resourceSetModel = new ImportExportModelResourceSet
                    {
                        Name = resourceSet.Attribute("name") == null ? null : (string)resourceSet.Attribute("name")
                    };

                    foreach (var resource in resourceSet.Elements("resource"))
                    {
                        var resourceModel = new ImportExportModelResource
                        {
                            Key = (string)resource.Attribute("key"),
                            Type = (ResourceType)Enum.Parse(typeof(ResourceType), (string)resource.Attribute("type")),
                            Value = (string)resource.Attribute("value")
                        };

                        resourceSetModel.Resources.Add(resourceModel.Key, resourceModel);
                    }

                    languageModel.ResourceSets.Add(resourceSetModel);
                }

                model.Languages.Add(languageModel.Culture, languageModel);
            }

            return model;
        }
    }
}

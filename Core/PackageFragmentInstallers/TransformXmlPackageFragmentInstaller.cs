using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using Composite.Core.IO;
using Composite.Core.PackageSystem;
using Composite.Core.PackageSystem.PackageFragmentInstallers;
using Composite.Core.Xml;

namespace CompositeC1Contrib.PackageFragmentInstallers
{
    public class TransformXmlPackageFragmentInstaller : BasePackageFragmentInstaller
    {
        private sealed class XmlToAdd
        {
            public string Source { get; set; }
            public string Target { get; set; }
        }

        private const string _loggerSenderText = "MergeXmlPackageFragmentInstaller";

        private IList<XmlToAdd> _xmlToAdd;

        public override IEnumerable<XElement> Install()
        {
            if (_xmlToAdd == null) throw new InvalidOperationException("MergeXmlPackageFragmentInstaller has not been validated");

            foreach (var file in _xmlToAdd)
            {
                string targetXml = PathUtil.Resolve(file.Target);

                using (var stream = this.InstallerContext.ZipFileSystem.GetFileStream(file.Source))
                {
                    var source = XElement.Load(stream);
                    var target = XDocument.Load(targetXml);

                    target.Root.MergeWith(source);

                    target.SaveToFile(targetXml);
                }
            }

            return new[] { this.Configuration.FirstOrDefault() };
        }

        public override IEnumerable<PackageFragmentValidationResult> Validate()
        {
            var validationResult = new List<PackageFragmentValidationResult>();

            if (Configuration.Where(f => f.Name == "XmlFiles").Count() > 1)
            {
                validationResult.Add(new PackageFragmentValidationResult(PackageFragmentValidationResultType.Fatal, "OnlyOneFilesElement"));

                return validationResult;
            }

            var filesElement = this.Configuration.Where(f => f.Name == "XmlFiles");

            _xmlToAdd = new List<XmlToAdd>();

            foreach (var fileElement in filesElement.Elements("XmlFile"))
            {
                var sourceAttribute = fileElement.Attribute("source");
                var targetAttribute = fileElement.Attribute("target");

                if (sourceAttribute == null || targetAttribute == null)
                {
                    validationResult.Add(new PackageFragmentValidationResult(PackageFragmentValidationResultType.Fatal, "MissingAttribute", fileElement));

                    continue;
                }

                XmlToAdd xmlFile = new XmlToAdd
                    {
                        Source = sourceAttribute.Value,
                        Target = targetAttribute.Value
                    };


                if (!C1File.Exists(PathUtil.Resolve(xmlFile.Target)))
                {
                    validationResult.Add(new PackageFragmentValidationResult(PackageFragmentValidationResultType.Fatal, "FileNotFound", fileElement));

                    continue;
                }

                _xmlToAdd.Add(xmlFile);
            }

            if (validationResult.Count > 0)
            {
                _xmlToAdd = null;
            }

            return validationResult;
        }
    }
}

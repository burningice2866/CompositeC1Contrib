using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using Composite.Core.IO;
using Composite.Core.PackageSystem;
using Composite.Core.PackageSystem.PackageFragmentInstallers;

namespace CompositeC1Contrib.PackageFragmentInstallers
{
    public class FileDeleteFragmentInstaller : BasePackageFragmentInstaller
    {
        private IList<string> _files;
        private IList<string> _directories;

        public override IEnumerable<XElement> Install()
        {
            if (_files == null || _directories == null)
            {
                throw new InvalidOperationException(GetType().Name + " has not been validated");
            }

            if (_files != null)
            {
                foreach (var file in _files)
                {
                    C1File.Delete(file);
                }
            }

            if (_directories != null)
            {
                foreach (var directory in _directories)
                {
                    C1Directory.Delete(directory, true);
                }
            }

            return Configuration;
        }

        public override IEnumerable<PackageFragmentValidationResult> Validate()
        {
            var validationResult = new List<PackageFragmentValidationResult>();

            HandleFiles(validationResult);
            ValidateDirectories(validationResult);

            return validationResult;
        }

        private void HandleFiles(ICollection<PackageFragmentValidationResult> validationResult)
        {
            var filesElements = Configuration.Where(el => el.Name == "Files").ToList();
            if (filesElements.Count > 1)
            {
                validationResult.Add(
                    new PackageFragmentValidationResult(PackageFragmentValidationResultType.Fatal,
                    "You can only specify one Files element", ConfigurationParent));
            }

            var filesElement = filesElements.SingleOrDefault();
            if (filesElement == null)
            {
                return;
            }

            _files = new List<string>();

            foreach (var el in filesElement.Elements("File"))
            {
                var pathAttr = el.Attribute("path");
                if (pathAttr == null)
                {
                    validationResult.Add(
                        new PackageFragmentValidationResult(PackageFragmentValidationResultType.Fatal,
                            "Missing path attribute", el));

                    continue;
                }

                var path = PathUtil.Resolve(pathAttr.Value);
                if (C1File.Exists(path))
                {
                    _files.Add(path);
                }
            }
        }

        private void ValidateDirectories(ICollection<PackageFragmentValidationResult> validationResult)
        {
            var directoriesElements = Configuration.Where(el => el.Name == "Directories").ToList();
            if (directoriesElements.Count > 1)
            {
                validationResult.Add(
                    new PackageFragmentValidationResult(PackageFragmentValidationResultType.Fatal,
                    "You can only specify one Directories element", ConfigurationParent));
            }

            var directoriesElement = directoriesElements.SingleOrDefault();
            if (directoriesElement == null)
            {
                return;
            }

            _directories = new List<string>();

            foreach (var el in directoriesElement.Elements("Directory"))
            {
                var pathAttr = el.Attribute("path");
                if (pathAttr == null)
                {
                    validationResult.Add(
                        new PackageFragmentValidationResult(PackageFragmentValidationResultType.Fatal,
                            "Missing path attribute", el));

                    continue;
                }

                var path = PathUtil.Resolve(pathAttr.Value);
                if (C1Directory.Exists(path))
                {
                    _directories.Add(path);
                }
            }
        }
    }
}

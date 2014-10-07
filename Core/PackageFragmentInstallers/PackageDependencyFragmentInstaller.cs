using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using Composite.Core.PackageSystem;
using Composite.Core.PackageSystem.PackageFragmentInstallers;

namespace CompositeC1Contrib.PackageFragmentInstallers
{
    public class PackageDependencyFragmentInstaller : BasePackageFragmentInstaller
    {
        private bool _validated;

        public override IEnumerable<XElement> Install()
        {
            if (!_validated)
            {
                throw new InvalidOperationException("PackageDependencyFragmentInstaller has not been validated");
            }

            return Configuration;
        }

        public override IEnumerable<PackageFragmentValidationResult> Validate()
        {
            var validationSummary = new List<PackageFragmentValidationResult>();
            var installedPackages = PackageManager.GetInstalledPackages().ToList();

            var packages = Configuration.Where(e => e.Name == "Package");
            foreach (var package in packages)
            {
                var id = Guid.Parse(package.Attribute("id").Value);

                var installedPackage = installedPackages.SingleOrDefault(p => p.Id == id);
                if (installedPackage == null)
                {
                    var validationMessage = String.Format("Required package '{0}' not installed", id);

                    validationSummary.Add(new PackageFragmentValidationResult(PackageFragmentValidationResultType.Fatal, validationMessage, package));

                    continue;
                }

                var packageVersion = Version.Parse(installedPackage.Version);

                var minVersionAttribute = package.Attribute("minimumVersion");
                if (minVersionAttribute != null)
                {
                    var minVersion = Version.Parse(minVersionAttribute.Value);
                    if (packageVersion < minVersion)
                    {
                        var validationMessage =
                            String.Format(
                                "Package '{0}' doesn't meet required minimum version, version is '{1}' but required version is '{2}'",
                                installedPackage.Id, installedPackage.Version, minVersion);

                        validationSummary.Add(new PackageFragmentValidationResult(PackageFragmentValidationResultType.Fatal, validationMessage, package));

                        continue;
                    }
                }

                var maxVersionAttribute = package.Attribute("maximumVersion");
                if (maxVersionAttribute != null)
                {
                    var maxVersion = Version.Parse(maxVersionAttribute.Value);
                    if (packageVersion > maxVersion)
                    {
                        var validationMessage =
                            String.Format(
                                "Package '{0}' doesn't meet required maximum version, version is '{1}' but required version is '{2}'",
                                installedPackage.Id, installedPackage.Version, maxVersion);

                        validationSummary.Add(new PackageFragmentValidationResult(PackageFragmentValidationResultType.Fatal, validationMessage, package));

                        continue;
                    }
                }
            }

            _validated = true;

            return validationSummary;
        }
    }
}

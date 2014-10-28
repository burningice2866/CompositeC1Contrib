using System;
using System.Collections.Generic;
using System.Xml.Linq;

using Composite.Core.PackageSystem;
using Composite.Core.PackageSystem.PackageFragmentInstallers;

namespace CompositeC1Contrib.PackageFragmentInstallers
{
    public class SqlFileFragmentInstaller : BasePackageFragmentInstaller
    {
        private IList<string> _files;

        public override IEnumerable<XElement> Install()
        {
            if (_files == null)
            {
                throw new InvalidOperationException(GetType().Name + " has not been validated");
            }

            SqlFileFragmentHelper.ExecuteFiles(_files, ConfigurationParent, InstallerContext.ZipFileSystem);

            return Configuration;
        }

        public override IEnumerable<PackageFragmentValidationResult> Validate()
        {
            IList<PackageFragmentValidationResult> validationResult;

            _files = SqlFileFragmentHelper.GetFiles("installPath", ConfigurationParent, InstallerContext.ZipFileSystem, out validationResult);

            return validationResult;
        }
    }
}

using System;
using System.Collections.Generic;

using Composite.Core.PackageSystem;
using Composite.Core.PackageSystem.PackageFragmentInstallers;

namespace CompositeC1Contrib.PackageFragmentInstallers
{
    public class SqlFileFragmentUninstaller : BasePackageFragmentUninstaller
    {
        private IList<string> _files;

        public override void Uninstall()
        {
            if (_files == null)
            {
                throw new InvalidOperationException(GetType().Name + " has not been validated");
            }

            SqlFileFragmentHelper.ExecuteFiles(_files, ConfigurationParent, UninstallerContext.ZipFileSystem);
        }

        public override IEnumerable<PackageFragmentValidationResult> Validate()
        {
            IList<PackageFragmentValidationResult> validationResult;

            _files = SqlFileFragmentHelper.GetFiles("uninstallPath", ConfigurationParent, UninstallerContext.ZipFileSystem, out validationResult);

            return validationResult;
        }
    }
}

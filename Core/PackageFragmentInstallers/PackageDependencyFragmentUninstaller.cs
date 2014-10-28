using System.Collections.Generic;
using System.Linq;

using Composite.Core.PackageSystem;
using Composite.Core.PackageSystem.PackageFragmentInstallers;

namespace CompositeC1Contrib.PackageFragmentInstallers
{
    public class PackageDependencyFragmentUninstaller : BasePackageFragmentUninstaller
    {
        public override void Uninstall() { }

        public override IEnumerable<PackageFragmentValidationResult> Validate()
        {
            return Enumerable.Empty<PackageFragmentValidationResult>();
        }
    }
}

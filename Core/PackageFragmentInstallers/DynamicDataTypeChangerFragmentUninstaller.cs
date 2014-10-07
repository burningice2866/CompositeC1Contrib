using System.Collections.Generic;

using Composite.Core.PackageSystem;
using Composite.Core.PackageSystem.PackageFragmentInstallers;

namespace CompositeC1Contrib.PackageFragmentInstallers
{
    public class DynamicDataTypeChangerFragmentUninstaller : BasePackageFragmentUninstaller
    {
        public override void Uninstall() { }

        public override IEnumerable<PackageFragmentValidationResult> Validate()
        {
            return new[] { new PackageFragmentValidationResult(PackageFragmentValidationResultType.Fatal, "Doesn't support uninstall") };
        }
    }
}

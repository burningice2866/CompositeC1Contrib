using System;
using System.Web.Compilation;

namespace CompositeC1Contrib.Localization.Web
{
    public class C1ResourceProviderFactory : ResourceProviderFactory
    {
        public override IResourceProvider CreateGlobalResourceProvider(string classKey)
        {
            return new C1ResourceProvider(classKey);
        }

        public override IResourceProvider CreateLocalResourceProvider(string virtualPath)
        {
            throw new NotSupportedException();
        }
    }
}

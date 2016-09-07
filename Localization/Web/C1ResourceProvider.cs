using System;
using System.Collections;
using System.Globalization;
using System.Resources;
using System.Web.Compilation;

namespace CompositeC1Contrib.Localization.Web
{
    public class C1ResourceProvider : IResourceProvider, IImplicitResourceProvider
    {
        private readonly string _resourceSet;

        public C1ResourceManager ResourceManager { get; private set; }

        public C1ResourceProvider(string resourceSet)
        {
            _resourceSet = resourceSet;

            ResourceManager = new C1ResourceManager(_resourceSet);
        }

        public object GetObject(string resourceKey, CultureInfo culture)
        {
            return ResourceManager.GetObject(resourceKey, culture);
        }

        public IResourceReader ResourceReader
        {
            get { return new C1ResourceReader(_resourceSet, CultureInfo.CurrentUICulture); }
        }

        public ICollection GetImplicitResourceKeys(string keyPrefix)
        {
            throw new NotImplementedException();
        }

        public object GetObject(ImplicitResourceKey key, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

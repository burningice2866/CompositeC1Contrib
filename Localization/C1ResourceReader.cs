using System.Collections;
using System.Globalization;
using System.Resources;

namespace CompositeC1Contrib.Localization
{
    public class C1ResourceReader : IResourceReader
    {
        private readonly string _resourceSet;
        private readonly CultureInfo _culture;

        public C1ResourceReader(string resourceSet, CultureInfo culture)
        {
            _resourceSet = resourceSet;
            _culture = culture;
        }

        public IDictionaryEnumerator GetEnumerator()
        {
            var dataManager = new C1ResourceDataManager(_resourceSet, _culture);

            return dataManager.GetResourceSet().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Close() { }
        public void Dispose() { }
    }
}

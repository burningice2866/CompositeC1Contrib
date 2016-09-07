using System.Collections;
using System.Globalization;
using System.Resources;

namespace CompositeC1Contrib.Localization
{
    public class C1ResourceReader : IResourceReader
    {
        private string _resourceSet;
        private CultureInfo _culture;

        public C1ResourceReader(string resourceSet, CultureInfo culture)
        {
            _resourceSet = resourceSet;
            _culture = culture;
        }

        public IDictionaryEnumerator GetEnumerator()
        {
            var dataManager = C1ResourceDataManager.Instance;

            return dataManager.GetResourceSet(_resourceSet, _culture).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Close() { }
        public void Dispose() { }
    }
}

using System.Collections.Generic;
using System.Globalization;

namespace CompositeC1Contrib.Localization.ImportExport
{
    public class ImportExportModelLanguage
    {
        public CultureInfo Culture { get; private set; }
        public IList<ImportExportModelResourceSet> ResourceSets { get; private set; }

        public ImportExportModelLanguage(CultureInfo culture)
        {
            Culture = culture;
            ResourceSets = new List<ImportExportModelResourceSet>();
        }
    }
}

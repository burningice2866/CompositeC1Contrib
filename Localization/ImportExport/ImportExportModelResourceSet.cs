using System.Collections.Generic;

namespace CompositeC1Contrib.Localization.ImportExport
{
    public class ImportExportModelResourceSet
    {
        public string Name { get; set; }

        public Dictionary<string, ImportExportModelResource> Resources { get; set; }

        public ImportExportModelResourceSet()
        {
            Resources = new Dictionary<string, ImportExportModelResource>();
        }
    }
}

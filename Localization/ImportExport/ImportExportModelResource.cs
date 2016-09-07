
namespace CompositeC1Contrib.Localization.ImportExport
{
    public class ImportExportModelResource
    {
        public string Key { get; set; }
        public ResourceType Type { get; set; }
        public string Value { get; set; }

        public ImportExportModelResource()
        {
            Type = ResourceType.Text;
        }
    }
}

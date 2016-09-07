using System.Globalization;

namespace CompositeC1Contrib.Localization.ImportExport
{
    public class ImportResult
    {
        public CultureInfo[] Languages { get; set; }
        public string[] ResourceSets { get; set; }

        public int KeysAdded { get; set; }
        public int ValuesAdded { get; set; }
        public int ValuesUpdated { get; set; }

        public int ValuesWereTheSame { get; set; }
    }
}

using System;
using System.IO;

using ICSharpCode.SharpZipLib.Zip;

namespace CompositeC1Contrib.SiteUpdate
{
    public class ZipFileHelper
    {
        public static string GetContentFromFile(string zipFilename, string fileName)
        {
            using (var fs = new FileStream(zipFilename, FileMode.Open, FileAccess.Read))
            {
                using (var zf = new ZipFile(fs))
                {
                    var ze = zf.GetEntry(fileName);
                    if (ze == null)
                    {
                        throw new ArgumentException(fileName + " not found", "fileName");
                    }

                    using (var s = zf.GetInputStream(ze))
                    {
                        using (var textReader = new StreamReader(s))
                        {
                            return textReader.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}

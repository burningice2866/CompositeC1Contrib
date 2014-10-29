using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using Composite.Core.IO.Zip;
using Composite.Core.PackageSystem;

namespace CompositeC1Contrib.PackageFragmentInstallers
{
    public static class SqlFileFragmentHelper
    {
        public static void ExecuteFiles(IList<string> files, XElement configuration, IZipFileSystem zipFileSystem)
        {
            var filesElement = configuration.Elements("Files").Single();
            var connectionStringName = filesElement.Attribute("connectionStringName").Value;
            var connectionStringSettings = ConfigurationManager.ConnectionStrings[connectionStringName];

            using (var conn = new SqlConnection(connectionStringSettings.ConnectionString))
            {
                conn.Open();

                using (var trans = conn.BeginTransaction())
                {
                    foreach (var path in files)
                    {
                        using (var s = zipFileSystem.GetFileStream(path))
                        {
                            using (var sr = new StreamReader(s))
                            {
                                var text = sr.ReadToEnd();

                                using (var cmd = conn.CreateCommand())
                                {
                                    cmd.Transaction = trans;
                                    cmd.CommandText = text;

                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }
                    }

                    trans.Commit();
                }
            }
        }

        public static List<string> GetFiles(string attributeName, XElement configuration, IZipFileSystem zipFileSystem, out IList<PackageFragmentValidationResult> validationResult)
        {
            var files = new List<string>();

            validationResult = new List<PackageFragmentValidationResult>();

            var filesElements = configuration.Elements("Files").ToList();
            if (filesElements.Count() > 1)
            {
                validationResult.Add(
                    new PackageFragmentValidationResult(PackageFragmentValidationResultType.Fatal,
                    "You can only specify one Files element", configuration));
            }

            var filesElement = filesElements.SingleOrDefault();
            if (filesElement == null)
            {
                return files;
            }

            var connectionStringNameAttr = filesElement.Attribute("connectionStringName");
            if (connectionStringNameAttr == null)
            {
                validationResult.Add(new PackageFragmentValidationResult(PackageFragmentValidationResultType.Fatal, "Missing connectionstring name attribute", filesElement));

                return files;
            }

            var connectionStringSettings = ConfigurationManager.ConnectionStrings[connectionStringNameAttr.Value];
            if (connectionStringSettings == null)
            {
                validationResult.Add(new PackageFragmentValidationResult(PackageFragmentValidationResultType.Fatal, "C1 connectionstring missing"));

                return files;
            }

            var elements = filesElement.Elements("File");
            foreach (var file in elements)
            {
                var pathAttribute = file.Attribute(attributeName);
                if (pathAttribute == null)
                {
                    validationResult.Add(new PackageFragmentValidationResult(PackageFragmentValidationResultType.Fatal, "Missing path attribute", file));

                    continue;
                }

                var path = pathAttribute.Value;
                if (!zipFileSystem.ContainsFile(path))
                {
                    validationResult.Add(new PackageFragmentValidationResult(PackageFragmentValidationResultType.Fatal, "File is missing", pathAttribute));

                    continue;
                }

                files.Add(path);
            }

            return files;
        }
    }
}

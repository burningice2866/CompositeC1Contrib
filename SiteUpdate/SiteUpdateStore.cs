using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Web.Hosting;

using Composite.Core.PackageSystem;

namespace CompositeC1Contrib.SiteUpdate
{
    public class SiteUpdateStore
    {
        private readonly string _location;

        public SiteUpdateStore(string location)
        {
            _location = location;
        }

        public IEnumerable<SiteUpdateInformation> GetUpdateSummaries(Guid installationId)
        {
            if (_location.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                using (var client = new HttpClient())
                {
                    var url = _location + "/" + installationId;

                    return client.GetAsync(url)
                        .Result
                        .EnsureSuccessStatusCode()
                        .Content
                        .ReadAsAsync<IEnumerable<SiteUpdateInformation>>()
                        .Result;
                }
            }

            var basePath = HostingEnvironment.MapPath(_location);
            var folder = Path.Combine(basePath, installationId.ToString());

            var list = new List<SiteUpdateInformation>();

            if (!Directory.Exists(folder))
            {
                return list;
            }

            var files = Directory.GetFiles(folder, "*.zip");
            foreach (var f in files)
            {
                try
                {
                    var fi = new FileInfo(f);
                    var packageInformation = PackageSystemServices.GetPackageInformationFromZipfile(f);
                    var changelog = ZipFileHelper.GetContentFromFile(f, "changelog.txt");

                    list.Add(new SiteUpdateInformation
                    {
                        Id = packageInformation.Id,
                        InstallationId = installationId,
                        FileName = fi.Name,
                        Name = packageInformation.Name,
                        Version = packageInformation.Version,
                        ReleasedDate = fi.CreationTime,
                        ChangeLog = changelog
                    });
                }
                catch { }
            }

            return list;
        }

        public Stream GetZipStream(SiteUpdateInformation update)
        {
            if (_location.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                using (var client = new HttpClient())
                {
                    var url = _location + "/" + update.InstallationId + "/" + update.Id + "/stream";

                    return client.GetStreamAsync(url).Result;
                }
            }

            var basePath = HostingEnvironment.MapPath(_location);
            var folder = Path.Combine(basePath, update.InstallationId.ToString());
            var file = Path.Combine(folder, update.FileName);

            return File.OpenRead(file);
        }
    }
}

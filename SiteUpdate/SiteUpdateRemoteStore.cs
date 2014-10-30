using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;

namespace CompositeC1Contrib.SiteUpdate
{
    public class SiteUpdateRemoteStore : ISiteUpdateStore
    {
        private readonly string _location;

        public SiteUpdateRemoteStore(string location)
        {
            _location = location;
        }

        public IEnumerable<SiteUpdateInformation> GetUpdateSummaries(Guid installationId)
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
        
        public Stream GetZipStream(SiteUpdateInformation update)
        {
            using (var client = new HttpClient())
            {
                var url = _location + "/" + update.InstallationId + "/" + update.Id + "/stream";

                return client.GetStreamAsync(url).Result;
            }
        }
    }
}

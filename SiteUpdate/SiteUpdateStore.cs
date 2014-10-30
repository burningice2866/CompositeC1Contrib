using System;
using System.Collections.Generic;
using System.IO;

namespace CompositeC1Contrib.SiteUpdate
{
    public class SiteUpdateStore
    {
        private readonly ISiteUpdateStore _siteUpdateStore;

        public SiteUpdateStore(string location)
        {
            if (location.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                _siteUpdateStore = new SiteUpdateRemoteStore(location);
            }
            else
            {
                _siteUpdateStore = new SiteUpdateLocalStore(location);
            }
        }

        public IEnumerable<SiteUpdateInformation> GetUpdateSummaries(Guid installationId)
        {
            return _siteUpdateStore.GetUpdateSummaries(installationId);
        }

        public Stream GetZipStream(SiteUpdateInformation update)
        {
            return _siteUpdateStore.GetZipStream(update);
        }
    }
}

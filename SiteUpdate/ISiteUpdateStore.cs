using System;
using System.Collections.Generic;
using System.IO;

namespace CompositeC1Contrib.SiteUpdate
{
    public interface ISiteUpdateStore
    {
        IEnumerable<SiteUpdateInformation> GetUpdateSummaries(Guid installationId);
        Stream GetZipStream(SiteUpdateInformation update);
    }
}

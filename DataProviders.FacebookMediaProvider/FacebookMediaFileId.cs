using System;

using Composite.Data;

namespace CompositeC1Contrib.DataProviders.FacebookMediaProvider
{
    public class FacebookMediaFileId : IDataId
    {
        public string Id { get; set; }
        public Guid AlbumId { get; set; }
    }
}

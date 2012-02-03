using Composite.Data;

namespace CompositeC1Contrib.DataProviders.FacebookMediaProvider
{
    public class FacebookPhotoId : IDataId
    {
        public string Id { get; set; }
        public string AlbumId { get; set; }
    }
}

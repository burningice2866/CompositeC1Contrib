using System;

using Composite.Data;

namespace CompositeC1Contrib.DataProviders.FacebookMediaProvider
{
    public class FacebookPhoto : IFacebookPhoto
    {
        private DataSourceId _dataSourceId;

        public string Id { get; set; }
        public string AlbumId { get; set; }
        public string Title { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime UpdatedTime { get; set; }

        public DataSourceId DataSourceId
        {
            get { return _dataSourceId; }
        }

        public FacebookPhoto(DataSourceId dataSourceId)
        {
            _dataSourceId = dataSourceId;
        }        
    }
}

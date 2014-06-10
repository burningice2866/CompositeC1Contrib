using Composite.Data;
using Composite.Data.Types;

namespace CompositeC1Contrib.DataProviders.FacebookMediaProvider
{
    public class FacebookMediaStore : IMediaFileStore
    {
        private readonly DataSourceId _dataSourceId;
        public DataSourceId DataSourceId
        {
            get { return _dataSourceId; }
        }

        public string Description
        {
            get { return "Access facebook album photos"; }
        }

        public string Id
        {
            get { return "FacebookPhotos"; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public bool ProvidesMetadata
        {
            get { return false; }
        }

        public string Title
        {
            get { return "Facebook photos"; }
        }

        public FacebookMediaStore(DataSourceId dataSourceId)
        {
            _dataSourceId = dataSourceId;
        }
    }
}
